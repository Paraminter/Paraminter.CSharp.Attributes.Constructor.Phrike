namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;

internal interface IFixture
{
    public abstract ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllSyntacticCSharpAttributeConstructorArgumentsData>> Sut { get; }

    public abstract Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> NormalIndividualAssociatorMock { get; }
    public abstract Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> ParamsIndividualAssociatorMock { get; }
    public abstract Mock<ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> DefaultIndividualAssociatorMock { get; }

    public abstract Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock { get; }

    public abstract Mock<ICSharpAttributeConstructorAssociatorErrorHandler> ErrorHandlerMock { get; }
}
