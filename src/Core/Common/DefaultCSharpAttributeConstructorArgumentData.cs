namespace Paraminter.CSharp.Attributes.Constructor.Phrike.Common;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;

internal sealed class DefaultCSharpAttributeConstructorArgumentData
    : IDefaultCSharpAttributeConstructorArgumentData
{
    public static IDefaultCSharpAttributeConstructorArgumentData Instance { get; } = new DefaultCSharpAttributeConstructorArgumentData();

    private DefaultCSharpAttributeConstructorArgumentData() { }
}
