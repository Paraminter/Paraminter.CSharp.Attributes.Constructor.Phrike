namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Values.Collectors;

using System;
using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void NullQuery_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!, Mock.Of<IValuedQueryResponseCollector<bool>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullQueryResponseCollector_ThrowsArgumentNullException()
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
        Mock<IValuedQueryResponseCollector<bool>> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Value.Set(false), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Value.Set(It.IsAny<bool>()), Times.Once());
    }

    [Fact]
    public void NotArraySymbol_SetsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(true);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();
        Mock<IValuedQueryResponseCollector<bool>> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify(static (collector) => collector.Value.Set(false), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Value.Set(It.IsAny<bool>()), Times.Once());
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

    private void Target(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        IValuedQueryResponseCollector<bool> queryResponseCollector)
    {
        Fixture.Sut.Handle(query, queryResponseCollector);
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
        Mock<IValuedQueryResponseCollector<bool>> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Value.Set(expected), Times.Once());
        queryResponseCollectorMock.Verify(static (collector) => collector.Value.Set(It.IsAny<bool>()), Times.Once());
    }
}
