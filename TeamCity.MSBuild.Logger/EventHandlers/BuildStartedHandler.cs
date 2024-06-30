namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class BuildStartedHandler(
    ILoggerContext context,
    ILogWriter logWriter,
    IMessageWriter messageWriter,
    IHierarchicalMessageWriter hierarchicalMessageWriter,
    IStringService stringService)
    : IBuildEventHandler<BuildStartedEventArgs>
{
    private readonly IStringService _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
    private readonly IHierarchicalMessageWriter _hierarchicalMessageWriter = hierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(hierarchicalMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));

    public void Handle(BuildStartedEventArgs e)
    {
            if (e == null) throw new ArgumentNullException(nameof(e));
            _context.BuildStarted = e.Timestamp;
            _context.HasBuildStarted = true;
            if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
            {
                return;
            }

            if (_context.IsVerbosityAtLeast(LoggerVerbosity.Normal))
            {
                _messageWriter.WriteLinePrettyFromResource("BuildStartedWithTime", e.Timestamp);
            }

            WriteEnvironment(e.BuildEnvironment);
        }

    private void WriteEnvironment(IDictionary<string, string>? environment)
    {
            if (environment == null || environment.Count == 0 || _context.Verbosity != LoggerVerbosity.Diagnostic && !_context.Parameters.ShowEnvironment)
            {
                return;
            }

            OutputEnvironment(environment);
            _messageWriter.WriteNewLine();
        }

    private void OutputEnvironment(IDictionary<string, string> environment)
    {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            _logWriter.SetColor(Color.SummaryHeader);
            _hierarchicalMessageWriter.StartBlock("Environment");
            _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("EnvironmentHeader"), true);
            foreach (var keyValuePair in environment)
            {
                _logWriter.SetColor(Color.SummaryInfo);
                _messageWriter.WriteMessageAligned(string.Format(CultureInfo.CurrentCulture, "{0} = {1}", keyValuePair.Key, keyValuePair.Value), false);
            }

            _hierarchicalMessageWriter.FinishBlock();
            _logWriter.ResetColor();
        }
}