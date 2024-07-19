namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

using System;

/// <summary>Identifies <see langword="params"/> C# attribute constructor arguments.</summary>
public sealed class ParamsCSharpAttributeConstructorArgumentIdentifier
    : IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>>
{
    /// <summary>Instantiates a <see cref="ParamsCSharpAttributeConstructorArgumentIdentifier"/>, identifying <see langword="params"/> C# attribute constructor arguments.</summary>
    public ParamsCSharpAttributeConstructorArgumentIdentifier() { }

    void IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>>.Handle(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        IValuedQueryResponseCollector<bool> queryResponseCollector)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseCollector is null)
        {
            throw new ArgumentNullException(nameof(queryResponseCollector));
        }

        var result = Handle(query);

        queryResponseCollector.Value.Set(result);
    }

    private bool Handle(
        IIsCSharpAttributeConstructorArgumentParamsQuery query)
    {
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
