namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using Paraminter.Cqs;

/// <summary>Represents a command to handle an error encountered when associating syntactic C# attribute constructor arguments with parameters, caused by there being multiple parameters with the same name.</summary>
public interface IHandleDuplicateParameterNamesCommand
    : ICommand
{
    /// <summary>The name of the parameters.</summary>
    public abstract string ParameterName { get; }
}
