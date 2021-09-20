using System;
using System.Linq;
using CSharpier.SyntaxPrinter;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace CSharpier.Tests.SyntaxPrinter
{
    public class PreprocessorSymbolsTests
    {
        [TestCase("BASIC_IF", "BASIC_IF")]
        [TestCase("!NOT_IF", "NOT_IF")]
        [TestCase("EQUALS_TRUE == true", "EQUALS_TRUE")]
        [TestCase("EQUALS_FALSE == false", "EQUALS_FALSE")]
        [TestCase("true == TRUE_EQUALS", "TRUE_EQUALS")]
        [TestCase("false == FALSE_EQUALS", "FALSE_EQUALS")]
        [TestCase("LEFT_OR || RIGHT_OR", "LEFT_OR")]
        [TestCase("LEFT_AND && RIGHT_AND", "LEFT_AND", "RIGHT_AND")]
        [TestCase("(EQUALS_TRUE_IN_PARENS == true)", "EQUALS_TRUE_IN_PARENS")]
        public void AddSymbolSet_For_If(string condition, params string[] symbols)
        {
            var trivia = WhenIfDirectiveHasCondition(condition);

            var result = AddSymbolSet(trivia);

            result.Should().ContainInOrder(symbols);
        }

        [Test]
        public void AddSymbolSet_For_Basic_Elif_Adds_Symbol()
        {
            var trivia = WhenElifDirectiveHasCondition("DEBUG");

            var result = AddSymbolSet(trivia);

            result.Should().ContainInOrder("DEBUG");
        }

        private static string[] AddSymbolSet(ConditionalDirectiveTriviaSyntax trivia)
        {
            TestablePreprocessorSymbols.Reset();

            TestablePreprocessorSymbols.AddSymbolSet(trivia);

            return TestablePreprocessorSymbols.GetSymbolSets().FirstOrDefault()
                ?? Array.Empty<string>();
        }

        private static IfDirectiveTriviaSyntax WhenIfDirectiveHasCondition(string condition)
        {
            return SyntaxFactory.IfDirectiveTrivia(
                SyntaxFactory.ParseExpression(condition),
                true,
                true,
                true
            );
        }

        private static ElifDirectiveTriviaSyntax WhenElifDirectiveHasCondition(string condition)
        {
            return SyntaxFactory.ElifDirectiveTrivia(
                SyntaxFactory.ParseExpression(condition),
                true,
                true,
                true
            );
        }

        private class TestablePreprocessorSymbols : PreprocessorSymbols
        {
            public static void AddSymbolSet(
                ConditionalDirectiveTriviaSyntax conditionalDirectiveTriviaSyntax
            ) {
                AddSymbolSetForConditional(conditionalDirectiveTriviaSyntax);
            }
        }
    }
}
