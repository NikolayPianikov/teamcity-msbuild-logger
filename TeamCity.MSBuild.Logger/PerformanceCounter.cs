﻿namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using DevTeam.IoC.Contracts;
    using Microsoft.Build.Framework;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PerformanceCounter: IPerformanceCounter
    {
        private readonly IDictionary<string, IPerformanceCounter> _internalPerformanceCounters = new Dictionary<string, IPerformanceCounter>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<BuildEventContext, long> _startedEvent;
        private bool _inScope;
        private DateTime _scopeStartTime;
        private readonly string _scopeName;
        [NotNull] private readonly ILogWriter _logWriter;
        [NotNull] private readonly IPerformanceCounterFactory _performanceCounterFactory;
        private int _calls;

        internal PerformanceCounter(
            [NotNull] [State] string scopeName,
            [NotNull] ILogWriter logWriter,
            [NotNull] IPerformanceCounterFactory performanceCounterFactory)
        {
            _scopeName = scopeName ?? throw new ArgumentNullException(nameof(scopeName));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
        }

        public TimeSpan ElapsedTime { get; private set; } = new TimeSpan(0L);

        public bool ReenteredScope { get; private set; }

        public int MessageIdentLevel { private get; set; } = 2;

        private bool InScope
        {
            get => _inScope;
            set
            {
                if (ReenteredScope)
                    return;
                if (InScope && !value)
                {
                    _inScope = false;
                    ElapsedTime = ElapsedTime + (DateTime.Now - _scopeStartTime);
                }
                else if (!InScope & value)
                {
                    _inScope = true;
                    _calls = _calls + 1;
                    _scopeStartTime = DateTime.Now;
                }
                else
                {
                    ReenteredScope = true;
                }
            }
        }

        public void AddEventStarted(string projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp, IEqualityComparer<BuildEventContext> comparer)
        {
            if (!string.IsNullOrEmpty(projectTargetNames))
            {
                var performanceCounter = _performanceCounterFactory.GetOrCreatePerformanceCounter(projectTargetNames, _internalPerformanceCounters);
                performanceCounter.AddEventStarted(null, buildEventContext, eventTimeStamp, ComparerContextNodeIdTargetId.Shared);
                performanceCounter.MessageIdentLevel = 7;
            }

            if (_startedEvent == null)
            {
                _startedEvent = comparer != null ? new Dictionary<BuildEventContext, long>(comparer) : new Dictionary<BuildEventContext, long>();
            }

            if (_startedEvent.ContainsKey(buildEventContext))
            {
                return;
            }

            _startedEvent.Add(buildEventContext, eventTimeStamp.Ticks);
            _calls = _calls + 1;
        }

        public void AddEventFinished(string projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp)
        {
            if (!string.IsNullOrEmpty(projectTargetNames))
            {
                _performanceCounterFactory.GetOrCreatePerformanceCounter(projectTargetNames, _internalPerformanceCounters).AddEventFinished(null, buildEventContext, eventTimeStamp);
            }

            if (_startedEvent == null)
            {
                throw new InvalidOperationException("Cannot have finished counter without started counter.");
            }

            if (!_startedEvent.TryGetValue(buildEventContext, out long ticks))
            {
                return;
            }

            ElapsedTime = ElapsedTime + TimeSpan.FromTicks(eventTimeStamp.Ticks - ticks);
            _startedEvent.Remove(buildEventContext);
        }

        public void PrintCounterMessage(WriteLinePrettyFromResourceDelegate writeLinePrettyFromResource)
        {
            var str = string.Format(CultureInfo.CurrentCulture, "{0,5}", Math.Round(ElapsedTime.TotalMilliseconds, 0));
            writeLinePrettyFromResource(MessageIdentLevel, "PerformanceLine", str, string.Format(CultureInfo.CurrentCulture, "{0,-40}", _scopeName), string.Format(CultureInfo.CurrentCulture, "{0,3}", _calls));
            if (_internalPerformanceCounters == null || _internalPerformanceCounters.Count <= 0)
            {
                return;
            }

            foreach (var performanceCounter in _internalPerformanceCounters.Values)
            {
                _logWriter.SetColor(Color.PerformanceCounterInfo);
                performanceCounter.PrintCounterMessage(writeLinePrettyFromResource);
                _logWriter.ResetColor();
            }
        }
    }
}