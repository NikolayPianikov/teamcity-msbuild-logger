// ReSharper disable All
namespace TeamCity.MSBuild.Logger.Tests.Helpers;

using System;
using System.Collections.Generic;
    
public class CommandLineResult
{
    public CommandLineResult(
        CommandLine commandLine,
        int exitCode,
        IList<string> stdOut,
        IList<string> stdError)
    {
            CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            ExitCode = exitCode;
            StdOut = stdOut ?? throw new ArgumentNullException(nameof(stdOut));
            StdError = stdError ?? throw new ArgumentNullException(nameof(stdError));
        }

    public CommandLine CommandLine { get; }

    public int ExitCode { get; }

    public IEnumerable<string> StdOut { get; }

    public IEnumerable<string> StdError { get; }
}