namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associators.Commands;
using Paraminter.Commands.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Common;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;
using Paraminter.Queries.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# attribute constructor arguments.</summary>
public sealed class SyntacticCSharpAttributeConstructorAssociator
    : ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>>
{
    private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> NormalRecorder;
    private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> ParamsRecorder;
    private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> DefaultRecorder;

    private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> ParamsArgumentIdentifier;

    private readonly ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> Invalidator;

    /// <summary>Instantiates a <see cref="SyntacticCSharpAttributeConstructorAssociator"/>, associating syntactic C# attribute constructor arguments.</summary>
    /// <param name="normalRecorder">Record associated normal C# attribute constructor arguments.</param>
    /// <param name="paramsRecorder">Record associated <see langword="params"/> C# attribute constructor arguments.</param>
    /// <param name="defaultRecorder">Record associated default C# attribute constructor arguments.</param>
    /// <param name="paramsArgumentIdentifier">Identifies <see langword="params"/> arguments.</param>
    /// <param name="invalidator">Invalidates the record of associated syntactic C# attribute constructor arguments.</param>
    public SyntacticCSharpAttributeConstructorAssociator(
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsRecorder,
        ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultRecorder,
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentIdentifier,
        ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator)
    {
        NormalRecorder = normalRecorder ?? throw new ArgumentNullException(nameof(normalRecorder));
        ParamsRecorder = paramsRecorder ?? throw new ArgumentNullException(nameof(paramsRecorder));
        DefaultRecorder = defaultRecorder ?? throw new ArgumentNullException(nameof(defaultRecorder));

        ParamsArgumentIdentifier = paramsArgumentIdentifier ?? throw new ArgumentNullException(nameof(paramsArgumentIdentifier));

        Invalidator = invalidator ?? throw new ArgumentNullException(nameof(invalidator));
    }

    void ICommandHandler<IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData>>.Handle(
        IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData> command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Associator.Associate(NormalRecorder, ParamsRecorder, DefaultRecorder, ParamsArgumentIdentifier, Invalidator, command);
    }

    private sealed class Associator
    {
        public static void Associate(
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultRecorder,
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentIdentifier,
            ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator,
            IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData> command)
        {
            var associator = new Associator(normalRecorder, paramsRecorder, defaultRecorder, paramsArgumentIdentifier, invalidator, command);

            associator.Associate();
        }

        private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> NormalRecorder;
        private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> ParamsRecorder;
        private readonly ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> DefaultRecorder;

        private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> ParamsArgumentIdentifier;

        private readonly ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> Invalidator;

        private readonly IAssociateSyntacticCSharpAttributeConstructorData UnassociatedInvocationData;

        private readonly IDictionary<string, IParameterSymbol> UnparsedParameterSymbolsByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredNamedArgument;
        private bool HasEncounteredError;

        private Associator(
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsRecorder,
            ICommandHandler<IRecordArgumentAssociationCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultRecorder,
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentIdentifier,
            ICommandHandler<IInvalidateArgumentAssociationsRecordCommand> invalidator,
            IAssociateArgumentsCommand<IAssociateSyntacticCSharpAttributeConstructorData> command)
        {
            NormalRecorder = normalRecorder;
            ParamsRecorder = paramsRecorder;
            DefaultRecorder = defaultRecorder;

            ParamsArgumentIdentifier = paramsArgumentIdentifier;

            Invalidator = invalidator;

            UnassociatedInvocationData = command.Data;

            UnparsedParameterSymbolsByName = new Dictionary<string, IParameterSymbol>(command.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetUnparsedParameterSymbolsByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnspecifiedArguments();

            if (HasEncounteredError)
            {
                Invalidator.Handle(InvalidateArgumentAssociationsRecordCommand.Instance);
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
            foreach (var parameterSymbol in UnparsedParameterSymbolsByName.Values)
            {
                if (parameterSymbol.IsOptional)
                {
                    var parameter = new MethodParameter(parameterSymbol);

                    var recordCommand = RecordCSharpAttributeConstructorAssociationCommandFactory.Create(parameter, DefaultCSharpAttributeConstructorArgumentData.Instance);

                    DefaultRecorder.Handle(recordCommand);

                    continue;
                }

                if (parameterSymbol.IsParams)
                {
                    var parameter = new MethodParameter(parameterSymbol);
                    var argumentData = new ParamsCSharpAttributeConstructorArgumentData([]);

                    var recordCommand = RecordCSharpAttributeConstructorAssociationCommandFactory.Create(parameter, argumentData);

                    ParamsRecorder.Handle(recordCommand);

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

            UnparsedParameterSymbolsByName.Remove(UnassociatedInvocationData.Parameters[index].Name);

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
            var parameter = new MethodParameter(UnassociatedInvocationData.Parameters[index]);
            var argumentData = new NormalCSharpAttributeConstructorArgumentData(UnassociatedInvocationData.SyntacticArguments[index]);

            var recordCommand = RecordCSharpAttributeConstructorAssociationCommandFactory.Create(parameter, argumentData);

            NormalRecorder.Handle(recordCommand);
        }

        private void AssociateNameColonArgument(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (UnparsedParameterSymbolsByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameterSymbol) is false)
            {
                HasEncounteredError = true;

                return;
            }

            UnparsedParameterSymbolsByName.Remove(nameColonSyntax.Name.Identifier.Text);

            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new NormalCSharpAttributeConstructorArgumentData(UnassociatedInvocationData.SyntacticArguments[index]);

            var recordCommand = RecordCSharpAttributeConstructorAssociationCommandFactory.Create(parameter, argumentData);

            NormalRecorder.Handle(recordCommand);
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
            var parameter = new MethodParameter(UnassociatedInvocationData.Parameters[index]);
            var argumentData = new ParamsCSharpAttributeConstructorArgumentData(syntacticArguments);

            var recordCommand = RecordCSharpAttributeConstructorAssociationCommandFactory.Create(parameter, argumentData);

            ParamsRecorder.Handle(recordCommand);

            HasEncounteredParamsArgument = true;
        }

        private void ResetUnparsedParameterSymbolsByNameDictionary()
        {
            UnparsedParameterSymbolsByName.Clear();

            foreach (var parameterSymbol in UnassociatedInvocationData.Parameters)
            {
                if (UnparsedParameterSymbolsByName.ContainsKey(parameterSymbol.Name))
                {
                    HasEncounteredError = true;

                    return;
                }

                UnparsedParameterSymbolsByName.Add(parameterSymbol.Name, parameterSymbol);
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

            return ParamsArgumentIdentifier.Handle(query);
        }
    }
}
