namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class WarningHandler(
    ILoggerContext context,
    ILogWriter logWriter,
    IMessageWriter messageWriter,
    IDeferredMessageWriter deferredMessageWriter,
    IBuildEventManager buildEventManager,
    IEventFormatter eventFormatter)
    : IBuildEventHandler<BuildWarningEventArgs>
{
    private readonly IEventFormatter _eventFormatter = eventFormatter ?? throw new ArgumentNullException(nameof(eventFormatter));
    private readonly IBuildEventManager _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
    private readonly IDeferredMessageWriter _deferredMessageWrite = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));

    public void Handle(BuildWarningEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
        _context.WarningCount += 1;
        _buildEventManager.SetErrorWarningFlagOnCallStack(e.BuildEventContext);
        var targetStartedEvent = _buildEventManager.GetTargetStartedEvent(e.BuildEventContext);
        if (targetStartedEvent != null)
        {
            targetStartedEvent.ErrorInTarget = true;
        }

        _deferredMessageWrite.DisplayDeferredStartedEvents(e.BuildEventContext);
        if (!_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
            }

            _logWriter.SetColor(Color.Warning);
            _messageWriter.WriteMessageAligned(_eventFormatter.FormatEventMessage(e, false, _context.Parameters.ShowProjectFile), true);
        }

        _deferredMessageWrite.ShownBuildEventContext(e.BuildEventContext);
        if (_context.WarningList != null && (_context.Parameters.ShowSummary ?? false) && !_context.WarningList.Contains(e))
        {
            _context.WarningList.Add(e);
        }

        _logWriter.ResetColor();
    }
}