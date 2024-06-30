namespace TeamCity.MSBuild.Logger;

using System.Diagnostics;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
internal class NodeLogger : INodeLogger
{
    private readonly Parameters _parameters;
    private readonly ILoggerContext _context;
    private readonly IEnvironment _environment;
    private readonly IDiagnostics _diagnostics;
    private readonly IEventRegistry _eventRegistry;
    private readonly IBuildEventHandler<BuildMessageEventArgs> _messageHandler;
    private readonly IBuildEventHandler<BuildFinishedEventArgs> _buildFinishedHandler;
    private readonly IBuildEventHandler<ProjectStartedEventArgs> _projectStartedHandler;
    private readonly IBuildEventHandler<ProjectFinishedEventArgs> _projectFinishedHandler;
    private readonly IBuildEventHandler<TargetStartedEventArgs> _targetStartedHandler;
    private readonly IBuildEventHandler<TargetFinishedEventArgs> _targetFinishedHandler;
    private readonly IBuildEventHandler<TaskStartedEventArgs> _taskStartedHandler;
    private readonly IBuildEventHandler<TaskFinishedEventArgs> _taskFinishedHandler;
    private readonly IBuildEventHandler<BuildErrorEventArgs> _errorHandler;
    private readonly IBuildEventHandler<BuildWarningEventArgs> _warningHandler;
    private readonly IBuildEventHandler<CustomBuildEventArgs> _customEventHandler;
    private readonly IBuildEventHandler<BuildStartedEventArgs> _buildStartedEventHandler;
    private readonly IParametersParser _parametersParser;
    private readonly ILogWriter _logWriter;
    private readonly object _lockObject = new();
    // ReSharper disable once IdentifierTypo
    private int _reentrancy;

    public NodeLogger(
        // ReSharper disable once UnusedParameter.Local
        // ReSharper disable once IdentifierTypo
        Parameters parameters,
        IInitializable[] initializables,
        IParametersParser parametersParser,
        ILogWriter logWriter,
        ILoggerContext context,
        IEnvironment environment,
        IDiagnostics diagnostics,
        IEventRegistry eventRegistry,
        IBuildEventHandler<BuildStartedEventArgs> buildStartedHandler,
        IBuildEventHandler<BuildMessageEventArgs> messageHandler,
        IBuildEventHandler<BuildFinishedEventArgs> buildFinishedHandler,
        IBuildEventHandler<ProjectStartedEventArgs> projectStartedHandler,
        IBuildEventHandler<ProjectFinishedEventArgs> projectFinishedHandler,
        IBuildEventHandler<TargetStartedEventArgs> targetStartedHandler,
        IBuildEventHandler<TargetFinishedEventArgs> targetFinishedHandler,
        IBuildEventHandler<TaskStartedEventArgs> taskStartedHandler,
        IBuildEventHandler<TaskFinishedEventArgs> taskFinishedHandler,
        IBuildEventHandler<BuildErrorEventArgs> errorHandler,
        IBuildEventHandler<BuildWarningEventArgs> warningHandler,
        IBuildEventHandler<CustomBuildEventArgs> customEventHandler)
    {
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _eventRegistry = eventRegistry;
        _parametersParser = parametersParser ?? throw new ArgumentNullException(nameof(parametersParser));
        _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));

        _buildStartedEventHandler = buildStartedHandler ?? throw new ArgumentNullException(nameof(buildStartedHandler));
        _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        _buildFinishedHandler = buildFinishedHandler ?? throw new ArgumentNullException(nameof(buildFinishedHandler));
        _projectStartedHandler = projectStartedHandler ?? throw new ArgumentNullException(nameof(projectStartedHandler));
        _projectFinishedHandler = projectFinishedHandler ?? throw new ArgumentNullException(nameof(projectFinishedHandler));
        _targetStartedHandler = targetStartedHandler ?? throw new ArgumentNullException(nameof(targetStartedHandler));
        _targetFinishedHandler = targetFinishedHandler ?? throw new ArgumentNullException(nameof(targetFinishedHandler));
        _taskStartedHandler = taskStartedHandler ?? throw new ArgumentNullException(nameof(taskStartedHandler));
        _taskFinishedHandler = taskFinishedHandler ?? throw new ArgumentNullException(nameof(taskFinishedHandler));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _warningHandler = warningHandler ?? throw new ArgumentNullException(nameof(warningHandler));
        _customEventHandler = customEventHandler ?? throw new ArgumentNullException(nameof(customEventHandler));
    }

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool SkipProjectStartedText { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedMember.Global
    public bool ShowSummary
    {
        get => _parameters.ShowSummary ?? false;
        // ReSharper disable once UnusedMember.Global
        set => _parameters.ShowSummary = value;
    }

    public LoggerVerbosity Verbosity { get; set; } =LoggerVerbosity.Normal;

    public string? Parameters { get; set; }

    public void Initialize(IEventSource eventSource, int nodeCount)
    {
        _diagnostics.Send(() => $"Initialize({eventSource}, {nodeCount})");
        _parameters.Verbosity = Verbosity;
        if (Parameters != null)
        {
            if (!_parametersParser.TryParse(Parameters, _parameters, out var error))
            {
                throw new LoggerException(error);
            }
        }

        _context.Initialize(
            nodeCount,
            SkipProjectStartedText,
            _parameters);

        if (nodeCount == 1 && _parameters.ShowEventId.HasValue)
        {
            _parameters.ShowEventId = false;
        }

        if (_parameters.Debug)
        {
            try
            {
                _logWriter.SetColor(Color.Warning);
                _logWriter.Write($"\nWaiting for debugger in process: [{Process.GetCurrentProcess().Id}] \"{Process.GetCurrentProcess().ProcessName}\"\n");
            }
            finally
            {
                _logWriter.ResetColor();
            }

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }

            Debugger.Break();
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
        {
            _parameters.ShowPerfSummary = true;
        }

        _parameters.ShowTargetOutputs = _environment.TargetOutputLogging;
        if (!_parameters.ShowSummary.HasValue && _context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
        {
            _parameters.ShowSummary = true;
        }

        if (_parameters.ShowOnlyWarnings || _parameters.ShowOnlyErrors)
        {
            _parameters.ShowSummary ??= false;
            _parameters.ShowPerfSummary = false;
        }

        if (_context.IsVerbosityAtLeast(LoggerVerbosity.Diagnostic))
        {
            try
            {
                _logWriter.SetColor(Color.Details);
                _logWriter.Write($"Logger parameters: {_parameters}\n");
            }
            finally
            {
                _logWriter.ResetColor();
            }
        }

        eventSource.BuildStarted += (_, e) => Handle(_buildStartedEventHandler, e);
        eventSource.BuildFinished += (_, e) => Handle(_buildFinishedHandler, e);
        eventSource.ProjectStarted += (_, e) => Handle(_projectStartedHandler, e);
        eventSource.ProjectFinished += (_, e) => Handle(_projectFinishedHandler, e);
        eventSource.TargetStarted += (_, e) => Handle(_targetStartedHandler, e);
        eventSource.TargetFinished += (_, e) => Handle(_targetFinishedHandler, e);
        eventSource.TaskStarted += (_, e) => Handle(_taskStartedHandler, e);
        eventSource.TaskFinished += (_, e) => Handle(_taskFinishedHandler, e);
        eventSource.ErrorRaised += (_, e) => Handle(_errorHandler, e);
        eventSource.WarningRaised += (_, e) => Handle(_warningHandler, e);
        eventSource.MessageRaised += (_, e) => Handle(_messageHandler, e);
        eventSource.CustomEventRaised += (_, e) => Handle(_customEventHandler, e);

        _diagnostics.Send(() => _parameters.ToString());
    }

    public void Initialize(IEventSource eventSource) => 
        Initialize(eventSource, 1);

    public virtual void Shutdown() => 
        _diagnostics.Send(() => "Shutdown()");

    private void Handle<TBuildEventArgs>(IBuildEventHandler<TBuildEventArgs> handler, TBuildEventArgs? e)
        where TBuildEventArgs : BuildEventArgs
    {
        if (e is null)
        {
            return;
        }

        // ReSharper disable once IdentifierTypo
        var reentrancy = Interlocked.Increment(ref _reentrancy) - 1;
        // ReSharper disable once AccessToModifiedClosure
        _diagnostics.Send(() => $"[{reentrancy} +] Handle<{typeof(TBuildEventArgs).Name}>()");
        try
        {
            lock (_lockObject)
            {
                using (_eventRegistry.Register(e))
                using (new HierarchicalContext(e.BuildEventContext?.NodeId ?? 0))
                {
                    handler.Handle(e);
                }
            }
        }
        catch (Exception ex)
        {
            var error = $"Exception was occurred while processing a message of type \"{e.GetType()}\":\n{ex}";
            _logWriter.Write(error);
            _diagnostics.Send(() => error);                
        }
        finally
        {
            reentrancy = Interlocked.Decrement(ref _reentrancy);
            _diagnostics.Send(() => $"[{reentrancy} -] Handle<{typeof(TBuildEventArgs).Name}>()");
        }
    }
}