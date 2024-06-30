// ReSharper disable UnusedMemberInSuper.Global
namespace TeamCity.MSBuild.Logger;

internal interface IPerformanceCounter
{
    string ScopeName { get; set; }

    TimeSpan ElapsedTime { get; }

    bool ReenteredScope { get; }

    int MessageIdentLevel { set; }

    void AddEventFinished(string? projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp);

    void AddEventStarted(string? projectTargetNames, BuildEventContext buildEventContext, DateTime eventTimeStamp, IEqualityComparer<BuildEventContext>? comparer);

    void PrintCounterMessage();
}