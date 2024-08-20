namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> Sut { get; }
}
