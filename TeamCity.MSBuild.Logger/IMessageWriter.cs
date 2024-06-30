namespace TeamCity.MSBuild.Logger;

using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;

internal interface IMessageWriter
{
    void DisplayCounters(IDictionary<string, IPerformanceCounter> counters);

    void PrintMessage(BuildMessageEventArgs e, bool lightenText);

    void WriteLinePrefix(BuildEventContext e, DateTime eventTimeStamp, bool isMessagePrefix);

    void WriteLinePrefix(string key, DateTime eventTimeStamp, bool isMessagePrefix);

    void WriteLinePretty(int indentLevel, string formattedString);

    void WriteLinePretty(string formattedString);

    void WriteLinePrettyFromResource(int indentLevel, string resourceString, params object[] args);

    void WriteLinePrettyFromResource(string resourceString, params object[] args);

    void WriteMessageAligned(string message, bool prefixAlreadyWritten, int prefixAdjustment = 0);

    void WriteNewLine();

    bool WriteTargetMessagePrefix(BuildEventArgs e, BuildEventContext context, DateTime timeStamp);
}