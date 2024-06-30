namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class DeferredMessageWriter(
    ILoggerContext context,
    ILogWriter logWriter,
    IMessageWriter messageWriter,
    IHierarchicalMessageWriter hierarchicalMessageWriter,
    IBuildEventManager buildEventManager,
    IPathService pathService,
    IStringService stringService)
    : IDeferredMessageWriter
{
    private readonly IStringService _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
    private readonly IPathService _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
    private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
    private readonly IBuildEventManager _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));

    public void ShownBuildEventContext(BuildEventContext? e)
    {
        _context.LastDisplayedBuildEventContext = e;
    }

    public void DisplayDeferredProjectStartedEvent(BuildEventContext e)
    {
        if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings || _context.SkipProjectStartedText)
        {
            return;
        }

        var projectStartedEvent = _buildEventManager.GetProjectStartedEvent(e);
        if (projectStartedEvent == null || projectStartedEvent.ShowProjectFinishedEvent)
        {
            return;
        }

        projectStartedEvent.ShowProjectFinishedEvent = true;
        var parentProjectStartedEvent = projectStartedEvent.ParentProjectStartedEvent;
        if (parentProjectStartedEvent != null)
        {
            DisplayDeferredStartedEvents(parentProjectStartedEvent.ProjectBuildEventContext);
        }

        var projectFile = projectStartedEvent.ProjectFile ?? string.Empty;
        var parentProjectFile = parentProjectStartedEvent?.ProjectFile;
        var targetNames = projectStartedEvent.TargetNames;
        var nodeId = projectStartedEvent.ProjectBuildEventContext.NodeId;
        var shortProjectFile = _pathService.GetFileName(projectFile);
        if (parentProjectFile == null)
        {
            string message;
            string shortName;
            if (string.IsNullOrEmpty(targetNames))
            {
                message = _stringService.FormatResourceString("ProjectStartedTopLevelProjectWithDefaultTargets", projectFile, nodeId);
                shortName = $"Project \"{shortProjectFile}\"";
            }
            else
            {
                message = _stringService.FormatResourceString("ProjectStartedTopLevelProjectWithTargetNames", projectFile, nodeId, targetNames);
                shortName = $"Project \"{shortProjectFile}\": {targetNames}";
            }

            _hierarchicalMessageWriter.StartBlock(shortName);

            _messageWriter.WriteLinePrefix(projectStartedEvent.FullProjectKey, projectStartedEvent.TimeStamp, false);
            _logWriter.SetColor(Color.BuildStage);
            _messageWriter.WriteMessageAligned(message, true);
            _logWriter.ResetColor();
        }
        else
        {
            if (parentProjectStartedEvent is not null)
            {
                if (string.IsNullOrEmpty(targetNames))
                {
                    var shortName = $"Project \"{shortProjectFile}\"";
                    _hierarchicalMessageWriter.StartBlock(shortName);

                    _messageWriter.WriteLinePrefix(parentProjectStartedEvent.FullProjectKey,
                        parentProjectStartedEvent.TimeStamp, false);
                    _logWriter.SetColor(Color.BuildStage);
                    _messageWriter.WriteMessageAligned(
                        _stringService.FormatResourceString("ProjectStartedWithDefaultTargetsMultiProc",
                            parentProjectFile, parentProjectStartedEvent.FullProjectKey, projectFile,
                            projectStartedEvent.FullProjectKey, nodeId), true);
                }
                else
                {
                    var shortName = $"Project \"{shortProjectFile}\": {targetNames}";
                    _hierarchicalMessageWriter.StartBlock(shortName);

                    _messageWriter.WriteLinePrefix(parentProjectStartedEvent.FullProjectKey,
                        parentProjectStartedEvent.TimeStamp, false);
                    _logWriter.SetColor(Color.BuildStage);
                    _messageWriter.WriteMessageAligned(
                        _stringService.FormatResourceString("ProjectStartedWithTargetsMultiProc", parentProjectFile,
                            parentProjectStartedEvent.FullProjectKey, projectFile,
                            projectStartedEvent.FullProjectKey, nodeId, targetNames), true);
                }
            }

            _logWriter.ResetColor();
        }

        ShownBuildEventContext(null);
    }

    public void DisplayDeferredTargetStartedEvent(BuildEventContext e)
    {
        if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            return;
        }

        var targetStartedEvent = _buildEventManager.GetTargetStartedEvent(e);
        if (targetStartedEvent == null || targetStartedEvent.ShowTargetFinishedEvent)
        {
            return;
        }

        targetStartedEvent.ShowTargetFinishedEvent = true;
        DisplayDeferredStartedEvents(targetStartedEvent.TargetBuildEventContext);
        var projectStartedEvent = _buildEventManager.GetProjectStartedEvent(e);
        if (projectStartedEvent == null)
        {
            throw new InvalidOperationException("Project Started should not be null in deferred target started");
        }

        var projectFile = projectStartedEvent.ProjectFile ?? string.Empty;
        string targetName;
        var shortTargetName = $"Target \"{targetStartedEvent.TargetName}\"";
        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic) || (_context.Parameters.ShowEventId ?? false))
        {
            targetName = _stringService.FormatResourceString("TargetMessageWithId", targetStartedEvent.TargetName, targetStartedEvent.TargetBuildEventContext.TargetId);
        }
        else
        {
            targetName = targetStartedEvent.TargetName;
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            if (string.Equals(projectFile, targetStartedEvent.TargetFile, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(targetStartedEvent.ParentTarget))
                {
                    _hierarchicalMessageWriter.StartBlock(shortTargetName);

                    _messageWriter.WriteLinePrefix(targetStartedEvent.TargetBuildEventContext, targetStartedEvent.TimeStamp, false);
                    _logWriter.SetColor(Color.BuildStage);
                    _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetStartedProjectDepends", targetName, projectFile, targetStartedEvent.ParentTarget), true);
                }
                else
                {
                    _hierarchicalMessageWriter.StartBlock(shortTargetName);

                    _messageWriter.WriteLinePrefix(targetStartedEvent.TargetBuildEventContext, targetStartedEvent.TimeStamp, false);
                    _logWriter.SetColor(Color.BuildStage);
                    _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetStartedProjectEntry", targetName, projectFile), true);
                }
            }
            else 
            if (!string.IsNullOrEmpty(targetStartedEvent.ParentTarget))
            {
                _hierarchicalMessageWriter.StartBlock(shortTargetName);

                _messageWriter.WriteLinePrefix(targetStartedEvent.TargetBuildEventContext, targetStartedEvent.TimeStamp, false);
                _logWriter.SetColor(Color.BuildStage);
                _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetStartedFileProjectDepends", targetName, targetStartedEvent.TargetFile, projectFile, targetStartedEvent.ParentTarget), true);
            }
            else
            {
                _hierarchicalMessageWriter.StartBlock(shortTargetName);

                _messageWriter.WriteLinePrefix(targetStartedEvent.TargetBuildEventContext, targetStartedEvent.TimeStamp, false);
                _logWriter.SetColor(Color.BuildStage);
                _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetStartedFileProjectEntry", targetName, targetStartedEvent.TargetFile, projectFile), true);
            }
        }
        else
        {
            _hierarchicalMessageWriter.StartBlock(shortTargetName);

            _messageWriter.WriteLinePrefix(targetStartedEvent.TargetBuildEventContext, targetStartedEvent.TimeStamp, false);
            _logWriter.SetColor(Color.BuildStage);
            _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("TargetStartedFileProjectEntry", targetName, targetStartedEvent.TargetFile, projectFile), true);
        }

        _logWriter.ResetColor();
        ShownBuildEventContext(e);
    }

    public void DisplayDeferredStartedEvents(BuildEventContext e)
    {
        if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            return;
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            DisplayDeferredProjectStartedEvent(e);
        }

        if (!_context.IsVerbosityAtLeast(LoggerVerbosity.Detailed))
        {
            return;
        }

        DisplayDeferredTargetStartedEvent(e);
    }
}