namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.Cqs.Handlers;

using Xunit;

public sealed class DuplicateArguments
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void ReturnsHandler()
    {
        var result = Target();

        Assert.Same(Fixture.DuplicateArgumentsMock.Object, result);
    }

    private ICommandHandler<IHandleDuplicateArgumentsCommand> Target() => Fixture.Sut.DuplicateArguments;
}
