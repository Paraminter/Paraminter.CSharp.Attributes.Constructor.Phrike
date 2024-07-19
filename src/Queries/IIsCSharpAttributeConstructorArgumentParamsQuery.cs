namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Queries;

/// <summary>Represents a query for whether a C# attribute constructor argument is a <see langword="params"/> argument.</summary>
public interface IIsCSharpAttributeConstructorArgumentParamsQuery
    : IQuery
{
    /// <summary>The associated attribute constructor parameter.</summary>
    public abstract IParameterSymbol Parameter { get; }

    /// <summary>The syntactic description of the attribute constructor argument.</summary>
    public abstract AttributeArgumentSyntax SyntacticArgument { get; }

    /// <summary>A semantic model containing the invocation.</summary>
    public abstract SemanticModel SemanticModel { get; }
}
