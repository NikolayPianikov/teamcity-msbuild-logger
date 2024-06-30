﻿namespace TeamCity.MSBuild.Logger.EventHandlers;

using System;
using Microsoft.Build.Framework;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ProjectFinishedHandler : IBuildEventHandler<ProjectFinishedEventArgs>
{
    private readonly IStringService _stringService;
    private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter;
    private readonly IBuildEventManager _buildEventManager;
    private readonly IDeferredMessageWriter _deferredMessageWriter;
    private readonly IMessageWriter _messageWriter;
    private readonly ILoggerContext _context;
    private readonly IPerformanceCounterFactory _performanceCounterFactory;
    private readonly ILogWriter _logWriter;

    public ProjectFinishedHandler(
        ILoggerContext context,
        ILogWriter logWriter,
        IPerformanceCounterFactory performanceCounterFactory,
        IMessageWriter messageWriter,
        IHierarchicalMessageWriter hierarchicalMessageWriter,
        IDeferredMessageWriter deferredMessageWriter,
        IBuildEventManager buildEventManager,
        IStringService stringService)
    {
        _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
        _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
        _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
        _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
        _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
        _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
    }

    public void Handle(ProjectFinishedEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        var projectStartedEvent = _buildEventManager.GetProjectStartedEvent(e.BuildEventContext);
        // ReSharper disable once LocalizableElement
        if (projectStartedEvent == null) throw new ArgumentException($"Project finished event for {e.ProjectFile} received without matching start event", nameof(e));
        if (_context.Parameters.ShowPerfSummary)
        {
            _performanceCounterFactory.GetOrCreatePerformanceCounter(e.ProjectFile, _context.ProjectPerformanceCounters).AddEventFinished(projectStartedEvent.TargetNames, e.BuildEventContext, e.Timestamp);
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) && projectStartedEvent.ShowProjectFinishedEvent)
        {
            _context.LastProjectFullKey = _context.GetFullProjectKey(e.BuildEventContext);
            if (!_context.Parameters.ShowOnlyErrors && !_context.Parameters.ShowOnlyWarnings)
            {
                _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
                _logWriter.SetColor(Color.BuildStage);
                var targetNames = projectStartedEvent.TargetNames;
                var projectFile = projectStartedEvent.ProjectFile ?? string.Empty;
                if (string.IsNullOrEmpty(targetNames))
                {
                    if (e.Succeeded)
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("ProjectFinishedPrefixWithDefaultTargetsMultiProc", projectFile), true);
                        _hierarchicalMessageWriter.FinishBlock();
                    }
                    else
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("ProjectFinishedPrefixWithDefaultTargetsMultiProcFailed", projectFile), true);
                        _hierarchicalMessageWriter.FinishBlock();
                    }
                }
                else
                {
                    if (e.Succeeded)
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("ProjectFinishedPrefixWithTargetNamesMultiProc", projectFile, targetNames), true);
                        _hierarchicalMessageWriter.FinishBlock();
                    }
                    else
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("ProjectFinishedPrefixWithTargetNamesMultiProcFailed", projectFile, targetNames), true);
                        _hierarchicalMessageWriter.FinishBlock();
                    }
                }
            }

            _deferredMessageWriter.ShownBuildEventContext(projectStartedEvent.ProjectBuildEventContext);
            _logWriter.ResetColor();
        }

        _buildEventManager.RemoveProjectStartedEvent(e.BuildEventContext);
    }
}