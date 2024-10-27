namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;

using System;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public async Task NullQuery_ThrowsArgumentNullException()
    {
        var result = await Record.ExceptionAsync(() => Target(null!, CancellationToken.None));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public async Task NotParamsParameter_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(false);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = await Target(queryMock.Object, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task NotArraySymbol_ReturnsFalse()
    {
        Mock<IParameterSymbol> parameterMock = new() { DefaultValue = DefaultValue.Mock };

        parameterMock.Setup(static (parameter) => parameter.IsParams).Returns(true);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameterMock.Object);

        var result = await Target(queryMock.Object, CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ArgumentNotOfElementType_ReturnsFalse()
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

        await ReturnsValue(source, false, CancellationToken.None);
    }

    [Fact]
    public async Task ArgumentOfElementType_ReturnsTrue()
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

        await ReturnsValue(source, true, CancellationToken.None);
    }

    private async Task<bool> Target(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        CancellationToken cancellationToken)
    {
        return await Fixture.Sut.Handle(query, cancellationToken);
    }

    private async Task ReturnsValue(
        string source,
        bool expected,
        CancellationToken cancellationToken)
    {
        var compilation = CompilationFactory.Create(source);

        var attribute = compilation.GetTypeByMetadataName("Foo")!.GetAttributes()[0];

        var parameters = attribute.AttributeConstructor!.Parameters;

        var attributeSyntax = (AttributeSyntax)await attribute.ApplicationSyntaxReference!.GetSyntaxAsync(CancellationToken.None);
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        var semanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);

        Mock<IIsCSharpAttributeConstructorArgumentParamsQuery> queryMock = new();

        queryMock.Setup(static (query) => query.Parameter).Returns(parameters[0]);
        queryMock.Setup(static (query) => query.SyntacticArgument).Returns(syntacticArguments[0]);
        queryMock.Setup(static (query) => query.SemanticModel).Returns(semanticModel);

        var result = await Target(queryMock.Object, cancellationToken);

        Assert.Equal(expected, result);
    }
}
