namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;

using System.Threading;
using System.Threading.Tasks;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public async Task Params_ImplicitlyConverted_ReturnsTrue()
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

        await ReturnsTrue(source, CancellationToken.None);
    }

    [Fact]
    public async Task Params_ExactType_ReturnsTrue()
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

        await ReturnsTrue(source, CancellationToken.None);
    }

    [Fact]
    public async Task Params_SameTypeExceptNullability_ReturnsTrue()
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

        await ReturnsTrue(source, CancellationToken.None);
    }

    [Fact]
    public async Task Params_Null_ReturnsTrue()
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

        await ReturnsTrue(source, CancellationToken.None);
    }

    [Fact]
    public async Task NonParams_ReturnsFalse()
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

        await ReturnsFalse(source, CancellationToken.None);
    }

    [Fact]
    public async Task NonParams_Null_ReturnsFalse()
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

        await ReturnsFalse(source, CancellationToken.None);
    }

    private async Task<bool> Target(
        IIsCSharpAttributeConstructorArgumentParamsQuery query,
        CancellationToken cancellationToken)
    {
        return await Fixture.Sut.Handle(query, cancellationToken);
    }

    private async Task ReturnsTrue(
        string source,
        CancellationToken cancellationToken)
    {
        await ReturnsValue(source, true, cancellationToken);
    }

    private async Task ReturnsFalse(
        string source,
        CancellationToken cancellationToken)
    {
        await ReturnsValue(source, false, cancellationToken);
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
