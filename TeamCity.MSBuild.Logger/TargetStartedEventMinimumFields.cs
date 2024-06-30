﻿namespace TeamCity.MSBuild.Logger;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
internal class TargetStartedEventMinimumFields
{
    public DateTime TimeStamp { get; }

    public string TargetName { get; }

    public string TargetFile { get; }

    public string ProjectFile { get; }

    public string Message { get; }

    public bool ShowTargetFinishedEvent { get; set; }

    public bool ErrorInTarget { get; set; }

    public BuildEventContext TargetBuildEventContext { get; }

    public string ParentTarget { get; }

    public string FullTargetKey { get; }

    public TargetStartedEventMinimumFields(
        TargetStartedEventArgs startedEvent,
        bool requireTimeStamp)
    {
        TargetName = startedEvent.TargetName;
        TargetFile = startedEvent.TargetFile;
        ProjectFile = startedEvent.ProjectFile;
        ShowTargetFinishedEvent = false;
        ErrorInTarget = false;
        Message = startedEvent.Message;
        TargetBuildEventContext = startedEvent.BuildEventContext;
        if (requireTimeStamp)
        {
            TimeStamp = startedEvent.Timestamp;
        }

        ParentTarget = startedEvent.ParentTarget;
        FullTargetKey = $"{TargetFile}.{TargetName}";
    }
}