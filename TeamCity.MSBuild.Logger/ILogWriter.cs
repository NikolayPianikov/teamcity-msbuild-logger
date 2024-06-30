namespace TeamCity.MSBuild.Logger;

internal interface ILogWriter
{
    void Write(string? message, IConsole? console = null);

    void SetColor(Color color);

    void ResetColor();
}