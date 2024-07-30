namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Values.Commands;
using Paraminter.Queries.Values.Handlers;

using System;
using System.Linq.Expressions;

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

    private static Expression<Action<IValuedQueryResponseHandler<TValue>>> SetValueExpression<TValue>(
        TValue value)
    {
        return (handler) => handler.Value.Handle(It.Is(MatchSetValueCommand(value)));
    }

    private static Expression<Func<ISetQueryResponseValueCommand<TValue>, bool>> MatchSetValueCommand<TValue>(
        TValue value)
    {
        return (command) => Equals(command.Value, value);
    }

    private void Target(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        IValuedQueryResponseHandler<bool> queryResponseHandler)
    {
        Fixture.Sut.Handle(query, queryResponseHandler);
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

        var semanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseHandler<bool>> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameter);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArgument);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Value.Handle(It.IsAny<ISetQueryResponseValueCommand<bool>>()), Times.Once());
        queryResponseHandlerMock.Verify(SetValueExpression(expected), Times.Once());
    }
}
