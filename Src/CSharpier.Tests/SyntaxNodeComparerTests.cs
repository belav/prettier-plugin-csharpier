using System;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace CSharpier.Tests
{
    public class SyntaxNodeComparerTests
    {
        [Test]
        public void Class_Not_Equal_Namespace()
        {
            var left = "class ClassName { }";
            var right = @"namespace Namespace { }";

            var result = AreEqual(left, right);

            ResultShouldBe(
                result,
                @"----------------------------- Original: Around Line 0 -----------------------------
class ClassName { }
----------------------------- Formatted: Around Line 0 -----------------------------
namespace Namespace { }
"
            );
        }

        [Test]
        public void Class_Not_Equal_Class_Different_Whitespace()
        {
            var left = "class ClassName { }";
            var right =
                @"class ClassName {
}";

            var result = AreEqual(left, right);

            result.Should().BeEmpty();
        }

        [Test]
        public void Constructor_Missing_Base()
        {
            var left =
                @"public class ConstructorWithBase
{
    public ConstructorWithBase(string value)
        : base(value)
    {
        return;
    }
}";
            var right =
                @"public class ConstructorWithBase
{
    public ConstructorWithBase(string value)
    {
        return;
    }
}
";

            var result = AreEqual(left, right);

            result.Should()
                .Be(
                    @"----------------------------- Original: Around Line 2 -----------------------------
public class ConstructorWithBase
{
    public ConstructorWithBase(string value)
        : base(value)
    {
        return;
    }
}
----------------------------- Formatted: Around Line 2 -----------------------------
public class ConstructorWithBase
{
    public ConstructorWithBase(string value)
    {
        return;
    }
}
"
                );
        }

        [Test]
        public void MissingAttribute()
        {
            var left =
                @"class Resources
{
    [Obsolete]
    public Resources()
    {
    }
}
";
            var right =
                @"class Resources
{
    public Resources() { }
}
";

            var result = AreEqual(left, right);

            ResultShouldBe(
                result,
                @"----------------------------- Original: Around Line 2 -----------------------------
class Resources
{
    [Obsolete]
    public Resources()
    {
    }
}
----------------------------- Formatted: Around Line 2 -----------------------------
class Resources
{
    public Resources() { }
}
"
            );
        }

        [Test]
        public void SeperatedSyntaxLists()
        {
            var left =
                @"namespace Insite.Automated.Core
{
    using System;

    public class DropdownAttribute : Attribute
    {
        public bool IgnoreIfNotPresent { get; set; }

        public DropdownAttribute()
        {
        }

        public DropdownAttribute(bool ignoreIfNotPresent)
        {
            this.IgnoreIfNotPresent = ignoreIfNotPresent;
        }
    }
}";

            var right =
                @"namespace Insite.Automated.Core
{
    using System;

    public class DropdownAttribute : Attribute
    {
        public bool IgnoreIfNotPresent { get; set; }

        public DropdownAttribute() { }

        public DropdownAttribute(bool ignoreIfNotPresent)
        {
            this.IgnoreIfNotPresent = ignoreIfNotPresent;
        }
    }
}
";

            var result = AreEqual(left, right);

            result.Should().BeEmpty();
        }

        [Test]
        public void MissingSemiColon()
        {
            var left =
                @"public enum Enum
{
    Integer,
    String,
};";
            var right =
                @"public enum Enum
{
    Integer,
    String,
}";
            var result = AreEqual(left, right);

            ResultShouldBe(
                result,
                @"----------------------------- Original: Around Line 4 -----------------------------
    Integer,
    String,
};
----------------------------- Formatted: Around Line 0 -----------------------------
public enum Enum
{
    Integer,
    String,
}
"
            );
        }

        [Test]
        public void Extra_SyntaxTrivia_Should_Work()
        {
            var left = "  public class ClassName { }";
            var right = "public class ClassName { }";

            var result = AreEqual(left, right);
            result.Should().BeEmpty();
        }

        [Test]
        public void Mismatched_Syntax_Trivia_Should_Print_Error()
        {
            var left =
                @"// leading comment
public class ClassName { }";
            var right = "public class ClassName { }";

            var result = AreEqual(left, right);
            ResultShouldBe(
                result,
                @"----------------------------- Original: Around Line 0 -----------------------------
// leading comment
public class ClassName { }
----------------------------- Formatted: Around Line 0 -----------------------------
public class ClassName { }
"
            );
        }

        [Test]
        public void Long_Mismatched_Syntax_Trivia_Should_Print_Error()
        {
            var left =
                @"// 1
// 2
// 3
// 4
// 5
// 6
public class ClassName { }";
            var right =
                @"// 1
// 2
// 3
// 4
// 5
// 7
public class ClassName { }";

            var result = AreEqual(left, right);
            ResultShouldBe(
                result,
                @"----------------------------- Original: Around Line 5 -----------------------------
// 4
// 5
// 6
public class ClassName { }
----------------------------- Formatted: Around Line 5 -----------------------------
// 4
// 5
// 7
public class ClassName { }
"
            );
        }

        [TestCase("@")]
        [TestCase("@$")]
        [TestCase("$@")]
        public void Mismatched_Line_Endings_In_Verbatim_String_Should_Not_Print_Error(string start)
        {
            var left =
                "public class ClassName\n{\npublic string Value = "
                + start
                + "\"EndThisLineWith\r\nEndThisLineWith\n\";\n}";
            var right =
                "public class ClassName\n{\npublic string Value = "
                + start
                + "\"EndThisLineWith\nEndThisLineWith\n\";\n}";

            var result = AreEqual(left, right);
            result.Should().BeEmpty();
        }

        [Test]
        public void Mismatched_Disabled_Text_Should_Not_Print_Error()
        {
            var left =
                @"class ClassName
{
#if DEBUG
    public string    Tester;
#endif
}";
            var right =
                @"class ClassName
{
#if DEBUG
    public string Tester;
#endif
}
";

            var result = AreEqual(left, right);
            result.Should().BeEmpty();
        }

        private static void ResultShouldBe(string result, string be)
        {
            if (Environment.GetEnvironmentVariable("NormalizeLineEndings") != null)
            {
                be = be.Replace("\r\n", "\n");
            }

            result.Should().Be(be);
        }

        private static string AreEqual(string left, string right)
        {
            var result = new SyntaxNodeComparer(
                left,
                right,
                CancellationToken.None
            ).CompareSource();

            if (Environment.GetEnvironmentVariable("NormalizeLineEndings") != null)
            {
                result = result.Replace("\r\n", "\n");
            }

            return result;
        }
    }
}
