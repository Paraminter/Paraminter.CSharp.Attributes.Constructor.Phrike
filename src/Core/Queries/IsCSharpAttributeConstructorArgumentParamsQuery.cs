namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
