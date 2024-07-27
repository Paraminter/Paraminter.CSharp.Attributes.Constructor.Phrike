namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Associators.Queries;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Collectors;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> paramsArgumentIdentifierMock = new();

        SyntacticCSharpAttributeConstructorAssociator sut = new(paramsArgumentIdentifierMock.Object);

        return new Fixture(sut, paramsArgumentIdentifierMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector> Sut;

        private readonly Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> ParamsArgumentIdentifierMock;

        public Fixture(
            IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector> sut,
            Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> paramsArgumentIdentifierMock)
        {
            Sut = sut;

            ParamsArgumentIdentifierMock = paramsArgumentIdentifierMock;
        }

        IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector> IFixture.Sut => Sut;

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>>> IFixture.ParamsArgumentIdentifierMock => ParamsArgumentIdentifierMock;
    }
}
