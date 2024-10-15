namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Cqs;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

internal interface IFixture
{
    public abstract ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>> Sut { get; }

    public abstract Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> NormalPairerMock { get; }
    public abstract Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> ParamsPairerMock { get; }
    public abstract Mock<ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> DefaultPairerMock { get; }

    public abstract Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> ParamsArgumentDistinguisherMock { get; }

    public abstract Mock<ICSharpAttributeConstructorAssociatorErrorHandler> ErrorHandlerMock { get; }
}
