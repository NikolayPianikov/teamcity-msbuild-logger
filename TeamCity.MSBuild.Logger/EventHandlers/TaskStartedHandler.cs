namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class TaskStartedHandler(
    ILoggerContext context,
    ILogWriter logWriter,
    IPerformanceCounterFactory performanceCounterFactory,
    IMessageWriter messageWriter,
    IDeferredMessageWriter deferredMessageWriter,
    IStringService stringService)
    : IBuildEventHandler<TaskStartedEventArgs>
{
    private readonly IStringService _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
    private readonly IDeferredMessageWriter _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
    private readonly IPerformanceCounterFactory _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));

    public void Handle(TaskStartedEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            _deferredMessageWriter.DisplayDeferredStartedEvents(e.BuildEventContext);
            if (_context.Parameters is { ShowOnlyErrors: false, ShowOnlyWarnings: false })
            {
                var prefixAlreadyWritten = _messageWriter.WriteTargetMessagePrefix(e, e.BuildEventContext, e.Timestamp);
                _logWriter.SetColor(Color.Task);
                if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic) || (_context.Parameters.ShowEventId ?? false))
                {
                    _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TaskMessageWithId", e.Message, e.BuildEventContext.TaskId), prefixAlreadyWritten);
                }
                else
                {
                    _messageWriter.WriteMessageAligned(e.Message, prefixAlreadyWritten);
                }

                _logWriter.ResetColor();
            }

            _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
        }

        if (!_context.Parameters.ShowPerfSummary)
        {
            return;
        }

        _performanceCounterFactory.GetOrCreatePerformanceCounter(e.TaskName, _context.TaskPerformanceCounters).AddEventStarted(null, e.BuildEventContext, e.Timestamp, null);
    }
}