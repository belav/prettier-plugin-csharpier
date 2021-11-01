using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpier.SyntaxPrinter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace CSharpier
{
    public class CodeFormatter
    {
        public static string Format(string code, CodeFormatterOptions options)
        {
            return FormatAsync(code, options).Result;
        }

        public static async Task<string> FormatAsync(
            string code,
            CodeFormatterOptions options,
            CancellationToken cancellationToken = default
        )
        {
            var result = await FormatAsync(
                code,
                new PrinterOptions { Width = options.Width },
                cancellationToken
            );
            return result.Code;
        }

        internal static CSharpierResult Format(string code, PrinterOptions printerOptions)
        {
            return FormatAsync(code, printerOptions, CancellationToken.None).Result;
        }

        internal static async Task<CSharpierResult> FormatAsync(
            string code,
            PrinterOptions printerOptions,
            CancellationToken cancellationToken
        )
        {
            SyntaxTree ParseText(string codeToFormat, params string[] preprocessorSymbols)
            {
                return CSharpSyntaxTree.ParseText(
                    codeToFormat,
                    new CSharpParseOptions(
                        LanguageVersion.CSharp9,
                        DocumentationMode.Diagnose,
                        preprocessorSymbols: preprocessorSymbols
                    ),
                    cancellationToken: cancellationToken
                );
            }

            // if a user supplied symbolSets, then we should start with the first one
            var initialSymbolSet = printerOptions.PreprocessorSymbolSets is { Count: > 0 }
                ? printerOptions.PreprocessorSymbolSets.First()
                : Array.Empty<string>();

            var syntaxTree = ParseText(code, initialSymbolSet);
            var syntaxNode = await syntaxTree.GetRootAsync(cancellationToken);
            if (syntaxNode is not CompilationUnitSyntax rootNode)
            {
                throw new Exception(
                    "Root was not CompilationUnitSyntax, it was " + syntaxNode.GetType()
                );
            }

            if (GeneratedCodeUtilities.BeginsWithAutoGeneratedComment(rootNode))
            {
                return new CSharpierResult { Code = code };
            }

            bool TryGetCompilationFailure(out CSharpierResult compilationResult)
            {
                var diagnostics = syntaxTree!
                    .GetDiagnostics(cancellationToken)
                    .Where(o => o.Severity == DiagnosticSeverity.Error && o.Id != "CS1029")
                    .ToList();
                if (diagnostics.Any())
                {
                    compilationResult = new CSharpierResult
                    {
                        Code = code,
                        Errors = diagnostics,
                        AST = printerOptions.IncludeAST ? PrintAST(rootNode) : string.Empty
                    };

                    return true;
                }

                compilationResult = CSharpierResult.Null;
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

                var document = Node.Print(rootNode);
                var lineEnding = GetLineEnding(code, printerOptions);
                var formattedCode = DocPrinter.DocPrinter.Print(
                    document,
                    printerOptions,
                    lineEnding
                );

                PreprocessorSymbols.StopCollecting();
                foreach (var symbolSet in PreprocessorSymbols.GetSymbolSets())
                {
                    syntaxTree = ParseText(formattedCode, symbolSet);

                    if (TryGetCompilationFailure(out result))
                    {
                        return result;
                    }

                    document = Node.Print(await syntaxTree.GetRootAsync(cancellationToken));
                    formattedCode = DocPrinter.DocPrinter.Print(
                        document,
                        printerOptions,
                        lineEnding
                    );
                }

                return new CSharpierResult
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
                return new CSharpierResult
                {
                    FailureMessage = "We can't handle this deep of recursion yet."
                };
            }
        }

        internal static string GetLineEnding(string code, PrinterOptions printerOptions)
        {
            if (printerOptions.EndOfLine == EndOfLine.Auto)
            {
                var lineIndex = code.IndexOf('\n');
                if (lineIndex <= 0)
                {
                    return "\n";
                }
                if (code[lineIndex - 1] == '\r')
                {
                    return "\r\n";
                }

                return "\n";
            }

            return printerOptions.EndOfLine == EndOfLine.CRLF ? "\r\n" : "\n";
        }

        private static string PrintAST(CompilationUnitSyntax rootNode)
        {
            var stringBuilder = new StringBuilder();
            SyntaxNodeJsonWriter.WriteCompilationUnitSyntax(stringBuilder, rootNode);
            return JsonConvert.SerializeObject(
                JsonConvert.DeserializeObject(stringBuilder.ToString()),
                Formatting.Indented
            );
        }
    }

    internal class CSharpierResult
    {
        public string Code { get; init; } = string.Empty;
        public string DocTree { get; init; } = string.Empty;
        public string AST { get; init; } = string.Empty;
        public IEnumerable<Diagnostic> Errors { get; init; } = Enumerable.Empty<Diagnostic>();

        public string FailureMessage { get; init; } = string.Empty;

        public static readonly CSharpierResult Null = new();
    }
}
