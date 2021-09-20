using System.Collections.Generic;
using System.Linq;
using CSharpier.DocTypes;
using FluentAssertions;
using NUnit.Framework;

namespace CSharpier.Tests
{
    public class DocUtilitiesTests
    {
        [Test]
        public void RemoveInitialDoubleHardLine_Should_Handle_Empty_List()
        {
            var doc = new List<Doc>();

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            doc.Should().BeEmpty();
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Not_Remove_Simple_HardLine()
        {
            var doc = new List<Doc> { Doc.HardLine };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            doc.Should().HaveCount(1);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Remove_Simple_Double_HardLine()
        {
            var doc = new List<Doc> { Doc.HardLine, Doc.HardLine };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            doc.Should().BeEquivalentTo(new List<Doc> { Doc.HardLine, Doc.Null });
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Not_Remove_Concated_HardLine()
        {
            var concat = new Concat(new List<Doc> { Doc.HardLine });
            var doc = new List<Doc> { concat };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            concat.Contents.Should().BeEquivalentTo(Doc.HardLine);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Remove_Concated_HardLine()
        {
            var concat = Doc.Concat(Doc.HardLine, Doc.HardLine);
            var doc = new List<Doc> { concat };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            concat.Should().BeEquivalentTo(Doc.Concat(Doc.HardLine, Doc.Null) as Concat);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Not_Remove_Deep_Concated_HardLine()
        {
            var concat = new Concat(new List<Doc> { Doc.HardLine });
            var doc = new List<Doc> { new Concat(new List<Doc> { concat }) };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            concat.Contents.Should().BeEquivalentTo(Doc.HardLine);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Remove_Deep_Concated_HardLine()
        {
            var concat = Doc.Concat(Doc.HardLine, Doc.HardLine) as Concat;
            var doc = new List<Doc> { new Concat(new List<Doc> { concat }) };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            concat!.Contents.Should().BeEquivalentTo(new List<Doc> { Doc.HardLine, Doc.Null });
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Remove_Single_HardLine()
        {
            var concat = Doc.Concat(Doc.HardLine, Doc.HardLine, Doc.HardLine) as Concat;
            var doc = new List<Doc> { new Concat(new List<Doc> { concat }) };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            concat.Contents.Should()
                .BeEquivalentTo(new List<Doc> { Doc.HardLine, Doc.Null, Doc.HardLine });
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Not_Remove_Indented_HardLine()
        {
            var indent = Doc.Indent(Doc.HardLine);
            var doc = new List<Doc> { indent };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            indent.Contents.Should().BeEquivalentTo(Doc.HardLine);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Remove_Indented_HardLine()
        {
            var indent = Doc.Indent(Doc.HardLine);
            var doc = new List<Doc> { Doc.HardLine, indent };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            indent.Contents.Should().Be(Doc.Null);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Not_Remove_Deep_Indented_HardLine()
        {
            var indent = Doc.Indent(Doc.HardLine);
            var doc = new List<Doc> { Doc.Indent(indent) };

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            indent.Contents.Should().BeEquivalentTo(Doc.HardLine);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Remove_Grouped_Double_HardLine()
        {
            var contents = new List<Doc> { Doc.HardLine, Doc.HardLine };
            var doc = Doc.Group(contents);

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            contents.Should().BeEquivalentTo(new List<Doc> { Doc.HardLine, Doc.Null });
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Should_Only_Remove_Initial_HardLines()
        {
            var doc = Doc.Concat("1", Doc.HardLine, Doc.HardLine);

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            doc.Should().BeEquivalentTo(Doc.Concat("1", Doc.HardLine, Doc.HardLine) as Concat);
        }

        [Test]
        public void RemoveInitialDoubleHardLine_Work_With_Doc_Null_Before_String()
        {
            var doc = Doc.Concat(Doc.HardLine, Doc.Null, "1", Doc.HardLine, "2");

            DocUtilities.RemoveInitialDoubleHardLine(doc);

            DocSerializer.Serialize(doc)
                .Should()
                .Be(
                    DocSerializer.Serialize(
                        Doc.Concat(Doc.HardLine, Doc.Null, "1", Doc.HardLine, "2")
                    )
                );
        }
    }
}
