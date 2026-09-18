using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WorkstationSearch
{
    [HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
    internal static class FavoriteRows
    {
        private static readonly FieldInfo Available = AccessTools.Field(typeof(InventoryGui), "m_availableRecipes");
        private static readonly System.Type Pair = Available.FieldType.GetGenericArguments()[0];
        private static readonly PropertyInfo RecipeProperty = Pair.GetProperty("Recipe");
        private static readonly PropertyInfo Element = Pair.GetProperty("InterfaceElement");
        private static readonly FieldInfo BaseSize = AccessTools.Field(typeof(InventoryGui), "m_recipeListBaseSize");
        private static readonly MethodInfo SelectedIndex = AccessTools.Method(typeof(InventoryGui), "GetSelectedRecipeIndex");
        private static readonly MethodInfo SetRecipe = AccessTools.Method(typeof(InventoryGui), "SetRecipe");
        private static readonly FieldInfo Selected = AccessTools.Field(typeof(InventoryGui), "m_selectedRecipe");
        private static InventoryGui owner;
        private static List<object> cachedRows;
        private static readonly Dictionary<GameObject, SpellingEntry> RowEntries = new Dictionary<GameObject, SpellingEntry>();
        private static SpellingEntry RowEntry(object pair)
        {
            var row = (GameObject)Element.GetValue(pair, null);
            return row && RowEntries.TryGetValue(row, out var entry) ? entry : null;
        }
        internal static string Key(Recipe recipe) => recipe && recipe.m_item ? recipe.m_item.name : null;

        // Give vanilla ownership of every row, including hidden rows, before it
        // destroys/rebuilds the list for an inventory or station change.
        [HarmonyPriority(Priority.First)]
        private static void Prefix(InventoryGui __instance)
        {
            if (owner != __instance || cachedRows == null) return;
            var rows = (IList)Available.GetValue(__instance);
            rows.Clear();
            foreach (var row in cachedRows) rows.Add(row);
            cachedRows = null;
            RowEntries.Clear();
        }

        [HarmonyPriority(Priority.Last)]
        private static void Postfix(InventoryGui __instance)
        {
            SearchCatalog.Ensure();
            owner = __instance;
            RowEntries.Clear();
            cachedRows = ((IList)Available.GetValue(__instance)).Cast<object>().ToList();
            foreach (var pair in cachedRows)
            {
                var row = (GameObject)Element.GetValue(pair, null);
                var recipe = (Recipe)RecipeProperty.GetValue(pair, null);
                var entry = SearchCatalog.Get(recipe);
                var titleRoot = row.transform.Find("name");
                var title = titleRoot ? titleRoot.GetComponent<TMP_Text>() : null;
                if (title && !string.IsNullOrWhiteSpace(title.text))
                    entry = (entry ?? new SpellingEntry("", "")).WithDisplayName(title.text);
                RowEntries[row] = entry;
                string key = Key(recipe);
                if (string.IsNullOrEmpty(key)) continue;
                var control = row.GetComponent<FavoriteRow>() ?? row.AddComponent<FavoriteRow>();
                control.Initialize(__instance, key);
            }
            Apply(__instance, false);
        }

        internal static void Apply(InventoryGui __instance, bool updateSelection = true)
        {
            if (owner != __instance || cachedRows == null) return;
            var rows = (IList)Available.GetValue(__instance);
            var query = Plugin.Panel ? Plugin.Panel.Query : new SearchQuery("");
            var matched = query.SelectMatches(cachedRows, RowEntry, out bool approximate);
            if (Plugin.Panel) Plugin.Panel.SetApproximate(approximate);
            var kept = new HashSet<object>(matched);
            foreach (var pair in cachedRows)
            {
                var element = (GameObject)Element.GetValue(pair, null);
                bool visible = kept.Contains(pair);
                if (element.activeSelf != visible) element.SetActive(visible);
                var favorite = element.GetComponent<FavoriteRow>();
                if (favorite) favorite.RefreshMarker();
            }
            rows.Clear();
            float baseSize = (float)BaseSize.GetValue(__instance);
            __instance.m_recipeListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(baseSize, matched.Count * __instance.m_recipeListSpace));
            var ranked = query.RankMatches(matched, RowEntry);
            var ordered = FavoriteSet.First(ranked, row =>
                Plugin.Favorites.Contains(Key((Recipe)RecipeProperty.GetValue(row, null))));
            for (int i = 0; i < ordered.Count; i++)
            {
                rows.Add(ordered[i]);
                var row = (GameObject)Element.GetValue(ordered[i], null);
                var rect = (RectTransform)row.transform;
                var position = new Vector2(0, -i * __instance.m_recipeListSpace);
                if (rect.anchoredPosition != position) rect.anchoredPosition = position;
                if (row.transform.GetSiblingIndex() != i) row.transform.SetSiblingIndex(i);
            }
            if (updateSelection)
            {
                int index = rows.Count == 0 ? -1 : (int)SelectedIndex.Invoke(__instance, new object[] { true });
                // Keep unchanged selection details without rerunning vanilla's
                // selection work. Reconcile cached highlights separately.
                var selected = Selected.GetValue(__instance);
                if (index >= 0 ? !selected.Equals(rows[index]) : RecipeProperty.GetValue(selected, null) != null)
                    SetRecipe.Invoke(__instance, new object[] { index, false });
            }
            SyncSelection(__instance);
        }

        internal static void SyncSelection(InventoryGui gui)
        {
            if (owner != gui || cachedRows == null) return;
            var selectedElement = (GameObject)Element.GetValue(Selected.GetValue(gui), null);
            foreach (var pair in cachedRows)
            {
                var row = (GameObject)Element.GetValue(pair, null);
                if (!row) continue;
                var marker = row.transform.Find("selected");
                if (!marker) continue;
                bool active = row.activeSelf && row == selectedElement;
                if (marker.gameObject.activeSelf != active) marker.gameObject.SetActive(active);
            }
        }

        internal static void Release(InventoryGui gui)
        {
            if (owner != gui) return;
            owner = null;
            cachedRows = null;
            RowEntries.Clear();
        }
    }

    // Vanilla only clears selection markers in its currently visible list.
    // Clear hidden cached rows too, including mouse/controller selections.
    [HarmonyPatch(typeof(InventoryGui), "SetRecipe")]
    internal static class RecipeSelectionVisuals
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(InventoryGui __instance) => FavoriteRows.SyncSelection(__instance);
    }

    public sealed class FavoriteRow : MonoBehaviour, IPointerClickHandler
    {
        private static readonly FieldInfo CraftTimer = AccessTools.Field(typeof(InventoryGui), "m_craftTimer");
        private InventoryGui gui;
        private string key;
        private GameObject marker;

        internal void Initialize(InventoryGui owner, string itemKey)
        {
            gui = owner;
            key = itemKey;
            if (!marker)
            {
                marker = new GameObject("CraftSearchFavorite", typeof(RectTransform));
                var rect = (RectTransform)marker.transform;
                // Overlay a small star on the item icon, leaving names, quality
                // and durability in their original positions.
                rect.SetParent(transform.Find("icon") ?? transform, false);
                rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
                rect.pivot = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(14, 14);
                Image nativeStar = null;
                if (Hud.instance && Hud.instance.m_buildUi)
                {
                    var prefab = AccessTools.Field(typeof(BuildUi), "m_pieceButtonPrefab").GetValue(Hud.instance.m_buildUi) as GameObject;
                    var button = prefab ? prefab.GetComponent<BuildUiPieceButton>() : null;
                    if (button) nativeStar = AccessTools.Field(typeof(BuildUiPieceButton), "m_favoriteStar").GetValue(button) as Image;
                }
                if (nativeStar && nativeStar.sprite)
                {
                    var image = marker.AddComponent<Image>();
                    image.sprite = nativeStar.sprite;
                    image.color = nativeStar.color;
                    image.material = nativeStar.material;
                    image.preserveAspect = true;
                    image.raycastTarget = false;
                }
                else
                {
                    var text = marker.AddComponent<TextMeshProUGUI>();
                    text.font = gui.m_recipeName.font;
                    text.text = "★";
                    text.fontSize = 14;
                    text.color = new Color(1, 0.8f, 0.3f);
                    text.alignment = TextAlignmentOptions.Center;
                    text.raycastTarget = false;
                }
            }
            RefreshMarker();
        }

        internal void RefreshMarker()
        {
            bool active = Plugin.Favorites.Contains(key);
            if (marker && marker.activeSelf != active) marker.SetActive(active);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle || !gui ||
                (float)CraftTimer.GetValue(gui) >= 0) return;
            Plugin.ToggleFavorite(key);
            marker.SetActive(Plugin.Favorites.Contains(key));
            if (Plugin.Panel) Plugin.Panel.RequestRefresh();
        }
    }
}
