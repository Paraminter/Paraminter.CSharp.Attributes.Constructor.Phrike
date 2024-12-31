namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;

internal static class FixtureFactory
{
    public static IFixture Create()
    {
        CSharpAttributeConstructorParamsArgumentDistinguisher sut = new();

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
