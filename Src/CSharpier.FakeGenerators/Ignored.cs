using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CSharpier.FakeGenerators;

public static class Ignored
{
    public static readonly string[] Properties =
    {
        "language",
        "parent",
        "parentTrivia",
        "spanStart",
        "rawKind",
        "containsDiagnostics",
        "containsDirectives",
        "isStructuredTrivia",
        "hasStructuredTrivia",
        "containsSkippedText",
        "containsAnnotations"
    };

    public static readonly Type[] Types = { typeof(TextSpan), typeof(SyntaxTree) };

    public static readonly Dictionary<Type, string[]> PropertiesByType =
        new()
        {
            { typeof(PropertyDeclarationSyntax), new[] { "semicolon" } },
            { typeof(IndexerDeclarationSyntax), new[] { "semicolon" } },
            { typeof(SyntaxTrivia), new[] { "token" } },
            { typeof(SyntaxToken), new[] { "value", "valueText" } },
            { typeof(ParameterSyntax), new[] { "exclamationExclamationToken" } },
            // TODO 11 what are these? why do they makes things fail to compile?
            // maybe this?? https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%207#checked-operators-on-systemintptr-and-systemuintptr
            { typeof(ConversionOperatorDeclarationSyntax), new[] { "checkedKeyword" } },
            { typeof(OperatorDeclarationSyntax), new[] { "checkedKeyword" } },
            { typeof(ConversionOperatorMemberCrefSyntax), new[] { "checkedKeyword" } },
            { typeof(OperatorMemberCrefSyntax), new[] { "checkedKeyword" } }
        };

    public static readonly HashSet<string> UnsupportedNodes =
        new()
        {
            // if new versions of c# add node types, we need to ignore them in the generators
            // until codeAnalysis + sdks are updated
            // global.json doesn't seem to always be respected for builds (namely VS but rider started having the same problem)
            // which causes the generators to generate code for the new node types
            // but then the build fails because those types don't exist in the packages the actual project references
            // "ListPatternSyntax", "SlicePatternSyntax"
        };
}
