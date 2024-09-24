namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand
    : IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand
{
    private readonly AttributeArgumentSyntax SyntacticUnlabeledArgument;

    public HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
        AttributeArgumentSyntax syntacticUnlabeledArgument)
    {
        SyntacticUnlabeledArgument = syntacticUnlabeledArgument;
    }

    AttributeArgumentSyntax IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand.SyntacticUnlabeledArgument => SyntacticUnlabeledArgument;
}
