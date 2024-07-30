namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Common;

using Microsoft.CodeAnalysis;

using Paraminter.CSharp.Attributes.Constructor.Commands;

internal sealed class AddDefaultCSharpAttributeConstructorCommand
    : IAddDefaultCSharpAttributeConstructorCommand
{
    private readonly IParameterSymbol Parameter;

    public AddDefaultCSharpAttributeConstructorCommand(
        IParameterSymbol parameter)
    {
        Parameter = parameter;
    }

    IParameterSymbol IAddDefaultCSharpAttributeConstructorCommand.Parameter => Parameter;
}
