namespace TeamCity.MSBuild.Logger;

using System;

internal interface ILogFormatter
{
    string FormatLogTimeStamp(DateTime timeStamp);

    string FormatTimeSpan(TimeSpan timeSpan);
}