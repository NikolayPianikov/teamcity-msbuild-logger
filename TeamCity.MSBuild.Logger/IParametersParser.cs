namespace TeamCity.MSBuild.Logger;

internal interface IParametersParser
{
    bool TryParse(string? parametersString, Parameters parameters, out string? error);
}