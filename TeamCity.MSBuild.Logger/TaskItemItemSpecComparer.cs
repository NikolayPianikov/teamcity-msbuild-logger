namespace TeamCity.MSBuild.Logger;

internal class TaskItemItemSpecComparer : IComparer<ITaskItem>
{
    public static readonly IComparer<ITaskItem> Shared = new TaskItemItemSpecComparer();

    private TaskItemItemSpecComparer()
    {
    }

    public int Compare(ITaskItem? x, ITaskItem? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        return string.Compare(x.ItemSpec, y.ItemSpec, StringComparison.CurrentCultureIgnoreCase);
    }
}