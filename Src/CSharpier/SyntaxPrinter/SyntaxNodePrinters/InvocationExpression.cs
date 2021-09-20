using System;
using System.Collections.Generic;
using System.Linq;
using CSharpier.DocTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public record PrintedNode(CSharpSyntaxNode Node, Doc Doc);

    public static class InvocationExpression
    {
        public static Doc Print(InvocationExpressionSyntax node)
        {
            var printedNodes = new List<PrintedNode>();

            void Traverse(ExpressionSyntax expression)
            {
                /*InvocationExpression
                  [this.DoSomething().DoSomething][()]
                  
                  SimpleMemberAccessExpression
                  [this.DoSomething()][.][DoSomething]
                  
                  InvocationExpression
                  [this.DoSomething][()]
                  
                  SimpleMemberAccessExpression
                  [this][.][DoSomething]
                */
                if (expression is InvocationExpressionSyntax invocationExpressionSyntax)
                {
                    Traverse(invocationExpressionSyntax.Expression);
                    printedNodes.Add(
                        new PrintedNode(
                            Node: invocationExpressionSyntax,
                            Doc: ArgumentList.Print(invocationExpressionSyntax.ArgumentList)
                        )
                    );
                }
                else if (expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                {
                    Traverse(memberAccessExpressionSyntax.Expression);
                    printedNodes.Add(
                        new PrintedNode(
                            Node: memberAccessExpressionSyntax,
                            Doc: Doc.Concat(
                                Token.Print(memberAccessExpressionSyntax.OperatorToken),
                                Node.Print(memberAccessExpressionSyntax.Name)
                            )
                        )
                    );
                }
                else
                {
                    printedNodes.Add(
                        new PrintedNode(Node: expression, Doc: Node.Print(expression))
                    );
                }
            }

            Traverse(node);

            var groups = new List<List<Doc>>();
            var currentGroup = new List<Doc> { printedNodes[0].Doc };
            var index = 1;
            for (; index < printedNodes.Count; index++)
            {
                if (printedNodes[index].Node is MemberAccessExpressionSyntax)
                {
                    currentGroup.Add(printedNodes[index].Doc);
                }
                else
                {
                    break;
                }
            }

            // TODO GH-7 there is a lot more code in prettier for how to get this all working, also include some documents
            if (printedNodes[index].Node is MemberAccessExpressionSyntax)
            {
                currentGroup.Add(printedNodes[index].Doc);
                index++;
            }

            groups.Add(currentGroup);
            currentGroup = new List<Doc>();

            var hasSeenInvocationExpression = false;
            for (; index < printedNodes.Count; index++)
            {
                if (hasSeenInvocationExpression && IsMemberish(printedNodes[index].Node))
                {
                    // [0] should be appended at the end of the group instead of the
                    // beginning of the next one
                    // if (printedNodes[i].node.computed && isNumericLiteral(printedNodes[i].node.property)) {
                    //     currentGroup.push(printedNodes[i]);
                    //     continue;
                    // }
                    groups.Add(currentGroup);
                    currentGroup = new List<Doc>();
                    hasSeenInvocationExpression = false;
                }

                if (printedNodes[index].Node is InvocationExpressionSyntax)
                {
                    hasSeenInvocationExpression = true;
                }
                currentGroup.Add(printedNodes[index].Doc);
                // if (printedNodes[i].node.comments && printedNodes[i].node.comments.some(comment => comment.trailing)) {
                //     groups.push(currentGroup);
                //     currentGroup = [];
                //     hasSeenCallExpression = false;
                // }
            }

            if (currentGroup.Any())
            {
                groups.Add(currentGroup);
            }

            var cutoff = 3;
            if (groups.Count < cutoff)
            {
                return Doc.Group(groups.SelectMany(o => o).ToArray());
            }

            return Doc.Concat(
                Doc.Group(groups[0].ToArray()),
                PrintIndentedGroup(node, groups.Skip(1))
            );
        }

        private static bool IsMemberish(CSharpSyntaxNode node)
        {
            return node is MemberAccessExpressionSyntax;
        }

        private static Doc PrintIndentedGroup(
            InvocationExpressionSyntax node,
            IEnumerable<List<Doc>> groups
        ) {
            if (!groups.Any())
            {
                return Doc.Null;
            }

            // TODO GH-7 softline here?
            return Doc.IndentIf(
                node.Parent is not ConditionalExpressionSyntax,
                Doc.Group(Doc.Join(Doc.SoftLine, groups.Select(o => Doc.Group(o.ToArray()))))
            );
        }
    }
}

/*
class ClassName
{
    // this is better, but still looks weird
    public AccessTokenNotAvailableException(
        NavigationManager navigation,
        AccessTokenResult tokenResult,
        IEnumerable<string> scopes
    ) : base(
        message: "Unable to provision an access token for the requested scopes: " + scopes != null
          ? $"'{string.Join(", ", scopes ?? Array.Empty<string>())}'"
          : "(default scopes)"
    ) { }

    void MethodName()
    {

        // this isn't how prettier does it, stretch goal
        this.Address1 = addressFields_________________
            .FirstOrDefault(field => field.FieldName == "Address1");

        this.Address1 = addressFields_________________
            .FirstOrDefault(field => field.FieldName == "Address1").ToList();

        this.Address1 = addressFields
            .Where(field => field.FieldName == "Address1__________________________")
            .Where(field => yeah)
            .Where(field => yeah);

        roleNames
            .ToList()
            .ForEach(
                (role) =>
                    this.adminContextMock.Setup((ctx) => ctx.IsUserInRole(role)).Returns(false)
            );

        var superLongMethodNameForceLine = someFactoryName__________
            .SuperLongMethodNameForceLine()
            .SomeOtherReallyLongMethodName();

        // prettier does it this way, but ugly!
        string signedRequest = htmlHelper.ViewContext.HttpContext.Request.Params[
            "signed_request____________"
        ];

        // prettier does it this way, but ugly!
        result[i / ReferenceFrameSize] = RenderTreeFrame.ComponentReferenceCapture(
            0,
            null__________,
            0
        );

        // not sure if anything here should change
        Diagnostic.Create(
            _descriptor,
            symbolForDiagnostic.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
                .GetLocation() ?? Location.None,
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)
        );
        
        // see below
        FacebookGroupConnection<TStatus> statuses = await GetFacebookObjectAsync<
            FacebookGroupConnection<TStatus>
        >(client, "me/statuses");

        /* should maybe be
            FacebookGroupConnection<TStatus> statuses = 
                await GetFacebookObjectAsync<FacebookGroupConnection<TStatus>>(
                    client,
                    "me/statuses"
                );
                

        // this one seems fine?
        var proxy1 = generator.CreateInterfaceProxyWithTargetInterface<IEventHandler<EventArgs1>>(
            null,
            new[] { lazyInterceptor1 }
        );
        
        // this is just gross, it used to break parameters instead of generic types
        var count = (int)await _invoker.InvokeUnmarshalled<
            string[],
            object?,
            object?,
            Task<object>
        >(GetSatelliteAssemblies, culturesToLoad.ToArray(), null, null);



        // extra indent?
        return value == null
          ? CallMethod(
                    value.ToString(),
                    "([a-z])([A-Z])",
                    "$1-$2",
                    RegexOptions.None,
                    TimeSpan.FromMilliseconds(100)
                )
                .ToLowerInvariant()
          : Regex.Replace(
                    value.ToString(),
                    "([a-z])([A-Z])",
                    "$1-$2",
                    RegexOptions.None,
                    TimeSpan.FromMilliseconds(100)
                )
                .ToLowerInvariant();

        var ex = Assert.Throws<ArgumentException>(
            () =>
                generator.CreateInterfaceProxyWithTargetInterface<IList<IList<PrivateInterface>>>(
                    new List<IList<PrivateInterface>>(),
                    new IInterceptor[0]
                )
        );

        // these used to not break so weird
        ref var parentFrame = ref diffContext.NewTree[
            newFrame.ComponentReferenceCaptureParentFrameIndexField
        ];

        var newComponentInstance = (CaptureSetParametersComponent)oldTree.GetFrames().Array[
            0
        ].Component;

        parent = (ContainerNode)parent.Children[
            childIndexAtCurrentDepth__________ + siblingIndex__________
        ];

        configuration.EncryptionAlgorithmKeySize = (int)encryptionElement.Attribute(
            "keyLength__________________"
        )!;

        // used to break onto the next line
        var cbTempSubkeys = checked(
            _symmetricAlgorithmSubkeyLengthInBytes + _hmacAlgorithmSubkeyLengthInBytes
        );
        // but then this looks better with checked next to =
        var cbEncryptedData = checked(
            cbCiphertext
            - (
                KEY_MODIFIER_SIZE_IN_BYTES
                + _symmetricAlgorithmBlockSizeInBytes
                + _hmacAlgorithmDigestLengthInBytes
            )
        );

        // used to be 2 lines
        var cacheKey__________ = (
            ModelType: fieldIdentifier.Model.GetType(),
            fieldIdentifier.FieldName
        );

        // not sure what to do with this, nothing??
        storedArguments_______[i] = localVariables[i] = Expression.Parameter(
            parameters[i].ParameterType
        );
    }
}

 */
