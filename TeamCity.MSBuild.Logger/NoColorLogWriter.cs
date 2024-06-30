namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class NoColorLogWriter(IConsole defaultConsole) : ILogWriter
{
    private readonly IConsole _defaultConsole = defaultConsole ?? throw new ArgumentNullException(nameof(defaultConsole));

    public void Write(string? message, IConsole? console = null)
{
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        (console ?? _defaultConsole).Write(message);
    }

    public void SetColor(Color color)
    {
    }

    public void ResetColor()
    {
    }
}