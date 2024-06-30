namespace TeamCity.MSBuild.Logger;

using System;
using Pure.DI;

// ReSharper disable once ClassNeverInstantiated.Global
internal class TeamCityColorTheme : IColorTheme
{
    private readonly IColorTheme _defaultColorTheme;

    public TeamCityColorTheme(
        [Tag(ColorThemeMode.Default)] IColorTheme defaultColorTheme) =>
        _defaultColorTheme = defaultColorTheme ?? throw new ArgumentNullException(nameof(defaultColorTheme));

    public ConsoleColor GetConsoleColor(Color color) => 
        _defaultColorTheme.GetConsoleColor(color);

    public string GetAnsiColor(Color color) =>
        color switch
        {
            Color.SummaryInfo or Color.PerformanceCounterInfo => "35",
            Color.Details => "34;1",
            Color.Task => "36",
            _ => _defaultColorTheme.GetAnsiColor(color)
        };
}