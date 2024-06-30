// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.MSBuild.Logger;

internal class EventContext : IEventRegistry, IEventContext
{
    private BuildEventArgs? _event;

    public IDisposable Register(BuildEventArgs buildEventArgs)
    {
        var prevEvent = _event;
        _event = buildEventArgs;
        return Disposable.Create(() => { _event = prevEvent; });
    }

    public bool TryGetEvent(out BuildEventArgs? buildEventArgs)
    {
        if (_event != null)
        {
            buildEventArgs = _event;
            return true;
        }

        buildEventArgs = default;
        return false;
    }
}