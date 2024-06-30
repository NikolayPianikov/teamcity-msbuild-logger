namespace TeamCity.MSBuild.Logger;

internal interface IPathService
{
    string GetFileName(string path);
}