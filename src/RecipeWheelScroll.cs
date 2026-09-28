using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WorkstationSearch
{
    // Handle the event at the recipe content, before it bubbles to a scroll view.
    public sealed class RecipeWheelScroll : MonoBehaviour, IScrollHandler
    {
        public void OnScroll(PointerEventData eventData) => Move(eventData);

        internal static bool Move(PointerEventData eventData)
        {
            var panel = Plugin.Panel;
            if (!panel || !panel.Gui || !panel.Gui.m_recipeListScroll) return false;
            if (eventData.used) return true;
            float delta = eventData.scrollDelta.y;
            if (delta == 0) delta = -eventData.scrollDelta.x;
            if (delta == 0) return true;
            var bar = panel.Gui.m_recipeListScroll;
            // Handle size is the visible fraction of the entire list. Convert
            // one-third of that fraction to the scrollbar's remaining travel.
            float step = WheelStep.FromVisibleFraction(bar.size);
            if (panel.RecipeScroll) panel.RecipeScroll.StopMovement();
            bar.value = Mathf.Clamp01(bar.value + Mathf.Sign(delta) * step);
            eventData.Use();
            return true;
        }
    }

    // Also handle wheel events over the empty viewport background.
    [HarmonyPatch(typeof(ScrollRect), nameof(ScrollRect.OnScroll), typeof(PointerEventData))]
    internal static class RecipeViewportWheel
    {
        private static bool Prefix(ScrollRect __instance, PointerEventData __0)
        {
            if (!Plugin.Panel || !Plugin.Panel.Gui ||
                (__instance != Plugin.Panel.RecipeScroll && __instance.content != Plugin.Panel.Gui.m_recipeListRoot)) return true;
            return !RecipeWheelScroll.Move(__0);
        }
    }
}
