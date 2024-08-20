namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void Params_ImplicitlyConverted_ReturnsTrue()
    {
        var source = """
            using System;

            public class ParamsAttribute : Attribute
            {
                public ParamsAttribute(params double[] values) { }
            }

            [ParamsAttribute(4)]
            public class Foo { }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void Params_ExactType_ReturnsTrue()
    {
        var source = """
            using System;

            public class ParamsAttribute : Attribute
            {
                public ParamsAttribute(params double[] values) { }
            }

            [ParamsAttribute(4.2)]
            public class Foo { }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void Params_SameTypeExceptNullability_ReturnsTrue()
    {
        var source = """
            using System;

            public class ParamsAttribute : Attribute
            {
                public ParamsAttribute(params double?[] values) { }
            }

            [ParamsAttribute(4.2)]
            public class Foo { }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void Params_Null_ReturnsTrue()
    {
        var source = """
            using System;

            public class ParamsAttribute : Attribute
            {
                public ParamsAttribute(params object?[]? values) { }
            }

            [ParamsAttribute(null, null)]
            public class Foo { }
            """;

        ReturnsTrue(source);
    }

    [Fact]
    public void NonParams_ReturnsFalse()
    {
        var source = """
            using System;

            public class ParamsAttribute : Attribute
            {
                public ParamsAttribute(params double[] values) { }
            }

            [ParamsAttribute([4.2])]
            public class Foo { }
            """;

        ReturnsFalse(source);
    }

    [Fact]
    public void NonParams_Null_ReturnsFalse()
    {
        var source = """
            using System;

            public class ParamsAttribute : Attribute
            {
                public ParamsAttribute(params object?[]? values) { }
            }

            [ParamsAttribute(null)]
            public class Foo { }
            """;

        ReturnsFalse(source);
    }

    private bool Target(
        IIsCSharpAttributeConstructorArgumentParamsQuery query)
    {
        return Fixture.Sut.Handle(query);
    }

    private void ReturnsTrue(
        string source)
    {
        ReturnsValue(source, true);
    }

    private void ReturnsFalse(
        string source)
    {
        ReturnsValue(source, false);
    }

    private void ReturnsValue(
        string source,
        bool expected)
    {
        var compilation = CompilationFactory.Create(source);

        var attribute = compilation.GetTypeByMetadataName("Foo")!.GetAttributes()[0];

        var parameter = attribute.AttributeConstructor!.Parameters[0];

        var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax();
        var syntacticArgument = attributeSyntax.ArgumentList!.Arguments[0];

        var semanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameter);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArgument);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        var result = Target(queryMock.Object);

        Assert.Equal(expected, result);
    }
}
