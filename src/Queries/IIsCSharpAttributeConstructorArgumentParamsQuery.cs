namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;

/// <summary>Represents a query for whether a syntactic C# attribute constructor argument is a <see langword="params"/> argument.</summary>
public interface IIsCSharpAttributeConstructorArgumentParamsQuery
    : IQuery
{
    /// <summary>The attribute constructor parameter.</summary>
    public abstract IParameterSymbol Parameter { get; }

    /// <summary>The syntactic C# attribute constructor argument.</summary>
    public abstract AttributeArgumentSyntax SyntacticArgument { get; }

    /// <summary>A semantic model describing the syntactic argument.</summary>
    public abstract SemanticModel SemanticModel { get; }
}
