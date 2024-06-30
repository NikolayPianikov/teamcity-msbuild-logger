namespace TeamCity.MSBuild.Logger;

internal interface ILoggerContext
{
    DateTime BuildStarted { get; set; }

    int CurrentIndentLevel { get; }

    IDictionary<BuildEventContext, IList<BuildMessageEventArgs>> DeferredMessages { get; }

    int ErrorCount { get; set; }

    IList<BuildErrorEventArgs>? ErrorList { get; }

    bool HasBuildStarted { get; set; }

    BuildEventContext? LastDisplayedBuildEventContext { get; set; }

    ProjectFullKey LastProjectFullKey { get; set; }

    int NumberOfProcessors { get; }

    Parameters Parameters { get; }

    int PrefixWidth { get; set; }

    bool SkipProjectStartedText { get; }

    IDictionary<string, IPerformanceCounter> ProjectPerformanceCounters { get; }

    IDictionary<string, IPerformanceCounter> TargetPerformanceCounters { get; }

    IDictionary<string, IPerformanceCounter> TaskPerformanceCounters { get; }

    LoggerVerbosity Verbosity { get; }

    int WarningCount { get; set; }

    IList<BuildWarningEventArgs>? WarningList { get; }

    ProjectFullKey GetFullProjectKey(BuildEventContext? e);

    void Initialize(
        int numberOfProcessors,
        bool skipProjectStartedText,
        Parameters parameters);

    bool IsVerbosityAtLeast(LoggerVerbosity checkVerbosity);

    void ResetConsoleLoggerState();
}