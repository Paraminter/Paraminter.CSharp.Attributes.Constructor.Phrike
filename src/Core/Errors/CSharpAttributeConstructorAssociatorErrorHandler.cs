namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using System;

/// <inheritdoc cref="ICSharpAttributeConstructorAssociatorErrorHandler"/>
public sealed class CSharpAttributeConstructorAssociatorErrorHandler
    : ICSharpAttributeConstructorAssociatorErrorHandler
{
    private readonly ICommandHandler<IHandleMissingRequiredArgumentCommand> MissingRequiredArgument;
    private readonly ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand> OutOfOrderLabeledArgumentFollowedByUnlabeled;
    private readonly ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand> UnrecognizedLabeledArgument;
    private readonly ICommandHandler<IHandleDuplicateParameterNamesCommand> DuplicateParameterNames;
    private readonly ICommandHandler<IHandleDuplicateArgumentsCommand> DuplicateArguments;

    /// <summary>Instantiates a handler of errors encountered when associating syntactic C# attribute constructor arguments with parameters.</summary>
    /// <param name="missingRequiredArgument">Handles a missing required argument.</param>
    /// <param name="outOfOrderLabeledArgumentFollowedByUnlabeled">Handles an out-of-order labeled argument followed by an unlabeled argument.</param>
    /// <param name="unrecognizedLabeledArgument">Handles a labeled argument of an unrecognized parameter.</param>
    /// <param name="duplicateParameterNames">Handles multiple parameters having the same name.</param>
    /// <param name="duplicateArguments">Handles multiple arguments for the same parameter.</param>
    public CSharpAttributeConstructorAssociatorErrorHandler(
        ICommandHandler<IHandleMissingRequiredArgumentCommand> missingRequiredArgument,
        ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand> outOfOrderLabeledArgumentFollowedByUnlabeled,
        ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand> unrecognizedLabeledArgument,
        ICommandHandler<IHandleDuplicateParameterNamesCommand> duplicateParameterNames,
        ICommandHandler<IHandleDuplicateArgumentsCommand> duplicateArguments)
    {
        MissingRequiredArgument = missingRequiredArgument ?? throw new ArgumentNullException(nameof(missingRequiredArgument));
        OutOfOrderLabeledArgumentFollowedByUnlabeled = outOfOrderLabeledArgumentFollowedByUnlabeled ?? throw new ArgumentNullException(nameof(outOfOrderLabeledArgumentFollowedByUnlabeled));
        UnrecognizedLabeledArgument = unrecognizedLabeledArgument ?? throw new ArgumentNullException(nameof(unrecognizedLabeledArgument));
        DuplicateParameterNames = duplicateParameterNames ?? throw new ArgumentNullException(nameof(duplicateParameterNames));
        DuplicateArguments = duplicateArguments ?? throw new ArgumentNullException(nameof(duplicateArguments));
    }

    ICommandHandler<IHandleMissingRequiredArgumentCommand> ICSharpAttributeConstructorAssociatorErrorHandler.MissingRequiredArgument => MissingRequiredArgument;
    ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand> ICSharpAttributeConstructorAssociatorErrorHandler.OutOfOrderLabeledArgumentFollowedByUnlabeled => OutOfOrderLabeledArgumentFollowedByUnlabeled;
    ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand> ICSharpAttributeConstructorAssociatorErrorHandler.UnrecognizedLabeledArgument => UnrecognizedLabeledArgument;
    ICommandHandler<IHandleDuplicateParameterNamesCommand> ICSharpAttributeConstructorAssociatorErrorHandler.DuplicateParameterNames => DuplicateParameterNames;
    ICommandHandler<IHandleDuplicateArgumentsCommand> ICSharpAttributeConstructorAssociatorErrorHandler.DuplicateArguments => DuplicateArguments;
}
