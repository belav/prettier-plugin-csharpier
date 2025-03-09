using System.Diagnostics.CodeAnalysis;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters;

internal record PrintedNode(CSharpSyntaxNode Node, Doc Doc);

// This is based on prettier/src/language-js/print/member-chain.js
// various discussions/prs about how to potentially improve the formatting
// https://github.com/prettier/prettier/issues/5737
// https://github.com/prettier/prettier/issues/7884
// https://github.com/prettier/prettier/issues/8003
// https://github.com/prettier/prettier/issues/8902
// https://github.com/prettier/prettier/pull/7889
internal static class InvocationExpression
{
    public static Doc Print(InvocationExpressionSyntax node, PrintingContext context)
    {
        return PrintMemberChain(node, context);
    }

    public static Doc PrintMemberChain(ExpressionSyntax node, PrintingContext context)
    {
        var parent = node.Parent;
        var printedNodes = new List<PrintedNode>();

        FlattenAndPrintNodes(node, printedNodes, context);

        var groups = printedNodes.Any(o => o.Node is InvocationExpressionSyntax)
            ? GroupPrintedNodesPrettierStyle(printedNodes)
            : GroupPrintedNodesOnLines(printedNodes);

        var oneLine = SelectManyDocsToArray(groups);

        var shouldMergeFirstTwoGroups = ShouldMergeFirstTwoGroups(groups, parent);

        var cutoff = shouldMergeFirstTwoGroups ? 3 : 2;

        var forceOneLine =
            (
                groups.Count <= cutoff
                && (
                    groups
                        .Skip(shouldMergeFirstTwoGroups ? 1 : 0)
                        .Any(o =>
                            o.Last().Node
                                is not (
                                    InvocationExpressionSyntax
                                    or ElementAccessExpressionSyntax
                                    or PostfixUnaryExpressionSyntax
                                    {
                                        Operand: InvocationExpressionSyntax
                                    }
                                )
                        )
                    // if the last group contains just a !, make sure it doesn't end up on a new line
                    || (
                        groups.Last().Count == 1
                        && groups.Last()[0].Node is PostfixUnaryExpressionSyntax
                    )
                )
            )
            || node.HasParent(typeof(InterpolatedStringExpressionSyntax))
            // this handles the case of a multiline string being part of an invocation chain
            // conditional groups don't propagate breaks so we need to avoid the conditional group
            || groups[0]
                .Any(o =>
                    o.Node
                        is LiteralExpressionSyntax
                            {
                                Token.RawKind: (int)SyntaxKind.MultiLineRawStringLiteralToken
                            }
                            or InterpolatedStringExpressionSyntax
                            {
                                StringStartToken.RawKind: (int)
                                    SyntaxKind.InterpolatedMultiLineRawStringStartToken
                            }
                    || o.Node
                        is LiteralExpressionSyntax
                        {
                            Token.Text.Length: > 0
                        } literalExpressionSyntax
                        && literalExpressionSyntax.Token.Text.Contains('\n')
                );

        if (forceOneLine)
        {
            return Doc.Group(oneLine);
        }

        var expanded = Doc.Concat(
            Doc.Concat(groups[0].Select(o => o.Doc).ToArray()),
            shouldMergeFirstTwoGroups
                ? Doc.IndentIf(
                    groups.Count > 2 && groups[1].Last().Doc is not Group { Contents: IndentDoc },
                    Doc.Concat(groups[1].Select(o => o.Doc).ToArray())
                )
                : Doc.Null,
            PrintIndentedGroup(groups.Skip(shouldMergeFirstTwoGroups ? 2 : 1).ToList())
        );

        return
            oneLine.Skip(1).Any(DocUtilities.ContainsBreak)
            || groups[0]
                .Any(o =>
                    o.Node
                        is ArrayCreationExpressionSyntax
                            or ObjectCreationExpressionSyntax { Initializer: not null }
                )
            ? expanded
            : Doc.ConditionalGroup(Doc.Concat(oneLine), expanded);
    }

    private static void FlattenAndPrintNodes(
        ExpressionSyntax expression,
        List<PrintedNode> printedNodes,
        PrintingContext context
    )
    {
        /*
          We need to flatten things out because the AST has them this way
          InvocationExpression
          Expression                        ArgumentList
          this.DoSomething().DoSomething    ()
          
          MemberAccessExpression
          Expression            OperatorToken   Name
          this.DoSomething()    .               DoSomething
          
          InvocationExpression
          Expression            ArgumentList
          this.DoSomething      ()
          
          MemberAccessExpression
          Expression    OperatorToken   Name
          this          .               DoSomething
          
          And we want to work with them from Left to Right
        */
        if (expression is InvocationExpressionSyntax invocationExpressionSyntax)
        {
            FlattenAndPrintNodes(invocationExpressionSyntax.Expression, printedNodes, context);
            printedNodes.Add(
                new PrintedNode(
                    invocationExpressionSyntax,
                    ArgumentList.Print(invocationExpressionSyntax.ArgumentList, context)
                )
            );
        }
        else if (expression is ElementAccessExpressionSyntax elementAccessExpression)
        {
            FlattenAndPrintNodes(elementAccessExpression.Expression, printedNodes, context);
            printedNodes.Add(
                new PrintedNode(
                    elementAccessExpression,
                    Node.Print(elementAccessExpression.ArgumentList, context)
                )
            );
        }
        else if (expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            FlattenAndPrintNodes(memberAccessExpressionSyntax.Expression, printedNodes, context);
            printedNodes.Add(
                new PrintedNode(
                    memberAccessExpressionSyntax,
                    Doc.Concat(
                        Token.Print(memberAccessExpressionSyntax.OperatorToken, context),
                        Node.Print(memberAccessExpressionSyntax.Name, context)
                    )
                )
            );
        }
        else if (expression is ConditionalAccessExpressionSyntax conditionalAccessExpressionSyntax)
        {
            FlattenAndPrintNodes(
                conditionalAccessExpressionSyntax.Expression,
                printedNodes,
                context
            );
            printedNodes.Add(
                new PrintedNode(
                    conditionalAccessExpressionSyntax,
                    Token.Print(conditionalAccessExpressionSyntax.OperatorToken, context)
                )
            );
            FlattenAndPrintNodes(
                conditionalAccessExpressionSyntax.WhenNotNull,
                printedNodes,
                context
            );
        }
        else if (
            expression is PostfixUnaryExpressionSyntax
            {
                Operand: InvocationExpressionSyntax or MemberAccessExpressionSyntax
            } postfixUnaryExpression
        )
        {
            FlattenAndPrintNodes(postfixUnaryExpression.Operand, printedNodes, context);
            printedNodes.Add(
                new PrintedNode(
                    postfixUnaryExpression,
                    Token.Print(postfixUnaryExpression.OperatorToken, context)
                )
            );
        }
        else
        {
            printedNodes.Add(new PrintedNode(expression, Node.Print(expression, context)));
        }
    }

    private static List<List<PrintedNode>> GroupPrintedNodesOnLines(List<PrintedNode> printedNodes)
    {
        // We want to group the printed nodes in the following manner
        //
        //   a?.b.c!.d

        // so that we can print it like this if it breaks
        //   a
        //     ?.b
        //     .c!
        //     .d

        var groups = new List<List<PrintedNode>>();

        var currentGroup = new List<PrintedNode> { printedNodes[0] };
        groups.Add(currentGroup);

        for (var index = 1; index < printedNodes.Count; index++)
        {
            if (printedNodes[index].Node is ConditionalAccessExpressionSyntax)
            {
                currentGroup = [];
                groups.Add(currentGroup);
            }
            else if (
                printedNodes[index].Node
                    is MemberAccessExpressionSyntax
                        or MemberBindingExpressionSyntax
                        or IdentifierNameSyntax
                && printedNodes[index + -1].Node is not ConditionalAccessExpressionSyntax
            )
            {
                currentGroup = [];
                groups.Add(currentGroup);
            }

            currentGroup.Add(printedNodes[index]);
        }

        return groups;
    }

    private static List<List<PrintedNode>> GroupPrintedNodesPrettierStyle(
        List<PrintedNode> printedNodes
    )
    {
        // We want to group the printed nodes in the following manner
        //
        //   a().b.c().d().e
        // will be grouped as
        //   [
        //     [Identifier, InvocationExpression],
        //     [MemberAccessExpression], [MemberAccessExpression, InvocationExpression],
        //     [MemberAccessExpression, InvocationExpression],
        //     [MemberAccessExpression],
        //   ]

        // so that we can print it as
        //   a()
        //     .b.c()
        //     .d()
        //     .e

        // TODO #451 this whole thing could possibly just turn into a big loop
        // based on the current node, and the next/previous node, decide when to create new groups.
        // certain nodes need to stay in the current group, other nodes indicate that a new group needs to be created.
        var groups = new List<List<PrintedNode>>();
        var currentGroup = new List<PrintedNode> { printedNodes[0] };
        var index = 1;
        for (; index < printedNodes.Count; index++)
        {
            if (printedNodes[index].Node is InvocationExpressionSyntax)
            {
                currentGroup.Add(printedNodes[index]);
            }
            else
            {
                break;
            }
        }

        if (
            printedNodes[0].Node is not (InvocationExpressionSyntax or PostfixUnaryExpressionSyntax)
            && index < printedNodes.Count
            && printedNodes[index].Node
                is ElementAccessExpressionSyntax
                    or PostfixUnaryExpressionSyntax
        )
        {
            currentGroup.Add(printedNodes[index]);
            index++;
        }

        groups.Add(currentGroup);
        currentGroup = [];

        var hasSeenNodeThatRequiresBreak = false;
        for (; index < printedNodes.Count; index++)
        {
            if (
                hasSeenNodeThatRequiresBreak
                && printedNodes[index].Node
                    is MemberAccessExpressionSyntax
                        or ConditionalAccessExpressionSyntax
            )
            {
                groups.Add(currentGroup);
                currentGroup = [];
                hasSeenNodeThatRequiresBreak = false;
            }

            if (
                printedNodes[index].Node
                is (InvocationExpressionSyntax or ElementAccessExpressionSyntax)
            )
            {
                hasSeenNodeThatRequiresBreak = true;
            }
            currentGroup.Add(printedNodes[index]);
        }

        if (currentGroup.Count != 0)
        {
            groups.Add(currentGroup);
        }

        return groups;
    }

    [SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    private static Doc[] SelectManyDocsToArray(List<List<PrintedNode>> groups)
    {
        var arrayLength = 0;
        foreach (var group in groups)
        {
            arrayLength += group.Count;
        }

        var outputArray = new Doc[arrayLength];

        var pos = 0;
        foreach (var group in groups)
        {
            foreach (var node in group)
            {
                outputArray[pos] = node.Doc;
                pos++;
            }
        }

        return outputArray;
    }

    private static Doc PrintIndentedGroup(List<List<PrintedNode>> groups)
    {
        if (groups.Count == 0)
        {
            return Doc.Null;
        }

        return Doc.Indent(
            Doc.Group(
                Doc.HardLine,
                Doc.Join(
                    Doc.HardLine,
                    groups.Select(o => Doc.Group(o.Select(p => p.Doc).ToArray()))
                )
            )
        );
    }

    // There are cases where merging the first two groups looks better
    // For example
    /*
        // without merging we get this
        this
            .CallMethod()
            .CallMethod();

        // merging gives us this
        this.CallMethod()
            .CallMethod();
     */
    private static bool ShouldMergeFirstTwoGroups(
        List<List<PrintedNode>> groups,
        SyntaxNode? parent
    )
    {
        if (groups.Count < 2 || groups[0].Count != 1)
        {
            return false;
        }

        var firstNode = groups[0][0].Node;

        if (
            firstNode
            is not (
                IdentifierNameSyntax { Identifier.Text.Length: <= 4 }
                or ThisExpressionSyntax
                or PredefinedTypeSyntax
                or BaseExpressionSyntax
            )
        )
        {
            return false;
        }

        // TODO maybe some things to fix in here
        // https://github.com/belav/csharpier-repos/pull/100/files
        if (
            groups[1].Count == 1
            || parent
                is SimpleLambdaExpressionSyntax
                    or ArgumentSyntax
                    or BinaryExpressionSyntax
                    or ExpressionStatementSyntax
            || groups[1].Skip(1).First().Node
                is InvocationExpressionSyntax
                    or ElementAccessExpressionSyntax
                    or PostfixUnaryExpressionSyntax
        )
        {
            return true;
        }

        return false;
    }
}
