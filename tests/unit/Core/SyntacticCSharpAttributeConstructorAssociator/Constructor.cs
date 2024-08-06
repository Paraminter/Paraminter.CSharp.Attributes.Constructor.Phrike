namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;

using System;

using Xunit;

public sealed class Constructor
{
    [Fact]
    public void NullNormalRecorder_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            null!,
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsRecorder_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullDefaultRecorder_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullParamsArgumentIdentifier_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            null!,
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>()));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void NullInvalidator_ThrowsArgumentNullException()
    {
        var result = Record.Exception(() => Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            null!));

        Assert.IsType<ArgumentNullException>(result);
    }

    [Fact]
    public void ValidArguments_ReturnsAssociator()
    {
        var result = Target(
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>>(),
            Mock.Of<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>>(),
            Mock.Of<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>>());

        Assert.NotNull(result);
    }

    private static SyntacticCSharpAttributeConstructorAssociator Target(
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultRecorder,
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentIdentifier,
        ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator)
    {
        return new SyntacticCSharpAttributeConstructorAssociator(normalRecorder, paramsRecorder, defaultRecorder, paramsArgumentIdentifier, invalidator);
    }
}
