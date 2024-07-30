namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        ParamsCSharpAttributeConstructorArgumentIdentifier sut = new();

        return new Fixture(sut);
    }

    private sealed class Fixture
        : IFixture
    {
        private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> Sut;

        public Fixture(
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> sut)
        {
            Sut = sut;
        }

        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> IFixture.Sut => Sut;
    }
}
