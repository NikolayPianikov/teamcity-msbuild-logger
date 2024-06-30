namespace TeamCity.MSBuild.Logger;

using Microsoft.Build.Framework;

internal interface IEventFormatter
{
    string FormatEventMessage(BuildErrorEventArgs e, bool removeCarriageReturn, bool showProjectFile);

    string FormatEventMessage(BuildMessageEventArgs e, bool removeCarriageReturn, bool showProjectFile);

    string FormatEventMessage(BuildWarningEventArgs e, bool removeCarriageReturn, bool showProjectFile);
}