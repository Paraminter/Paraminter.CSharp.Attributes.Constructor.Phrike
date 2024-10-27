namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.Cqs;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public async Task AttributeUsage_NormalArguments_PairsAll()
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

        var attribute = compilation.GetTypeByMetadataName("Foo")!.GetAttributes()[0];

        var parameters = attribute.AttributeConstructor!.Parameters;

        var attributeSyntax = (AttributeSyntax)await attribute.ApplicationSyntaxReference!.GetSyntaxAsync(CancellationToken.None);
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameters[0], syntacticArguments[0], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameters[1], syntacticArguments[1], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameters[2], syntacticArguments[2], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task AttributeUsage_ParamsArguments_PairsAll()
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

        var attribute = compilation.GetTypeByMetadataName("Foo")!.GetAttributes()[0];

        var parameters = attribute.AttributeConstructor!.Parameters;

        var attributeSyntax = (AttributeSyntax)await attribute.ApplicationSyntaxReference!.GetSyntaxAsync(CancellationToken.None);
        var syntacticArguments = attributeSyntax.ArgumentList!.Arguments;

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns(parameters);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameters[0], syntacticArguments, It.IsAny<CancellationToken>()), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task AttributeUsage_DefaultArgument_PairsAll()
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

        var parameters = compilation.GetTypeByMetadataName("Foo")!.GetAttributes()[0].AttributeConstructor!.Parameters;

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup((query) => query.Data.Parameters).Returns(parameters);
        commandMock.Setup((query) => query.Data.SyntacticArguments).Returns([]);

        await Target(commandMock.Object, CancellationToken.None);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>(), It.IsAny<CancellationToken>()), Times.Never());

        Fixture.DefaultPairerMock.Verify(PairDefaultArgumentExpression(parameters[0], It.IsAny<CancellationToken>()), Times.Once());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    private static Expression<Func<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>, Task>> PairNormalArgumentExpression(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.Handle(It.Is(MatchPairNormalArgumentCommand(parameterSymbol, syntacticArgument)), cancellationToken);
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>, bool>> MatchPairNormalArgumentCommand(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Func<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>, Task>> PairParamsArgumentExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.Handle(It.Is(MatchPairParamsArgumentCommand(parameterSymbol, syntacticArguments)), cancellationToken);
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>, bool>> MatchPairParamsArgumentCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Func<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>, Task>> PairDefaultArgumentExpression(
        IParameterSymbol parameterSymbol,
        CancellationToken cancellationToken)
    {
        return (handler) => handler.Handle(It.Is(MatchPairDefaultArgumentCommand(parameterSymbol)), cancellationToken);
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>, bool>> MatchPairDefaultArgumentCommand(
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

    private async Task Target(
        IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData> command,
        CancellationToken cancellationToken)
    {
        await Fixture.Sut.Handle(command, cancellationToken);
    }
}
