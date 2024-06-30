namespace TeamCity.MSBuild.Logger;

internal interface IStringService
{
    string FormatResourceString(string resourceName, params object?[] args);

    // ReSharper disable once IdentifierTypo
    string? UnescapeAll(string? escapedString);
}