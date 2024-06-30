namespace TeamCity.MSBuild.Logger;

internal interface IEventFormatter
{
    string FormatEventMessage(BuildErrorEventArgs e, bool removeCarriageReturn, bool showProjectFile);

    string FormatEventMessage(BuildMessageEventArgs e, bool removeCarriageReturn, bool showProjectFile);

    string FormatEventMessage(BuildWarningEventArgs e, bool removeCarriageReturn, bool showProjectFile);
}