namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Associators.Queries;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Common;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.CSharp.Attributes.Constructor.Queries.Handlers;
using Paraminter.Queries.Handlers;
using Paraminter.Queries.Values.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# attribute constructor arguments.</summary>
public sealed class SyntacticCSharpAttributeConstructorAssociator
    : IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler>
{
    private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> ParamsArgumentIdentifier;

    /// <summary>Instantiates a <see cref="SyntacticCSharpAttributeConstructorAssociator"/>, associating syntactic C# attribute constructor arguments.</summary>
    /// <param name="paramsArgumentIdentifier">Identifies <see langword="params"/> arguments.</param>
    public SyntacticCSharpAttributeConstructorAssociator(
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier)
    {
        ParamsArgumentIdentifier = paramsArgumentIdentifier ?? throw new ArgumentNullException(nameof(paramsArgumentIdentifier));
    }

    void IQueryHandler<IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData>, IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler>.Handle(
        IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
        IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler queryResponseHandler)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (queryResponseHandler is null)
        {
            throw new ArgumentNullException(nameof(queryResponseHandler));
        }

        Associator.Associate(ParamsArgumentIdentifier, query, queryResponseHandler);
    }

    private sealed class Associator
    {
        public static void Associate(
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
            IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler queryResponseHandler)
        {
            var associator = new Associator(paramsArgumentIdentifier, query, queryResponseHandler);

            associator.Associate();
        }

        private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> ParamsArgumentIdentifier;

        private readonly IAssociateSyntacticCSharpAttributeConstructorData UnassociatedInvocationData;
        private readonly IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler QueryResponseHandler;

        private readonly IDictionary<string, IParameterSymbol> UnparsedParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredNamedArgument;
        private bool HasEncounteredError;

        private Associator(
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, IValuedQueryResponseHandler<bool>> paramsArgumentIdentifier,
            IAssociateArgumentsQuery<IAssociateSyntacticCSharpAttributeConstructorData> query,
            IInvalidatingAssociateSyntacticCSharpAttributeConstructorQueryResponseHandler queryResponseHandler)
        {
            ParamsArgumentIdentifier = paramsArgumentIdentifier;
            UnassociatedInvocationData = query.Data;
            QueryResponseHandler = queryResponseHandler;

            UnparsedParametersByName = new Dictionary<string, IParameterSymbol>(query.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetUnparsedParametersByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnspecifiedArguments();

            if (HasEncounteredError)
            {
                QueryResponseHandler.Invalidator.Handle(InvalidateQueryResponseCommand.Instance);
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
                    var command = new AddDefaultCSharpAttributeConstructorCommand(parameter);

                    QueryResponseHandler.AssociationCollector.Default.Handle(command);

                    continue;
                }

                if (parameter.IsParams)
                {
                    var command = new AddParamsCSharpAttributeConstructorAssociationCommand(parameter, []);

                    QueryResponseHandler.AssociationCollector.Params.Handle(command);

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
            var command = new AddNormalCSharpAttributeConstructorAssociationCommand(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);

            QueryResponseHandler.AssociationCollector.Normal.Handle(command);
        }

        private void AssociateNameColonArgument(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (UnparsedParametersByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameter) is false)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParametersByName.Remove(nameColonSyntax.Name.Identifier.Text);

            var command = new AddNormalCSharpAttributeConstructorAssociationCommand(parameter, UnassociatedInvocationData.SyntacticArguments[index]);

            QueryResponseHandler.AssociationCollector.Normal.Handle(command);
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
            var command = new AddParamsCSharpAttributeConstructorAssociationCommand(UnassociatedInvocationData.Parameters[index], syntacticArguments);

            QueryResponseHandler.AssociationCollector.Params.Handle(command);

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
            var queryResponseHandler = new ValuedQueryResponseHandler<bool>();

            ParamsArgumentIdentifier.Handle(query, queryResponseHandler);

            if (queryResponseHandler.HasSetValue is false)
            {
                HasEncounteredError = true;

                return false;
            }

            return queryResponseHandler.GetValue();
        }
    }
}
