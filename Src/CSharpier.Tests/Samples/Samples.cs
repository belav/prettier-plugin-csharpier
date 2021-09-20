using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Tests.Samples
{
    public class Samples
    {
        [Test]
        public void Scratch()
        {
            RunTest("Scratch");
        }

        [Test]
        public void AllInOne()
        {
            RunTest("AllInOne");
        }

        public static void RunTest(string fileName)
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory.Name != "CSharpier.Tests")
            {
                directory = directory.Parent;
            }

            var file = Path.Combine(directory.FullName, $"Samples/{fileName}.cst");

            var code = File.ReadAllText(file);
            var result = new CodeFormatter().Format(
                code,
                new PrinterOptions { IncludeDocTree = true, IncludeAST = true, }
            );

            var syntaxNodeComparer = new SyntaxNodeComparer(
                code,
                result.Code,
                CancellationToken.None
            );

            var compareResult = syntaxNodeComparer.CompareSource();
            compareResult.Should().BeEmpty();

            File.WriteAllText(file.Replace(".cst", ".actual.cst"), result.Code, Encoding.UTF8);
            File.WriteAllText(file.Replace(".cst", ".doctree.txt"), result.DocTree, Encoding.UTF8);
        }
    }
}
