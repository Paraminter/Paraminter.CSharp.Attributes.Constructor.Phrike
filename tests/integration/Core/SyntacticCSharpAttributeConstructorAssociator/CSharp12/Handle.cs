namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Collectors;

using System.Collections.Generic;
using System.Linq;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void AttributeUsage_NormalArguments_AssociatesAll()
    {
        var source = """
            using System;

            [Test(1, "", false)]
            public class Foo { }

            public class TestAttribute : Attribute
            {
                public TestAttribute(int a, string b, bool c) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var parameters = type.GetAttributes()[0].AttributeConstructor!.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var attributeSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().Single();
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameters[0], syntacticArguments[0]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameters[1], syntacticArguments[1]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(parameters[2], syntacticArguments[2]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<AttributeArgumentSyntax>()), Times.Exactly(3));

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<AttributeArgumentSyntax>>()), Times.Never());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify((collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    [Fact]
    public void AttributeUsage_ParamsArguments_AssociatesAll()
    {
        var source = """
            using System;

            [Test(1, 2, 3)]
            public class Foo { }

            public class TestAttribute : Attribute
            {
                public TestAttribute(params int[] a) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var parameters = type.GetAttributes()[0].AttributeConstructor!.Parameters;

        var syntaxTree = compilation.SyntaxTrees[0];

        var attributeSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().Single();
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(parameters[0], syntacticArguments), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<AttributeArgumentSyntax>>()), Times.Exactly(1));

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<AttributeArgumentSyntax>()), Times.Never());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Never());

        queryResponseCollectorMock.Verify((collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    [Fact]
    public void AttributeUsage_DefaultArgument_AssociatesAll()
    {
        var source = """
            using System;

            [Test]
            public class Foo { }

            public class TestAttribute : Attribute
            {
                public TestAttribute(int a = 3) { }
            }
            """;

        var compilation = CompilationFactory.Create(source);

        var type = compilation.GetTypeByMetadataName("Foo")!;
        var parameters = type.GetAttributes()[0].AttributeConstructor!.Parameters;

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector> queryResponseCollectorMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseCollectorMock.Object);

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(parameters[0]), Times.Once());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Default.Add(It.IsAny<IParameterSymbol>()), Times.Exactly(1));

        queryResponseCollectorMock.Verify((collector) => collector.Associations.Params.Add(It.IsAny<IParameterSymbol>(), It.IsAny<IReadOnlyList<AttributeArgumentSyntax>>()), Times.Never());
        queryResponseCollectorMock.Verify((collector) => collector.Associations.Normal.Add(It.IsAny<IParameterSymbol>(), It.IsAny<AttributeArgumentSyntax>()), Times.Never());

        queryResponseCollectorMock.Verify((collector) => collector.Invalidator.Invalidate(), Times.Never());
    }

    private void Target(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
        IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector queryResponseCollector)
    {
        Fixture.Sut.Handle(query, queryResponseCollector);
    }
}
