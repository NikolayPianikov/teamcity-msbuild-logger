namespace TeamCity.MSBuild.Logger;

using System;
using System.Globalization;

// ReSharper disable once ClassNeverInstantiated.Global
internal class LogFormatter: ILogFormatter
{
    public string FormatLogTimeStamp(DateTime timeStamp) => 
        timeStamp.ToString("HH:mm:ss.fff", CultureInfo.CurrentCulture);

    public string FormatTimeSpan(TimeSpan timeSpan) => 
        timeSpan.ToString().Substring(0, Math.Min(11, timeSpan.ToString().Length));
}