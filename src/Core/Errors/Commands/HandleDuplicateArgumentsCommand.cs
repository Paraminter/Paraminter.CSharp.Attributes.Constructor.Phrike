namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Parameters.Method.Models;

internal sealed class HandleDuplicateArgumentsCommand
    : IHandleDuplicateArgumentsCommand
{
    private readonly IMethodParameter Parameter;
    private readonly AttributeArgumentSyntax SyntacticArgument;

    public HandleDuplicateArgumentsCommand(
        IMethodParameter parameter,
        AttributeArgumentSyntax syntacticArgument)
    {
        Parameter = parameter;
        SyntacticArgument = syntacticArgument;
    }

    IMethodParameter IHandleDuplicateArgumentsCommand.Parameter => Parameter;
    AttributeArgumentSyntax IHandleDuplicateArgumentsCommand.SyntacticArgument => SyntacticArgument;
}
