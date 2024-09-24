namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;

using System;
using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void NullQuery_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NotParamsParameter_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(false);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = Target(queryMock.Object);

        Assert.False(result);
    }

    [Fact]
    public void NotArraySymbol_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(true);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = Target(queryMock.Object);

        Assert.False(result);
    }

    [Fact]
    public void ArgumentNotOfElementType_ReturnsFalse()
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

        ReturnsValue(source, false);
    }

    [Fact]
    public void ArgumentOfElementType_ReturnsTrue()
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

        ReturnsValue(source, true);
    }

    private bool Target(
        IIsCSharpAttributeConstructorArgumentParamsQuery query)
    {
        return Fixture.Sut.Handle(query);
    }

    private void ReturnsValue(
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

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        var result = Target(queryMock.Object);

        Assert.Equal(expected, result);
    }
}
