namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Handlers;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

internal interface IFixture
{
    public abstract IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> Sut { get; }

    public abstract Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> ParamsArgumentIdentifierMock { get; }
}
