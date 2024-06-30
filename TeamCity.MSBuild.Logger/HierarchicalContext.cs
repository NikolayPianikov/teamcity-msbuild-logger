namespace TeamCity.MSBuild.Logger;

internal class HierarchicalContext: IDisposable
{
    public const int DefaultFlowId = 0;
    private static readonly HierarchicalContext Default = new(0);

    [ThreadStatic] private static HierarchicalContext? _currentHierarchicalContext;
    private readonly HierarchicalContext? _prevHierarchicalContext;

    public HierarchicalContext(int? flowId)
    {
        FlowId = flowId ?? DefaultFlowId;
        _prevHierarchicalContext = _currentHierarchicalContext;
        _currentHierarchicalContext = this;
    }

    public static HierarchicalContext Current =>
        _currentHierarchicalContext ?? Default;

    public int FlowId { get; }

    public void Dispose() => 
        _currentHierarchicalContext = _prevHierarchicalContext;
}