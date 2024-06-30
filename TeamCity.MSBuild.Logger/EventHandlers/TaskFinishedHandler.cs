namespace TeamCity.MSBuild.Logger.EventHandlers;

using System;
using Microsoft.Build.Framework;

// ReSharper disable once ClassNeverInstantiated.Global
internal class TaskFinishedHandler : IBuildEventHandler<TaskFinishedEventArgs>
{
    private readonly IStringService _stringService;
    private readonly IDeferredMessageWriter _deferredMessageWriter;
    private readonly IMessageWriter _messageWriter;
    private readonly ILoggerContext _context;
    private readonly ILogWriter _logWriter;
    private readonly IPerformanceCounterFactory _performanceCounterFactory;

    public TaskFinishedHandler(
        ILoggerContext context,
        ILogWriter logWriter,
        IPerformanceCounterFactory performanceCounterFactory,
        IMessageWriter messageWriter,
        IDeferredMessageWriter deferredMessageWriter,
        IStringService stringService)
    {
            _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
            _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
            _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
            _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
        }

    public void Handle(TaskFinishedEventArgs e)
    {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));
            if (_context.Parameters.ShowPerfSummary)
            {
                _performanceCounterFactory.GetOrCreatePerformanceCounter(e.TaskName, _context.TaskPerformanceCounters).AddEventFinished(null, e.BuildEventContext, e.Timestamp);
            }

            if (!_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            {
                return;
            }

            if (!_context.Parameters.ShowOnlyErrors && !_context.Parameters.ShowOnlyWarnings)
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
}