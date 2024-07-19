namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

internal interface IFixture
{
    public abstract IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>> Sut { get; }
}
