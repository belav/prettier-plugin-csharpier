using System;
using System.Text;
using CSharpier.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Tests.Utilities
{
    [TestFixture]
    public class StringDifferTests
    {
        [Test]
        public void PrintDifference_Should_Not_Print_Anything_If_Values_Are_Identical()
        {
            var result = PrintDifference("value", "value");

            result.Should().BeNullOrEmpty();
        }

        [Test]
        public void PrintDifference_Should_Print_Visible_Spaces()
        {
            var result = PrintDifference("value", "value   ");

            result.Should()
                .Be(
                    @"----------------------------- Expected: Around Line 1 -----------------------------
value
----------------------------- Actual: Around Line 1 -----------------------------
value···
"
                );
        }

        [Test]
        public void PrintDifference_Should_Print_Visible_Tabs()
        {
            var result = PrintDifference("value", "value\t");

            result.Should()
                .Be(
                    @"----------------------------- Expected: Around Line 1 -----------------------------
value
----------------------------- Actual: Around Line 1 -----------------------------
value→
"
                );
        }

        [Test]
        public void PrintDifference_Should_Print_LineEnding_Message()
        {
            var result = PrintDifference("lineEndings\r\ndiffer", "lineEndings\ndiffer");

            result.Should()
                .Be(
                    @"The file contained different line endings than formatting it would result in.
"
                );
        }

        [Test]
        public void PrintDifference_Should_Print_Single_Line_Difference()
        {
            var result = PrintDifference("one", "two");

            result.Should()
                .Be(
                    @"----------------------------- Expected: Around Line 1 -----------------------------
one
----------------------------- Actual: Around Line 1 -----------------------------
two
"
                );
        }

        [Test]
        public void PrintDifference_Should_Print_Multi_Line_Difference()
        {
            var result = PrintDifference(
                @"one
two
four",
                @"one
two
three
four"
            );

            result.Should()
                .Be(
                    @"----------------------------- Expected: Around Line 3 -----------------------------
two
four
----------------------------- Actual: Around Line 3 -----------------------------
two
three
four
"
                );
        }

        [Test]
        public void PrintDifference_Should_Print_Extra_Line_Difference()
        {
            var result = PrintDifference(
                @"one
two
three",
                @"one
two
three
four"
            );

            result.Should()
                .Be(
                    @"----------------------------- Expected: Around Line 4 -----------------------------
three

----------------------------- Actual: Around Line 4 -----------------------------
three
four
"
                );
        }

        private static string PrintDifference(string expected, string actual)
        {
            return StringDiffer.PrintFirstDifference(expected, actual);
        }
    }
}
