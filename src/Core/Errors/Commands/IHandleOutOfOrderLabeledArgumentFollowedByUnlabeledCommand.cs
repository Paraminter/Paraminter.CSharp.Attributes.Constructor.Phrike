namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Cqs;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# attribute constructor arguments with parameters, caused by an out-of-order labeled argument being followed by an unlabeled argument.</summary>
public interface IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand
    : ICommand
{
    /// <summary>The unlabeled syntactic C# attribute constructor argument.</summary>
    public abstract AttributeArgumentSyntax SyntacticUnlabeledArgument { get; }
}
