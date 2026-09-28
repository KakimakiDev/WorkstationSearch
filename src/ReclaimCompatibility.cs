using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace WorkstationSearch
{
    internal static class ReclaimCompatibility
    {
        internal static void Install(Harmony harmony, ManualLogSource log)
        {
            if (!Chainloader.PluginInfos.TryGetValue("Azumatt.Recycle_N_Reclaim", out var plugin)) return;
            var type = plugin.Instance.GetType().Assembly.GetType(
                "Recycle_N_Reclaim.GamePatches.UI.StationRecyclingTabHolder");
            var rebuild = type == null ? null : AccessTools.DeclaredMethod(type, "UpdateRecyclingList");
            if (rebuild == null)
            {
                log.LogWarning("Reclaim list compatibility unavailable: UpdateRecyclingList was not found.");
                return;
            }
            harmony.Patch(rebuild,
                prefix: new HarmonyMethod(typeof(ReclaimCompatibility), nameof(BeforeRebuild)) { priority = Priority.First },
                postfix: new HarmonyMethod(typeof(ReclaimCompatibility), nameof(AfterRebuild)) { priority = Priority.Last });
            log.LogInfo("Recycle_N_Reclaim search compatibility enabled");
        }

        private static void BeforeRebuild()
        {
            if (InventoryGui.instance) FavoriteRows.BeforeRebuild(InventoryGui.instance);
        }

        private static void AfterRebuild()
        {
            if (InventoryGui.instance) FavoriteRows.Capture(InventoryGui.instance, true);
        }
    }
}
