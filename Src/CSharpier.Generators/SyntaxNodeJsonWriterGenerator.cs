using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CSharpier.Generators
{
    [Generator]
    public class SyntaxNodeJsonWriterGenerator : ISourceGenerator
    {
        readonly List<string> missingTypes = new();

        public void Initialize(GeneratorInitializationContext context) { }

        // this would probably be easier to understand as a scriban template but is a lot of effort
        // to switch and doesn't really change at this point
        public void Execute(GeneratorExecutionContext context)
        {
            var sourceText = SourceText.From(GenerateSource(), Encoding.UTF8);

            context.AddSource("SyntaxNodeJsonWriter.generated", sourceText);
        }

        private string GenerateSource()
        {
            var syntaxNodeTypes = typeof(CompilationUnitSyntax).Assembly.GetTypes()
                .Where(o => !o.IsAbstract && typeof(CSharpSyntaxNode).IsAssignableFrom(o))
                .OrderBy(o => o.Name)
                .ToList();

            var sourceBuilder = new StringBuilder();
            sourceBuilder.AppendLine("using System.Collections.Generic;");
            sourceBuilder.AppendLine("using System.IO;");
            sourceBuilder.AppendLine("using System.Linq;");
            sourceBuilder.AppendLine("using System.Text;");
            sourceBuilder.AppendLine("using Microsoft.CodeAnalysis;");
            sourceBuilder.AppendLine("using Microsoft.CodeAnalysis.CSharp;");
            sourceBuilder.AppendLine("using Microsoft.CodeAnalysis.CSharp.Syntax;");
            sourceBuilder.AppendLine();
            sourceBuilder.AppendLine("namespace CSharpier");
            sourceBuilder.AppendLine("{");
            sourceBuilder.AppendLine("    public partial class SyntaxNodeJsonWriter");
            sourceBuilder.AppendLine("    {");

            sourceBuilder.AppendLine(
                $"        public static void WriteSyntaxNode(StringBuilder builder, SyntaxNode syntaxNode)"
            );
            sourceBuilder.AppendLine("        {");
            foreach (var syntaxNodeType in syntaxNodeTypes)
            {
                sourceBuilder.AppendLine(
                    $"            if (syntaxNode is {syntaxNodeType.Name}) Write{syntaxNodeType.Name}(builder, syntaxNode as {syntaxNodeType.Name});"
                );
            }

            sourceBuilder.AppendLine("        }");
            sourceBuilder.AppendLine();

            foreach (var syntaxNodeType in syntaxNodeTypes)
            {
                GenerateMethod(sourceBuilder, syntaxNodeType);
            }

            GenerateMethod(sourceBuilder, typeof(SyntaxToken));
            GenerateMethod(sourceBuilder, typeof(SyntaxTrivia));

            sourceBuilder.AppendLine("    }");
            sourceBuilder.AppendLine("}");

            if (missingTypes.Any())
            {
                throw new Exception(
                    Environment.NewLine + string.Join(Environment.NewLine, missingTypes)
                );
            }

            return sourceBuilder.ToString();
        }

        private void GenerateMethod(StringBuilder sourceBuilder, Type type)
        {
            sourceBuilder.AppendLine(
                $"        public static void Write{type.Name}(StringBuilder builder, {type.Name} syntaxNode)"
            );
            sourceBuilder.AppendLine("        {");
            sourceBuilder.AppendLine("            builder.Append(\"{\");");
            sourceBuilder.AppendLine("            var properties = new List<string>();");
            sourceBuilder.AppendLine(
                $"            properties.Add($\"\\\"nodeType\\\":\\\"{{GetNodeType(syntaxNode.GetType())}}\\\"\");"
            );
            sourceBuilder.AppendLine(
                $"            properties.Add($\"\\\"kind\\\":\\\"{{syntaxNode.Kind().ToString()}}\\\"\");"
            );

            if (type == typeof(SyntaxTrivia))
            {
                sourceBuilder.AppendLine(
                    $"            properties.Add(WriteString(\"text\", syntaxNode.ToString()));"
                );
            }

            foreach (var propertyInfo in type.GetProperties().OrderBy(o => o.Name))
            {
                var propertyName = propertyInfo.Name;
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

                if (propertyType == typeof(bool))
                {
                    sourceBuilder.AppendLine(
                        $"            properties.Add(WriteBoolean(\"{camelCaseName}\", syntaxNode.{propertyName}));"
                    );
                }
                else if (propertyType == typeof(string))
                {
                    sourceBuilder.AppendLine(
                        $"            properties.Add(WriteString(\"{camelCaseName}\", syntaxNode.{propertyName}));"
                    );
                }
                else if (propertyType == typeof(int))
                {
                    sourceBuilder.AppendLine(
                        $"            properties.Add(WriteInt(\"{camelCaseName}\", syntaxNode.{propertyName}));"
                    );
                }
                else if (
                    typeof(CSharpSyntaxNode).IsAssignableFrom(propertyType)
                    || propertyType == typeof(SyntaxToken)
                    || propertyType == typeof(SyntaxTrivia)
                ) {
                    var methodName = "WriteSyntaxNode";
                    if (propertyType == typeof(SyntaxToken))
                    {
                        methodName = "WriteSyntaxToken";
                    }
                    else if (propertyType == typeof(SyntaxTrivia))
                    {
                        methodName = "WriteSyntaxTrivia";
                    }
                    else if (!propertyType.IsAbstract)
                    {
                        methodName = "Write" + propertyType.Name;
                    }

                    sourceBuilder.AppendLine(
                        $"            if (syntaxNode.{propertyName} != default({propertyType.Name}))"
                    );
                    sourceBuilder.AppendLine("            {");
                    sourceBuilder.AppendLine(
                        $"                var {camelCaseName}Builder = new StringBuilder();"
                    );
                    sourceBuilder.AppendLine(
                        $"                {methodName}({camelCaseName}Builder, syntaxNode.{propertyName});"
                    );
                    sourceBuilder.AppendLine(
                        $"                properties.Add($\"\\\"{camelCaseName}\\\":{{{camelCaseName}Builder.ToString()}}\");"
                    );
                    sourceBuilder.AppendLine("            }");
                }
                else if (
                    (
                        propertyType.IsGenericType
                        && (
                            propertyType.GetGenericTypeDefinition() == typeof(SyntaxList<>)
                            || propertyType.GetGenericTypeDefinition()
                                == typeof(SeparatedSyntaxList<>)
                        )
                    )
                    || propertyType == typeof(SyntaxTokenList)
                    || propertyType == typeof(SyntaxTriviaList)
                ) {
                    var methodName = "WriteSyntaxNode";
                    if (propertyType == typeof(SyntaxTokenList))
                    {
                        methodName = "WriteSyntaxToken";
                    }
                    else if (propertyType == typeof(SyntaxTriviaList))
                    {
                        methodName = "WriteSyntaxTrivia";
                    }
                    else
                    {
                        var genericArgument = propertyType.GetGenericArguments()[0];
                        if (!genericArgument.IsAbstract)
                        {
                            methodName = "Write" + genericArgument.Name;
                        }
                    }

                    sourceBuilder.AppendLine(
                        $"            var {camelCaseName} = new List<string>();"
                    );
                    sourceBuilder.AppendLine(
                        $"            foreach(var node in syntaxNode.{propertyName})"
                    );
                    sourceBuilder.AppendLine("            {");
                    sourceBuilder.AppendLine(
                        $"                var innerBuilder = new StringBuilder();"
                    );
                    sourceBuilder.AppendLine($"                {methodName}(innerBuilder, node);");
                    sourceBuilder.AppendLine(
                        $"                {camelCaseName}.Add(innerBuilder.ToString());"
                    );
                    sourceBuilder.AppendLine("            }");
                    sourceBuilder.AppendLine(
                        $"            properties.Add($\"\\\"{camelCaseName}\\\":[{{string.Join(\",\", {camelCaseName})}}]\");"
                    );
                }
                else
                {
                    missingTypes.Add(
                        PadToSize(type.Name + "." + propertyName + ": ", 40) + propertyType
                    );
                }
            }

            sourceBuilder.AppendLine(
                "            builder.Append(string.Join(\",\", properties.Where(o => o != null)));"
            );
            sourceBuilder.AppendLine("            builder.Append(\"}\");");
            sourceBuilder.AppendLine("        }");
        }

        private static string CamelCaseName(string name)
        {
            return name.ToLower()[0] + name[1..];
        }

        private static string PadToSize(string value, int size)
        {
            while (value.Length < size)
            {
                value += " ";
            }

            return value;
        }
    }
}
