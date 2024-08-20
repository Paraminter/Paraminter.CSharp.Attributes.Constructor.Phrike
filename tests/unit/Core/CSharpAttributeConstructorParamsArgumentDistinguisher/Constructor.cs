namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void ReturnsDistinguisher()
    {
        var result = Target();

        Assert.NotNull(result);
    }

    private static CSharpAttributeConstructorParamsArgumentDistinguisher Target() => new();
}
