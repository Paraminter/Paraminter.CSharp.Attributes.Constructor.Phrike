namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void ReturnsIdentifier()
    {
        var result = Target();

        Assert.NotNull(result);
    }

    private static ParamsCSharpAttributeConstructorArgumentIdentifier Target() => new();
}
