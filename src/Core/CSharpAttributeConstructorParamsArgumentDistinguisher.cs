namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Cqs;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>Distinguishes between <see langword="params"/> and non-<see langword="params"/> syntactic C# attribute constructor arguments.</summary>
public sealed class CSharpAttributeConstructorParamsArgumentDistinguisher
    : IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>
{
    /// <summary>Instantiates a <see cref="CSharpAttributeConstructorParamsArgumentDistinguisher"/>, distinguishing between <see langword="params"/> and non-<see langword="params"/> syntactic C# attribute constructor arguments.</summary>
    public CSharpAttributeConstructorParamsArgumentDistinguisher() { }

    async Task<bool> IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>.Handle(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        CancellationToken cancellationToken)
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

        return await Task.FromResult(SymbolEqualityComparer.Default.Equals(expressedType.ConvertedType, arrayType.ElementType));
    }
}
