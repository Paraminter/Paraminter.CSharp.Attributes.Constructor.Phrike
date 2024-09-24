namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;

internal sealed class NormalCSharpAttributeConstructorArgumentData
    : INormalCSharpAttributeConstructorArgumentData
{
    private readonly AttributeArgumentSyntax SyntacticArgument;

    public NormalCSharpAttributeConstructorArgumentData(
        AttributeArgumentSyntax syntacticArgument)
    {
        SyntacticArgument = syntacticArgument;
    }

    AttributeArgumentSyntax INormalCSharpAttributeConstructorArgumentData.SyntacticArgument => SyntacticArgument;
}
