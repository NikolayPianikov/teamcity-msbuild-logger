namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class MessageHandler(
    ILoggerContext context,
    IMessageWriter messageWriter,
    IDeferredMessageWriter deferredMessageWriter,
    IBuildEventManager buildEventManager)
    : IBuildEventHandler<BuildMessageEventArgs>
{
    private readonly IBuildEventManager _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
    private readonly IDeferredMessageWriter _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Handle(BuildMessageEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            return;
        }

        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
        bool showMessages;
        var lightenText = false;
        if (e is TaskCommandLineEventArgs)
        {
            if (_context.Parameters.ShowCommandLine != true && !_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                return;
            }

            showMessages = true;
        }
        else
        {
            switch (e.Importance)
            {
                case MessageImportance.High:
                    showMessages = _context.IsVerbosityAtLeast(LoggerVerbosity.Minimal);
                    break;
                case MessageImportance.Normal:
                    showMessages = _context.IsVerbosityAtLeast(LoggerVerbosity.Normal);
                    lightenText = true;
                    break;
                case MessageImportance.Low:
                    showMessages = _context.IsVerbosityAtLeast(LoggerVerbosity.Detailed);
                    lightenText = true;
                    break;
                default:
                    throw new InvalidOperationException("Impossible");
            }
        }

        if (!showMessages)
        {
            return;
        }

        if (_context.HasBuildStarted && e.BuildEventContext.ProjectContextId != -2 && _buildEventManager.GetProjectStartedEvent(e.BuildEventContext) == null && _context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            if (!_context.DeferredMessages.TryGetValue(e.BuildEventContext, out var messageEventArgsList))
            {
                messageEventArgsList = new List<BuildMessageEventArgs>();
                _context.DeferredMessages.Add(e.BuildEventContext, messageEventArgsList);
            }
                
            messageEventArgsList.Add(e);
        }
        else
        {
            _deferredMessageWriter.DisplayDeferredStartedEvents(e.BuildEventContext);
            _messageWriter.PrintMessage(e, lightenText);
            _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
        }
    }
}