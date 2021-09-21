using System;
using System.Collections.Generic;
using System.Linq;
using CSharpier.DocTypes;
using CSharpier.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class BaseTypeDeclaration
    {
        public static Doc Print(BaseTypeDeclarationSyntax node)
        {
            ParameterListSyntax? parameterList = null;
            TypeParameterListSyntax? typeParameterList = null;
            var constraintClauses = Enumerable.Empty<TypeParameterConstraintClauseSyntax>();
            var hasMembers = false;
            SyntaxToken? keyword = null;
            Doc members = Doc.Null;
            SyntaxToken? semicolonToken = null;

            if (node is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                typeParameterList = typeDeclarationSyntax.TypeParameterList;
                constraintClauses = typeDeclarationSyntax.ConstraintClauses;
                hasMembers = typeDeclarationSyntax.Members.Count > 0;
                if (typeDeclarationSyntax.Members.Count > 0)
                {
                    members = Doc.Indent(
                        Doc.HardLine,
                        Doc.Join(Doc.HardLine, typeDeclarationSyntax.Members.Select(Node.Print))
                    );
                }
                if (node is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    keyword = classDeclarationSyntax.Keyword;
                }
                else if (node is StructDeclarationSyntax structDeclarationSyntax)
                {
                    keyword = structDeclarationSyntax.Keyword;
                }
                else if (node is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
                {
                    keyword = interfaceDeclarationSyntax.Keyword;
                }
                else if (node is RecordDeclarationSyntax recordDeclarationSyntax)
                {
                    keyword = recordDeclarationSyntax.Keyword;
                    parameterList = recordDeclarationSyntax.ParameterList;
                }

                semicolonToken = typeDeclarationSyntax.SemicolonToken;
            }
            else if (node is EnumDeclarationSyntax enumDeclarationSyntax)
            {
                members = Doc.Indent(
                    Doc.HardLine,
                    SeparatedSyntaxList.Print(
                        enumDeclarationSyntax.Members,
                        EnumMemberDeclaration.Print,
                        Doc.HardLine
                    )
                );
                hasMembers = enumDeclarationSyntax.Members.Count > 0;
                keyword = enumDeclarationSyntax.EnumKeyword;
                semicolonToken = enumDeclarationSyntax.SemicolonToken;
            }

            var docs = new List<Doc>
            {
                ExtraNewLines.Print(node),
                AttributeLists.Print(node, node.AttributeLists),
                Modifiers.Print(node.Modifiers)
            };
            if (keyword != null)
            {
                docs.Add(Token.PrintWithSuffix(keyword.Value, " "));
            }

            docs.Add(Token.Print(node.Identifier));

            if (typeParameterList != null)
            {
                docs.Add(TypeParameterList.Print(typeParameterList));
            }

            if (parameterList != null)
            {
                docs.Add(ParameterList.Print(parameterList));
            }

            if (node.BaseList != null)
            {
                var baseListDoc = Doc.Concat(
                    Token.Print(node.BaseList.ColonToken),
                    " ",
                    Doc.Align(
                        2,
                        Doc.Concat(
                            SeparatedSyntaxList.Print(node.BaseList.Types, Node.Print, Doc.Line)
                        )
                    )
                );

                docs.Add(Doc.Group(Doc.Indent(Doc.Line, baseListDoc)));
            }

            docs.Add(ConstraintClauses.Print(constraintClauses));

            if (hasMembers)
            {
                DocUtilities.RemoveInitialDoubleHardLine(members);

                docs.Add(
                    Doc.HardLine,
                    Token.Print(node.OpenBraceToken),
                    members,
                    Doc.HardLine,
                    node.CloseBraceToken.LeadingTrivia.Any(
                        o => o.Kind() == SyntaxKind.DisabledTextTrivia
                    )
                        ? Doc.Concat(
                              Token.PrintLeadingTriviaWithNewLines(
                                  node.CloseBraceToken.LeadingTrivia
                              ),
                              Token.PrintWithoutLeadingTrivia(node.CloseBraceToken)
                          )
                        : Token.Print(node.CloseBraceToken)
                );
            }
            else if (node.OpenBraceToken.Kind() != SyntaxKind.None)
            {
                Doc separator = node.CloseBraceToken.LeadingTrivia.Any() ? Doc.Line : " ";

                docs.Add(
                    separator,
                    Token.Print(node.OpenBraceToken),
                    separator,
                    Token.Print(node.CloseBraceToken)
                );
            }

            if (semicolonToken.HasValue)
            {
                docs.Add(Token.Print(semicolonToken.Value));
            }

            return Doc.Concat(docs);
        }
    }
}
