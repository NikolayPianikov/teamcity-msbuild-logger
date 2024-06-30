namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class BuildFinishedHandler(
    ILoggerContext context,
    ILogWriter logWriter,
    IMessageWriter messageWriter,
    IBuildEventManager buildEventManager,
    ILogFormatter logFormatter,
    IEventFormatter eventFormatter,
    IHierarchicalMessageWriter hierarchicalMessageWriter,
    IStringService stringService,
    IStatistics statistics)
    : IBuildEventHandler<BuildFinishedEventArgs>
{
    private readonly IStatistics _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
    private readonly IStringService _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
    private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
    private readonly IEventFormatter _eventFormatter = eventFormatter ?? throw new ArgumentNullException(nameof(eventFormatter));
    private readonly ILogFormatter _logFormatter = logFormatter ?? throw new ArgumentNullException(nameof(logFormatter));
    private readonly IBuildEventManager _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));

    public void Handle(BuildFinishedEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));

        _statistics.Publish();

        if (_context is { Parameters: { ShowOnlyErrors: false, ShowOnlyWarnings: false }, DeferredMessages.Count: > 0 } && _context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            _messageWriter.WriteLinePrettyFromResource("DeferredMessages");
            foreach (var message in _context.DeferredMessages.Values.SelectMany(i => i))
            {
                _messageWriter.PrintMessage(message, false);
            }
        }

        if (_context.Parameters.ShowPerfSummary)
        {
            ShowPerfSummary();
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) || (_context.Parameters.ShowSummary ?? false))
        {
            if (e.Succeeded)
            {
                _logWriter.SetColor(Color.Success);
            }
            else
            {
                _logWriter.SetColor(_context.ErrorCount > 0 ? Color.Error : Color.Warning);
            }

            _messageWriter.WriteNewLine();
            _messageWriter.WriteLinePretty(e.Message);
            _logWriter.ResetColor();
        }

        if (_context.Parameters.ShowSummary ?? false)
        {
            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                ShowNestedErrorWarningSummary();
            }
            else
            {
                ShowFlatErrorWarningSummary();
            }

            if (_context.WarningCount > 0)
            {
                _logWriter.SetColor(Color.Warning);
            }

            _messageWriter.WriteLinePrettyFromResource(2, "WarningCount", _context.WarningCount);

            _logWriter.ResetColor();
            if (_context.ErrorCount > 0)
            {
                _logWriter.SetColor(Color.Error);
            }

            _messageWriter.WriteLinePrettyFromResource(2, "ErrorCount", _context.ErrorCount);
            _logWriter.ResetColor();
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal) || (_context.Parameters.ShowSummary ?? false))
        {
            var str = _logFormatter.FormatTimeSpan(e.Timestamp - _context.BuildStarted);
            _messageWriter.WriteNewLine();
            _messageWriter.WriteLinePrettyFromResource("TimeElapsed", str);
        }

        _context.ResetConsoleLoggerState();
    }

    private void ShowFlatErrorWarningSummary()
    {
        if (_context.WarningList?.Count == 0 && _context.ErrorList?.Count == 0 || _context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            return;
        }

        _messageWriter.WriteNewLine();

        if (_context.WarningList is { Count: > 0 })
        {
            _logWriter.SetColor(Color.WarningSummary);
            foreach (var warning in _context.WarningList)
            {
                _messageWriter.WriteMessageAligned(_eventFormatter.FormatEventMessage(warning, false, _context.Parameters.ShowProjectFile), true);
            }
        }

        if (_context.ErrorList is { Count: > 0 })
        {
            _logWriter.SetColor(Color.ErrorSummary);
            foreach (var error in _context.ErrorList)
            {
                _messageWriter.WriteMessageAligned(_eventFormatter.FormatEventMessage(error, false, _context.Parameters.ShowProjectFile), true);
            }
        }

        _logWriter.ResetColor();
    }

    private void ShowPerfSummary()
    {
        _hierarchicalMessageWriter.StartBlock("Performance Summary");

        _logWriter.SetColor(Color.PerformanceHeader);
        _messageWriter.WriteNewLine();
        _messageWriter.WriteLinePrettyFromResource("ProjectPerformanceSummary");
        _logWriter.SetColor(Color.SummaryInfo);
        _messageWriter.DisplayCounters(_context.ProjectPerformanceCounters);

        _logWriter.SetColor(Color.PerformanceHeader);
        _messageWriter.WriteNewLine();
        _messageWriter.WriteLinePrettyFromResource("TargetPerformanceSummary");
        _logWriter.SetColor(Color.SummaryInfo);
        _messageWriter.DisplayCounters(_context.TargetPerformanceCounters);

        _logWriter.SetColor(Color.PerformanceHeader);
        _messageWriter.WriteNewLine();
        _messageWriter.WriteLinePrettyFromResource("TaskPerformanceSummary");
        _logWriter.SetColor(Color.SummaryInfo);
        _messageWriter.DisplayCounters(_context.TaskPerformanceCounters);

        _hierarchicalMessageWriter.FinishBlock();
        _logWriter.ResetColor();
    }


    private void ShowNestedErrorWarningSummary()
    {
        if (_context.WarningList?.Count == 0 && _context.ErrorList?.Count == 0 || _context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            return;
        }

        if (_context.WarningCount > 0)
        {
            _logWriter.SetColor(Color.WarningSummary);
            ShowErrorWarningSummary(_context.WarningList);
        }

        if (_context.ErrorCount > 0)
        {
            _logWriter.SetColor(Color.ErrorSummary);
            ShowErrorWarningSummary(_context.ErrorList);
        }

        _logWriter.ResetColor();
    }

    private void ShowErrorWarningSummary<T>(IEnumerable<T>? events) where T : BuildEventArgs
    {
        if (events == null)
        {
            return;
        }

        var dictionary = new Dictionary<ErrorWarningSummaryDictionaryKey, List<T>>();
        foreach (var warningEventArgs in events)
        {
            string? targetName = null;
            var targetStartedEvent = _buildEventManager.GetTargetStartedEvent(warningEventArgs.BuildEventContext);
            if (targetStartedEvent != null)
            {
                targetName = targetStartedEvent.TargetName;
            }

            var key = new ErrorWarningSummaryDictionaryKey(warningEventArgs.BuildEventContext, targetName);
            if (!dictionary.TryGetValue(key, out var list))
            {
                list = [];
                dictionary.Add(key, list);
            }

            list.Add(warningEventArgs);
        }

        BuildEventContext? buildEventContext = default;
        string? curTargetName = default;
        foreach (var keyValuePair in dictionary)
        {
            if (buildEventContext != keyValuePair.Key.EntryPointContext)
            {
                _messageWriter.WriteNewLine();
                foreach (var message in _buildEventManager.ProjectCallStackFromProject(keyValuePair.Key.EntryPointContext))
                {
                    _messageWriter.WriteMessageAligned(message, false);
                }

                buildEventContext = keyValuePair.Key.EntryPointContext;
            }

            if (string.Compare(curTargetName, keyValuePair.Key.TargetName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                if (!string.IsNullOrEmpty(keyValuePair.Key.TargetName))
                {
                    _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("ErrorWarningInTarget", keyValuePair.Key.TargetName), false);
                }

                curTargetName = keyValuePair.Key.TargetName;
            }

            foreach (var obj in keyValuePair.Value)
            {
                switch (obj)
                {
                    case BuildErrorEventArgs errorEventArgs:
                        _messageWriter.WriteMessageAligned("  " + _eventFormatter.FormatEventMessage(errorEventArgs, false, _context.Parameters.ShowProjectFile), false);
                        continue;

                    case BuildWarningEventArgs warningEventArgs:
                        _messageWriter.WriteMessageAligned("  " + _eventFormatter.FormatEventMessage(warningEventArgs, false, _context.Parameters.ShowProjectFile), false);
                        break;
                }
            }

            _messageWriter.WriteNewLine();
        }
    }
}