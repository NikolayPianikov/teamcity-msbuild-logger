﻿namespace TeamCity.MSBuild.Logger;

using System;
using System.Collections.Generic;
using Pure.DI;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ColorTheme : IColorTheme
{
    private readonly Dictionary<ColorThemeMode, IColorTheme> _colorThemes;
    private readonly ILoggerContext _context;

    public ColorTheme(
        ILoggerContext context,
        [Tag(ColorThemeMode.Default)] IColorTheme defaultColorTheme,
        [Tag(ColorThemeMode.TeamCity)] IColorTheme teamCityColorTheme)
    {
            if (defaultColorTheme == null) throw new ArgumentNullException(nameof(defaultColorTheme));
            if (teamCityColorTheme == null) throw new ArgumentNullException(nameof(teamCityColorTheme));
            _colorThemes = new Dictionary<ColorThemeMode, IColorTheme>
            {
                { ColorThemeMode.Default, defaultColorTheme },
                { ColorThemeMode.TeamCity, teamCityColorTheme }
            };

            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

    private IColorTheme CurrentColorTheme => _colorThemes[_context.Parameters?.ColorThemeMode ?? ColorThemeMode.Default];

    public ConsoleColor GetConsoleColor(Color color)
    {
            return CurrentColorTheme.GetConsoleColor(color);
        }

    public string GetAnsiColor(Color color)
    {
            return CurrentColorTheme.GetAnsiColor(color);
        }
}