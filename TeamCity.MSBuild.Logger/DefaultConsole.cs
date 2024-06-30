namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class DefaultConsole(IDiagnostics diagnostics) : IConsole, IInitializable
{
    private readonly IDiagnostics _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    private readonly TextWriter _out = Console.Out;
    private int _reentrancy;

    public void Write(string? text)
    {
        if (text is null || string.IsNullOrEmpty(text))
        {
            return;
        }

        // ReSharper disable once IdentifierTypo
        var reentrancy = Interlocked.Increment(ref _reentrancy) - 1;
        // ReSharper disable once AccessToModifiedClosure
        _diagnostics.Send(() => $"[{reentrancy} +] Write({text.Trim()})");
        try
        {
            _out.Write(text);
        }
        finally
        {
            reentrancy = Interlocked.Decrement(ref _reentrancy);
            _diagnostics.Send(() => $"[{reentrancy} -] Write({text.Trim()})");
        }
    }
}