using System.Collections.Generic;
using System.Linq;
using CSharpier.DocTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class BasePropertyDeclaration
    {
        public static Doc Print(BasePropertyDeclarationSyntax node)
        {
            EqualsValueClauseSyntax? initializer = null;
            ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifierSyntax = null;
            Doc identifier = Doc.Null;
            Doc eventKeyword = Doc.Null;
            ArrowExpressionClauseSyntax? expressionBody = null;
            SyntaxToken? semicolonToken = null;

            if (node is PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                expressionBody = propertyDeclarationSyntax.ExpressionBody;
                initializer = propertyDeclarationSyntax.Initializer;
                explicitInterfaceSpecifierSyntax =
                    propertyDeclarationSyntax.ExplicitInterfaceSpecifier;
                identifier = Token.Print(propertyDeclarationSyntax.Identifier);
                semicolonToken = propertyDeclarationSyntax.SemicolonToken;
            }
            else if (node is IndexerDeclarationSyntax indexerDeclarationSyntax)
            {
                expressionBody = indexerDeclarationSyntax.ExpressionBody;
                explicitInterfaceSpecifierSyntax =
                    indexerDeclarationSyntax.ExplicitInterfaceSpecifier;
                identifier = Doc.Concat(
                    Token.Print(indexerDeclarationSyntax.ThisKeyword),
                    Node.Print(indexerDeclarationSyntax.ParameterList)
                );
                semicolonToken = indexerDeclarationSyntax.SemicolonToken;
            }
            else if (node is EventDeclarationSyntax eventDeclarationSyntax)
            {
                eventKeyword = Token.PrintWithSuffix(eventDeclarationSyntax.EventKeyword, " ");
                explicitInterfaceSpecifierSyntax =
                    eventDeclarationSyntax.ExplicitInterfaceSpecifier;
                identifier = Token.Print(eventDeclarationSyntax.Identifier);
                semicolonToken = eventDeclarationSyntax.SemicolonToken;
            }

            Doc contents = string.Empty;
            if (node.AccessorList != null)
            {
                Doc separator = " ";
                if (
                    node.AccessorList.Accessors.Any(
                        o =>
                            o.Body != null
                            || o.ExpressionBody != null
                            || o.Modifiers.Any()
                            || o.AttributeLists.Any()
                    )
                ) {
                    separator = Doc.Line;
                }

                contents = Doc.Group(
                    Doc.Concat(
                        separator,
                        Token.Print(node.AccessorList.OpenBraceToken),
                        Doc.Indent(
                            node.AccessorList.Accessors.Select(
                                    o => PrintAccessorDeclarationSyntax(o, separator)
                                )
                                .ToArray()
                        ),
                        separator,
                        Token.Print(node.AccessorList.CloseBraceToken)
                    )
                );
            }
            else if (expressionBody != null)
            {
                contents = Doc.Concat(ArrowExpressionClause.Print(expressionBody));
            }

            var docs = new List<Doc>
            {
                ExtraNewLines.Print(node),
                AttributeLists.Print(node, node.AttributeLists)
            };

            return Doc.Group(
                Doc.Concat(
                    Doc.Concat(docs),
                    Modifiers.Print(node.Modifiers),
                    eventKeyword,
                    Node.Print(node.Type),
                    " ",
                    explicitInterfaceSpecifierSyntax != null
                        ? Doc.Concat(
                              Node.Print(explicitInterfaceSpecifierSyntax.Name),
                              Token.Print(explicitInterfaceSpecifierSyntax.DotToken)
                          )
                        : Doc.Null,
                    identifier,
                    contents,
                    initializer != null ? EqualsValueClause.Print(initializer) : Doc.Null,
                    semicolonToken.HasValue ? Token.Print(semicolonToken.Value) : Doc.Null
                )
            );
        }

        private static Doc PrintAccessorDeclarationSyntax(
            AccessorDeclarationSyntax node,
            Doc separator
        ) {
            var docs = new List<Doc>();
            if (node.AttributeLists.Count > 0 || node.Body != null || node.ExpressionBody != null)
            {
                docs.Add(Doc.HardLine);
            }
            else
            {
                docs.Add(separator);
            }

            docs.Add(AttributeLists.Print(node, node.AttributeLists));
            docs.Add(Modifiers.Print(node.Modifiers));
            docs.Add(Token.Print(node.Keyword));

            if (node.Body != null)
            {
                docs.Add(Block.Print(node.Body));
            }
            else if (node.ExpressionBody != null)
            {
                docs.Add(ArrowExpressionClause.Print(node.ExpressionBody));
            }

            docs.Add(Token.Print(node.SemicolonToken));

            return Doc.Concat(docs);
        }
    }
}
