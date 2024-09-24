namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Paraminter.Arguments.CSharp.Attributes.Constructor.Models;
using Paraminter.Associating.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Errors.Commands;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;
using Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Queries;
using Paraminter.Cqs.Handlers;
using Paraminter.Pairing.Commands;
using Paraminter.Parameters.Method.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Associates syntactic C# attribute constructor arguments with parameters.</summary>
public sealed class CSharpAttributeConstructorAssociator
    : ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>>
{
    private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> NormalPairer;
    private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> ParamsPairer;
    private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> DefaultPairer;

    private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

    private readonly ICSharpAttributeConstructorAssociatorErrorHandler ErrorHandler;

    /// <summary>Instantiates an associator of syntactic C# attribute constructor arguments with parameters.</summary>
    /// <param name="normalPairer">Pairs normal syntactic C# attribute constructor arguments with parameters.</param>
    /// <param name="paramsPairer">Pairs <see langword="params"/> syntactic C# attribute constructor arguments with parameters.</param>
    /// <param name="defaultPairer">Pairs default syntactic C# attribute constructor arguments with parameters.</param>
    /// <param name="paramsArgumentDistinguisher">Distinguishes between <see langword="params"/> and non-<see langword="params"/> syntactic C# attribute constructor arguments.</param>
    /// <param name="errorHandler">Handles encountered errors.</param>
    public CSharpAttributeConstructorAssociator(
        ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsPairer,
        ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultPairer,
        IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
        ICSharpAttributeConstructorAssociatorErrorHandler errorHandler)
    {
        NormalPairer = normalPairer ?? throw new ArgumentNullException(nameof(normalPairer));
        ParamsPairer = paramsPairer ?? throw new ArgumentNullException(nameof(paramsPairer));
        DefaultPairer = defaultPairer ?? throw new ArgumentNullException(nameof(defaultPairer));

        ParamsArgumentDistinguisher = paramsArgumentDistinguisher ?? throw new ArgumentNullException(nameof(paramsArgumentDistinguisher));

        ErrorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    void ICommandHandler<IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData>>.Handle(
        IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData> command)
    {
        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Associator.Associate(NormalPairer, ParamsPairer, DefaultPairer, ParamsArgumentDistinguisher, ErrorHandler, command);
    }

    private sealed class Associator
    {
        public static void Associate(
            ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultPairer,
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpAttributeConstructorAssociatorErrorHandler errorHandler,
            IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData> command)
        {
            var associator = new Associator(normalPairer, paramsPairer, defaultPairer, paramsArgumentDistinguisher, errorHandler, command);

            associator.Associate();
        }

        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> NormalPairer;
        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> ParamsPairer;
        private readonly ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> DefaultPairer;

        private readonly IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> ParamsArgumentDistinguisher;

        private readonly ICSharpAttributeConstructorAssociatorErrorHandler ErrorHandler;

        private readonly IAssociateCSharpAttributeConstructorArgumentsData UnassociatedInvocationData;

        private readonly IDictionary<string, ParameterStatus> ParametersByName;

        private bool HasEncounteredOutOfOrderLabelledArgument;
        private bool HasEncounteredParamsArgument;
        private bool HasEncounteredNamedArgument;

        private Associator(
            ICommandHandler<IPairArgumentCommand<IMethodParameter, INormalCSharpAttributeConstructorArgumentData>> normalPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IParamsCSharpAttributeConstructorArgumentData>> paramsPairer,
            ICommandHandler<IPairArgumentCommand<IMethodParameter, IDefaultCSharpAttributeConstructorArgumentData>> defaultPairer,
            IQueryHandler<IIsCSharpAttributeConstructorArgumentParamsQuery, bool> paramsArgumentDistinguisher,
            ICSharpAttributeConstructorAssociatorErrorHandler errorHandler,
            IAssociateArgumentsCommand<IAssociateCSharpAttributeConstructorArgumentsData> command)
        {
            NormalPairer = normalPairer;
            ParamsPairer = paramsPairer;
            DefaultPairer = defaultPairer;

            ParamsArgumentDistinguisher = paramsArgumentDistinguisher;

            ErrorHandler = errorHandler;

            UnassociatedInvocationData = command.Data;

            ParametersByName = new Dictionary<string, ParameterStatus>(command.Data.Parameters.Count, StringComparer.Ordinal);
        }

        private void Associate()
        {
            ResetParametersByNameDictionary();

            PairSpecifiedArguments();
            ValidateUnpairedArguments();
        }

        private void PairSpecifiedArguments()
        {
            var maximumNumberOfSpecifiedArguments = Math.Min(UnassociatedInvocationData.Parameters.Count, UnassociatedInvocationData.SyntacticArguments.Count);

            for (var i = 0; i < maximumNumberOfSpecifiedArguments; i++)
            {
                PairArgumentAtIndex(i);

                if (HasEncounteredParamsArgument || HasEncounteredNamedArgument)
                {
                    break;
                }
            }
        }

        private void ValidateUnpairedArguments()
        {
            var unpairedParameters = ParametersByName.Values.Where(static (parsableParameter) => parsableParameter.HasBeenPaired is false);

            foreach (var parameterSymbol in unpairedParameters.Select(static (parsableParameter) => parsableParameter.Symbol))
            {
                if (parameterSymbol.IsOptional)
                {
                    PairDefaultArgument(parameterSymbol);

                    continue;
                }

                if (parameterSymbol.IsParams)
                {
                    PairParamsArgument(parameterSymbol, []);

                    continue;
                }

                HandleMissingRequiredArgument(parameterSymbol);
            }
        }

        private void PairArgumentAtIndex(
            int index)
        {
            if (UnassociatedInvocationData.SyntacticArguments[index].NameEquals is not null)
            {
                HasEncounteredNamedArgument = true;

                return;
            }

            if (UnassociatedInvocationData.SyntacticArguments[index].NameColon is NameColonSyntax nameColonSyntax)
            {
                PairNameColonArgumentAtIndex(index, nameColonSyntax);

                return;
            }

            if (HasEncounteredOutOfOrderLabelledArgument)
            {
                HandleOutOfOrderLabeledArgumentFollowedByUnlabeledCommand(UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            if (UnassociatedInvocationData.Parameters[index].IsParams)
            {
                PairParamsParameterArgumentAtIndex(index);

                return;
            }

            PairNormalArgumentAtIndex(index);
        }

        private void PairNormalArgumentAtIndex(
            int index)
        {
            PairNormalArgument(UnassociatedInvocationData.Parameters[index], UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void PairNameColonArgumentAtIndex(
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

            if (parameterStatus.HasBeenPaired)
            {
                HandleDuplicateArgumentsCommand(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);

                return;
            }

            PairNormalArgument(parameterStatus.Symbol, UnassociatedInvocationData.SyntacticArguments[index]);
        }

        private void PairParamsParameterArgumentAtIndex(
            int index)
        {
            if (HasAtLeastConstructorArguments(index + 2))
            {
                var syntacticArguments = CollectSyntacticParamsArgument(index);

                PairParamsArgumentAtIndex(index, syntacticArguments);

                return;
            }

            if (IsParamsArgument(index) is false)
            {
                PairNormalArgumentAtIndex(index);

                return;
            }

            PairParamsArgumentAtIndex(index, [UnassociatedInvocationData.SyntacticArguments[index]]);
        }

        private void PairParamsArgumentAtIndex(
            int index,
            IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
        {
            PairParamsArgument(UnassociatedInvocationData.Parameters[index], syntacticArguments);
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

        private void PairNormalArgument(
            IParameterSymbol parameterSymbol,
            AttributeArgumentSyntax syntacticArgument)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new NormalCSharpAttributeConstructorArgumentData(syntacticArgument);

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            NormalPairer.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);
        }

        private void PairParamsArgument(
            IParameterSymbol parameterSymbol,
            IReadOnlyList<AttributeArgumentSyntax> syntacticArguments)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = new ParamsCSharpAttributeConstructorArgumentData(syntacticArguments);

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            ParamsPairer.Handle(command);

            ParametersByName[parameterSymbol.Name] = new ParameterStatus(parameterSymbol, true);

            HasEncounteredParamsArgument = true;
        }

        private void PairDefaultArgument(
            IParameterSymbol parameterSymbol)
        {
            var parameter = new MethodParameter(parameterSymbol);
            var argumentData = DefaultCSharpAttributeConstructorArgumentData.Instance;

            var command = PairArgumentCommandFactory.Create(parameter, argumentData);

            DefaultPairer.Handle(command);

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
            public bool HasBeenPaired { get; }

            public ParameterStatus(
                IParameterSymbol symbol,
                bool hasBeenParsed)
            {
                Symbol = symbol;
                HasBeenPaired = hasBeenParsed;
            }
        }
    }
}
