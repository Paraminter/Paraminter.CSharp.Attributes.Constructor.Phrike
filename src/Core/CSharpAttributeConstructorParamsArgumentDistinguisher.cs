namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;

using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;

using System;

/// <summary>Distinguishes between <see langword="params"/> and non-<see langword="params"/> syntactic C# attribute constructor arguments.</summary>
public sealed class CSharpAttributeConstructorParamsArgumentDistinguisher
    : IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>
{
    /// <summary>Instantiates a <see cref="CSharpAttributeConstructorParamsArgumentDistinguisher"/>, distinguishing between <see langword="params"/> and non-<see langword="params"/> syntactic C# attribute constructor arguments.</summary>
    public CSharpAttributeConstructorParamsArgumentDistinguisher() { }

    bool IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>.Handle(
        IIsCSharpAttributeConstructorArgumentParamsQuery query)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (query.Parameter.IsParams is false)
        {
            return false;
        }

        if (query.Parameter.Type is not IArrayTypeSymbol arrayType)
        {
            return false;
        }

        var expressedType = query.SemanticModel.GetTypeInfo(query.SyntacticArgument.Expression);

        return SymbolEqualityComparer.Default.Equals(expressedType.ConvertedType, arrayType.ElementType);
    }
}
