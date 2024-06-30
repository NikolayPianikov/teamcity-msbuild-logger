namespace TeamCity.MSBuild.Logger;

// ReSharper disable once ClassNeverInstantiated.Global
internal class PerformanceCounterFactory(Func<IPerformanceCounter> performanceCounterFactory)
    : IPerformanceCounterFactory
{
    private readonly Func<IPerformanceCounter> _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));

    public IPerformanceCounter GetOrCreatePerformanceCounter(string scopeName, IDictionary<string, IPerformanceCounter> performanceCounters)
    {
        if (scopeName == null) throw new ArgumentNullException(nameof(scopeName));
        if (performanceCounters.TryGetValue(scopeName, out var performanceCounter))
        {
            return performanceCounter;
        }

        performanceCounter = _performanceCounterFactory();
        performanceCounter.ScopeName = scopeName;
        performanceCounters.Add(scopeName, performanceCounter);

        return performanceCounter;
    }
}