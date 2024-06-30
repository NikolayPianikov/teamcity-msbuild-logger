namespace TeamCity.MSBuild.Logger;

using System;
using Microsoft.Build.Framework;

internal readonly struct TaskItem
{
    public readonly string Name;

    public readonly ITaskItem Item;

    public TaskItem(string name, ITaskItem item)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Item = item ?? throw new ArgumentNullException(nameof(item));
    }
}