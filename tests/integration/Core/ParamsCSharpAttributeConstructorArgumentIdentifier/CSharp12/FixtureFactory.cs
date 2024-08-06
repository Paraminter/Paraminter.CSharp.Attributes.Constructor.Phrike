namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Handlers;

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
        private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> Sut;

        public Fixture(
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> sut)
        {
            Sut = sut;
        }

        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> IFixture.Sut => Sut;
    }
}
