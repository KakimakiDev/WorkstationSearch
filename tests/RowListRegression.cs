using System;
using System.Collections;
using System.Linq;
using WorkstationSearch;

internal static class RowListRegression
{
    internal static void Run(Action<string, bool> check)
    {
        // Two separate enchanted copies of the same weapon must keep their
        // original indices into the reclaim mod's per-item analysis contexts.
        var plainSword = new object();
        var enchantedSword = new object();
        var secondEnchantedSword = new object();
        var rows = new ArrayList { plainSword, enchantedSword, secondEnchantedSword };
        var snapshot = RowListPolicy.ApplyVisibleOrder(rows,
            new[] { secondEnchantedSword, enchantedSword }, true);
        check("reclaim filter preserves all item indices", rows.Count == 3 &&
            ReferenceEquals(rows[0], plainSword) && ReferenceEquals(rows[1], enchantedSword) &&
            ReferenceEquals(rows[2], secondEnchantedSword));
        check("reclaim visual ranking retains ownership", RowListPolicy.MatchesSnapshot(rows, snapshot));
        RowListPolicy.ApplyVisibleOrder(rows, Array.Empty<object>(), true);
        check("empty reclaim search retains cleanup ownership", rows.Count == 3);

        // Simulate reclaim destroying its old rows and replacing them before
        // Workstation Search next runs. Even the same count is a new generation.
        rows.Clear();
        rows.Add(new object()); rows.Add(new object()); rows.Add(new object());
        check("external rebuild invalidates cached rows", !RowListPolicy.MatchesSnapshot(rows, snapshot));
        rows.Clear();
        check("reclaiming last item invalidates cache", !RowListPolicy.MatchesSnapshot(rows, snapshot));
        check("released cache never owns an empty list", !RowListPolicy.MatchesSnapshot(rows, null));

        rows.Add(plainSword); rows.Add(enchantedSword); rows.Add(secondEnchantedSword);
        snapshot = RowListPolicy.ApplyVisibleOrder(rows, new[] { secondEnchantedSword, plainSword }, false);
        check("craft filtering still orders actual recipes", rows.Cast<object>().SequenceEqual(
            new[] { secondEnchantedSword, plainSword }));
        check("craft filtered snapshot remains current", RowListPolicy.MatchesSnapshot(rows, snapshot));
        RowListPolicy.ApplyVisibleOrder(rows, Array.Empty<object>(), false);
        check("craft empty search clears visible recipes", rows.Count == 0);
    }
}
