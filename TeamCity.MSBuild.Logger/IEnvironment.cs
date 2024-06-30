namespace TeamCity.MSBuild.Logger;

internal interface IEnvironment
{
    string? GetEnvironmentVariable(string name);
        
    bool TargetOutputLogging { get; }

    string DiagnosticsFile { get; }
}