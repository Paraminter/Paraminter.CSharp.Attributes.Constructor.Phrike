namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> Sut { get; }
}
