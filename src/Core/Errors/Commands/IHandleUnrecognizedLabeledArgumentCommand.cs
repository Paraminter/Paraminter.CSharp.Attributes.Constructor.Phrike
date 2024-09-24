namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# attribute constructor arguments with parameters, caused by a labeled argument of an unrecognized parameter.</summary>
public interface IHandleUnrecognizedLabeledArgumentCommand
    : ICommand
{
    /// <summary>The syntactic C# attribute constructor argument.</summary>
    public abstract AttributeArgumentSyntax SyntacticArgument { get; }
}
