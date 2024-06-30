namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class AnsiLogWriter(
    IConsole defaultConsole,
    IColorTheme colorTheme,
    IColorStorage colorStorage)
    : ILogWriter
{
    private readonly IColorStorage _colorStorage = colorStorage ?? throw new ArgumentNullException(nameof(colorStorage));
    private readonly IColorTheme _colorTheme = colorTheme ?? throw new ArgumentNullException(nameof(colorTheme));
    private readonly IConsole _defaultConsole = defaultConsole ?? throw new ArgumentNullException(nameof(defaultConsole));

    public void Write(string? message, IConsole? console = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        (console ?? _defaultConsole).Write(_colorStorage.Color.HasValue ? $"\x001B[{_colorTheme.GetAnsiColor(_colorStorage.Color.Value)}m{message}" : message);
    }

    public void SetColor(Color color)
    {
        _colorStorage.SetColor(color);
    }

    public void ResetColor()
    {
        _colorStorage.ResetColor();
    }
}