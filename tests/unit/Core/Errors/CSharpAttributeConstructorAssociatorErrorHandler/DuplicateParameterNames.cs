namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.Cqs;

using Xunit;

public sealed class DuplicateParameterNames
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void ReturnsHandler()
    {
        var result = Target();

        Assert.Same(Fixture.DuplicateParameterNamesMock.Object, result);
    }

    private ICommandHandler<IHandleDuplicateParameterNamesCommand> Target() => Fixture.Sut.DuplicateParameterNames;
}
