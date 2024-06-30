namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class HierarchicalMessageWriter(
    ILoggerContext context,
    [Tag(TeamCityMode.Off)] IHierarchicalMessageWriter defaultHierarchicalMessageWriter,
    [Tag(TeamCityMode.SupportHierarchy)] IHierarchicalMessageWriter teamcityHierarchicalMessageWriter)
    : IHierarchicalMessageWriter
{
    private readonly Dictionary<TeamCityMode, IHierarchicalMessageWriter> _hierarchicalMessageWriter = new()
    {
        { TeamCityMode.Off, defaultHierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(defaultHierarchicalMessageWriter))},
        { TeamCityMode.SupportHierarchy, teamcityHierarchicalMessageWriter ?? throw new ArgumentNullException(nameof(teamcityHierarchicalMessageWriter))}
    };
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));

    private IHierarchicalMessageWriter CurrentHierarchicalMessageWriter =>
        _hierarchicalMessageWriter[_context.Parameters.TeamCityMode];

    public void StartBlock(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        CurrentHierarchicalMessageWriter.StartBlock(name);
    }

    public void FinishBlock() => 
        CurrentHierarchicalMessageWriter.FinishBlock();
}