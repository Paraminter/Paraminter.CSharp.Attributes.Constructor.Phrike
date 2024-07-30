namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.CSharp.Attributes.Constructor.Commands;

internal sealed class AddNormalCSharpAttributeConstructorAssociationCommand
    : IAddNormalCSharpAttributeConstructorAssociationCommand
{
    private readonly IParameterSymbol Parameter;
    private readonly AttributeArgumentSyntax SyntacticArgument;

    public AddNormalCSharpAttributeConstructorAssociationCommand(
        IParameterSymbol parameter,
        AttributeArgumentSyntax syntacticArgument)
    {
        Parameter = parameter;
        SyntacticArgument = syntacticArgument;
    }

    IParameterSymbol IAddNormalCSharpAttributeConstructorAssociationCommand.Parameter => Parameter;
    AttributeArgumentSyntax IAddNormalCSharpAttributeConstructorAssociationCommand.SyntacticArgument => SyntacticArgument;
}
