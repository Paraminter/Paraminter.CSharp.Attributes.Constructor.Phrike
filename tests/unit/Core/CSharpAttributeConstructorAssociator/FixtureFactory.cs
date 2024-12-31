namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> normalPairerMock = new();
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> paramsPairerMock = new();
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> defaultPairerMock = new();

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock = new();

        Mock<ICSharpAttributeConstructorAssociatorErrorHandler> errorHandlerMock = new() { DefaultValue = DefaultValue.Mock };

        CSharpAttributeConstructorAssociator sut = new(normalPairerMock.Object, paramsPairerMock.Object, defaultPairerMock.Object, paramsArgumentDistinguisherMock.Object, errorHandlerMock.Object);

        return new Fixture(sut, normalPairerMock, paramsPairerMock, defaultPairerMock, paramsArgumentDistinguisherMock, errorHandlerMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> Sut;

        private readonly Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> NormalPairerMock;
        private readonly Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> ParamsPairerMock;
        private readonly Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> DefaultPairerMock;

        private readonly Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock;

        private readonly Mock<ICSharpAttributeConstructorAssociatorErrorHandler> ErrorHandlerMock;

        public Fixture(
            ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> sut,
            Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> normalPairerMock,
            Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> paramsPairerMock,
            Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> defaultPairerMock,
            Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> paramsArgumentDistinguisherMock,
            Mock<ICSharpAttributeConstructorAssociatorErrorHandler> errorHandlerMock)
        {
            Sut = sut;

            NormalPairerMock = normalPairerMock;
            ParamsPairerMock = paramsPairerMock;
            DefaultPairerMock = defaultPairerMock;

            ParamsArgumentDistinguisherMock = paramsArgumentDistinguisherMock;

            ErrorHandlerMock = errorHandlerMock;
        }

        ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> IFixture.Sut => Sut;

        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> IFixture.NormalPairerMock => NormalPairerMock;
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> IFixture.ParamsPairerMock => ParamsPairerMock;
        Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> IFixture.DefaultPairerMock => DefaultPairerMock;

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> IFixture.ParamsArgumentDistinguisherMock => ParamsArgumentDistinguisherMock;

        Mock<ICSharpAttributeConstructorAssociatorErrorHandler> IFixture.ErrorHandlerMock => ErrorHandlerMock;
    }
}
