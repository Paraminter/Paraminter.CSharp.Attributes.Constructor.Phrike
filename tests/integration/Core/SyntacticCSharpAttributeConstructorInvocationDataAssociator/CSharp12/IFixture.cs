namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Collectors;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

internal interface IFixture
{
    public abstract IQueryHandler<IAssociateArgumentsQuery<IUnassociatedSyntacticCSharpAttributeConstructorInvocationData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector> Sut { get; }

    public abstract Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> ParamsArgumentIdentifierMock { get; }
}
