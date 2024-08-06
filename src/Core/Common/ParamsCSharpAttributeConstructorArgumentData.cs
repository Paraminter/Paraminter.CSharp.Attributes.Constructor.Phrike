namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Common;

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
