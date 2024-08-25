namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;
using Paraminter.Parameters.Method.Models;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# attribute constructor arguments with parameters, caused by there being multiple arguments for the same parameter.</summary>
public interface IHandleDuplicateArgumentsCommand
    : ICommand
{
    /// <summary>The parameter.</summary>
    public abstract IMethodParameter Parameter { get; }

    /// <summary>The syntactic C# attribute constructor argument.</summary>
    public abstract AttributeArgumentSyntax SyntacticArgument { get; }
}
