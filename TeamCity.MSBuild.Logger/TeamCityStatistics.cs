// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class TeamCityStatistics(
    ILoggerContext context,
    ITeamCityWriter writer)
    : IStatistics
{
    private readonly ITeamCityWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public void Publish()
    {
        // _writer.WriteBuildStatistics("BuildStatsW", _context.WarningCount.ToString(CultureInfo.InvariantCulture));
        // _writer.WriteBuildStatistics("BuildStatsE", _context.ErrorCount.ToString(CultureInfo.InvariantCulture));
    }
}