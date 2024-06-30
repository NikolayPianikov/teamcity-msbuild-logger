namespace TeamCity.MSBuild.Logger;

using System;
using System.IO;

// ReSharper disable once ClassNeverInstantiated.Global
internal class PathService: IPathService
{
    public string GetFileName(string path) => 
        Path.GetFileName(path ?? throw new ArgumentNullException(nameof(path)));
}