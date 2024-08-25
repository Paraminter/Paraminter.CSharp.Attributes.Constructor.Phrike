namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Commands;
using Paraminter.Cqs.Handlers;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Commands;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# attribute constructor arguments with parameters.</summary>
public sealed class CSharpAttributeConstructorAssociator
    : ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpAttributeConstructorArgumentsData>>
{
    private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> NormalIndividualAssociator;
    private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> ParamsIndividualAssociator;
    private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> DefaultIndividualAssociator;

    private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

    private readonly ICSharpAttributeConstructorAssociatorErrorHandler ErrorHandler;

    /// <summary>Instantiates an associator of syntactic C# attribute constructor arguments with parameters.</summary>
    /// <param name="normalIndividualAssociator">Associates individual normal syntactic C# attribute constructor arguments with parameters.</param>
    /// <param name="paramsIndividualAssociator">Associates individual <see langword="params"/> syntactic C# attribute constructor arguments with parameters.</param>
    /// <param name="defaultIndividualAssociator">Associates individual default syntactic C# attribute constructor arguments with parameters.</param>
    /// <param name="paramsArgumentDistinguisher">Distinguishes between <see langword="params"/> and non-<see langword="params"/> syntactic C# attribute constructor arguments.</param>
    /// <param name="errorHandler">Handles encountered errors.</param>
    public CSharpAttributeConstructorAssociator(
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalIndividualAssociator,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsIndividualAssociator,
        ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultIndividualAssociator,
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpAttributeConstructorAssociatorErrorHandler errorHandler)
    {
        NormalIndividualAssociator = normalIndividualAssociator ?? throw new ArgumentNullException(nameof(normalIndividualAssociator));
        ParamsIndividualAssociator = paramsIndividualAssociator ?? throw new ArgumentNullException(nameof(paramsIndividualAssociator));
        DefaultIndividualAssociator = defaultIndividualAssociator ?? throw new ArgumentNullException(nameof(defaultIndividualAssociator));

        ParamsArgumentDistinguisher = paramsArgumentDistinguisher ?? throw new ArgumentNullException(nameof(paramsArgumentDistinguisher));

        ErrorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    void ICommandHandler<IAssociateAllArgumentsCommand<IAssociateAllCSharpAttributeConstructorArgumentsData>>.Handle(
        IAssociateAllArgumentsCommand<IAssociateAllCSharpAttributeConstructorArgumentsData> command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Associator.Associate(NormalIndividualAssociator, ParamsIndividualAssociator, DefaultIndividualAssociator, ParamsArgumentDistinguisher, ErrorHandler, command);
    }

    private sealed class Associator
    {
        public static void Associate(
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultIndividualAssociator,
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpAttributeConstructorAssociatorErrorHandler errorHandler,
            IAssociateAllArgumentsCommand<IAssociateAllCSharpAttributeConstructorArgumentsData> command)
        {
            var associator = new Associator(normalIndividualAssociator, paramsIndividualAssociator, defaultIndividualAssociator, paramsArgumentDistinguisher, errorHandler, command);

            associator.Associate();
        }

        private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> NormalIndividualAssociator;
        private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> ParamsIndividualAssociator;
        private readonly ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> DefaultIndividualAssociator;

        private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

        private readonly ICSharpAttributeConstructorAssociatorErrorHandler ErrorHandler;

        private readonly IAssociateAllCSharpAttributeConstructorArgumentsData UnassociatedInvocationData;

        private readonly IDictionary<string, ParameterStatus> ParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredNamedArgument;

        private Associator(
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsIndividualAssociator,
            ICommandHandler<IAssociateSingleArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultIndividualAssociator,
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpAttributeConstructorAssociatorErrorHandler errorHandler,
            IAssociateAllArgumentsCommand<IAssociateAllCSharpAttributeConstructorArgumentsData> command)
        {
            NormalIndividualAssociator = normalIndividualAssociator;
            ParamsIndividualAssociator = paramsIndividualAssociator;
            DefaultIndividualAssociator = defaultIndividualAssociator;

            ParamsArgumentDistinguisher = paramsArgumentDistinguisher;

            ErrorHandler = errorHandler;

            UnassociatedInvocationData = command.Data;

            ParametersByName = new Dictionary<string, ParameterStatus>(command.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetParametersByNameDictionary();

            AssociateSpecifiedArguments();
            ValidateUnassociatedArguments();
        }

        private void AssociateSpecifiedArguments()
        {
            var maximumNumberOfSpecifiedArguments = Math.Min(UnassociatedInvocationData.Parameters.Count, UnassociatedInvocationData.SyntacticArguments.Count);

            for (var i = 0; i < maximumNumberOfSpecifiedArguments; i++)
            {
                AssociateArgumentAtIndex(i);

                if (HasEncounteredParamsArgument || HasEncounteredNamedArgument)
                {
                    break;
                }
            }
        }

        private void ValidateUnassociatedArguments()
        {
            var unassociatedParameters = ParametersByName.Values.Where(static (parsableParameter) => parsableParameter.HasBeenAssociated is false);

            foreach (var parameterSymbol in unassociatedParameters.Select(static (parsableParameter) => parsableParameter.Symbol))
            {
                if (parameterSymbol.IsOptional)
                {
                    AssociateDefaultArgument(parameterSymbol);

                    continue;
                }

                if (parameterSymbol.IsParams)
                {
                    AssociateParamsArgument(parameterSymbol, []);

                    continue;
                }

                HandleMissingRequiredArgument(parameterSymbol);
            }
        }

        private void AssociateArgumentAtIndex(
            int index)
        {
            if (UnassociatedInvocationData.SyntacticArguments[index].NameEquals is not null)
            {
                HasEncounteredNamedArgument = true;

                return;
            }

            if (UnassociatedInvocationData.SyntacticArguments[index].NameColon is NameColonSyntax nameColonSyntax)
            {
                AssociateNameColonArgumentAtIndex(index, nameColonSyntax);

                return;
            }

            if (HasEncounteredOutOfOrderLabelledArgument)
            {
                HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            if (UnassociatedInvocationData.Parameters[index].IsParams)
            {
                AssociateParamsParameterArgumentAtIndex(index);

                return;
            }

            AssociateNormalArgumentAtIndex(index);
        }

        private void AssociateNormalArgumentAtIndex(
            int index)
        {
            AssociateNormalArgument(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void AssociateNameColonArgumentAtIndex(
            int index,
            NameColonSyntax nameColonSyntax)
        {
            if (nameColonSyntax.Name.Identifier.Text != UnassociatedInvocationData.Parameters[index].Name)
            {
                HasEncounteredOutOfOrderLabelledArgument = true;
            }

            if (ParametersByName.TryGetValue(nameColonSyntax.Name.Identifier.Text, out var parameterStatus) is false)
            {
                HandleUnrecognizedLabeledArgumentCommand(UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            if (parameterStatus.HasBeenAssociated)
            {
                HandleDuplicateArgumentsCommand(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            AssociateNormalArgument(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void AssociateParamsParameterArgumentAtIndex(
            int index)
        {
            if (HasAtLeastConstructorArguments(index + 2))
            {
                var syntacticArguments = CollectSyntacticParamsArgument(index);

                AssociateParamsArgumentAtIndex(index, syntacticArguments);

                return;
            }

            if (IsParamsArgument(index) is false)
            {
                AssociateNormalArgumentAtIndex(index);

                return;
            }

            AssociateParamsArgumentAtIndex(index, [UnassociatedInvocationData.SyntacticArguments[index]]);
        }

        private void AssociateParamsArgumentAtIndex(
            int index,
            IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
        {
            AssociateParamsArgument(UnassociatedInvocationData.Parameters[index], syntacticArguments);
        }

        private void ResetParametersByNameDictionary()
        {
            ParametersByName.Clear();

            foreach (var parameterSymbol in UnassociatedInvocationData.Parameters)
            {
                if (ParametersByName.ContainsKey(parameterSymbol.Name))
                {
                    HandleDuplicateParameterNamesCommand(parameterSymbol.Name);

                    continue;
                }

                var parameterStatus = new ParameterStatus(parameterSymbol, false);

                ParametersByName.Add(parameterSymbol.Name, parameterStatus);
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

            return ParamsArgumentDistinguisher.Handle(query);
        }

        private void AssociateNormalArgument(
            IParameterSymbol parameterSymbol,
            AttributeArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new NormalCSharpAttributeConstructorArgumentData(syntacticArgument);

            var command = AssociateSingleArgumentCommandFactory.Create(parameter, argumentData);

            NormalIndividualAssociator.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private void AssociateParamsArgument(
            IParameterSymbol parameterSymbol,
            IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new ParamsCSharpAttributeConstructorArgumentData(syntacticArguments);

            var command = AssociateSingleArgumentCommandFactory.Create(parameter, argumentData);

            ParamsIndividualAssociator.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);

            HasEncounteredParamsArgument = true;
        }

        private void AssociateDefaultArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = DefaultCSharpAttributeConstructorArgumentData.Instance;

            var command = AssociateSingleArgumentCommandFactory.Create(parameter, argumentData);

            DefaultIndividualAssociator.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private void HandleMissingRequiredArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);

            var command = new HandleMissingRequiredArgumentCommand(parameter);

            ErrorHandler.MissingRequiredArgument.Handle(command);
        }

        private void HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(
            AttributeArgumentSyntax syntacticUnlabeledArgument)
        {
            var command = new HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(syntacticUnlabeledArgument);

            ErrorHandler.OutOfOrderLabeledArgumentFollowedByUnlabeled.Handle(command);
        }

        private void HandleUnrecognizedLabeledArgumentCommand(
            AttributeArgumentSyntax syntacticArgument)
        {
            var command = new HandleUnrecognizedLabeledArgumentCommand(syntacticArgument);

            ErrorHandler.UnrecognizedLabeledArgument.Handle(command);
        }

        private void HandleDuplicateParameterNamesCommand(
           string parameterName)
        {
            var command = new HandleDuplicateParameterNamesCommand(parameterName);

            ErrorHandler.DuplicateParameterNames.Handle(command);
        }

        private void HandleDuplicateArgumentsCommand(
            IParameterSymbol parameterSymbol,
            AttributeArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);

            var command = new HandleDuplicateArgumentsCommand(parameter, syntacticArgument);

            ErrorHandler.DuplicateArguments.Handle(command);
        }

        private readonly struct ParameterStatus
        {
            public IParameterSymbol Symbol { get; }
            public bool HasBeenAssociated { get; }

            public ParameterStatus(
                IParameterSymbol symbol,
                bool hasBeenParsed)
            {
                Symbol = symbol;
                HasBeenAssociated = hasBeenParsed;
            }
        }
    }
}
