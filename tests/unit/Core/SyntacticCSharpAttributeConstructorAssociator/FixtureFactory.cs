namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;
using Paraminter.Recorders.Commands;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> normalRecorderMock = new();
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> paramsRecorderMock = new();
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> defaultRecorderMock = new();

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> paramsArgumentIdentifierMock = new();

        Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> invalidatorMock = new();

        SyntacticCSharpAttributeConstructorAssociator sut = new(normalRecorderMock.Object, paramsRecorderMock.Object, defaultRecorderMock.Object, paramsArgumentIdentifierMock.Object, invalidatorMock.Object);

        return new Fixture(sut, normalRecorderMock, paramsRecorderMock, defaultRecorderMock, paramsArgumentIdentifierMock, invalidatorMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> Sut;

        private readonly Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> NormalRecorderMock;
        private readonly Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> ParamsRecorderMock;
        private readonly Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> DefaultRecorderMock;

        private readonly Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> ParamsArgumentIdentifierMock;

        private readonly Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> InvalidatorMock;

        public Fixture(
            ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> sut,
            Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> normalRecorderMock,
            Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> paramsRecorderMock,
            Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> defaultRecorderMock,
            Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> paramsArgumentIdentifierMock,
            Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> invalidatorMock)
        {
            Sut = sut;

            NormalRecorderMock = normalRecorderMock;
            ParamsRecorderMock = paramsRecorderMock;
            DefaultRecorderMock = defaultRecorderMock;

            ParamsArgumentIdentifierMock = paramsArgumentIdentifierMock;

            InvalidatorMock = invalidatorMock;
        }

        ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> IFixture.Sut => Sut;

        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> IFixture.NormalRecorderMock => NormalRecorderMock;
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> IFixture.ParamsRecorderMock => ParamsRecorderMock;
        Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> IFixture.DefaultRecorderMock => DefaultRecorderMock;

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> IFixture.ParamsArgumentIdentifierMock => ParamsArgumentIdentifierMock;

        Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> IFixture.InvalidatorMock => InvalidatorMock;
    }
}
