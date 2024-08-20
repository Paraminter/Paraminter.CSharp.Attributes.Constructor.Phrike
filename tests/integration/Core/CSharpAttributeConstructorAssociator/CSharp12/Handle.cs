namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.Parameters.Method.Models;

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

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameters[0], syntacticArguments[0]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameters[1], syntacticArguments[1]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(AssociateIndividualNormalExpression(parameters[2], syntacticArguments[2]), Times.Once());
        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Exactly(3));

        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
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

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsIndividualAssociatorMock.Verify(AssociateIndividualParamsExpression(parameters[0], syntacticArguments), Times.Once());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Exactly(1));

        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (assoacitor) => assoacitor.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
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

        Mock<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        commandMock.Setup((query) => query.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.DefaultIndividualAssociatorMock.Verify(AssociateIndividualDefaultExpression(parameters[0]), Times.Once());
        Fixture.DefaultIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Exactly(1));

        Fixture.NormalIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.ParamsIndividualAssociatorMock.Verify(static (associator) => associator.Handle(It.IsAny<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    private static Expression<Action<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>> AssociateIndividualNormalExpression(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (associator) => associator.Handle(It.Is(MatchAssociateIndividualNormalCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>, bool>> MatchAssociateIndividualNormalCommand(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>> AssociateIndividualParamsExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (associator) => associator.Handle(It.Is(MatchAssociateIndividualParamsCommand(parameterSymbol, syntacticArguments)));
    }

    private static Expression<Func<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>, bool>> MatchAssociateIndividualParamsCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>> AssociateIndividualDefaultExpression(
        IParameterSymbol parameterSymbol)
    {
        return (associator) => associator.Handle(It.Is(MatchAssociateIndividualDefaultCommand(parameterSymbol)));
    }

    private static Expression<Func<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>, bool>> MatchAssociateIndividualDefaultCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static bool MatchParameter(
        IParameterSymbol parameterSymbol,
        IMethodParameter parameter)
    {
        return ReferenceEquals(parameterSymbol, parameter.Symbol);
    }

    private static bool MatchNormalArgumentData(
        AttributeArgumentSyntax syntacticArgument,
        INormalCSharpAttributeConstructorArgumentData argumentData)
    {
        return ReferenceEquals(syntacticArgument, argumentData.SyntacticArgument);
    }

    private static bool MatchParamsArgumentData(
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments,
        IParamsCSharpAttributeConstructorArgumentData argumentData)
    {
        return Enumerable.SequenceEqual(syntacticArguments, argumentData.SyntacticArguments);
    }

    private void Target(
        IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
