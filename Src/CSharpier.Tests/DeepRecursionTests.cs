using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Tests
{
    public class DeepRecursionTests
    {
        [Test]
        public void Format_Should_Return_Error_For_Deep_Recursion()
        {
            var code = uglyLongConcatenatedString;
            var result = CodeFormatter.Format(code, new PrinterOptions());

            result.FailureMessage.Should().Be("We can't handle this deep of recursion yet.");
        }

        private readonly string uglyLongConcatenatedString =
            @"public class ClassName 
{
    private string field = ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"" +
    ""1"";
}";
    }
}
