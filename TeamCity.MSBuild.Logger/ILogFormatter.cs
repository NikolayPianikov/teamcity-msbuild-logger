namespace TeamCity.MSBuild.Logger;

internal interface ILogFormatter
{
    string FormatLogTimeStamp(DateTime timeStamp);

    string FormatTimeSpan(TimeSpan timeSpan);
}