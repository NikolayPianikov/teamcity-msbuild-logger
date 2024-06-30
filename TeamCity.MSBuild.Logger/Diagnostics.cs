namespace TeamCity.MSBuild.Logger;

using System;
using System.IO;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Diagnostics : IDiagnostics
{
    private readonly bool _isEnabled;
    private readonly string _diagnosticsFile;
    private readonly object _lockObject = new();        

    public Diagnostics(IEnvironment environment)
    {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            _diagnosticsFile = environment.DiagnosticsFile;
            _isEnabled = !string.IsNullOrWhiteSpace(_diagnosticsFile);
        }

    public void Send(Func<string> diagnosticsBuilder)
    {
            if (!_isEnabled)
            {
                return;                
            }

            try
            {
                var diagnosticsInfo = GetPrefix() + diagnosticsBuilder() + System.Environment.NewLine;
                lock (_lockObject)
                {
                    File.AppendAllText(_diagnosticsFile, diagnosticsInfo);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

    private static string GetPrefix() => $"{System.Threading.Thread.CurrentThread.ManagedThreadId:0000}: ";
}