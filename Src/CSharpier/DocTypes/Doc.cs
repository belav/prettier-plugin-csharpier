using System.Collections.Generic;
using System.Linq;

namespace CSharpier.DocTypes
{
    public class Doc
    {
        public static implicit operator Doc(string value)
        {
            return new StringDoc(value);
        }

        public static NullDoc Null => NullDoc.Instance;

        public static Doc BreakParent => new BreakParent();

        public static readonly HardLine HardLine = new();

        public static readonly HardLine HardLineSkipBreakIfFirstInGroup = new(false, true);

        public static readonly HardLine HardLineIfNoPreviousLine = new(true);

        public static readonly HardLine HardLineIfNoPreviousLineSkipBreakIfFirstInGroup =
            new(true, true);

        public static readonly LiteralLine LiteralLine = new();

        public static readonly LineDoc Line = new() { Type = LineDoc.LineType.Normal };

        public static readonly LineDoc SoftLine = new() { Type = LineDoc.LineType.Soft };

        public static readonly Trim Trim = new();

        public static LeadingComment LeadingComment(string comment, CommentType commentType) =>
            new() { Type = commentType, Comment = comment };

        public static TrailingComment TrailingComment(string comment, CommentType commentType) =>
            new() { Type = commentType, Comment = comment };

        public static Doc Concat(List<Doc> contents) =>
            contents.Count == 1 ? contents[0] : new Concat(contents);

        public static Doc Concat(params Doc[] contents) =>
            contents.Length == 1 ? contents[0] : new Concat(contents);

        public static Doc Join(Doc separator, IEnumerable<Doc> array)
        {
            var docs = new List<Doc>();

            var list = array.ToList();

            if (list.Count == 1)
            {
                return list[0];
            }

            for (var x = 0; x < list.Count; x++)
            {
                if (x != 0)
                {
                    docs.Add(separator);
                }

                docs.Add(list[x]);
            }

            return Concat(docs);
        }

        public static ForceFlat ForceFlat(List<Doc> contents) =>
            new() { Contents = contents.Count == 0 ? contents[0] : Concat(contents) };

        public static ForceFlat ForceFlat(params Doc[] contents) =>
            new() { Contents = contents.Length == 0 ? contents[0] : Concat(contents) };

        public static Group Group(List<Doc> contents) =>
            new() { Contents = contents.Count == 1 ? contents[0] : Concat(contents) };

        public static Group GroupWithId(string groupId, List<Doc> contents)
        {
            var group = Group(contents);
            group.GroupId = groupId;
            return group;
        }

        public static Group GroupWithId(string groupId, params Doc[] contents)
        {
            var group = Group(contents);
            group.GroupId = groupId;
            return group;
        }

        public static Group Group(params Doc[] contents) =>
            new() { Contents = contents.Length == 1 ? contents[0] : Concat(contents) };

        public static IndentDoc Indent(params Doc[] contents) =>
            new() { Contents = contents.Length == 1 ? contents[0] : Concat(contents) };

        public static IndentDoc Indent(List<Doc> contents) => new() { Contents = Concat(contents) };

        public static Doc IndentIf(bool condition, Doc contents)
        {
            return condition ? Doc.Indent(contents) : contents;
        }

        public static IfBreak IfBreak(
            Doc breakContents,
            Doc flatContents,
            string? groupId = null
        ) =>
            new()
            {
                FlatContents = flatContents,
                BreakContents = breakContents,
                GroupId = groupId,
            };

        public static IndentIfBreak IndentIfBreak(Doc contents, string groupId) =>
            new(contents, groupId);

        public static Doc Directive(string value) => new StringDoc(value, true);

        public static ConditionalGroup ConditionalGroup(params Doc[] options) => new(options);

        public static Align Align(int alignment, params Doc[] contents) =>
            new(alignment, contents.Length == 1 ? contents[0] : Concat(contents));
    }

    public enum CommentType
    {
        SingleLine,
        MultiLine
    }

    interface IHasContents
    {
        Doc Contents { get; }
    }
}
