namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class TargetFinishedHandler(
    ILoggerContext context,
    ILogWriter logWriter,
    IPerformanceCounterFactory performanceCounterFactory,
    IMessageWriter messageWriter,
    IHierarchicalMessageWriter hierarchicalMessageWriter,
    IDeferredMessageWriter deferredMessageWriter,
    IBuildEventManager buildEventManager,
    IStringService stringService)
    : IBuildEventHandler<TargetFinishedEventArgs>
{
    private readonly IStringService _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
    private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
    private readonly IBuildEventManager _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
    private readonly IDeferredMessageWriter _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
    private readonly IPerformanceCounterFactory _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));

    public void Handle(TargetFinishedEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
        if (_context.Parameters.ShowPerfSummary)
        {
            _performanceCounterFactory.GetOrCreatePerformanceCounter(e.TargetName, _context.TargetPerformanceCounters).AddEventFinished(null, e.BuildEventContext, e.Timestamp);
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            _deferredMessageWriter.DisplayDeferredTargetStartedEvent(e.BuildEventContext);
            var targetStartedEvent = _buildEventManager.GetTargetStartedEvent(e.BuildEventContext);
            // ReSharper disable once NotResolvedInText
            if (targetStartedEvent == null) throw new ArgumentNullException("Started event should not be null in the finished event handler");
            if (targetStartedEvent.ShowTargetFinishedEvent)
            {
                if (_context.Parameters.ShowTargetOutputs)
                {
                    var targetOutputs = e.TargetOutputs;
                    if (targetOutputs != null)
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetOutputItemsHeader"), false);
                        foreach (ITaskItem taskItem in targetOutputs)
                        {
                            _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetOutputItem", taskItem.ItemSpec), false);
                            foreach (DictionaryEntry dictionaryEntry in taskItem.CloneCustomMetadata())
                            {
                                _messageWriter.WriteMessageAligned(new string(' ', 8) + dictionaryEntry.Key + " = " + taskItem.GetMetadata((string)dictionaryEntry.Key), false);
                            }
                        }
                    }
                }

                if (_context.Parameters is { ShowOnlyErrors: false, ShowOnlyWarnings: false })
                {
                    _context.LastProjectFullKey = _context.GetFullProjectKey(e.BuildEventContext);
                    _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
                    _logWriter.SetColor(Color.BuildStage);
                    if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic) || (_context.Parameters.ShowEventId ?? false))
                    {
                        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetMessageWithId", e.Message, e.BuildEventContext.TargetId), true);
                    }
                    else
                    {
                        _messageWriter.WriteMessageAligned(e.Message, true);
                    }

                    _logWriter.ResetColor();
                }

                _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
                _hierarchicalMessageWriter.FinishBlock();
            }
        }

        _buildEventManager.RemoveTargetStartedEvent(e.BuildEventContext);
    }
}