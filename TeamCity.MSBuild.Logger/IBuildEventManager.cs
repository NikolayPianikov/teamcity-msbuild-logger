namespace TeamCity.MSBuild.Logger;

internal interface IBuildEventManager
{
    void AddProjectStartedEvent(ProjectStartedEventArgs e, bool requireTimestamp);

    void AddTargetStartedEvent(TargetStartedEventArgs e, bool requireTimeStamp);

    ProjectStartedEventMinimumFields? GetProjectStartedEvent(BuildEventContext e);

    TargetStartedEventMinimumFields? GetTargetStartedEvent(BuildEventContext e);

    IEnumerable<string> ProjectCallStackFromProject(BuildEventContext e);

    void RemoveProjectStartedEvent(BuildEventContext e);

    void RemoveTargetStartedEvent(BuildEventContext e);

    void SetErrorWarningFlagOnCallStack(BuildEventContext e);

    void Reset();
}