namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;

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
    public void DuplicateParameter_Invalidates()
    {
        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void MissingRequiredArgument_Invalidates()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void OutOfOrderLabelledArgumentFollowedByUnlabelled_Invalidates()
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

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void MultipleArgumentsForParameter_Invalidates()
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

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void ArgumentForNonExistingParameter_Invalidates()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(null, SyntaxFactory.NameColon("_1"), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.AtLeastOnce());
    }

    [Fact]
    public void UnspecifiedOptionalArguments_NamedArguments_RecordsDefaultArguments()
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

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.DefaultRecorderMock.Verify(RecordDefaultExpression(parameter1SymbolMock.Object), Times.Once());
        Fixture.DefaultRecorderMock.Verify(RecordDefaultExpression(parameter2SymbolMock.Object), Times.Once());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Exactly(2));

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void ValidNormalArgument_RecordsNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void UnspecifiedParamsArgument_RecordsEmptyArray()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns([]);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameterSymbolMock.Object, []), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void MultipleParamsArguments_RecordsParamsArguments()
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

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameterSymbolMock.Object, syntacticArguments.Take(2).ToList()), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameters_IsParamsArgument_RecordsParamsArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(true);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.ParamsRecorderMock.Verify(RecordParamsExpression(parameterSymbolMock.Object, syntacticArguments), Times.Once());
        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_IsNotParamsArgument_AssociatesNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> commandMock = new();

        commandMock.Setup(static (command) => command.Data.Parameters).Returns([parameterSymbolMock.Object]);
        commandMock.Setup(static (command) => command.Data.SyntacticArguments).Returns(syntacticArguments);
        commandMock.Setup(static (command) => command.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel)).Returns(false);

        Target(commandMock.Object);

        Fixture.InvalidatorMock.Verify(static (invalidator) => invalidator.Handle(It.IsAny<IInvalidateArgumentAssociationsRecordCommand>()), Times.Never());

        Fixture.NormalRecorderMock.Verify(RecordNormalExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        Fixture.NormalRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>()), Times.Once());

        Fixture.ParamsRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>()), Times.Never());
        Fixture.DefaultRecorderMock.Verify(static (recorder) => recorder.Handle(It.IsAny<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>()), Times.Never());
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

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>> RecordNormalExpression(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordNormalCommand(parameterSymbol, syntacticArgument)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>, bool>> MatchRecordNormalCommand(
        IParameterSymbol parameterSymbol,
        AttributeArgumentSyntax syntacticArgument)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchNormalArgumentData(syntacticArgument, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>> RecordParamsExpression(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordParamsCommand(parameterSymbol, syntacticArguments)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>, bool>> MatchRecordParamsCommand(
        IParameterSymbol parameterSymbol,
        IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
    {
        return (command) => MatchParameter(parameterSymbol, command.Parameter) && MatchParamsArgumentData(syntacticArguments, command.ArgumentData);
    }

    private static Expression<Action<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>> RecordDefaultExpression(
        IParameterSymbol parameterSymbol)
    {
        return (recorder) => recorder.Handle(It.Is(MatchRecordDefaultCommand(parameterSymbol)));
    }

    private static Expression<Func<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>, bool>> MatchRecordDefaultCommand(
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
        IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData> command)
    {
        Fixture.Sut.Handle(command);
    }
}
