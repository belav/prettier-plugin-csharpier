using CSharpier.DocTypes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.SyntaxPrinter.SyntaxNodePrinters
{
    public static class ParameterList
    {
        public static Doc Print(ParameterListSyntax node)
        {
            return Doc.Group(
                Token.Print(node.OpenParenToken),
                node.Parameters.Count > 0
                    ? Doc.Concat(
                          Doc.Indent(
                              Doc.SoftLine,
                              SeparatedSyntaxList.Print(node.Parameters, Parameter.Print, Doc.Line)
                          ),
                          Doc.SoftLine
                      )
                    : Doc.Null,
                Token.Print(node.CloseParenToken)
            );
        }
    }
}
