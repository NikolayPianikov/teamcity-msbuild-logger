namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class PerformanceCounter(
    ILogWriter logWriter,
    IPerformanceCounterFactory performanceCounterFactory,
    IMessageWriter messageWriter)
    : IPerformanceCounter
{
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly IDictionary<string, IPerformanceCounter>? _internalPerformanceCounters = new Dictionary<string, IPerformanceCounter>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<BuildEventContext, long>? _startedEvent;
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
    private readonly IPerformanceCounterFactory _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
    private int _calls;

    public string ScopeName { get; set; } = string.Empty;

    public TimeSpan ElapsedTime { get; private set; } = new(0L);

    public bool ReenteredScope => false;

    public int MessageIdentLevel { private get; set; } = 2;

    public void AddEventStarted(string? projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp, IEqualityComparer<BuildEventContext>? comparer)
    {
        if (projectTargetNames is not null && !string.IsNullOrEmpty(projectTargetNames) && _internalPerformanceCounters is not null)
        {
            var performanceCounter = _performanceCounterFactory.GetOrCreatePerformanceCounter(projectTargetNames, _internalPerformanceCounters);
            performanceCounter.AddEventStarted(null, buildEventContext, eventTimeStamp, ComparerContextNodeIdTargetId.Shared);
            performanceCounter.MessageIdentLevel = 7;
        }

        _startedEvent ??= comparer != null
            ? new Dictionary<BuildEventContext, long>(comparer)
            : new Dictionary<BuildEventContext, long>();

        if (_startedEvent.ContainsKey(buildEventContext))
        {
            return;
        }

        _startedEvent.Add(buildEventContext, eventTimeStamp.Ticks);
        _calls += 1;
    }

    public void AddEventFinished(string? projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp)
    {
        if (projectTargetNames is not null && !string.IsNullOrEmpty(projectTargetNames) && _internalPerformanceCounters is not null)
        {
            _performanceCounterFactory.GetOrCreatePerformanceCounter(projectTargetNames, _internalPerformanceCounters).AddEventFinished(null, buildEventContext, eventTimeStamp);
        }

        if (_startedEvent == null)
        {
            throw new InvalidOperationException("Cannot have finished counter without started counter.");
        }

        if (!_startedEvent.TryGetValue(buildEventContext, out var ticks))
        {
            return;
        }

        ElapsedTime += TimeSpan.FromTicks(eventTimeStamp.Ticks - ticks);
        _startedEvent.Remove(buildEventContext);
    }

    public void PrintCounterMessage()
    {
        var str = string.Format(CultureInfo.CurrentCulture, "{0,5}", Math.Round(ElapsedTime.TotalMilliseconds, 0));
        _messageWriter.WriteLinePrettyFromResource(MessageIdentLevel, "PerformanceLine", str, string.Format(CultureInfo.CurrentCulture, "{0,-40}", ScopeName), string.Format(CultureInfo.CurrentCulture, "{0,3}", _calls));
        if (_internalPerformanceCounters is not { Count: > 0 })
        {
            return;
        }

        foreach (var performanceCounter in _internalPerformanceCounters.Values)
        {
            _logWriter.SetColor(Color.PerformanceCounterInfo);
            performanceCounter.PrintCounterMessage();
            _logWriter.ResetColor();
        }
    }
}