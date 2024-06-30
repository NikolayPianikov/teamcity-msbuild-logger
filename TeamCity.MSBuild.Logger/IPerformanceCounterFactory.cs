namespace TeamCity.MSBuild.Logger;

internal interface IPerformanceCounterFactory
{
    IPerformanceCounter GetOrCreatePerformanceCounter(string scopeName, IDictionary<string, IPerformanceCounter> performanceCounters);
}