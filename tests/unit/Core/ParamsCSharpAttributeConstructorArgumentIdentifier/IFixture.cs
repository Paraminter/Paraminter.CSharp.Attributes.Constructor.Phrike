namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Handlers;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> Sut { get; }
}
