using System.Collections.Generic;

namespace WorkstationSearch
{
    // Ownership lasts for one rebuild. Taken rows return to the live list;
    // everything left in Unused must be destroyed before the next capture.
    internal sealed class RebuildPool<TKey, TRow> where TRow : class
    {
        private readonly Dictionary<TKey, Queue<TRow>> rows = new Dictionary<TKey, Queue<TRow>>();
        private readonly HashSet<TRow> unused = new HashSet<TRow>();
        internal IEnumerable<TRow> Unused => unused;
        internal bool Contains(TRow row) => unused.Contains(row);

        internal void Add(TKey key, TRow row)
        {
            if (!unused.Add(row)) return;
            if (!rows.TryGetValue(key, out var queue)) rows[key] = queue = new Queue<TRow>();
            queue.Enqueue(row);
        }

        internal TRow Take(TKey key)
        {
            if (!rows.TryGetValue(key, out var queue) || queue.Count == 0) return null;
            var row = queue.Dequeue();
            unused.Remove(row);
            return row;
        }

        internal void Clear()
        {
            rows.Clear();
            unused.Clear();
        }
    }
}
