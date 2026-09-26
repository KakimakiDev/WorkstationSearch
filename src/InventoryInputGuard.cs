using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace WorkstationSearch
{
    [HarmonyPatch(typeof(InventoryGui), "Update")]
    internal static class InventoryInputGuard
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var consoleVisible = AccessTools.Method(typeof(global::Console), "IsVisible");
            var inputActive = AccessTools.Method(typeof(InventoryInputGuard), nameof(IsConsoleOrSearchInputActive));
            foreach (var instruction in instructions)
            {
                // Extend the existing shortcut guard without skipping inventory updates
                // or legitimate closes caused by death, teleporting or leaving a station.
                if (instruction.Calls(consoleVisible))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = inputActive;
                }
                yield return instruction;
            }
        }

        private static bool IsConsoleOrSearchInputActive() =>
            global::Console.IsVisible() || SearchPanel.BlockInput;
    }
}
