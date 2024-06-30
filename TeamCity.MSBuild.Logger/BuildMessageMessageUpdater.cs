namespace TeamCity.MSBuild.Logger;

internal class BuildMessageMessageUpdater(IEventContext eventContext) : IServiceMessageUpdater
{
    public IServiceMessage UpdateServiceMessage(IServiceMessage message)
    {
        if (!eventContext.TryGetEvent(out var buildEventManager)
            || buildEventManager is not BuildMessageEventArgs msg)
        {
            return message;
        }
        
        var newMessage = new PatchedServiceMessage(message);
        if (!string.IsNullOrWhiteSpace(msg.Code))
        {
            newMessage.Add("code", msg.Code);
        }
            
        if (!string.IsNullOrWhiteSpace(msg.File))
        {
            newMessage.Add("file", msg.File);
        }
            
        if (!string.IsNullOrWhiteSpace(msg.Subcategory))
        {
            newMessage.Add("subcategory", msg.Subcategory);
        }
            
        if (!string.IsNullOrWhiteSpace(msg.ProjectFile))
        {
            newMessage.Add("projectFile", msg.ProjectFile);
        }
            
        if (!string.IsNullOrWhiteSpace(msg.SenderName))
        {
            newMessage.Add("senderName", msg.SenderName);
        }
            
        newMessage.Add("columnNumber", msg.ColumnNumber.ToString());
        newMessage.Add("endColumnNumber", msg.EndColumnNumber.ToString());
        newMessage.Add("lineNumber", msg.LineNumber.ToString());
        newMessage.Add("endLineNumber", msg.EndLineNumber.ToString());
        newMessage.Add("importance", msg.Importance.ToString());
        return newMessage;
    }
}