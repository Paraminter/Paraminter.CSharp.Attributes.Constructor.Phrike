namespace Paraminter.CSharp.Attributes.Constructor.Phrike;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal static class CompilationFactory
{
    private static readonly CSharpCompilation EmptyCompilation = CreateEmptyCompilation();

    private static readonly CSharpCompilationOptions CompilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

    public static CSharpCompilation Create(
        string source)
    {
        CSharpParseOptions parseOptions = new(languageVersion: LanguageVersion.CSharp12);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        return EmptyCompilation.AddSyntaxTrees(syntaxTree);
    }

    private static CSharpCompilation CreateEmptyCompilation() => CSharpCompilation.Create("TestAssembly", options: CompilationOptions);
}
