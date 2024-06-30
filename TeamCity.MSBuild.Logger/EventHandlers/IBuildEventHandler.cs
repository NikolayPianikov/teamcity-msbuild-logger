namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal interface IBuildEventHandler<in TBuildEventArgs> where TBuildEventArgs : BuildEventArgs
{
    void Handle(TBuildEventArgs e);
}