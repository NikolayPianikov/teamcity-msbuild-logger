namespace TeamCity.MSBuild.Logger;

using System.Collections.Generic;

internal interface IPerformanceCounterFactory
{
    IPerformanceCounter GetOrCreatePerformanceCounter(string scopeName, IDictionary<string, IPerformanceCounter> performanceCounters);
}