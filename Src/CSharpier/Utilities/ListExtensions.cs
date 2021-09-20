using System;
using System.Collections.Generic;
using System.Threading.Tasks.Sources;
using CSharpier.DocTypes;

namespace CSharpier.Utilities
{
    public static class ListExtensions
    {
        public static void Add(this List<Doc> list, params Doc[] values)
        {
            if (values.Length == 1 && values[0] == Doc.Null)
            {
                return;
            }
            list.AddRange(values);
        }

        public static void AddIfNotNull(this List<Doc> value, Doc doc)
        {
            if (doc != Doc.Null)
            {
                value.Add(doc);
            }
        }
    }
}
