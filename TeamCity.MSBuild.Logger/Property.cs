namespace TeamCity.MSBuild.Logger;

internal readonly struct Property(string name, string value)
{
    public readonly string Name = name;

    public readonly string Value = value;
}