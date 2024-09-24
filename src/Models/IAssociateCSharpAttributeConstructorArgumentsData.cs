namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Associating.Models;

using System.Collections.Generic;

/// <summary>Represents data used to associate syntactic C# attribute constructor arguments with parameters.</summary>
public interface IAssociateCSharpAttributeConstructorArgumentsData
    : IAssociateArgumentsData
{
    /// <summary>The attribute constructor parameters.</summary>
    public abstract IReadOnlyList<IParameterSymbol> Parameters { get; }

    /// <summary>The syntactic C# attribute constructor arguments, possibly also containing syntactic C# named attribute arguments.</summary>
    public abstract IReadOnlyList<AttributeArgumentSyntax> SyntacticArguments { get; }

    /// <summary>A semantic model describing the syntactic arguments.</summary>
    public abstract SemanticModel SemanticModel { get; }
}
