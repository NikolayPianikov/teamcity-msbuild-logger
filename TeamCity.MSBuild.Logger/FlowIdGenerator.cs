// ReSharper disable ClassNeverInstantiated.Global
namespace TeamCity.MSBuild.Logger;

internal class FlowIdGenerator(Parameters parameters) : IFlowIdGenerator
{
    private bool _isFirst = true;

    public string NewFlowId()
    {
        // ReSharper disable once InvertIf
        if (_isFirst)
        {
            _isFirst = false;
            var flowId = parameters.FlowId;
            if (!string.IsNullOrWhiteSpace(flowId))
            {
                return flowId;
            }
        }
        
        return Guid.NewGuid().ToString().Replace("-", string.Empty);
    }
}