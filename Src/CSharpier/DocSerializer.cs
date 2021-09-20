using System;
using System.Linq;
using CSharpier.DocTypes;

namespace CSharpier
{
    /// <summary>
    /// Used to create a string representation of a Doc
    /// The string representation can be pasted into DocPrinterTests
    /// </summary>
    public static class DocSerializer
    {
        public static string Serialize(Doc document)
        {
            return Serialize(document, string.Empty);
        }

        // this is inefficient without a stringBuilder, but only used for dev work and changing it to use one isn't straightforward.
        private static string Serialize(Doc document, string indent, Doc? parent = null)
        {
            var newLine = Environment.NewLine;
            var nextIndent = indent + "    ";

            string PrintIndentedDocArray(Doc[] docs)
            {
                var result = "";
                for (var x = 0; x < docs.Length; x++)
                {
                    var printResult = PrintIndentedDocTree(docs[x]);
                    result += printResult;
                    if (x < docs.Length - 1)
                    {
                        result += "," + newLine;
                    }
                }

                return result;
            }

            string PrintIndentedDocTree(Doc doc, Doc? parent = null)
            {
                return Serialize(doc, nextIndent, parent);
            }

            string PrintConcat(Concat concatToPrint)
            {
                var skipConcat =
                    parent is IHasContents hasContents && hasContents.Contents == concatToPrint;

                var result = "";
                if (!skipConcat)
                {
                    result += indent + "Doc.Concat(";
                    if (concatToPrint.Contents.Count > 0)
                    {
                        result += newLine;
                    }
                }

                for (var x = 0; x < concatToPrint.Contents.Count; x++)
                {
                    var printResult = skipConcat
                        ? Serialize(concatToPrint.Contents[x], indent)
                        : PrintIndentedDocTree(concatToPrint.Contents[x]);
                    result += printResult;
                    if (x < concatToPrint.Contents.Count - 1)
                    {
                        result += "," + newLine;
                    }
                }

                if (!skipConcat)
                {
                    result += newLine + indent + ")";
                }
                return result;
            }

            return document switch
            {
                NullDoc => indent + "Doc.Null",
                StringDoc{
                    IsDirective: true
                } directive
                  => $"{indent}Doc.Directive({directive.Value.Replace("\"", "\\\"")})",
                StringDoc stringDoc => indent + "\"" + stringDoc.Value.Replace("\"", "\\\"") + "\"",
                HardLine hardLine
                  => indent
                      + "Doc.HardLine"
                      + (hardLine.Squash ? "IfNoPreviousLine" : string.Empty)
                      + (
                          hardLine.SkipBreakIfFirstInGroup
                              ? "SkipBreakIfFirstInGroup"
                              : string.Empty
                      ),
                LiteralLine => indent + "Doc.LiteralLine",
                Concat concat => PrintConcat(concat),
                LineDoc lineDoc
                  => indent
                      + (lineDoc.Type == LineDoc.LineType.Normal ? "Doc.Line" : "Doc.SoftLine"),
                BreakParent => "",
                Align align
                  => $"{indent}Doc.Align({align.Width},{newLine}{PrintIndentedDocTree(align.Contents)}{newLine}{indent})",
                Trim => $"{indent}Doc.Trim",
                ForceFlat forceFlat
                  => $"{indent}Doc.ForceFlat({newLine}{PrintIndentedDocTree(forceFlat.Contents)})",
                IndentDoc indentDoc
                  => $"{indent}Doc.Indent({newLine}{PrintIndentedDocTree(indentDoc.Contents, indentDoc)}{newLine}{indent})",
                ConditionalGroup conditionalGroup
                  => $"{indent}Doc.ConditionalGroup({newLine}{PrintIndentedDocArray(conditionalGroup.Options)}{newLine}{indent})",
                Group group
                  => @$"{indent}Doc.Group{(@group.GroupId != null ? "WithId" : string.Empty)}(
{(@group.GroupId != null ? $"{nextIndent}\"{@group.GroupId}\",{newLine}" : string.Empty)}{PrintIndentedDocTree(@group.Contents, @group)}{newLine}{indent})",
                LeadingComment leadingComment
                  => $"{indent}Doc.LeadingComment(\"{leadingComment.Comment}\", CommentType.{(leadingComment.Type == CommentType.SingleLine ? "SingleLine" : "MultiLine")})",
                TrailingComment trailingComment
                  => $"{indent}Doc.TrailingComment(\"{trailingComment.Comment}\", CommentType.{(trailingComment.Type == CommentType.SingleLine ? "SingleLine" : "MultiLine")})",
                IndentIfBreak indentIfBreak
                  => @$"{indent}Doc.IndentIfBreak(
{PrintIndentedDocTree(indentIfBreak.FlatContents)},
{nextIndent}""{indentIfBreak.GroupId}"")",
                IfBreak ifBreak
                  => @$"{indent}Doc.IfBreak(
{PrintIndentedDocTree(ifBreak.BreakContents)},
{PrintIndentedDocTree(ifBreak.FlatContents)},
{nextIndent}""{ifBreak.GroupId}"")",
                _ => throw new Exception("Can't handle " + document)
            };
        }
    }
}
