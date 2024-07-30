namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.CSharp.Attributes.Constructor.Commands;

using System.Collections.Generic;

internal sealed class AddParamsCSharpAttributeConstructorAssociationCommand
    : IAddParamsCSharpAttributeConstructorAssociationCommand
{
    private readonly IParameterSymbol Parameter;
    private readonly IReadOnlyList<AttributeArgumentSyntax> SyntacticArguments;

    public AddParamsCSharpAttributeConstructorAssociationCommand(
        IParameterSymbol parameter,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        Parameter = parameter;
        SyntacticArguments = syntacticArguments;
    }

    IParameterSymbol IAddParamsCSharpAttributeConstructorAssociationCommand.Parameter => Parameter;
    IReadOnlyList<AttributeArgumentSyntax> IAddParamsCSharpAttributeConstructorAssociationCommand.SyntacticArguments => SyntacticArguments;
}
