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
    public static class BaseMethodDeclaration
    {
        public static Doc Print(CSharpSyntaxNode node)
        {
            SyntaxList<AttributeListSyntax>? attributeLists = null;
            SyntaxTokenList? modifiers = null;
            TypeSyntax? returnType = null;
            ExplicitInterfaceSpecifierSyntax? explicitInterfaceSpecifier = null;
            TypeParameterListSyntax? typeParameterList = null;
            Doc identifier = Doc.Null;
            SyntaxList<TypeParameterConstraintClauseSyntax>? constraintClauses = null;
            ParameterListSyntax? parameterList = null;
            ConstructorInitializerSyntax? constructorInitializer = null;
            BlockSyntax? body = null;
            ArrowExpressionClauseSyntax? expressionBody = null;
            SyntaxToken? semicolonToken = null;

            string? parameterGroupId = null;

            if (node is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax)
            {
                attributeLists = baseMethodDeclarationSyntax.AttributeLists;
                modifiers = baseMethodDeclarationSyntax.Modifiers;
                parameterList = baseMethodDeclarationSyntax.ParameterList;
                body = baseMethodDeclarationSyntax.Body;
                expressionBody = baseMethodDeclarationSyntax.ExpressionBody;
                if (node is MethodDeclarationSyntax methodDeclarationSyntax)
                {
                    returnType = methodDeclarationSyntax.ReturnType;
                    explicitInterfaceSpecifier = methodDeclarationSyntax.ExplicitInterfaceSpecifier;
                    identifier = Token.Print(methodDeclarationSyntax.Identifier);
                    typeParameterList = methodDeclarationSyntax.TypeParameterList;
                    constraintClauses = methodDeclarationSyntax.ConstraintClauses;
                }
                else if (node is DestructorDeclarationSyntax destructorDeclarationSyntax)
                {
                    identifier = Doc.Concat(
                        Token.Print(destructorDeclarationSyntax.TildeToken),
                        Token.Print(destructorDeclarationSyntax.Identifier)
                    );
                }
                else if (node is ConstructorDeclarationSyntax constructorDeclarationSyntax)
                {
                    identifier = Token.Print(constructorDeclarationSyntax.Identifier);
                    constructorInitializer = constructorDeclarationSyntax.Initializer;
                }

                semicolonToken = baseMethodDeclarationSyntax.SemicolonToken;
            }
            else if (node is LocalFunctionStatementSyntax localFunctionStatementSyntax)
            {
                attributeLists = localFunctionStatementSyntax.AttributeLists;
                modifiers = localFunctionStatementSyntax.Modifiers;
                returnType = localFunctionStatementSyntax.ReturnType;
                identifier = Token.Print(localFunctionStatementSyntax.Identifier);
                typeParameterList = localFunctionStatementSyntax.TypeParameterList;
                parameterList = localFunctionStatementSyntax.ParameterList;
                constraintClauses = localFunctionStatementSyntax.ConstraintClauses;
                body = localFunctionStatementSyntax.Body;
                expressionBody = localFunctionStatementSyntax.ExpressionBody;
                semicolonToken = localFunctionStatementSyntax.SemicolonToken;
            }

            var docs = new List<Doc> { ExtraNewLines.Print(node) };

            if (attributeLists.HasValue && attributeLists.Value.Any())
            {
                docs.Add(AttributeLists.Print(node, attributeLists.Value));
            }

            var declarationGroup = new List<Doc>();

            if (modifiers.HasValue && modifiers.Value.Any())
            {
                declarationGroup.Add(Modifiers.PrintWithoutLeadingTrivia(modifiers.Value));
            }

            if (returnType != null)
            {
                if (!(modifiers.HasValue && modifiers.Value.Count > 0))
                {
                    Token.ShouldSkipNextLeadingTrivia = true;
                }

                declarationGroup.Add(Node.Print(returnType), " ");
                Token.ShouldSkipNextLeadingTrivia = false;
            }

            if (explicitInterfaceSpecifier != null)
            {
                declarationGroup.Add(
                    Node.Print(explicitInterfaceSpecifier.Name),
                    Token.Print(explicitInterfaceSpecifier.DotToken)
                );
            }

            if (identifier != Doc.Null)
            {
                declarationGroup.Add(identifier);
            }

            if (node is ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax)
            {
                declarationGroup.Add(
                    Token.PrintWithSuffix(
                        conversionOperatorDeclarationSyntax.ImplicitOrExplicitKeyword,
                        " "
                    ),
                    Token.PrintWithSuffix(conversionOperatorDeclarationSyntax.OperatorKeyword, " "),
                    Node.Print(conversionOperatorDeclarationSyntax.Type)
                );
            }
            else if (node is OperatorDeclarationSyntax operatorDeclarationSyntax)
            {
                declarationGroup.Add(
                    Node.Print(operatorDeclarationSyntax.ReturnType),
                    " ",
                    Token.PrintWithSuffix(operatorDeclarationSyntax.OperatorKeyword, " "),
                    Token.Print(operatorDeclarationSyntax.OperatorToken)
                );
            }

            if (typeParameterList != null)
            {
                declarationGroup.Add(TypeParameterList.Print(typeParameterList));
            }

            if (parameterList != null)
            {
                declarationGroup.Add(ParameterList.Print(parameterList));
                declarationGroup.Add(Doc.IfBreak(Doc.Null, Doc.SoftLine));
            }

            if (constructorInitializer != null)
            {
                var colonToken = Token.PrintWithSuffix(constructorInitializer.ColonToken, " ");
                var argumentList = Doc.Group(
                    ArgumentList.Print(constructorInitializer.ArgumentList)
                );

                if (parameterGroupId != null)
                {
                    declarationGroup.Add(
                        Doc.Group(
                            Doc.Indent(Doc.IfBreak(" ", Doc.Line, parameterGroupId)),
                            Doc.IfBreak(
                                Doc.Align(2, colonToken),
                                Doc.Indent(colonToken),
                                parameterGroupId
                            ),
                            Token.Print(constructorInitializer.ThisOrBaseKeyword),
                            Doc.IfBreak(argumentList, Doc.Indent(argumentList), parameterGroupId)
                        )
                    );
                }
                else
                {
                    declarationGroup.Add(
                        Doc.Group(
                            Doc.Indent(Doc.Line),
                            Doc.Indent(colonToken),
                            Token.Print(constructorInitializer.ThisOrBaseKeyword),
                            Doc.Indent(argumentList)
                        )
                    );
                }
            }

            if (modifiers is { Count: > 0 })
            {
                docs.Add(Token.PrintLeadingTrivia(modifiers.Value[0]));
            }
            else if (returnType != null)
            {
                docs.Add(Token.PrintLeadingTrivia(returnType.GetLeadingTrivia()));
            }

            docs.Add(Doc.Group(declarationGroup));

            if (constraintClauses != null)
            {
                docs.Add(ConstraintClauses.Print(constraintClauses));
            }

            if (body != null)
            {
                docs.Add(Block.Print(body));
            }
            else
            {
                if (expressionBody != null)
                {
                    docs.Add(ArrowExpressionClause.Print(expressionBody));
                }
            }

            if (semicolonToken.HasValue)
            {
                docs.Add(Token.Print(semicolonToken.Value));
            }

            return Doc.Group(docs);
        }
    }
}
