using System;
using System.Linq;
using WorkstationSearch;

internal static class RebuildPoolRegression
{
    internal static void Run(Action<string, bool> check)
    {
        var pool = new RebuildPool<string, object>();
        var sword = new object();
        var secondSword = new object();
        var hidden = new object();
        pool.Add("sword", sword);
        pool.Add("sword", sword);
        pool.Add("sword", secondSword);
        pool.Add("hidden", hidden);
        check("rebuild retains hidden rows for cleanup", pool.Contains(hidden));
        check("same recipe keeps its row", ReferenceEquals(pool.Take("sword"), sword));
        check("taken row is no longer scheduled for cleanup", !pool.Contains(sword));
        check("duplicate recipes receive distinct rows", ReferenceEquals(pool.Take("sword"), secondSword));
        check("one row cannot be reused twice", pool.Take("sword") == null);
        check("newly unlocked recipes need new rows", pool.Take("new") == null);
        check("removed recipes are the only cleanup candidates", pool.Unused.SequenceEqual(new[] { hidden }));
        pool.Clear();
        check("tab transition clears all pool ownership", !pool.Unused.Any() && pool.Take("hidden") == null);
        pool.Add("sword", sword);
        check("later rebuild can retain the same live row", ReferenceEquals(pool.Take("sword"), sword));
    }
}
