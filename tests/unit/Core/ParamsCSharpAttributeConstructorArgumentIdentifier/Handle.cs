namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Values.Commands;
using Paraminter.Queries.Values.Handlers;

using System;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void NullQuery_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!, Mock.Of<IValuedQueryResponseHandler<bool>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullQueryResponseHandler_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(Mock.Of<IIsCSharpAttributeConstructorArgumentParamsQuery>(), null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NotParamsParameter_SetsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(false);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseHandler<bool>> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Value.Handle(It.IsAny<ISetQueryResponseValueCommand<bool>>()), Times.Once());
        queryResponseHandlerMock.Verify(SetValueExpression(false), Times.Once());
    }

    [Fact]
    public void NotArraySymbol_SetsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(true);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseHandler<bool>> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Value.Handle(It.IsAny<ISetQueryResponseValueCommand<bool>>()), Times.Once());
        queryResponseHandlerMock.Verify(SetValueExpression(false), Times.Once());
    }

    [Fact]
    public void ArgumentNotOfElementType_SetsFalse()
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

        SetsValue(source, false);
    }

    [Fact]
    public void ArgumentOfElementType_SetsTrue()
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

        SetsValue(source, true);
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

    private void SetsValue(
        string source,
        bool expected)
    {
        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var parameters = type.GetAttributes()[0].AttributeConstructor!.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        var attributeSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().Single();
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseHandler<bool>> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Value.Handle(It.IsAny<ISetQueryResponseValueCommand<bool>>()), Times.Once());
        queryResponseHandlerMock.Verify(SetValueExpression(expected), Times.Once());
    }
}
