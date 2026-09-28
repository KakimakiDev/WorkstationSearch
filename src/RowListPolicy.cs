using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WorkstationSearch
{
    internal static class RowListPolicy
    {
        internal static bool MatchesSnapshot(IList current, IEnumerable<object> snapshot) =>
            snapshot != null && snapshot.SequenceEqual(current.Cast<object>());

        internal static List<object> ApplyVisibleOrder(IList current, IList<object> visible, bool preserveIndices)
        {
            if (!preserveIndices)
            {
                current.Clear();
                foreach (var row in visible) current.Add(row);
            }
            return current.Cast<object>().ToList();
        }
    }
}
