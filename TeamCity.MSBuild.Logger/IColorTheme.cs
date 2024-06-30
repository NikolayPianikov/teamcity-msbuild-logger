namespace TeamCity.MSBuild.Logger;

internal interface IColorTheme
{
    ConsoleColor GetConsoleColor(Color color);

    string GetAnsiColor(Color color);
}