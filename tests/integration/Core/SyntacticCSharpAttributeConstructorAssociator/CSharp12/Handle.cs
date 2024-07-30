namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Attributes.Constructor.Commands;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Handlers;
using Paraminter.Queries.Invalidation.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameters[0], syntacticArguments[0]), Times.Once());
        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameters[1], syntacticArguments[1]), Times.Once());
        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameters[2], syntacticArguments[2]), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Exactly(3));

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
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
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(ParamsAssociationExpression(parameters[0], syntacticArguments), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Exactly(1));

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
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
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        queryMock.Setup((query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(DefaultAssociationExpression(parameters[0]), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Exactly(1));

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    private static Expression<Action<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler>> NormalAssociationExpression(
        IParameterSymbol parameter,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (handler) => handler.AssociationCollector.Normal.Handle(It.Is(MatchNormalAssociationCommand(parameter, syntacticArgument)));
    }

    private static Expression<Func<IAddNormalCSharpAttributeConstructorAssociationCommand, bool>> MatchNormalAssociationCommand(
        IParameterSymbol parameter,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => ReferenceEquals(command.Parameter, parameter) && ReferenceEquals(command.SyntacticArgument, syntacticArgument);
    }

    private static Expression<Action<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler>> ParamsAssociationExpression(
        IParameterSymbol parameter,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (handler) => handler.AssociationCollector.Params.Handle(It.Is(MatchParamsAssociationCommand(parameter, syntacticArguments)));
    }

    private static Expression<Func<IAddParamsCSharpAttributeConstructorAssociationCommand, bool>> MatchParamsAssociationCommand(
        IParameterSymbol parameter,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (command) => ReferenceEquals(command.Parameter, parameter) && Enumerable.SequenceEqual(command.SyntacticArguments, syntacticArguments);
    }

    private static Expression<Action<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler>> DefaultAssociationExpression(
        IParameterSymbol parameter)
    {
        return (handler) => handler.AssociationCollector.Default.Handle(It.Is(MatchDefaultAssociationCommand(parameter)));
    }

    private static Expression<Func<IAddDefaultCSharpAttributeConstructorCommand, bool>> MatchDefaultAssociationCommand(
        IParameterSymbol parameter)
    {
        return (command) => ReferenceEquals(command.Parameter, parameter);
    }

    private void Target(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
        IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler queryResponseHandler)
    {
        Fixture.Sut.Handle(query, queryResponseHandler);
    }
}
