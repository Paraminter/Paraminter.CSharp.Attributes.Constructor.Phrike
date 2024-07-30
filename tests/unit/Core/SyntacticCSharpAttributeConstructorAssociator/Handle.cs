namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Attributes.Constructor.Commands;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Handlers;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Invalidation.Commands;
using Paraminter.Queries.Values.Commands;
using Paraminter.Queries.Values.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

public sealed class Handle
{
    private readonly IFixture Fixture = FixtureFactory.Create();

    [Fact]
    public void NullQuery_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(null!, Mock.Of<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullQueryResponseHandler_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(Mock.Of<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>>(), null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void DuplicateParameter_InvalidatesResponse()
    {
        Mock<IParameterSymbol> parameter1SymbolMock = new();
        Mock<IParameterSymbol> parameter2SymbolMock = new();

        parameter1SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameter2SymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Once());
    }

    [Fact]
    public void MissingRequiredArgument_InvalidatesResponse()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Once());
    }

    [Fact]
    public void OutOfOrderLabelledArgumentFollowedByUnlabelled_InvalidatesResponse()
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Once());
    }

    [Fact]
    public void MultipleArgumentsForParameter_InvalidatesResponse()
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Once());
    }

    [Fact]
    public void ArgumentForNonExistingParameter_InvalidatesResponse()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(null, SyntaxFactory.NameColon("_1"), SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Once());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_UndeterminedIfParamsArgument_InvalidatesResponse()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpAttributeConstructorArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseHandler<bool>>()))
            .Callback<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>((query, queryResponseHandler) => { });

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Once());
    }

    [Fact]
    public void SingleArgumentOfParamsParameter_MakesCorrectQueryToIdentifier()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpAttributeConstructorArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseHandler<bool>>()))
            .Callback<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>((query, queryResponseHandler) => { });

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        Fixture.ParamsArgumentIdentifierMock.Verify(IsParamsExpression(parameterSymbolMock.Object, syntacticArguments[0], semanticModel), Times.Once());
    }

    [Fact]
    public void UnspecifiedOptionalArguments_NamedArguments_AssociatesDefaultArguments()
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameter1SymbolMock.Object, parameter2SymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(DefaultAssociationExpression(parameter1SymbolMock.Object), Times.Once());
        queryResponseHandlerMock.Verify(DefaultAssociationExpression(parameter2SymbolMock.Object), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Exactly(2));

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    [Fact]
    public void ValidNormalArgument_AssociatesNormalArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(false);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Once());

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    [Fact]
    public void UnspecifiedParamsArgument_AssociatesEmptyArray()
    {
        Mock<IParameterSymbol> parameterSymbolMock = new();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns([]);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(ParamsAssociationExpression(parameterSymbolMock.Object, []), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Once());

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    [Fact]
    public void MultipleParamsArguments_AssociatesParamsArguments()
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(ParamsAssociationExpression(parameterSymbolMock.Object, syntacticArguments.Take(2).ToList()), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Once());

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    [Fact]
    public void SingleArgumentOfParamsParameters_IsParamsArgument_AssociatesParamsArgument()
    {
        var syntacticArguments = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList([
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("42"))
        ])).Arguments;

        Mock<IParameterSymbol> parameterSymbolMock = new();

        var semanticModel = Mock.Of<SemanticModel>();

        parameterSymbolMock.Setup(static (symbol) => symbol.Name).Returns("Foo");
        parameterSymbolMock.Setup(static (symbol) => symbol.IsOptional).Returns(false);
        parameterSymbolMock.Setup(static (symbol) => symbol.IsParams).Returns(true);

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpAttributeConstructorArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseHandler<bool>>()))
            .Callback<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>((query, queryResponseHandler) => SetResponseValue(true, queryResponseHandler));

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(ParamsAssociationExpression(parameterSymbolMock.Object, syntacticArguments), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Once());

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
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

        Mock<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>> queryMock = new();
        Mock<IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> queryResponseHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        queryMock.Setup(static (query) => query.Data.Parameters).Returns([parameterSymbolMock.Object]);
        queryMock.Setup(static (query) => query.Data.SyntacticArguments).Returns(syntacticArguments);
        queryMock.Setup(static (query) => query.Data.SemanticModel).Returns(semanticModel);

        Fixture.ParamsArgumentIdentifierMock.Setup(static (identifier) => identifier.Handle(It.IsAny<IIsCSharpAttributeConstructorArgumentParamsQuery>(), It.IsAny<IValuedQueryResponseHandler<bool>>()))
            .Callback<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>((query, queryResponseHandler) => SetResponseValue(false, queryResponseHandler));

        Target(queryMock.Object, queryResponseHandlerMock.Object);

        queryResponseHandlerMock.Verify(NormalAssociationExpression(parameterSymbolMock.Object, syntacticArguments[0]), Times.Once());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Normal.Handle(It.IsAny<IAddNormalCSharpAttributeConstructorAssociationCommand>()), Times.Once());

        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Params.Handle(It.IsAny<IAddParamsCSharpAttributeConstructorAssociationCommand>()), Times.Never());
        queryResponseHandlerMock.Verify(static (handler) => handler.AssociationCollector.Default.Handle(It.IsAny<IAddDefaultCSharpAttributeConstructorCommand>()), Times.Never());

        queryResponseHandlerMock.Verify(static (handler) => handler.Invalidator.Handle(It.IsAny<IInvalidateQueryResponseCommand>()), Times.Never());
    }

    private static void SetResponseValue<TValue>(
        TValue value,
        IValuedQueryResponseHandler<TValue> queryResponseHandler)
    {
        Mock<ISetQueryResponseValueCommand<TValue>> commandMock = new();

        commandMock.Setup(static (command) => command.Value).Returns(value);

        queryResponseHandler.Value.Handle(commandMock.Object);
    }

    private static Expression<Action<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>>> IsParamsExpression(
        IParameterSymbol parameter,
        AttributeArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        return (handler) => handler.Handle(It.Is(MatchIsParamsQuery(parameter, argument, semanticModel)), It.IsAny<IValuedQueryResponseHandler<bool>>());
    }

    private static Expression<Func<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> MatchIsParamsQuery(
        IParameterSymbol parameter,
        AttributeArgumentSyntax argument,
        SemanticModel semanticModel)
    {
        return (query) => ReferenceEquals(query.Parameter, parameter) && ReferenceEquals(query.SyntacticArgument, argument) && ReferenceEquals(query.SemanticModel, semanticModel);
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
