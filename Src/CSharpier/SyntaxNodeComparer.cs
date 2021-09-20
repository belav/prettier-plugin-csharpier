using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CSharpier
{
    public partial class SyntaxNodeComparer
    {
        protected string OriginalSourceCode { get; }
        protected string NewSourceCode { get; }
        protected SyntaxTree OriginalSyntaxTree { get; }
        protected SyntaxTree NewSyntaxTree { get; }

        private static readonly CompareResult Equal = new();

        public SyntaxNodeComparer(
            string originalSourceCode,
            string newSourceCode,
            CancellationToken cancellationToken
        ) {
            this.OriginalSourceCode = originalSourceCode;
            this.NewSourceCode = newSourceCode;

            var cSharpParseOptions = new CSharpParseOptions(LanguageVersion.CSharp9);
            this.OriginalSyntaxTree = CSharpSyntaxTree.ParseText(
                this.OriginalSourceCode,
                cSharpParseOptions,
                cancellationToken: cancellationToken
            );
            this.NewSyntaxTree = CSharpSyntaxTree.ParseText(
                this.NewSourceCode,
                cSharpParseOptions,
                cancellationToken: cancellationToken
            );
        }

        public string CompareSource()
        {
            return this.CompareSourceAsync(CancellationToken.None).Result;
        }

        public async Task<string> CompareSourceAsync(CancellationToken cancellationToken)
        {
            var result = this.AreEqualIgnoringWhitespace(
                await OriginalSyntaxTree.GetRootAsync(cancellationToken),
                await NewSyntaxTree.GetRootAsync(cancellationToken)
            );

            if (!result.IsInvalid)
            {
                return string.Empty;
            }

            var message =
                $"----------------------------- Original: {GetLine(result.OriginalSpan, this.OriginalSyntaxTree, this.OriginalSourceCode)}";

            message +=
                $"----------------------------- Formatted: {GetLine(result.NewSpan, this.NewSyntaxTree, this.NewSourceCode)}";
            return message;
        }

        private static string GetLine(TextSpan? textSpan, SyntaxTree syntaxTree, string source)
        {
            if (!textSpan.HasValue)
            {
                return "Missing";
            }

            var line = syntaxTree.GetLineSpan(textSpan.Value).StartLinePosition.Line;
            var endLine = syntaxTree.GetLineSpan(textSpan.Value).EndLinePosition.Line;

            var result = $"Around Line {line} -----------------------------{Environment.NewLine}";

            var stringReader = new StringReader(source);
            var x = 0;
            var linesWritten = 0;
            var currentLine = stringReader.ReadLine();
            while (x <= endLine + 2 || linesWritten < 8)
            {
                if (x >= line - 2)
                {
                    result += currentLine + Environment.NewLine;
                    linesWritten++;
                }

                if (linesWritten > 15)
                {
                    break;
                }

                currentLine = stringReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }

                x++;
            }

            return result;
        }

        private readonly Stack<(SyntaxNode? node, SyntaxNode? parent)> originalStack = new();
        private readonly Stack<(SyntaxNode? node, SyntaxNode? parent)> formattedStack = new();

        private CompareResult AreEqualIgnoringWhitespace(
            SyntaxNode originalStart,
            SyntaxNode formattedStart
        ) {
            originalStack.Push((originalStart, originalStart));
            formattedStack.Push((formattedStart, formattedStart));
            while (originalStack.Count > 0)
            {
                var result = this.Compare(originalStack.Pop(), formattedStack.Pop());
                if (result.IsInvalid)
                {
                    return result;
                }
            }

            return Equal;
        }

        private CompareResult CompareLists<T>(
            IReadOnlyList<T> originalList,
            IReadOnlyList<T> formattedList,
            Func<T, T, CompareResult> comparer,
            Func<T, TextSpan> getSpan,
            TextSpan originalParentSpan,
            TextSpan newParentSpan
        ) {
            for (var x = 0; x < originalList.Count || x < formattedList.Count; x++)
            {
                if (x == originalList.Count)
                {
                    return NotEqual(originalParentSpan, getSpan(formattedList[x]));
                }

                if (x == formattedList.Count)
                {
                    return NotEqual(getSpan(originalList[x]), newParentSpan);
                }

                if (
                    originalList[x] is SyntaxNode originalNode
                    && formattedList[x] is SyntaxNode formattedNode
                ) {
                    originalStack.Push((originalNode, originalNode.Parent));
                    formattedStack.Push((formattedNode, formattedNode.Parent));
                }
                else
                {
                    var result = comparer(originalList[x], formattedList[x]);
                    if (result.IsInvalid)
                    {
                        return result;
                    }
                }
            }

            return Equal;
        }

        private static CompareResult NotEqual(SyntaxNode? originalNode, SyntaxNode? formattedNode)
        {
            return new()
            {
                IsInvalid = true,
                OriginalSpan = originalNode?.Span,
                NewSpan = formattedNode?.Span
            };
        }

        private static CompareResult NotEqual(TextSpan? originalSpan, TextSpan? formattedSpan)
        {
            return new() { IsInvalid = true, OriginalSpan = originalSpan, NewSpan = formattedSpan };
        }

        private CompareResult Compare(SyntaxToken originalToken, SyntaxToken formattedToken)
        {
            return Compare(originalToken, formattedToken, null, null);
        }

        private CompareResult Compare(
            SyntaxToken originalToken,
            SyntaxToken formattedToken,
            SyntaxNode? originalNode,
            SyntaxNode? formattedNode
        ) {
            // when a verbatim string contains mismatched line endings they will become consistent
            // this validation will fail unless we also get them consistent here
            // adding a semi-complicated if check to determine when to do the string replacement
            // did not appear to have any performance benefits
            if (originalToken.Text.Replace("\r", "") != formattedToken.Text.Replace("\r", ""))
            {
                return NotEqual(
                    originalToken.RawKind == 0 ? originalNode?.Span : originalToken.Span,
                    formattedToken.RawKind == 0 ? formattedNode?.Span : formattedToken.Span
                );
            }

            var result = this.Compare(originalToken.LeadingTrivia, formattedToken.LeadingTrivia);
            if (result.IsInvalid)
            {
                return result;
            }

            var result2 = this.Compare(originalToken.TrailingTrivia, formattedToken.TrailingTrivia);

            return result2.IsInvalid ? result2 : Equal;
        }

        private CompareResult Compare(SyntaxTrivia originalTrivia, SyntaxTrivia formattedTrivia)
        {
            if (originalTrivia.Kind() is SyntaxKind.DisabledTextTrivia)
            {
                return DisabledTextComparer.IsCodeBasicallyEqual(
                    originalTrivia.ToString(),
                    formattedTrivia.ToString()
                )
                    ? Equal
                    : NotEqual(originalTrivia.Span, formattedTrivia.Span);
            }

            return originalTrivia.ToString().TrimEnd() == formattedTrivia.ToString().TrimEnd()
                ? Equal
                : NotEqual(originalTrivia.Span, formattedTrivia.Span);
        }

        private CompareResult Compare(SyntaxTriviaList originalList, SyntaxTriviaList formattedList)
        {
            static SyntaxTrivia? FindNextSyntaxTrivia(SyntaxTriviaList list, ref int next)
            {
                SyntaxTrivia result;
                do
                {
                    if (next >= list.Count)
                    {
                        return null;
                    }

                    result = list[next];
                    next++;
                } while (
                    result.Kind() is SyntaxKind.EndOfLineTrivia or SyntaxKind.WhitespaceTrivia
                );

                return result;
            }

            var nextOriginal = 0;
            var nextFormatted = 0;
            var original = FindNextSyntaxTrivia(originalList, ref nextOriginal);
            var formatted = FindNextSyntaxTrivia(formattedList, ref nextFormatted);
            while (original != null && formatted != null)
            {
                var result = Compare(original.Value, formatted.Value);
                if (result.IsInvalid)
                {
                    return result;
                }

                original = FindNextSyntaxTrivia(originalList, ref nextOriginal);
                formatted = FindNextSyntaxTrivia(formattedList, ref nextFormatted);
            }

            if (original != formatted)
            {
                return NotEqual(originalList.Span, formattedList.Span);
            }

            return Equal;
        }
    }

    public struct CompareResult
    {
        public bool IsInvalid;
        public TextSpan? OriginalSpan;
        public TextSpan? NewSpan;
    }
}
