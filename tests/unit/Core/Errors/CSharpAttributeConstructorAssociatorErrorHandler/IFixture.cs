namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;

using Moq;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.Cqs;

internal interface IFixture
{
    public abstract ICSharpAttributeConstructorAssociatorErrorHandler Sut { get; }

    public abstract Mock<ICommandHandler<IHandleMissingRequiredArgumentCommand>> MissingRequiredArgumentMock { get; }
    public abstract Mock<ICommandHandler<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>> OutOfOrderLabeledArgumentFollowedByUnlabeledMock { get; }
    public abstract Mock<ICommandHandler<IHandleUnrecognizedLabeledArgumentCommand>> UnrecognizedLabeledArgumentMock { get; }
    public abstract Mock<ICommandHandler<IHandleDuplicateParameterNamesCommand>> DuplicateParameterNamesMock { get; }
    public abstract Mock<ICommandHandler<IHandleDuplicateArgumentsCommand>> DuplicateArgumentsMock { get; }
}
