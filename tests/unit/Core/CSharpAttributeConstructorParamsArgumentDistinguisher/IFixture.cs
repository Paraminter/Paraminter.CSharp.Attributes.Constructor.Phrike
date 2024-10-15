namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Cqs;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> Sut { get; }
}
