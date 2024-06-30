namespace TeamCity.MSBuild.Logger.EventHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class ProjectStartedHandler(
    ILoggerContext context,
    ILogWriter logWriter,
    IPerformanceCounterFactory performanceCounterFactory,
    IBuildEventHandler<BuildMessageEventArgs> messageHandler,
    IMessageWriter messageWriter,
    IDeferredMessageWriter deferredMessageWriter,
    IBuildEventManager buildEventManager,
    IStringService stringService)
    : IBuildEventHandler<ProjectStartedEventArgs>
{
    private readonly IStringService _stringService = stringService ?? throw new ArgumentNullException(nameof(stringService));
    private readonly IBuildEventManager _buildEventManager = buildEventManager ?? throw new ArgumentNullException(nameof(buildEventManager));
    private readonly IDeferredMessageWriter _deferredMessageWriter = deferredMessageWriter ?? throw new ArgumentNullException(nameof(deferredMessageWriter));
    private readonly IMessageWriter _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
    private readonly ILoggerContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IBuildEventHandler<BuildMessageEventArgs> _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
    private readonly IPerformanceCounterFactory _performanceCounterFactory = performanceCounterFactory ?? throw new ArgumentNullException(nameof(performanceCounterFactory));
    private readonly ILogWriter _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));

    public void Handle(ProjectStartedEventArgs e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (e.BuildEventContext == null) throw new ArgumentException(nameof(e));

        _buildEventManager.AddProjectStartedEvent(e, _context.Parameters.ShowTimeStamp || _context.IsVerbosityAtLeast(LoggerVerbosity.Detailed));
        if (_context.Parameters.ShowPerfSummary)
        {
            _performanceCounterFactory.GetOrCreatePerformanceCounter(e.ProjectFile, _context.ProjectPerformanceCounters).AddEventStarted(e.TargetNames, e.BuildEventContext, e.Timestamp, ComparerContextNodeId.Shared);
        }

        if (_context.DeferredMessages.TryGetValue(e.BuildEventContext, out var messages))
        {
            if (_context.Parameters is { ShowOnlyErrors: false, ShowOnlyWarnings: false })
            {
                foreach (var message in messages)
                {
                    _messageHandler.Handle(message);
                }
            }

            _context.DeferredMessages.Remove(e.BuildEventContext);
        }

        if (_context.Verbosity != LoggerVerbosity.Diagnostic || !_context.Parameters.ShowItemAndPropertyList)
        {
            return;
        }

        if (_context.Parameters is { ShowOnlyErrors: false, ShowOnlyWarnings: false })
        {
            _deferredMessageWriter.DisplayDeferredProjectStartedEvent(e.BuildEventContext);
        }

        if (e.Properties != null)
        {
            WriteProperties(e, e.Properties.Cast<DictionaryEntry>().Select(i => new Property((string)i.Key, (string)i.Value)).ToList());
        }

        if (e.Items != null)
        {
            WriteItems(e, e.Items.Cast<DictionaryEntry>().Select(i => new TaskItem((string)i.Key, (ITaskItem)i.Value)).ToList());
        }
    }

    private void WriteItems(BuildEventArgs e, IList<TaskItem> items)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            return;
        }

        if (items.Count == 0)
        {
            return;
        }

        _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
        WriteItems(items);
        _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
    }

    private void WriteItems(IList<TaskItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (_context.Verbosity != LoggerVerbosity.Diagnostic || !_context.Parameters.ShowItemAndPropertyList || items.Count == 0)
        {
            return;
        }

        _logWriter.SetColor(Color.Items);
        _messageWriter.WriteLinePretty(_context.CurrentIndentLevel, _stringService.FormatResourceString("ItemListHeader"));

        var groupedItems = from item in items
            group item by item.Name
            into groupedByName
            orderby groupedByName.Key.ToLowerInvariant()
            select new { ItemType = groupedByName.Key, Items = groupedByName.Select(i => i.Item).OrderBy(i => i, TaskItemItemSpecComparer.Shared) };

        foreach (var groupedItem in groupedItems)
        {
            OutputItems(groupedItem.ItemType, groupedItem.Items);
        }

        _messageWriter.WriteNewLine();
    }

    private void OutputItems(string itemType, IEnumerable<ITaskItem> items)
    {
        if (itemType == null) throw new ArgumentNullException(nameof(itemType));
        if (items == null) throw new ArgumentNullException(nameof(items));
        var isFirst = true;
        foreach (var item in items)
        {
            if (isFirst)
            {
                _logWriter.SetColor(Color.Details);
                _messageWriter.WriteMessageAligned(itemType, false);
                isFirst = false;
            }

            _logWriter.SetColor(Color.SummaryInfo);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(' ', 4).Append(item.ItemSpec);
            _messageWriter.WriteMessageAligned(stringBuilder.ToString(), false);
            foreach (DictionaryEntry dictionaryEntry in item.CloneCustomMetadata())
            {
                _messageWriter.WriteMessageAligned(new string(' ', 8) + dictionaryEntry.Key + " = " + item.GetMetadata((string)dictionaryEntry.Key), false);
            }
        }

        _logWriter.ResetColor();
    }

    private void WriteProperties(IEnumerable<Property> properties)
    {
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        if (_context.Verbosity != LoggerVerbosity.Diagnostic || !_context.Parameters.ShowItemAndPropertyList)
        {
            return;
        }

        OutputProperties(properties);
        _messageWriter.WriteNewLine();
    }

    private void WriteProperties(BuildEventArgs e, ICollection<Property> properties)
    {
        if (_context.Parameters.ShowOnlyErrors || _context.Parameters.ShowOnlyWarnings)
        {
            return;
        }

        if (properties.Count == 0)
        {
            return;
        }

        _messageWriter.WriteLinePrefix(e.BuildEventContext, e.Timestamp, false);
        WriteProperties(properties);
        _deferredMessageWriter.ShownBuildEventContext(e.BuildEventContext);
    }

    private void OutputProperties(IEnumerable<Property> properties)
    {
        if (properties == null) throw new ArgumentNullException(nameof(properties));
        _logWriter.SetColor(Color.Items);
        _messageWriter.WriteMessageAligned(_stringService.FormatResourceString("PropertyListHeader"), true);
        foreach (var property in properties.OrderBy(i => i, DictionaryEntryKeyComparer.Shared))
        {
            _logWriter.SetColor(Color.SummaryInfo);
            _messageWriter.WriteMessageAligned(string.Format(CultureInfo.CurrentCulture, "{0} = {1}", property.Name, _stringService.UnescapeAll(property.Value)), false);
        }

        _logWriter.ResetColor();
    }

}