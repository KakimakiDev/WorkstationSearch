using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WorkstationSearch
{
    // Pool only across one native Craft rebuild. Upgrade and reclaim keep their
    // own lifecycles, and unused rows are disposed before search captures the list.
    [HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
    internal static class CraftRowReuse
    {
        private static InventoryGui owner;
        private static readonly RebuildPool<Recipe, GameObject> pool = new RebuildPool<Recipe, GameObject>();

        internal static bool ReleaseHookReady;
        internal static bool CreationHookReady;
        private static readonly Dictionary<UnityEvent, CraftRowIdentity> clicks =
            new Dictionary<UnityEvent, CraftRowIdentity>();

        internal static void Begin(InventoryGui __instance, IEnumerable<GameObject> rows)
        {
            Finish();
            if (!ReleaseHookReady || !CreationHookReady || !__instance.InCraftTab()) return;
            owner = __instance;
            foreach (var row in rows)
            {
                var identity = row ? row.GetComponent<CraftRowIdentity>() : null;
                if (!identity || !identity.Recipe || identity.Prefab != __instance.m_recipeElementPrefab) continue;
                pool.Add(identity.Recipe, row);
            }
        }

        private static void Finalizer() => Finish();

        internal static void Finish()
        {
            foreach (var row in pool.Unused)
                if (row) { row.SetActive(false); Object.Destroy(row); }
            pool.Clear();
            clicks.Clear();
            owner = null;
        }

        private static void ReleaseRow(Object value)
        {
            if (value is GameObject row && pool.Contains(row)) return;
            Object.Destroy(value);
        }

        internal static GameObject GetRow(GameObject prefab, Transform parent, Recipe recipe)
        {
            if (owner && parent == owner.m_recipeListRoot)
            {
                GameObject row;
                while (!ReferenceEquals(row = pool.Take(recipe), null))
                {
                    if (!row) continue;
                    var identity = row.GetComponent<CraftRowIdentity>();
                    // Native AddRecipeToList adds its click listener again below.
                    if (identity.Click != null) row.GetComponent<Button>().onClick.RemoveListener(identity.Click);
                    identity.Click = null;
                    clicks[row.GetComponent<Button>().onClick] = identity;
                    var selected = row.transform.Find("selected");
                    if (selected) selected.gameObject.SetActive(false);
                    return row;
                }
            }
            var created = Object.Instantiate(prefab, parent);
            if (owner && parent == owner.m_recipeListRoot)
            {
                var identity = created.AddComponent<CraftRowIdentity>();
                identity.Recipe = recipe;
                identity.Prefab = prefab;
                clicks[created.GetComponent<Button>().onClick] = identity;
            }
            return created;
        }

        internal static void AddClick(UnityEvent clickEvent, UnityAction listener)
        {
            clickEvent.AddListener(listener);
            if (clicks.TryGetValue(clickEvent, out var identity))
            {
                identity.Click = listener;
                clicks.Remove(clickEvent);
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var destroy = AccessTools.Method(typeof(Object), nameof(Object.Destroy), new[] { typeof(Object) });
            var code = instructions.ToList();
            ReleaseHookReady = code.Count(instruction => instruction.Calls(destroy)) == 1;
            foreach (var instruction in code)
            {
                if (ReleaseHookReady && instruction.Calls(destroy))
                    instruction.operand = AccessTools.Method(typeof(CraftRowReuse), nameof(ReleaseRow));
                yield return instruction;
            }
        }
    }

    internal sealed class CraftRowIdentity : MonoBehaviour
    {
        internal Recipe Recipe;
        internal GameObject Prefab;
        internal UnityAction Click;
    }

    [HarmonyPatch(typeof(InventoryGui), "AddRecipeToList")]
    internal static class CraftRowCreation
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var addListener = AccessTools.Method(typeof(UnityEvent), nameof(UnityEvent.AddListener),
                new[] { typeof(UnityAction) });
            CraftRowReuse.CreationHookReady = code.Count(IsRowCreation) == 1 &&
                code.Count(instruction => instruction.Calls(addListener)) == 1;
            if (!CraftRowReuse.CreationHookReady) return code;
            var result = new List<CodeInstruction>();
            foreach (var instruction in code)
            {
                if (IsRowCreation(instruction))
                {
                    var recipe = new CodeInstruction(OpCodes.Ldarg_2);
                    recipe.labels.AddRange(instruction.labels);
                    instruction.labels.Clear();
                    recipe.blocks.AddRange(instruction.blocks);
                    instruction.blocks.Clear();
                    result.Add(recipe);
                    instruction.operand = AccessTools.Method(typeof(CraftRowReuse), nameof(CraftRowReuse.GetRow));
                }
                else if (instruction.Calls(addListener))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(CraftRowReuse), nameof(CraftRowReuse.AddClick));
                }
                result.Add(instruction);
            }
            return result;
        }

        private static bool IsRowCreation(CodeInstruction instruction) =>
            instruction.operand is MethodInfo method && method.DeclaringType == typeof(Object) &&
            method.Name == nameof(Object.Instantiate) && method.IsGenericMethod &&
            method.GetGenericArguments()[0] == typeof(GameObject) && method.GetParameters().Length == 2 &&
            method.GetParameters()[1].ParameterType == typeof(Transform);
    }
}
