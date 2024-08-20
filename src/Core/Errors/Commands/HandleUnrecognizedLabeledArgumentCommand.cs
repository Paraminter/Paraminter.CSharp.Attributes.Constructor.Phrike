namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class HandleUnrecognizedLabeledArgumentCommand
    : IHandleUnrecognizedLabeledArgumentCommand
{
    private readonly AttributeArgumentSyntax SyntacticArgument;

    public HandleUnrecognizedLabeledArgumentCommand(
        AttributeArgumentSyntax syntacticArgument)
    {
        SyntacticArgument = syntacticArgument;
    }

    AttributeArgumentSyntax IHandleUnrecognizedLabeledArgumentCommand.SyntacticArgument => SyntacticArgument;
}
