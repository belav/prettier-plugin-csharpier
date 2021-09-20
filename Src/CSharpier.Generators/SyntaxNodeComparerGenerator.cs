using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CSharpier.Generators
{
    [Generator]
    public class SyntaxNodeComparerGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        // this would probably be easier to understand as a scriban template but is a lot of effort
        // to switch and doesn't really change at this point
        public void Execute(GeneratorExecutionContext context)
        {
            var sourceText = SourceText.From(GenerateSource(), Encoding.UTF8);

            context.AddSource("SyntaxNodeComparer.generated", sourceText);
        }

        private string GenerateSource()
        {
            var sourceBuilder = new StringBuilder();
            sourceBuilder.AppendLine(
                @"#pragma warning disable CS0168
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier
{
    public partial class SyntaxNodeComparer
    {
        private CompareResult Compare(
            (SyntaxNode originalNode, SyntaxNode originalParent) original,
            (SyntaxNode formattedNode, SyntaxNode formattedParent) formatted
        )
        {
            var (originalNode, originalParent) = original;
            var (formattedNode, formattedParent) = formatted;
            if (originalNode == null && formattedNode == null)
            {
                return Equal;
            }

            if (originalNode == null || formattedNode == null)
            {
                return NotEqual(originalParent, formattedParent);
            }

            var type = originalNode?.GetType();
            if (type != formattedNode?.GetType())
            {
                return NotEqual(originalNode, formattedNode);
            }

            if (originalNode.RawKind != formattedNode.RawKind)
            {
                return NotEqual(originalNode, formattedNode);
            }

            switch (originalNode)
            {"
            );

            var syntaxNodeTypes = typeof(CompilationUnitSyntax).Assembly.GetTypes()
                .Where(o => !o.IsAbstract && typeof(CSharpSyntaxNode).IsAssignableFrom(o))
                .OrderBy(o => o.Name)
                .ToList();

            foreach (var syntaxNodeType in syntaxNodeTypes)
            {
                var lowerCaseName =
                    syntaxNodeType.Name[0].ToString().ToLower() + syntaxNodeType.Name[1..];
                sourceBuilder.AppendLine(
                    $@"                case {syntaxNodeType.Name} {lowerCaseName}:
                    return this.Compare{syntaxNodeType.Name}({lowerCaseName}, formattedNode as {syntaxNodeType.Name});"
                );
            }

            sourceBuilder.AppendLine(
                @"                default:
                    throw new Exception(""Can't handle "" + originalNode.GetType().Name);
            }
        }
        "
            );

            foreach (var syntaxNodeType in syntaxNodeTypes)
            {
                GenerateMethod(sourceBuilder, syntaxNodeType);
            }

            sourceBuilder.AppendLine(
                @"    }
}"
            );

            return sourceBuilder.ToString();
        }

        private static void GenerateMethod(StringBuilder sourceBuilder, Type type)
        {
            sourceBuilder.AppendLine(
                @$"        private CompareResult Compare{type.Name}({type.Name} originalNode, {type.Name} formattedNode)
        {{
            CompareResult result;"
            );

            foreach (var propertyInfo in type.GetProperties().OrderBy(o => o.Name))
            {
                var propertyName = propertyInfo.Name;

                if (
                    propertyName
                    is "Language"
                    or "Parent"
                    or "HasLeadingTrivia" // we modify/remove whitespace and new lines so we can't look at these properties.
                    or "HasTrailingTrivia"
                    or "ParentTrivia"
                    or "Arity"
                    or "SpanStart"
                ) {
                    continue;
                }

                var camelCaseName = CamelCaseName(propertyName);
                var propertyType = propertyInfo.PropertyType;

                if (
                    Ignored.Properties.Contains(camelCaseName)
                    || Ignored.Types.Contains(propertyType)
                    || (
                        Ignored.PropertiesByType.ContainsKey(type)
                        && Ignored.PropertiesByType[type].Contains(camelCaseName)
                    )
                ) {
                    continue;
                }

                if (propertyType == typeof(bool) || propertyType == typeof(Int32))
                {
                    sourceBuilder.AppendLine(
                        $@"            if (originalNode.{propertyName} != formattedNode.{propertyName}) return NotEqual(originalNode, formattedNode);"
                    );
                }
                else if (propertyType == typeof(SyntaxToken))
                {
                    sourceBuilder.AppendLine(
                        $"            result = this.Compare(originalNode.{propertyName}, formattedNode.{propertyName}, originalNode, formattedNode);"
                    );
                    sourceBuilder.AppendLine($"            if (result.IsInvalid) return result;");
                }
                else if (propertyType == typeof(SyntaxTrivia))
                {
                    sourceBuilder.AppendLine(
                        $"            result = this.Compare(originalNode.{propertyName}, formattedNode.{propertyName});"
                    );
                    sourceBuilder.AppendLine($"            if (result.IsInvalid) return result;");
                }
                else if (typeof(CSharpSyntaxNode).IsAssignableFrom(propertyType))
                {
                    sourceBuilder.AppendLine(
                        $"            originalStack.Push((originalNode.{propertyName}, originalNode));"
                    );
                    sourceBuilder.AppendLine(
                        $"            formattedStack.Push((formattedNode.{propertyName}, formattedNode));"
                    );
                }
                else if (
                    propertyType == typeof(SyntaxTokenList)
                    || (
                        propertyType.IsGenericType
                        && propertyType.GetGenericTypeDefinition() == typeof(SyntaxList<>)
                    )
                ) {
                    var compare = propertyType == typeof(SyntaxTokenList) ? "Compare" : "null";
                    sourceBuilder.AppendLine(
                        $"            result = this.CompareLists(originalNode.{propertyName}, formattedNode.{propertyName}, {compare}, o => o.Span, originalNode.Span, formattedNode.Span);"
                    );
                    sourceBuilder.AppendLine($"            if (result.IsInvalid) return result;");
                }
                else if (
                    propertyType.IsGenericType
                    && propertyType.GetGenericTypeDefinition() == typeof(SeparatedSyntaxList<>)
                ) {
                    sourceBuilder.AppendLine(
                        $"            result = this.CompareLists(originalNode.{propertyName}, formattedNode.{propertyName}, null, o => o.Span, originalNode.Span, formattedNode.Span);"
                    );
                    sourceBuilder.AppendLine($"            if (result.IsInvalid) return result;");

                    sourceBuilder.AppendLine(
                        $"            result = this.CompareLists(originalNode.{propertyName}.GetSeparators().ToList(), formattedNode.{propertyName}.GetSeparators().ToList(), Compare, o => o.Span, originalNode.Span, formattedNode.Span);"
                    );
                    sourceBuilder.AppendLine($"            if (result.IsInvalid) return result;");
                }
            }
            sourceBuilder.AppendLine("            return Equal;");
            sourceBuilder.AppendLine("        }");
        }

        private static string CamelCaseName(string name)
        {
            return name.ToLower()[0] + name[1..];
        }
    }
}
