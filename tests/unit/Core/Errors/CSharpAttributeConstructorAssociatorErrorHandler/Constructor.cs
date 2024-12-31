﻿namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;

using Moq;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullMissingRequiredArgument_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            null!,
            Mock.Of<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>>(),
            Mock.Of<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateParameterNamesCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateArgumentsCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullOutOfOrderLabeledArgumentFollowedByUnlabeled_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IHandleMissingRequiredArgumentCommand>>(),
            null!,
            Mock.Of<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateParameterNamesCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateArgumentsCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullUnrecognizedLabeledArgument_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IHandleMissingRequiredArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>>(),
            null!,
            Mock.Of<ICommandHandler<IHandleDuplicateParameterNamesCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateArgumentsCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDuplicateParameterNames_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IHandleMissingRequiredArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>>(),
            Mock.Of<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>>(),
            null!,
            Mock.Of<ICommandHandler<IHandleDuplicateArgumentsCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDuplicateArguments_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IHandleMissingRequiredArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>>(),
            Mock.Of<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateParameterNamesCommand>>(),
            null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsHandler()
    {
        var result = Target(
            Mock.Of<ICommandHandler<IHandleMissingRequiredArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>>(),
            Mock.Of<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateParameterNamesCommand>>(),
            Mock.Of<ICommandHandler<IHandleDuplicateArgumentsCommand>>());

        Assert.NotNull(result);
    }

    private static CSharpAttributeConstructorAssociatorErrorHandler Target(
        ICommandHandler<IHandleMissingRequiredArgumentCommand> missingRequiredArgument,
        ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand> outOfOrderLabeledArgumentFollowedByUnlabeled,
        ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand> unrecognizedLabeledArgument,
        ICommandHandler<IHandleDuplicateParameterNamesCommand> duplicateParameterNames,
        ICommandHandler<IHandleDuplicateArgumentsCommand> duplicateArguments)
    {
        return new CSharpAttributeConstructorAssociatorErrorHandler(missingRequiredArgument, outOfOrderLabeledArgumentFollowedByUnlabeled, unrecognizedLabeledArgument, duplicateParameterNames, duplicateArguments);
    }
}
