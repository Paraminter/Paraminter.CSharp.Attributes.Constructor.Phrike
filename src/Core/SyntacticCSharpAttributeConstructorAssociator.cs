namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Common;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Collectors;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Collectors;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# attribute constructor arguments.</summary>
public sealed class SyntacticCSharpAttributeConstructorAssociator
    : IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector>
{
    private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>> ParamsArgumentIdentifier;

    /// <summary>Instantiates a <see cref="SyntacticCSharpAttributeConstructorAssociator"/>, associating syntactic C# attribute constructor arguments.</summary>
    /// <param name="paramsArgumentIdentifier">Identifies <see langword="params"/> arguments.</param>
    public SyntacticCSharpAttributeConstructorAssociator(
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>> paramsArgumentIdentifier)
    {
        ParamsArgumentIdentifier = paramsArgumentIdentifier ?? throw new ArgumentNullException(nameof(paramsArgumentIdentifier));
    }

    void IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector>.Handle(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
        IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector queryResponseCollector)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseCollector is null)
        {
            throw new ArgumentNullException(nameof(queryResponseCollector));
        }

        Associator.Associate(ParamsArgumentIdentifier, query, queryResponseCollector);
    }

    private sealed class Associator
    {
        public static void Associate(
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>> isArgumentParamsQueryHandler,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
            IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector queryResponseCollector)
        {
            var associator = new Associator(isArgumentParamsQueryHandler, query, queryResponseCollector);

            associator.Associate();
        }

        private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>> IsArgumentParamsQueryHandler;

        private readonly IAssociateSyntacticCSharpAttributeConstructorData UnassociatedInvocationData;
        private readonly IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector QueryResponseCollector;

        private readonly IDictionary<string, IParameterSymbol> UnparsedParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredNamedArgument;
        private bool HasEncounteredError;

        private Associator(
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseCollector<bool>> isArgumentParamsQueryHandler,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
            IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseCollector queryResponseCollector)
        {
            IsArgumentParamsQueryHandler = isArgumentParamsQueryHandler;
            UnassociatedInvocationData = query.Data;
            QueryResponseCollector = queryResponseCollector;

            UnparsedParametersByName = new Dictionary<string, IParameterSymbol>(query.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetUnparsedParametersByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnspecifiedArguments();

            if (HasEncounteredError)
            {
                QueryResponseCollector.Invalidator.Invalidate();
            }
        }

        private void AssociateSpecifiedArguments()
        {
            if (HasEncounteredError)
            {
                return;
            }

            var maximumNumberOfSpecifiedArguments = Math.Min(UnassociatedInvocationData.Parameters.Count, UnassociatedInvocationData.SyntacticArguments.Count);

            for (var i = 0; i < maximumNumberOfSpecifiedArguments; i++)
            {
                AssociateArgument(i);

                if (HasEncounteredParamsArgument || HasEncounteredNamedArgument || HasEncounteredError)
                {
                    break;
                }
            }
        }

        private void ValidateUnspecifiedArguments()
        {
            foreach (var parameter in UnparsedParametersByName.Values)
            {
                if (parameter.IsOptional)
                {
                    QueryResponseCollector.Associations.Default.Add(parameter);

                    continue;
                }

                if (parameter.IsParams)
                {
                    QueryResponseCollector.Associations.Params.Add(parameter, []);

                    continue;
                }

                HasEncounteredError = true;

                return;
            }
        }

        private void AssociateArgument(
            int index)
        {
            if (UnassociatedInvocationData.SyntacticArguments[index].NameEquals is not null)
            {
                HasEncounteredNamedArgument = true;

                return;
            }

            if (UnassociatedInvocationData.SyntacticArguments[index].NameColon is NameColonSyntax nameColonSyntax)
            {
                AssociateNameColonArgument(index, nameColonSyntax);

                return;
            }

            if (HasEncounteredOutOfOrderLabelledArgument)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParametersByName.Remove(UnassociatedInvocationData.Parameters[index].Name);

            if (UnassociatedInvocationData.Parameters[index].IsParams)
            {
                AssociateParamsParameterArgument(index);

                return;
            }

            AssociateNormalArgument(index);
        }

        private void AssociateNormalArgument(
            int index)
        {
            QueryResponseCollector.Associations.Normal.Add(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void AssociateNameColonArgument(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (UnparsedParametersByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameterSymbol) is false)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParametersByName.Remove(nameColonSyntax.Name.Identifier.Text);

            QueryResponseCollector.Associations.Normal.Add(parameterSymbol, UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void AssociateParamsParameterArgument(
            int index)
        {
            if (HasAtLeastConstructorArguments(index + 2))
            {
                var syntacticArguments = CollectSyntacticParamsArgument(index);

                AssociateParamsArgument(index, syntacticArguments);

                return;
            }

            if (IsParamsArgument(index) is false)
            {
                AssociateNormalArgument(index);

                return;
            }

            AssociateParamsArgument(index, [UnassociatedInvocationData.SyntacticArguments[index]]);
        }

        private void AssociateParamsArgument(
            int index,
            IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
        {
            QueryResponseCollector.Associations.Params.Add(UnassociatedInvocationData.Parameters[index], syntacticArguments);

            HasEncounteredParamsArgument = true;
        }

        private void ResetUnparsedParametersByNameDictionary()
        {
            UnparsedParametersByName.Clear();

            foreach (var parameter in UnassociatedInvocationData.Parameters)
            {
                if (UnparsedParametersByName.ContainsKey(parameter.Name))
                {
                    HasEncounteredError = true;

                    return;
                }

                UnparsedParametersByName.Add(parameter.Name, parameter);
            }
        }

        private IReadOnlyList<AttributeArgumentSyntax> CollectSyntacticParamsArgument(
            int index)
        {
            var syntacticParamsArguments = new List<AttributeArgumentSyntax>(UnassociatedInvocationData.SyntacticArguments.Count - index);

            foreach (var syntacticArgument in UnassociatedInvocationData.SyntacticArguments.Skip(index))
            {
                if (syntacticArgument.NameEquals is not null)
                {
                    break;
                }

                syntacticParamsArguments.Add(syntacticArgument);
            }

            return syntacticParamsArguments;
        }

        private bool HasAtLeastConstructorArguments(
            int amount)
        {
            return UnassociatedInvocationData.SyntacticArguments.Count >= amount && UnassociatedInvocationData.SyntacticArguments[amount - 1].NameEquals is null;
        }

        private bool IsParamsArgument(
            int index)
        {
            var query = new IsCSharpAttributeConstructorArgumentParamsQuery(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index], UnassociatedInvocationData.SemanticModel);
            var queryResponseCollector = new ValuedQueryResponseCollector<bool>();

            IsArgumentParamsQueryHandler.Handle(query, queryResponseCollector);

            if (queryResponseCollector.HasSetValue is false)
            {
                HasEncounteredError = true;

                return false;
            }

            return queryResponseCollector.GetValue();
        }
    }
}
