namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Recorders.Commands;

internal interface IFixture
{
    public abstract ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>> Sut { get; }

    public abstract Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>>> NormalRecorderMock { get; }
    public abstract Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>>> ParamsRecorderMock { get; }
    public abstract Mock<ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>>> DefaultRecorderMock { get; }

    public abstract Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool>> ParamsArgumentIdentifierMock { get; }

    public abstract Mock<ICommandHandler<IInvalidateArgumentAssociationsRecordCommand>> InvalidatorMock { get; }
}
