﻿namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Common;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

using System;

/// <summary>Identifies <see langword="params"/> C# attribute constructor arguments.</summary>
public sealed class ParamsCSharpAttributeConstructorArgumentIdentifier
    : IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>
{
    /// <summary>Instantiates a <see cref="ParamsCSharpAttributeConstructorArgumentIdentifier"/>, identifying <see langword="params"/> C# attribute constructor arguments.</summary>
    public ParamsCSharpAttributeConstructorArgumentIdentifier() { }

    void IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>.Handle(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        IValuedQueryResponseHandler<bool> queryResponseHandler)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseHandler is null)
        {
            throw new ArgumentNullException(nameof(queryResponseHandler));
        }

        var result = Handle(query);

        var command = new SetQueryResponseValueCommand<bool>(result);

        queryResponseHandler.Value.Handle(command);
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
