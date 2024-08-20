namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullNormalIndividualAssociator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            null!,
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsIndividualAssociator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDefaultIndividualAssociator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsArgumentDistinguisher_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullErrorHandler_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>());

        Assert.NotNull(result);
    }

    private static CSharpAttributeConstructorAssociator Target(
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalIndividualAssocitor,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsIndividualAssociator,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultIndividualAssociator,
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpAttributeConstructorAssociatorErrorHandler errorHandler)
    {
        return new CSharpAttributeConstructorAssociator(normalIndividualAssocitor, paramsIndividualAssociator, defaultIndividualAssociator, paramsArgumentDistinguisher, errorHandler);
    }
}
