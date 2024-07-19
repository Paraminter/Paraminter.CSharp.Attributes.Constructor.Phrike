namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;

internal sealed class IsCSharpAttributeConstructorArgumentParamsQuery
    : IIsCSharpAttributeConstructorArgumentParamsQuery
{
    private readonly IParameterSymbol Parameter;
    private readonly AttributeArgumentSyntax SyntacticArgument;
    private readonly SemanticModel SemanticModel;

    public IsCSharpAttributeConstructorArgumentParamsQuery(
        IParameterSymbol parameter,
        AttributeArgumentSyntax syntacticArgument,
        SemanticModel semanticModel)
    {
        Parameter = parameter;
        SyntacticArgument = syntacticArgument;
        SemanticModel = semanticModel;
    }

    IParameterSymbol IIsCSharpAttributeConstructorArgumentParamsQuery.Parameter => Parameter;
    AttributeArgumentSyntax IIsCSharpAttributeConstructorArgumentParamsQuery.SyntacticArgument => SyntacticArgument;
    SemanticModel IIsCSharpAttributeConstructorArgumentParamsQuery.SemanticModel => SemanticModel;
}
