namespace TeamCity.MSBuild.Logger;

internal interface IEventContext
{
    bool TryGetEvent(out BuildEventArgs? buildEventArgs);
}