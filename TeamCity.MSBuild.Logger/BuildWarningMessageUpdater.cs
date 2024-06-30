namespace TeamCity.MSBuild.Logger;

internal class BuildWarningMessageUpdater(IEventContext eventContext) : IServiceMessageUpdater
{
    public IServiceMessage UpdateServiceMessage(IServiceMessage message)
    {
        if (!eventContext.TryGetEvent(out var buildEventManager)
            || buildEventManager is not BuildWarningEventArgs warning)
        {
            return message;
        }
        
        var newMessage = new PatchedServiceMessage(message);
        if (!string.IsNullOrWhiteSpace(warning.Code))
        {
            newMessage.Add("code", warning.Code);
        }
            
        if (!string.IsNullOrWhiteSpace(warning.File))
        {
            newMessage.Add("file", warning.File);
        }
            
        if (!string.IsNullOrWhiteSpace(warning.Subcategory))
        {
            newMessage.Add("subcategory", warning.Subcategory);
        }
            
        if (!string.IsNullOrWhiteSpace(warning.ProjectFile))
        {
            newMessage.Add("projectFile", warning.ProjectFile);
        }
            
        if (!string.IsNullOrWhiteSpace(warning.SenderName))
        {
            newMessage.Add("senderName", warning.SenderName);
        }
            
        newMessage.Add("columnNumber", warning.ColumnNumber.ToString());
        newMessage.Add("endColumnNumber", warning.EndColumnNumber.ToString());
        newMessage.Add("lineNumber", warning.LineNumber.ToString());
        newMessage.Add("endLineNumber", warning.EndLineNumber.ToString());
        return newMessage;
    }
}