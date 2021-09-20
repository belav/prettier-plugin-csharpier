using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CSharpier.Tests
{
    public class MissingTypeChecker
    {
        [Test]
        // at this point this is just useful when a new version of c# comes out
        public void Ensure_There_Are_No_Missing_Types()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory.Name != "Src")
            {
                directory = directory.Parent;
            }

            var files = Directory.GetFiles(
                    Path.Combine(directory.FullName, "CSharpier/SyntaxPrinter/SyntaxNodePrinters")
                )
                .Select(o => Path.GetFileNameWithoutExtension(o) + "Syntax")
                .ToList();

            var syntaxNodeTypes = typeof(CompilationUnitSyntax).Assembly.GetTypes()
                .Where(o => !o.IsAbstract && typeof(CSharpSyntaxNode).IsAssignableFrom(o))
                .ToList();

            var missingTypes = new List<Type>();

            foreach (var nodeType in syntaxNodeTypes)
            {
                var type = nodeType;
                if (
                    typeof(StructuredTriviaSyntax).IsAssignableFrom(type)
                    || typeof(XmlNodeSyntax).IsAssignableFrom(type)
                ) {
                    continue;
                }

                if (ignored.Contains(nodeType.Name))
                {
                    continue;
                }

                while (type != null)
                {
                    if (files.Contains(type.Name))
                    {
                        break;
                    }

                    type = type.BaseType;
                }

                if (type == null)
                {
                    missingTypes.Add(nodeType);
                }
            }

            missingTypes.Should().BeEmpty();
        }

        private readonly HashSet<string> ignored =
            new()
            {
                "AccessorDeclarationSyntax",
                "AccessorListSyntax",
                "AttributeArgumentListSyntax",
                "AttributeArgumentSyntax",
                "AttributeSyntax",
                "AttributeTargetSpecifierSyntax",
                "BaseListSyntax",
                "CatchDeclarationSyntax",
                "CatchFilterClauseSyntax",
                "ConstructorInitializerSyntax",
                "ConversionOperatorMemberCrefSyntax",
                "CrefBracketedParameterListSyntax",
                "CrefParameterListSyntax",
                "CrefParameterSyntax",
                "ExplicitInterfaceSpecifierSyntax",
                "FunctionPointerCallingConventionSyntax",
                "FunctionPointerParameterListSyntax",
                "FunctionPointerParameterSyntax",
                "FunctionPointerUnmanagedCallingConventionListSyntax",
                "FunctionPointerUnmanagedCallingConventionSyntax",
                "IndexerMemberCrefSyntax",
                "InterpolationAlignmentClauseSyntax",
                "InterpolationFormatClauseSyntax",
                "JoinIntoClauseSyntax",
                "NameMemberCrefSyntax",
                "OperatorMemberCrefSyntax",
                "OrderingSyntax",
                "PositionalPatternClauseSyntax",
                "PropertyPatternClauseSyntax",
                "QualifiedCrefSyntax",
                "SubpatternSyntax",
                "SwitchExpressionArmSyntax",
                "TypeCrefSyntax",
                "XmlCrefAttributeSyntax",
                "XmlElementEndTagSyntax",
                "XmlElementStartTagSyntax",
                "XmlNameAttributeSyntax",
                "XmlNameSyntax",
                "XmlPrefixSyntax",
                "XmlTextAttributeSyntax"
            };
    }
}
