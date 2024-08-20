namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

internal sealed class HandleDuplicateParameterNamesCommand
    : IHandleDuplicateParameterNamesCommand
{
    private readonly string ParameterName;

    public HandleDuplicateParameterNamesCommand(
        string parameterName)
    {
        ParameterName = parameterName;
    }

    string IHandleDuplicateParameterNamesCommand.ParameterName => ParameterName;
}
