namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Cqs.Handlers;
using Paraminter.Pairing.Commands;
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
    public void NullCommand_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void MissingRequiredArgument_HandlesError()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(HandleMissingRequiredArgumentExpression(parameterSymbolMock.Object), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void OutOfOrderLabelledArgumentFollowedByUnlabelled_HandlesError()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(null, SyntaxFactory.NameColon("_2"), SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.AttributeArgument(null, null, SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleOutOfOrderLabeledArgumentFollowedByUnlabeledExpression(syntacticArguments[1]), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void ArgumentForNonExistingParameter_HandlesError()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(null, SyntaxFactory.NameColon("_1"), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleUnrecognizedLabeledArgumentExpression(syntacticArguments[0]), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void DuplicateParameter_HandlesError()
    {
        var parameterName = "Foo";

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns(parameterName);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns(parameterName);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleDuplicateParameterNamesExpression(parameterName), Times.Once());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());
    }

    [Fact]
    public void MultipleArgumentsForParameter_HandlesError()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(null, SyntaxFactory.NameColon("_1"), SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.AttributeArgument(null, SyntaxFactory.NameColon("_1"), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(HandleDuplicateArgumentsExpression(parameter1SymbolMock.Object, syntacticArguments[1]), Times.Once());
    }

    [Fact]
    public void UnspecifiedOptionalArguments_NamedArguments_PairsDefaultArguments()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("_1"), null, SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("_2"), null, SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_1");
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter1SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("_2");
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(true);
        parameter2SymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.DefaultPairerMock.Verify(PairDefaultArgumentExpression(parameter1SymbolMock.Object), Times.Once());
        Fixture.DefaultPairerMock.Verify(PairDefaultArgumentExpression(parameter2SymbolMock.Object), Times.Once());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Exactly(2));

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void ValidNormalArgument_PairsNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void UnspecifiedParamsArgument_PairsEmptyArray()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameterSymbolMock.Object, []), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void MultipleParamsArguments_PairsParamsArguments()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42")),
            SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("_1"), null, SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameterSymbolMock.Object, syntacticArguments.Take(2).ToList()), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameters_IsParamsArgument_PairsParamsArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentDistinguisherMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(true);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.ParamsPairerMock.Verify(PairParamsArgumentExpression(parameterSymbolMock.Object, syntacticArguments), Times.Once());
        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_IsNotParamsArgument_PairsNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentDistinguisherMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(false);

        Target(commandMock.Object);

        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.MissingRequiredArgument.Handle(It.IsAny<IHandleMissingRequiredArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.IsAny<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.UnrecognizedLabeledArgument.Handle(It.IsAny<IHandleUnrecognizedLabeledArgumentCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateParameterNames.Handle(It.IsAny<IHandleDuplicateParameterNamesCommand>()), Times.Never());
        Fixture.ErrorHandlerMock.Verify(static (handler) => handler.DuplicateArguments.Handle(It.IsAny<IHandleDuplicateArgumentsCommand>()), Times.Never());

        Fixture.NormalPairerMock.Verify(PairNormalArgumentExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.ParamsPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultPairerMock.Verify(static (handler) => handler.Handle(It.IsAny<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    private static Expression<Func<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>, bool>> IsParamsExpression(
        IParameterSymbol parameter,
        AttributeArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        return (handler) => handler.Handle(It.Is(MatchIsParamsQuery(parameter, argument, semanticModel)));
    }

    private static Expression<Func<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> MatchIsParamsQuery(
        IParameterSymbol parameter,
        AttributeArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        return (query) => ReferenceEquals(query.Parameter, parameter) && ReferenceEquals(query.SyntacticArgument, argument) && ReferenceEquals(query.SemanticModel, semanticModel);
    }

    private static Expression<Action<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>> PairNormalArgumentExpression(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (handler) => handler.Handle(It.Is(MatchPairNormalArgumentCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>, bool>> MatchPairNormalArgumentCommand(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>> PairParamsArgumentExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (handler) => handler.Handle(It.Is(MatchPairParamsArgumentCommand(parameterSymbol, syntacticArguments)));
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>, bool>> MatchPairParamsArgumentCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>> PairDefaultArgumentExpression(
        IParameterSymbol parameterSymbol)
    {
        return (handler) => handler.Handle(It.Is(MatchPairDefaultArgumentCommand(parameterSymbol)));
    }

    private static Expression<Func<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>, bool>> MatchPairDefaultArgumentCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static Expression<Action<ICSharpAttributeConstructorAssociatorErrorHandler>> HandleMissingRequiredArgumentExpression(
        IParameterSymbol parameterSymbol)
    {
        return (handler) => handler.MissingRequiredArgument.Handle(It.Is(MatchHandleMissingRequiredArgumentCommand(parameterSymbol)));
    }

    private static Expression<Func<IHandleMissingRequiredArgumentCommand, bool>> MatchHandleMissingRequiredArgumentCommand(
        IParameterSymbol parameterSymbol)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter);
    }

    private static Expression<Action<ICSharpAttributeConstructorAssociatorErrorHandler>> HandleOutOfOrderLabeledArgumentFollowedByUnlabeledExpression(
        AttributeArgumentSyntax syntacticUnlabeledArgument)
    {
        return (handler) => handler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(It.Is(MatchHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(syntacticUnlabeledArgument)));
    }

    private static Expression<Func<IHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand, bool>> MatchHandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
        AttributeArgumentSyntax syntacticUnlabeledArgument)
    {
        return (command) => Equals(syntacticUnlabeledArgument, command.SyntacticUnlabeledArgument);
    }

    private static Expression<Action<ICSharpAttributeConstructorAssociatorErrorHandler>> HandleUnrecognizedLabeledArgumentExpression(
        AttributeArgumentSyntax syntacticArgument)
    {
        return (handler) => handler.UnrecognizedLabeledArgument.Handle(It.Is(MatchHandleUnrecognizedLabeledArgumentCommand(syntacticArgument)));
    }

    private static Expression<Func<IHandleUnrecognizedLabeledArgumentCommand, bool>> MatchHandleUnrecognizedLabeledArgumentCommand(
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => Equals(syntacticArgument, command.SyntacticArgument);
    }

    private static Expression<Action<ICSharpAttributeConstructorAssociatorErrorHandler>> HandleDuplicateParameterNamesExpression(
        string parameterName)
    {
        return (handler) => handler.DuplicateParameterNames.Handle(It.Is(MatchHandleDuplicateParameterNamesCommand(parameterName)));
    }

    private static Expression<Func<IHandleDuplicateParameterNamesCommand, bool>> MatchHandleDuplicateParameterNamesCommand(
        string parameterName)
    {
        return (command) => Equals(parameterName, command.ParameterName);
    }

    private static Expression<Action<ICSharpAttributeConstructorAssociatorErrorHandler>> HandleDuplicateArgumentsExpression(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (handler) => handler.DuplicateArguments.Handle(It.Is(MatchHandleDuplicateArgumentsCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IHandleDuplicateArgumentsCommand, bool>> MatchHandleDuplicateArgumentsCommand(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && Equals(syntacticArgument, command.SyntacticArgument);
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
        IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
