namespace TeamCity.MSBuild.Logger.EventHandlers;

using System;
using Microsoft.Build.Framework;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ErrorHandler : IBuildEventHandler<BuildErrorEventArgs>
{
    private readonly IEventFormatter _eventFormatter;
    private readonly IBuildEventManager _buildEventManager;
    private readonly IDeferredMessageWriter _deferredMessageWriter;
    private readonly IMessageWriter _messageWriter;
    private readonly ILoggerContext _context;
    private readonly ILogWriter _logWriter;

    public ErrorHandler(
        ILoggerContext context,
        ILogWriter logWriter,
        IMessageWriter messageWriter,
        IDeferredMessageWriter deferredMessageWriter,
        IBuildEventManager buildEventManager,
        IEventFormatter eventFormatter)
    {
        _eventFormatter = eventFormatter ?? throw new ArgumentNullException(nameof(eventFormatter));
        _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
        _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
        _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
    }

    public void Handle(BuildErrorEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));

        _context.ErrorCount += 1;
        _buildEventManager.SetErrorWarningFlagOnCallStack(e.BuildEventContext);
        var targetStartedEvent = _buildEventManager.GetTargetStartedEvent(e.BuildEventContext);
        if (targetStartedEvent != null)
        {
            targetStartedEvent.ErrorInTarget = true;
        }

        _deferredMessageWriter.DisplayDeferredStartedEvents(e.BuildEventContext);
        if (_context.Parameters is { ShowOnlyWarnings: true, ShowOnlyErrors: false })
        {
            return;
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
        }

        _logWriter.SetColor(Color.Error);
        _messageWriter.WriteMessageAligned(_eventFormatter.FormatEventMessage(e, false, _context.Parameters.ShowProjectFile), true);
        _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
        if (_context.ErrorList != null && (_context.Parameters.ShowSummary ?? false) && !_context.ErrorList.Contains(e))
        {
            _context.ErrorList.Add(e);
        }

        _logWriter.ResetColor();
    }
}