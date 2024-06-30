namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class DefaultHierarchicalMessageWriter : IHierarchicalMessageWriter
{
    public void StartBlock(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
    }

    public void FinishBlock()
    {
    }
}