namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class LogWriter(
    ILoggerContext context,
    [Tag(ColorMode.Default)] ILogWriter defaultLogWriter,
    [Tag(ColorMode.TeamCity)] ILogWriter ansiLogWriter,
    [Tag(ColorMode.NoColor)] ILogWriter noColorLogWriter,
    [Tag(ColorMode.AnsiColor)] ILogWriter ansiColorLogWriter)
    : ILogWriter
{
    private readonly Dictionary<ColorMode, ILogWriter> _logWriters = new()
    {
        { ColorMode.Default, defaultLogWriter ?? throw new ArgumentNullException(nameof(defaultLogWriter))},
        { ColorMode.TeamCity, ansiLogWriter ?? throw new ArgumentNullException(nameof(ansiLogWriter))},
        { ColorMode.NoColor, noColorLogWriter ?? throw new ArgumentNullException(nameof(noColorLogWriter))},
        { ColorMode.AnsiColor, ansiColorLogWriter ?? throw new ArgumentNullException(nameof(ansiColorLogWriter))}
    };
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));

    private ILogWriter CurrentLogWriter =>
        _logWriters[_context.Parameters.ColorMode];

    public void Write(string? message, IConsole? console = null) => 
        CurrentLogWriter.Write(message, console);

    public void SetColor(Color color) => 
        CurrentLogWriter.SetColor(color);

    public void ResetColor() => 
        CurrentLogWriter.ResetColor();
}