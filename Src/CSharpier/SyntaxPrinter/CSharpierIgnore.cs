using System.Text;
using System.Text.RegularExpressions;

namespace CSharpier.SyntaxPrinter;

internal static partial class CSharpierIgnore
{
#if NET8_0_OR_GREATER
    [GeneratedRegex("^// csharpier-ignore($| -)")]
    private static partial Regex IgnoreRegexGenerator();

    [GeneratedRegex("^// csharpier-ignore-start($| -)")]
    private static partial Regex IgnoreStartRegexGenerator();

    [GeneratedRegex("^// csharpier-ignore-end($| -)")]
    private static partial Regex IgnoreEndRegexGenerator();

    [GeneratedRegex(@"[\t\v\f ]*(\r\n?|\n)")]
    private static partial Regex WhiteSpaceLineEndingsGenerator();

    private static readonly Regex IgnoreRegex = IgnoreRegexGenerator();
    public static readonly Regex IgnoreStartRegex = IgnoreStartRegexGenerator();
    public static readonly Regex IgnoreEndRegex = IgnoreEndRegexGenerator();
    public static readonly Regex WhiteSpaceLineEndingsRegex = WhiteSpaceLineEndingsGenerator();
#else
    private static readonly Regex IgnoreRegex = new("^// csharpier-ignore($| -)");
    public static readonly Regex IgnoreStartRegex = new("^// csharpier-ignore-start($| -)");
    public static readonly Regex IgnoreEndRegex = new("^// csharpier-ignore-end($| -)");
    public static readonly Regex WhiteSpaceLineEndingsRegex = new(@"[\t\v\f ]*(\r\n?|\n)");
#endif

    public static bool HasIgnoreComment(SyntaxNode syntaxNode) =>
        Token.HasLeadingCommentMatching(syntaxNode, IgnoreRegex);

    public static bool HasIgnoreComment(SyntaxToken syntaxToken) =>
        Token.HasLeadingCommentMatching(syntaxToken, IgnoreRegex);

    public static bool IsNodeIgnored(SyntaxNode syntaxNode)
    {
        // this get handled in BaseMethodDeclaration and/or AttributeList
        if (
            syntaxNode is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax
            && baseMethodDeclarationSyntax.AttributeLists.Any()
        )
        {
            return false;
        }

        return syntaxNode.Parent
                is BaseTypeDeclarationSyntax
                    or BlockSyntax
                    or CompilationUnitSyntax
                    or NamespaceDeclarationSyntax
            && HasIgnoreComment(syntaxNode);
    }

    public static List<Doc> PrintNodesRespectingRangeIgnore<T>(
        SyntaxList<T> list,
        PrintingContext context
    )
        where T : SyntaxNode
    {
        var statements = new List<Doc>();
        var unFormattedCode = new StringBuilder();
        var printUnformatted = false;

        foreach (var node in list)
        {
            if (Token.HasLeadingCommentMatching(node, IgnoreEndRegex))
            {
                statements.Add(unFormattedCode.ToString().Trim());
                unFormattedCode.Clear();
                printUnformatted = false;
            }
            else if (Token.HasLeadingCommentMatching(node, IgnoreStartRegex))
            {
                printUnformatted = true;
            }

            if (printUnformatted)
            {
                unFormattedCode.Append(PrintWithoutFormatting(node, context));
            }
            else
            {
                statements.Add(Node.Print(node, context));
            }
        }

        if (unFormattedCode.Length > 0)
        {
            statements.Add(unFormattedCode.ToString().Trim());
        }

        return statements;
    }

    public static string PrintWithoutFormatting(SyntaxNode syntaxNode, PrintingContext context)
    {
        return PrintWithoutFormatting(syntaxNode.GetText().ToString(), context);
    }

    public static string PrintWithoutFormatting(string code, PrintingContext context)
    {
        // trim trailing whitespace + replace only existing line endings
        return WhiteSpaceLineEndingsRegex.Replace(code, context.Options.LineEnding);
    }
}
