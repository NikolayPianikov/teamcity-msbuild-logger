namespace TeamCity.MSBuild.Logger;

internal interface IDiagnostics
{
    void Send(Func<string> diagnosticsBuilder);
}