namespace TeamCity.MSBuild.Logger;

internal interface IHierarchicalMessageWriter
{
    void StartBlock(string name);

    void FinishBlock();
}