namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> normalIndividualAssociatorMock = new();
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> paramsIndividualAssociatorMock = new();
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> defaultIndividualAssociatorMock = new();

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock = new();

        Mock<ICSharpAttributeConstructorAssociatorErrorHandler> errorHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        CSharpAttributeConstructorAssociator sut = new(normalIndividualAssociatorMock.Object, paramsIndividualAssociatorMock.Object, defaultIndividualAssociatorMock.Object, paramsArgumentDistinguisherMock.Object, errorHandlerMock.Object);

        return new Fixture(sut, normalIndividualAssociatorMock, paramsIndividualAssociatorMock, defaultIndividualAssociatorMock, paramsArgumentDistinguisherMock, errorHandlerMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData>> Sut;

        private readonly Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> NormalIndividualAssociatorMock;
        private readonly Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> ParamsIndividualAssociatorMock;
        private readonly Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> DefaultIndividualAssociatorMock;

        private readonly Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock;

        private readonly Mock<ICSharpAttributeConstructorAssociatorErrorHandler> ErrorHandlerMock;

        public Fixture(
            ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData>> sut,
            Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> normalIndividualAssociatorMock,
            Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> paramsIndividualAssociatorMock,
            Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> defaultIndividualAssocitorMock,
            Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock,
            Mock<ICSharpAttributeConstructorAssociatorErrorHandler> errorHandlerMock)
        {
            Sut = sut;

            NormalIndividualAssociatorMock = normalIndividualAssociatorMock;
            ParamsIndividualAssociatorMock = paramsIndividualAssociatorMock;
            DefaultIndividualAssociatorMock = defaultIndividualAssocitorMock;

            ParamsArgumentDistinguisherMock = paramsArgumentDistinguisherMock;

            ErrorHandlerMock = errorHandlerMock;
        }

        ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData>> IFixture.Sut => Sut;

        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> IFixture.NormalIndividualAssociatorMock => NormalIndividualAssociatorMock;
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> IFixture.ParamsIndividualAssociatorMock => ParamsIndividualAssociatorMock;
        Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> IFixture.DefaultIndividualAssociatorMock => DefaultIndividualAssociatorMock;

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> IFixture.ParamsArgumentDistinguisherMock => ParamsArgumentDistinguisherMock;

        Mock<ICSharpAttributeConstructorAssociatorErrorHandler> IFixture.ErrorHandlerMock => ErrorHandlerMock;
    }
}
