namespace TeamCity.MSBuild.Logger;

internal readonly struct TaskItem(string name, ITaskItem item)
{
    public readonly string Name = name ?? throw new ArgumentNullException(nameof(name));

    public readonly ITaskItem Item = item ?? throw new ArgumentNullException(nameof(item));
}