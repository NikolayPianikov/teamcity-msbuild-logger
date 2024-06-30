namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class CustomEventHandler(
    ILoggerContext context,
    IMessageWriter messageWriter,
    IDeferredMessageWriter deferredMessageWriter)
    : IBuildEventHandler<CustomBuildEventArgs>
{
    private readonly IDeferredMessageWriter _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Handle(CustomBuildEventArgs e)
    {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
            {
                return;
            }

            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));

            if (!_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed) || e.Message == null)
            {
                return;
            }

            _deferredMessageWriter.DisplayDeferredStartedEvents(e.BuildEventContext);
            _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
            _messageWriter.WriteMessageAligned(e.Message, true);
            _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
        }
}