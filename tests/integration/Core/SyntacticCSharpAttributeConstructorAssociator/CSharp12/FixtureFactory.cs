namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Moq;

using Paraminter.Associators.Queries;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Handlers;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> paramsArgumentIdentifierMock = new();

        SyntacticCSharpAttributeConstructorAssociator sut = new(paramsArgumentIdentifierMock.Object);

        return new Fixture(sut, paramsArgumentIdentifierMock);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> Sut;

        private readonly Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> ParamsArgumentIdentifierMock;

        public Fixture(
            IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> sut,
            Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> paramsArgumentIdentifierMock)
        {
            Sut = sut;

            ParamsArgumentIdentifierMock = paramsArgumentIdentifierMock;
        }

        IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler> IFixture.Sut => Sut;

        Mock<IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>>> IFixture.ParamsArgumentIdentifierMock => ParamsArgumentIdentifierMock;
    }
}
