namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class DefaultColorTheme : IColorTheme
{
    public ConsoleColor GetConsoleColor(Color color) =>
        color switch
        {
            Color.BuildStage => ConsoleColor.Cyan,
            Color.SummaryHeader or Color.PerformanceHeader or Color.Items => ConsoleColor.Blue,
            Color.Success => ConsoleColor.Green,
            Color.Warning or Color.WarningSummary => ConsoleColor.Yellow,
            Color.Error or Color.ErrorSummary => ConsoleColor.Red,
            Color.SummaryInfo => ConsoleColor.Gray,
            Color.Details => ConsoleColor.DarkGray,
            Color.Task => ConsoleColor.DarkCyan,
            Color.PerformanceCounterInfo => ConsoleColor.White,
            _ => throw new ArgumentException($"Unknown color \"{color}\"")
        };

    public string GetAnsiColor(Color color) =>
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        color switch
        {
            Color.Task => "36",
            Color.SummaryInfo => "37",
            Color.Details => "30;1",
            Color.Success => "32;1",
            Color.SummaryHeader or Color.PerformanceHeader or Color.Items => "34;1",
            Color.BuildStage => "36;1",
            Color.Error => "31;1",
            Color.Warning => "33;1",
            Color.PerformanceCounterInfo => "37;1",
            _ => throw new ArgumentException($"Unknown color \"{color}\"")
        };
}