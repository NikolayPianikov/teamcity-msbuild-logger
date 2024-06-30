namespace TeamCity.MSBuild.Logger;

internal class BuildErrorMessageUpdater(IEventContext eventContext) : IServiceMessageUpdater
{
    public IServiceMessage UpdateServiceMessage(IServiceMessage message)
    {
        if (!eventContext.TryGetEvent(out var buildEventManager)
            || buildEventManager is not BuildErrorEventArgs error)
        {
            return message;
        }
        
        var newMessage = new PatchedServiceMessage(message);
        if (!string.IsNullOrWhiteSpace(error.Code))
        {
            newMessage.Add("code", error.Code);
        }
            
        if (!string.IsNullOrWhiteSpace(error.File))
        {
            newMessage.Add("file", error.File);
        }
            
        if (!string.IsNullOrWhiteSpace(error.Subcategory))
        {
            newMessage.Add("subcategory", error.Subcategory);
        }
            
        if (!string.IsNullOrWhiteSpace(error.ProjectFile))
        {
            newMessage.Add("projectFile", error.ProjectFile);
        }
            
        if (!string.IsNullOrWhiteSpace(error.SenderName))
        {
            newMessage.Add("senderName", error.SenderName);
        }
            
        newMessage.Add("columnNumber", error.ColumnNumber.ToString());
        newMessage.Add("endColumnNumber", error.EndColumnNumber.ToString());
        newMessage.Add("lineNumber", error.LineNumber.ToString());
        newMessage.Add("endLineNumber", error.EndLineNumber.ToString());
        return newMessage;
    }
}