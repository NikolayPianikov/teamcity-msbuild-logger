namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ProjectFinishedHandler(
    ILoggerContext context,
    ILogWriter logWriter,
    IPerformanceCounterFactory performanceCounterFactory,
    IMessageWriter messageWriter,
    IHierarchicalMessageWriter hierarchicalMessageWriter,
    IDeferredMessageWriter deferredMessageWriter,
    IBuildEventManager buildEventManager,
    IStringService stringService)
    : IBuildEventHandler<ProjectFinishedEventArgs>
{
    private readonly IStringService _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
    private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
    private readonly IBuildEventManager _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
    private readonly IDeferredMessageWriter _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IPerformanceCounterFactory _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));

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
            if (_context.Parameters is { ShowOnlyErrors: false, ShowOnlyWarnings: false })
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