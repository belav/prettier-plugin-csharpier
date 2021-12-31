namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal static class BaseTypeDeclaration
{
    public static Doc Print(BaseTypeDeclarationSyntax node)
    {
        ParameterListSyntax? parameterList = null;
        TypeParameterListSyntax? typeParameterList = null;
        var constraintClauses = Enumerable.Empty<TypeParameterConstraintClauseSyntax>();
        var hasMembers = false;
        SyntaxToken? recordKeyword = null;
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
                members = CreateTypeMembers(typeDeclarationSyntax);
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
                recordKeyword = recordDeclarationSyntax.Keyword;
                keyword = recordDeclarationSyntax.ClassOrStructKeyword;
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

        var docs = new List<Doc>();
        if (node.AttributeLists.Any())
        {
            docs.Add(AttributeLists.Print(node, node.AttributeLists));
        }

        if (node.Modifiers.Any())
        {
            docs.Add(Modifiers.Print(node.Modifiers));
        }

        if (recordKeyword != null)
        {
            docs.Add(Token.PrintWithSuffix(recordKeyword.Value, " "));
        }

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
                Doc.Align(2, SeparatedSyntaxList.Print(node.BaseList.Types, Node.Print, Doc.Line))
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
                Token.Print(node.CloseBraceToken)
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

    private static Doc CreateTypeMembers(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        var members = new List<Doc> { Doc.HardLine };
        for (var x = 0; x < typeDeclarationSyntax.Members.Count; x++)
        {
            var member = typeDeclarationSyntax.Members[x];

            if (x == 0)
            {
                members.Add(Node.Print(member));
                continue;
            }

            var lineIsForced =
                member is MethodDeclarationSyntax
                    && typeDeclarationSyntax is not InterfaceDeclarationSyntax
                || member
                    is ClassDeclarationSyntax
                        or EnumDeclarationSyntax
                        or InterfaceDeclarationSyntax
                        or ConstructorDeclarationSyntax
                        or ConversionOperatorDeclarationSyntax
                        or DestructorDeclarationSyntax
                        or RecordDeclarationSyntax
                        or StructDeclarationSyntax
                        or OperatorDeclarationSyntax;

            var addLine = lineIsForced;

            if (!lineIsForced)
            {
                addLine =
                    member.AttributeLists.Any()
                    || member
                        .GetLeadingTrivia()
                        .Any(o => o.Kind() is SyntaxKind.EndOfLineTrivia || o.IsComment());
            }

            members.Add(Doc.HardLine);

            if (addLine)
            {
                members.Add(Doc.HardLine);
            }

            members.Add(Node.Print(member));
        }

        return Doc.Indent(members);
    }
}
