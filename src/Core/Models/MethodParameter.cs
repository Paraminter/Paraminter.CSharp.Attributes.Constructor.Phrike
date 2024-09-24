﻿namespace Paraminter.Associating.CSharp.Attributes.Constructor.Phrike.Models;

using Microsoft.CodeAnalysis;

using Paraminter.Parameters.Method.Models;

internal sealed class MethodParameter
    : IMethodParameter
{
    private readonly IParameterSymbol Symbol;

    public MethodParameter(
        IParameterSymbol symbol)
    {
        Symbol = symbol;
    }

    IParameterSymbol IMethodParameter.Symbol => Symbol;
}
