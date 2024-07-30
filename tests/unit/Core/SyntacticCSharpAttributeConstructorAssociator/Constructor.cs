namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullParamsArgumentIdentifier_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>>());

        Assert.NotNull(result);
    }

    private static SyntacticCSharpAttributeConstructorAssociator Target(
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier)
    {
        return new SyntacticCSharpAttributeConstructorAssociator(paramsArgumentIdentifier);
    }
}
