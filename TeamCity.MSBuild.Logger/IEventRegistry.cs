namespace TeamCity.MSBuild.Logger;

internal interface IEventRegistry
{
    IDisposable Register(BuildEventArgs buildEventArgs);
}