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
            var printedNodes = new List<(CSharpSyntaxNode node, Doc doc)>();

            void Traverse(ExpressionSyntax expression)
            {
                /*
                  We need to flatten things out because the AST has them this way
                  where the first node is this 
                  InvocationExpression
                  [ this.DoSomething().DoSomething ] [ () ]
                  
                  SimpleMemberAccessExpression
                  [ this.DoSomething() ] [ . ] [ DoSomething ]
                  
                  InvocationExpression
                  [ this.DoSomething ] [ () ]
                  
                  SimpleMemberAccessExpression
                  [ this ] [ . ] [ DoSomething ]
                */
                if (expression is InvocationExpressionSyntax invocationExpressionSyntax)
                {
                    Traverse(invocationExpressionSyntax.Expression);
                    printedNodes.Add(
                        (
                            node: invocationExpressionSyntax,
                            doc: ArgumentList.Print(invocationExpressionSyntax.ArgumentList)
                        )
                    );
                }
                else if (expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
                {
                    Traverse(memberAccessExpressionSyntax.Expression);
                    printedNodes.Add(
                        (
                            node: memberAccessExpressionSyntax,
                            doc: Doc.Concat(
                                Token.Print(memberAccessExpressionSyntax.OperatorToken),
                                Node.Print(memberAccessExpressionSyntax.Name)
                            )
                        )
                    );
                }
                else
                {
                    printedNodes.Add((node: expression, doc: Node.Print(expression)));
                }
            }

            Traverse(node);

            var groups = new List<List<Doc>>();
            var currentGroup = new List<Doc> { printedNodes[0].doc };
            var index = 1;
            for (; index < printedNodes.Count; index++)
            {
                if (printedNodes[index].node is InvocationExpressionSyntax)
                {
                    currentGroup.Add(printedNodes[index].doc);
                }
                else
                {
                    break;
                }
            }

            if (printedNodes[0].node is not InvocationExpressionSyntax)
            {
                for (; index + 1 < printedNodes.Count; ++index)
                {
                    if (
                        IsMemberish(printedNodes[index].node)
                        && IsMemberish(printedNodes[index + 1].node)
                    ) {
                        currentGroup.Add(printedNodes[index].doc);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            groups.Add(currentGroup);
            currentGroup = new List<Doc>();

            var hasSeenInvocationExpression = false;
            for (; index < printedNodes.Count; index++)
            {
                if (hasSeenInvocationExpression && IsMemberish(printedNodes[index].node))
                {
                    groups.Add(currentGroup);
                    currentGroup = new List<Doc>();
                    hasSeenInvocationExpression = false;
                }

                if (printedNodes[index].node is InvocationExpressionSyntax)
                {
                    hasSeenInvocationExpression = true;
                }
                currentGroup.Add(printedNodes[index].doc);
            }

            if (currentGroup.Any())
            {
                groups.Add(currentGroup);
            }

            var oneLine = groups.SelectMany(o => o).ToArray();

            var cutoff = 3;
            if (groups.Count < cutoff)
            {
                return Doc.Group(oneLine);
            }

            var expanded = Doc.Concat(Doc.Concat(groups[0]), PrintIndentedGroup(node, groups));

            return Doc.ConditionalGroup(Doc.Concat(oneLine), expanded);
        }

        private static bool IsMemberish(CSharpSyntaxNode node)
        {
            return node is MemberAccessExpressionSyntax;
        }

        private static Doc PrintIndentedGroup(
            InvocationExpressionSyntax node,
            IList<List<Doc>> groups
        ) {
            if (groups.Count == 1)
            {
                return Doc.Null;
            }

            return Doc.IndentIf(
                node.Parent is not ConditionalExpressionSyntax,
                Doc.Group(
                    Doc.HardLine,
                    Doc.Join(Doc.HardLine, groups.Skip(1).Select(o => Doc.Group(o.ToArray())))
                )
            );
        }
    }
}

// TODO look into failing tests before continuing

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
