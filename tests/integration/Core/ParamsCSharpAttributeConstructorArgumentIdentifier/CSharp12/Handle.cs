namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Values.Collectors;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void Params_ImplicitlyConverted_RespondsWithTrue()
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

        RespondsWithTrue(source);
    }

    [Fact]
    public void Params_ExactType_RespondsWithTrue()
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

        RespondsWithTrue(source);
    }

    [Fact]
    public void Params_SameTypeExceptNullability_RespondsWithTrue()
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

        RespondsWithTrue(source);
    }

    [Fact]
    public void Params_Null_RespondsTrue()
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

        RespondsWithTrue(source);
    }

    [Fact]
    public void NonParams_RespondsFalse()
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

        ResponseWithFalse(source);
    }

    [Fact]
    public void NonParams_Null_RespondsFalse()
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

        ResponseWithFalse(source);
    }

    private void Target(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        IValuedQueryResponseCollector<bool> queryResponseCollector)
    {
        Fixture.Sut.Handle(query, queryResponseCollector);
    }

    private void RespondsWithTrue(
        string source)
    {
        RespondsWithValue(source, true);
    }

    private void ResponseWithFalse(
        string source)
    {
        RespondsWithValue(source, false);
    }

    private void RespondsWithValue(
        string source,
        bool expected)
    {
        var compilation = CompilationFactory.Create(source);

        var attribute = compilation.GetTypeByMetadataName("Foo")!.GetAttributes()[0];

        var parameter = attribute.AttributeConstructor!.Parameters[0];

        var attributeSyntax = (AttributeSyntax)attribute.ApplicationSyntaxReference!.GetSyntax();
        var syntacticArgument = attributeSyntax.ArgumentList!.Arguments[0];

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseCollector<bool>> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameter);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArgument);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(compilation.GetSemanticModel(attributeSyntax.SyntaxTree));

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Value.Set(expected), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Value.Set(It.IsAny<bool>()), Times.Once());
    }
}
