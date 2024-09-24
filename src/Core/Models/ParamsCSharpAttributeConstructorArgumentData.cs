namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;

using System.Collections.Generic;

internal sealed class ParamsCSharpAttributeConstructorArgumentData
    : IParamsCSharpAttributeConstructorArgumentData
{
    private readonly IReadOnlyList<AttributeArgumentSyntax> SyntacticArguments;

    public ParamsCSharpAttributeConstructorArgumentData(
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        SyntacticArguments = syntacticArguments;
    }

    IReadOnlyList<AttributeArgumentSyntax> IParamsCSharpAttributeConstructorArgumentData.SyntacticArguments => SyntacticArguments;
}
