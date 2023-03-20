namespace CSharpier;

using System.Text;
using System.Text.Json;
using CSharpier.SyntaxPrinter;

internal class CSharpFormatter : IFormatter
{
    internal static readonly LanguageVersion LanguageVersion = LanguageVersion.Preview;

    Task<CodeFormatterResult> IFormatter.FormatAsync(
        string code,
        PrinterOptions printerOptions,
        CancellationToken cancellationToken
    )
    {
        return FormatAsync(code, printerOptions, cancellationToken);
    }

    internal static Task<CodeFormatterResult> FormatAsync(
        string code,
        PrinterOptions printerOptions
    ) => FormatAsync(code, printerOptions, CancellationToken.None);

    internal static Task<CodeFormatterResult> FormatAsync(
        string code,
        PrinterOptions printerOptions,
        CancellationToken cancellationToken
    )
    {
        // if a user supplied symbolSets, then we should start with the first one
        var initialSymbolSet = printerOptions.PreprocessorSymbolSets is { Count: > 0 }
            ? printerOptions.PreprocessorSymbolSets.First()
            : Array.Empty<string>();

        return FormatAsync(
            ParseText(code, initialSymbolSet, cancellationToken),
            printerOptions,
            cancellationToken
        );
    }

    private static SyntaxTree ParseText(
        string codeToFormat,
        string[] preprocessorSymbols,
        CancellationToken cancellationToken
    )
    {
        return CSharpSyntaxTree.ParseText(
            codeToFormat,
            new CSharpParseOptions(
                LanguageVersion,
                DocumentationMode.Diagnose,
                preprocessorSymbols: preprocessorSymbols
            ),
            cancellationToken: cancellationToken
        );
    }

    internal static async Task<CodeFormatterResult> FormatAsync(
        SyntaxTree syntaxTree,
        PrinterOptions printerOptions,
        CancellationToken cancellationToken
    )
    {
        var syntaxNode = await syntaxTree.GetRootAsync(cancellationToken);
        if (syntaxNode is not CompilationUnitSyntax rootNode)
        {
            throw new Exception(
                "Root was not CompilationUnitSyntax, it was " + syntaxNode.GetType()
            );
        }

        if (GeneratedCodeUtilities.BeginsWithAutoGeneratedComment(rootNode))
        {
            return new CodeFormatterResult { Code = syntaxTree.ToString() };
        }

        bool TryGetCompilationFailure(out CodeFormatterResult compilationResult)
        {
            var diagnostics = syntaxTree
                .GetDiagnostics(cancellationToken)
                .Where(o => o.Severity == DiagnosticSeverity.Error && o.Id != "CS1029")
                .ToList();
            if (diagnostics.Any())
            {
                compilationResult = new CodeFormatterResult
                {
                    Code = syntaxTree.ToString(),
                    CompilationErrors = diagnostics,
                    AST = printerOptions.IncludeAST ? PrintAST(rootNode) : string.Empty
                };

                return true;
            }

            compilationResult = CodeFormatterResult.Null;
            return false;
        }

        if (TryGetCompilationFailure(out var result))
        {
            return result;
        }

        try
        {
            if (printerOptions.PreprocessorSymbolSets is { Count: > 0 })
            {
                PreprocessorSymbols.StopCollecting();
                PreprocessorSymbols.SetSymbolSets(
                    // we already formatted with the first set above
                    printerOptions.PreprocessorSymbolSets.Skip(1).ToList()
                );
            }
            else
            {
                PreprocessorSymbols.Reset();
            }

            var lineEnding = PrinterOptions.GetLineEnding(syntaxTree.ToString(), printerOptions);
            var document = Node.Print(rootNode, new FormattingContext { LineEnding = lineEnding });
            var formattedCode = DocPrinter.DocPrinter.Print(document, printerOptions, lineEnding);

            PreprocessorSymbols.StopCollecting();
            foreach (var symbolSet in PreprocessorSymbols.GetSymbolSets())
            {
                syntaxTree = ParseText(formattedCode, symbolSet, cancellationToken);

                if (TryGetCompilationFailure(out result))
                {
                    return result;
                }

                document = Node.Print(
                    await syntaxTree.GetRootAsync(cancellationToken),
                    new FormattingContext()
                );
                formattedCode = DocPrinter.DocPrinter.Print(document, printerOptions, lineEnding);
            }

            return new CodeFormatterResult
            {
                Code = formattedCode,
                DocTree = printerOptions.IncludeDocTree
                    ? DocSerializer.Serialize(document)
                    : string.Empty,
                AST = printerOptions.IncludeAST ? PrintAST(rootNode) : string.Empty
            };
        }
        catch (InTooDeepException)
        {
            return new CodeFormatterResult
            {
                FailureMessage = "We can't handle this deep of recursion yet."
            };
        }
    }

    private static string PrintAST(CompilationUnitSyntax rootNode)
    {
        try
        {
            var stringBuilder = new StringBuilder();
            SyntaxNodeJsonWriter.WriteCompilationUnitSyntax(stringBuilder, rootNode);
            // SyntaxNodeJsonWriter doesn't write things indented, so this cleans it up for us
            return JsonSerializer.Serialize(
                JsonSerializer.Deserialize<object>(stringBuilder.ToString()),
                new JsonSerializerOptions { WriteIndented = false }
            );
        }
        // in some cases with new unsupported c# language features
        // SyntaxNodeJsonWriter will not produce valid json
        catch (JsonException ex)
        {
            return JsonSerializer.Serialize(
                new { exception = ex.ToString() },
                new JsonSerializerOptions { WriteIndented = false }
            );
        }
    }
}
