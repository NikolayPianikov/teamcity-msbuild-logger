namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Statistics(
    ILoggerContext context,
    [Tag(StatisticsMode.Default)] IStatistics defaultStatistics,
    [Tag(StatisticsMode.TeamCity)] IStatistics teamcityStatistics)
    : IStatistics
{
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly Dictionary<StatisticsMode, IStatistics> _statistics = new()
    {
        { StatisticsMode.Default, defaultStatistics ?? throw new ArgumentNullException(nameof(defaultStatistics))},
        { StatisticsMode.TeamCity, teamcityStatistics ?? throw new ArgumentNullException(nameof(teamcityStatistics))}
    };

    private IStatistics CurrentStatistics =>
        _statistics[_context.Parameters.StatisticsMode];

    public void Publish() => 
        CurrentStatistics.Publish();
}