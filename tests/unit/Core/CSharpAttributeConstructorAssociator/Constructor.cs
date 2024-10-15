namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Cqs;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullNormalPairer_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            null!,
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsPairer_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDefaultPairer_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsArgumentDistinguisher_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullErrorHandler_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICSharpAttributeConstructorAssociatorErrorHandler>());

        Assert.NotNull(result);
    }

    private static CSharpAttributeConstructorAssociator Target(
        ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultPairer,
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpAttributeConstructorAssociatorErrorHandler errorHandler)
    {
        return new CSharpAttributeConstructorAssociator(normalPairer, paramsPairer, defaultPairer, paramsArgumentDistinguisher, errorHandler);
    }
}
