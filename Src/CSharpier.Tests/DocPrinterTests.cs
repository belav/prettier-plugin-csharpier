using System;
using CSharpier.DocTypes;
using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Tests
{
    public class DocPrinterTests
    {
        private static readonly string NewLine = System.Environment.NewLine;

        [Test]
        public void Lines_Allowed()
        {
            var doc = Doc.Concat(Doc.HardLine, "1");

            PrintedDocShouldBe(doc, $"{NewLine}1");
        }

        [Test]
        public void Basic_Concat()
        {
            var doc = Doc.Concat("1", "2", "3");

            PrintedDocShouldBe(doc, "123");
        }

        [Test]
        public void Concat_With_Hardline()
        {
            var doc = Doc.Concat("1", Doc.HardLine, "3");

            PrintedDocShouldBe(doc, $"1{NewLine}3");
        }

        [Test]
        public void Concat_With_Line()
        {
            var doc = Doc.Concat("1", Doc.Line, "3");

            PrintedDocShouldBe(doc, $"1{NewLine}3");
        }

        [Test]
        public void Group_With_Line()
        {
            var doc = Doc.Group(Doc.Concat("1", Doc.Line, "3"));

            PrintedDocShouldBe(doc, "1 3");
        }

        [Test]
        public void Group_With_HardLine()
        {
            var doc = Doc.Group(Doc.Concat("1", Doc.HardLine, "3"));

            PrintedDocShouldBe(doc, $"1{NewLine}3");
        }

        [Test]
        public void Group_With_Line_And_Hardline()
        {
            var doc = Doc.Group(Doc.Concat("1", Doc.Line, "2", Doc.HardLine, "3"));

            PrintedDocShouldBe(doc, $"1{NewLine}2{NewLine}3");
        }

        [Test]
        public void Group_With_Line_And_BreakParent()
        {
            var doc = Doc.Group(Doc.Concat("1", Doc.Line, "2", Doc.Line, "3", Doc.BreakParent));

            PrintedDocShouldBe(doc, $"1{NewLine}2{NewLine}3");
        }

        [Test]
        public void Indent_With_BreakParent()
        {
            var doc = Doc.Concat(
                "0",
                Doc.Group(
                    Doc.Indent(
                        Doc.Concat(Doc.SoftLine, "1", Doc.Line, "2", Doc.Line, "3", Doc.BreakParent)
                    )
                )
            );

            PrintedDocShouldBe(doc, $"0{NewLine}    1{NewLine}    2{NewLine}    3");
        }

        [Test]
        public void Large_Group_Concat_With_Line()
        {
            const string longText = "LongTextLongTextLongTextLongText";
            var doc = Doc.Group(Doc.Concat(longText, Doc.Line, longText, Doc.Line, longText));

            PrintedDocShouldBe(doc, $"{longText}{NewLine}{longText}{NewLine}{longText}", 80);
        }

        [Test]
        public void Indent_With_Hardline()
        {
            var doc = Doc.Concat("0", Doc.Indent(Doc.Concat(Doc.HardLine, "1", Doc.HardLine, "2")));

            PrintedDocShouldBe(doc, $"0{NewLine}    1{NewLine}    2");
        }

        [Test]
        public void Two_Indents_With_Hardline()
        {
            var doc = Doc.Concat(
                "0",
                Doc.Concat(
                    Doc.Indent(Doc.Concat(Doc.HardLine, "1")),
                    Doc.HardLine,
                    Doc.Indent(Doc.Concat(Doc.HardLine, "2"))
                )
            );

            PrintedDocShouldBe(doc, $"0{NewLine}    1{NewLine}{NewLine}    2");
        }

        [Test]
        public void Lines_Removed_From_Beginning()
        {
            var doc = Doc.Concat(Doc.HardLine, "1");

            PrintedDocShouldBe(doc, "1", trimInitialLines: true);
        }

        [Test]
        public void Literal_Lines_Removed_From_Beginning()
        {
            var doc = Doc.Concat(Doc.LiteralLine, "1");

            PrintedDocShouldBe(doc, $"1");
        }

        [Test]
        public void ForceFlat_Prevents_Breaking()
        {
            var doc = Doc.ForceFlat("1", Doc.HardLine, "2");

            PrintedDocShouldBe(doc, "1 2");
        }

        [Test]
        public void ForceFlat_Does_Not_Affect_Literal_Lines()
        {
            var doc = Doc.ForceFlat("1", Doc.LiteralLine, "2");

            PrintedDocShouldBe(doc, $"1{NewLine}2");
        }

        [Test]
        public void ForceFlat_With_Literal_String_Still_Breaks_Group()
        {
            var doc = Doc.Group(Doc.ForceFlat("1", Doc.LiteralLine, "2"), Doc.Line, "3");

            PrintedDocShouldBe(doc, $"1{NewLine}2{NewLine}3");
        }

        [Test]
        public void LiteralLine_Trims_Space()
        {
            var doc = Doc.Concat("{", Doc.Indent(Doc.HardLine, "indent", Doc.LiteralLine), "}");

            PrintedDocShouldBe(doc, $"{{{NewLine}    indent{NewLine}}}");
        }

        [Test]
        public void LiteralLine_Does_Not_Trim_Output_Or_Indent_Next_Line()
        {
            var doc = Doc.Concat(
                "(",
                Doc.Indent(Doc.HardLine, "1", " ", Doc.LiteralLine, "2", Doc.HardLine, "3"),
                ")"
            );

            PrintedDocShouldBe(doc, $"({NewLine}    1 {NewLine}2{NewLine}    3)");
        }

        [Test]
        public void ForceFlat_Prevents_Breaking_With_Long_Content()
        {
            var doc = Doc.ForceFlat(
                "lkjasdkfljalsjkdfkjlasdfjklakljsdfjkasdfkljsdafjk",
                Doc.Line,
                "jaksdlflkasdlfjkajklsdfkljasfjklaslfkjasdfkj"
            );

            PrintedDocShouldBe(
                doc,
                $"lkjasdkfljalsjkdfkjlasdfjklakljsdfjkasdfkljsdafjk jaksdlflkasdlfjkajklsdfkljasfjklaslfkjasdfkj"
            );
        }

        [Test]
        public void Long_Statement_With_Line_Should_Not_Break_Unrelated_Group()
        {
            var doc = Doc.Concat(
                "1",
                Doc.Group(Doc.Line, Doc.Concat("2")),
                Doc.HardLine,
                Doc.Concat(
                    "1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111",
                    Doc.Line,
                    "2"
                )
            );
            PrintedDocShouldBe(
                doc,
                $"1 2{NewLine}1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111{NewLine}2"
            );
        }

        [Test]
        public void ForceFlat_Should_Be_Included_In_Fits_Logic_Of_Printer()
        {
            var doc = Doc.Group(
                Doc.ForceFlat(
                    Doc.Concat(
                        "1111111111",
                        " ",
                        "1111111111",
                        " ",
                        "1111111111",
                        " ",
                        "1111111111",
                        " ",
                        "1111111111"
                    )
                ),
                Doc.Line,
                Doc.ForceFlat(
                    Doc.Concat(
                        "1111111111",
                        " ",
                        "1111111111",
                        " ",
                        "1111111111",
                        " ",
                        "1111111111",
                        " ",
                        "1111111111"
                    )
                )
            );

            PrintedDocShouldBe(
                doc,
                $"1111111111 1111111111 1111111111 1111111111 1111111111{NewLine}1111111111 1111111111 1111111111 1111111111 1111111111"
            );
        }

        [Test]
        public void IfBreak_Should_Print_Flat_Contents()
        {
            var doc = Doc.Group(Doc.IfBreak("break", "flat"));

            PrintedDocShouldBe(doc, "flat");
        }

        [Test]
        public void IfBreak_Should_Print_Break_Contents()
        {
            var doc = Doc.Group(Doc.HardLine, Doc.IfBreak("break", "flat"));

            PrintedDocShouldBe(doc, $"{NewLine}break");
        }

        [Test]
        public void IfBreak_Should_Print_Break_Contents_When_Group_Does_Not_Fit()
        {
            var doc = Doc.Group("another", Doc.Line, Doc.IfBreak("break", "flat"));

            PrintedDocShouldBe(doc, $"another{NewLine}break", 10);
        }

        [Test]
        public void IfBreak_Should_Print_Flat_Contents_When_GroupId_Does_Not_Break()
        {
            var doc = Doc.Concat(Doc.GroupWithId("id", "1"), Doc.IfBreak("break", "flat", "id"));

            PrintedDocShouldBe(doc, "1flat");
        }

        [Test]
        public void IfBreak_Should_Throw_Exception_When_It_Targets_Not_Yet_Printed_Group()
        {
            var doc = Doc.Concat(Doc.IfBreak("break", "flat", "id"), Doc.GroupWithId("id", "1"));

            this.Invoking(o => PrintedDocShouldBe(doc, "flat1"))
                .Should()
                .Throw<Exception>()
                .WithMessage("You cannot use an ifBreak before the group it targets.");
            ;
        }

        [Test]
        public void IfBreak_Should_Print_Break_Contents_When_GroupId_Breaks()
        {
            var doc = Doc.Concat(
                "1",
                Doc.GroupWithId("hl", Doc.HardLine),
                Doc.IfBreak("break", "flat", "hl")
            );

            PrintedDocShouldBe(doc, $"1{NewLine}break");
        }

        [Test]
        public void IfBreak_Should_Propagate_Breaks_From_Break_Contents()
        {
            var doc = Doc.Group(
                "1",
                Doc.Line,
                Doc.IfBreak(Doc.Concat("break", Doc.HardLine), "flat")
            );

            PrintedDocShouldBe(doc, $"1{NewLine}break");
        }

        [Test]
        public void IfBreak_Should_Propagate_Breaks_From_Flat_Contents()
        {
            var doc = Doc.Group(
                "1",
                Doc.Line,
                Doc.IfBreak("break", Doc.Concat("flat", Doc.HardLine))
            );

            PrintedDocShouldBe(doc, $"1{NewLine}break");
        }

        [Test]
        public void IndentIfBreak_Should_Print_Indented_When_Breaks()
        {
            var doc = Doc.Concat(
                Doc.GroupWithId("h1", Doc.Indent(Doc.HardLine)),
                Doc.IndentIfBreak("indent", "h1")
            );

            PrintedDocShouldBe(doc, $"{NewLine}    indent");
        }

        [Test]
        public void IndentIfBreak_Should_Not_Print_Indented_When_Flat()
        {
            var doc = Doc.Concat(
                Doc.GroupWithId("h1", Doc.Indent(Doc.SoftLine)),
                Doc.IndentIfBreak("indent", "h1")
            );

            PrintedDocShouldBe(doc, "indent");
        }

        [TestCase(" ")]
        [TestCase("   ")]
        public void Trim_Should_Trim_Current_Line(string indent)
        {
            var doc = Doc.Concat($"1{indent}", Doc.Trim);

            PrintedDocShouldBe(doc, "1");
        }

        [TestCase("1")]
        [TestCase("")]
        public void Trim_Should_Not_Trim_NonWhitespace(string value)
        {
            var doc = Doc.Concat(value, Doc.Trim);

            PrintedDocShouldBe(doc, value);
        }

        [Test]
        public void Trim_Should_Trim_Indentation()
        {
            var doc = Doc.Concat(
                "{",
                Doc.Indent(Doc.HardLine, "1", Doc.HardLine, Doc.Trim, "#if"),
                Doc.HardLine,
                "}"
            );

            PrintedDocShouldBe(doc, $"{{{NewLine}    1{NewLine}#if{NewLine}}}");
        }

        [Test]
        public void Trim_Should_Not_Affect_Fits()
        {
            var doc = Doc.Group("test    ", Doc.Trim, Doc.Line, "test");

            PrintedDocShouldBe(doc, "test test", 10);
        }

        [Test]
        public void TrailingComment_Does_Not_Get_Extra_Space()
        {
            var doc = Doc.Concat(
                "x",
                Doc.TrailingComment("// comment", CommentType.SingleLine),
                " ",
                "y"
            );

            PrintedDocShouldBe(doc, $"x // comment{NewLine}y");
        }

        [Test]
        public void TrailingComment_Does_Not_Get_Extra_Space_From_Line()
        {
            var doc = Doc.Group(
                "x",
                Doc.TrailingComment("// comment", CommentType.SingleLine),
                Doc.Line,
                "y"
            );

            PrintedDocShouldBe(doc, $"x // comment{NewLine}y");
        }

        [Test]
        public void TrailingComment_Does_Affect_Trim()
        {
            var doc = Doc.Concat(
                Doc.TrailingComment("// trailing", CommentType.SingleLine),
                Doc.HardLine,
                Doc.Indent(
                    Doc.HardLineIfNoPreviousLineSkipBreakIfFirstInGroup,
                    Doc.Trim,
                    "    #endregion"
                )
            );
            PrintedDocShouldBe(doc, $" // trailing{NewLine}    #endregion");
        }

        [Test]
        public void HardLineIfNoPreviousLine_Should_Insert_Line_If_There_Is_Not_One()
        {
            var doc = Doc.Concat("1", Doc.HardLineIfNoPreviousLine, "2");

            PrintedDocShouldBe(doc, $"1{NewLine}2");
        }

        [Test]
        public void HardLineIfNoPreviousLine_Should_Not_Insert_Line_If_There_Is_One()
        {
            var doc = Doc.Concat("1", Doc.HardLine, Doc.HardLineIfNoPreviousLine, "2");

            PrintedDocShouldBe(doc, $"1{NewLine}2");
        }

        [Test]
        public void HardLineIfNoPreviousLine_Does_Not_Blow_Up()
        {
            var doc = Doc.Concat(Doc.HardLineIfNoPreviousLine, "1");

            PrintedDocShouldBe(doc, $"{NewLine}1");
        }

        [Test]
        public void HardLineIfNoPreviousLine_Should_Not_Insert_After_Indented_HardLine()
        {
            var doc = Doc.Concat(Doc.Indent("1", Doc.HardLine, Doc.HardLineIfNoPreviousLine, "2"));

            PrintedDocShouldBe(doc, $"1{NewLine}    2");
        }

        [Test]
        public void HardLineSkipBreakIfFirstInGroup_Should_Not_Break()
        {
            var doc = Doc.Group(Doc.HardLineSkipBreakIfFirstInGroup, "1", Doc.Line, "2");

            PrintedDocShouldBe(doc, $"{NewLine}1 2");
        }

        [Test]
        public void HardLineSkipBreakIfFirstInGroup_With_HardLine_Should_Break()
        {
            var doc = Doc.Group(
                Doc.HardLineSkipBreakIfFirstInGroup,
                "1",
                Doc.HardLine,
                "2",
                Doc.Line,
                "3"
            );

            PrintedDocShouldBe(doc, $"{NewLine}1{NewLine}2{NewLine}3");
        }

        [Test]
        public void HardLineSkipBreakIfFirstInGroup_Should_Break_Outer_Group()
        {
            var doc = Doc.Group(
                "1",
                Doc.Line,
                "2",
                Doc.Group(Doc.HardLineSkipBreakIfFirstInGroup, "3")
            );

            PrintedDocShouldBe(doc, $"1{NewLine}2{NewLine}3");
        }

        [Test]
        public void HardLineSkipBreakIfFirstInGroup_Twice_Should_Not_Break()
        {
            var doc = Doc.Group(
                Doc.HardLineSkipBreakIfFirstInGroup,
                Doc.HardLineSkipBreakIfFirstInGroup,
                "1",
                Doc.Line,
                "2"
            );

            PrintedDocShouldBe(doc, $"{NewLine}{NewLine}1 2");
        }

        [Test]
        public void HardLineIfNoPreviousLineSkipBreakIfFirstInGroup_Should_Not_Break()
        {
            var doc = Doc.Group(
                Doc.HardLineIfNoPreviousLineSkipBreakIfFirstInGroup,
                "1",
                Doc.Line,
                "2"
            );

            PrintedDocShouldBe(doc, $"{NewLine}1 2");
        }

        [Test]
        public void HardLineIfNoPreviousLineSkipBreakIfFirstInGroup_Should_Break_When_Does_Not_Fit()
        {
            var doc = Doc.Group(
                Doc.HardLineSkipBreakIfFirstInGroup,
                "someValue",
                Doc.Line,
                "someValue"
            );
            PrintedDocShouldBe(
                doc,
                @"
someValue
someValue",
                10
            );
        }

        [Test]
        public void HardLineIfNoPreviousLineSkipBreakIfFirstInGroup_Should_Not_Break_With_Multiple_Comments()
        {
            var doc = Doc.Group(
                Doc.LeadingComment("// shouldn't break next line", CommentType.SingleLine),
                Doc.HardLineSkipBreakIfFirstInGroup,
                Doc.LeadingComment("// shouldn't break next line", CommentType.SingleLine),
                Doc.HardLineSkipBreakIfFirstInGroup,
                "true",
                Doc.Line,
                "|| false"
            );
            PrintedDocShouldBe(
                doc,
                @"// shouldn't break next line
// shouldn't break next line
true || false"
            );
        }

        [Test]
        public void Directive_Does_Not_Affect_FirstInGroup()
        {
            var doc = Doc.Group(
                Doc.Directive("#pragma"),
                Doc.HardLineSkipBreakIfFirstInGroup,
                "1",
                Doc.Line,
                "2"
            );

            PrintedDocShouldBe(doc, $"#pragma{NewLine}1 2");
        }

        [Test]
        public void Conditional_Group_Should_Print_Basic_Group()
        {
            var doc = Doc.ConditionalGroup("short");
            PrintedDocShouldBe(doc, "short");
        }

        [Test]
        public void Conditional_Group_Should_Print_Group_That_Fits()
        {
            var doc = Doc.ConditionalGroup(
                "tooLong tooLong",
                Doc.Concat("tooLong", Doc.HardLine, "tooLong")
            );
            PrintedDocShouldBe(doc, $"tooLong{NewLine}tooLong", 10);
        }

        [Test]
        public void Conditional_Group_Prints_As_Group()
        {
            var doc = Doc.ConditionalGroup(Doc.Concat("1", Doc.Line, "2"));
            PrintedDocShouldBe(doc, "1 2", 10);
        }

        [Test]
        public void Conditional_Group_Does_Propagate_Breaks_Within_Options()
        {
            var doc = Doc.ConditionalGroup(
                "ThisContentIsTooLong",
                Doc.Group("1", Doc.Line, "2", Doc.HardLine, "3")
            );
            PrintedDocShouldBe(doc, $"1{NewLine}2{NewLine}3", 10);
        }

        [Test]
        public void Conditional_Group_Does_Not_Propagate_Breaks_To_Parent()
        {
            var doc = Doc.Group(
                Doc.ConditionalGroup(
                    Doc.Concat("1", Doc.Line, "2"),
                    Doc.Concat("11", Doc.HardLine, "22")
                )
            );
            PrintedDocShouldBe(doc, "1 2", 10);
        }

        [Test]
        public void Align_Should_Print_Basic_Case()
        {
            var doc = Doc.Concat("+ ", Doc.Align(2, Doc.Group("1", Doc.HardLine, "2")));
            PrintedDocShouldBe(doc, $"+ 1{NewLine}  2");
        }

        [Test]
        public void Align_Should_Convert_Non_Trailing_Spaces_To_Tabs()
        {
            var doc = Doc.Concat(
                "+ ",
                Doc.Align(
                    2,
                    Doc.Indent(Doc.Concat("+ ", Doc.Align(2, Doc.Group("1", Doc.HardLine, "2"))))
                )
            );
            PrintedDocShouldBe(doc, $"+ + 1{NewLine}\t\t  2", useTabs: true);
        }

        [Test]
        public void Scratch()
        {
            var doc = "";
            PrintedDocShouldBe(doc, "");
        }

        private static void PrintedDocShouldBe(
            Doc doc,
            string expected,
            int width = PrinterOptions.WidthUsedByTests,
            bool trimInitialLines = false,
            bool useTabs = false
        ) {
            var result = Print(doc, width, trimInitialLines, useTabs);

            result.Should().Be(expected);
        }

        private static string Print(
            Doc doc,
            int width = PrinterOptions.WidthUsedByTests,
            bool trimInitialLines = false,
            bool useTabs = false
        ) {
            return DocPrinter.DocPrinter.Print(
                    doc,
                    new PrinterOptions
                    {
                        Width = width,
                        TrimInitialLines = trimInitialLines,
                        UseTabs = useTabs
                    },
                    Environment.NewLine
                )
                .TrimEnd('\r', '\n');
        }
    }
}
