using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WorkstationSearch
{
    [BepInPlugin(Id, "Workstation Search", "0.6.4")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string Id = "local.valheim.craftsearch";
        private Harmony harmony;
        internal static SearchPanel Panel;
        private static ConfigEntry<string> savedFavorites;
        internal static FavoriteSet Favorites;
        internal static CategoryOverrides Overrides = new CategoryOverrides();
        private static ConfigEntry<string> savedOverrides;
        private static bool overridesReadable = true;
        private static Plugin instance;
        internal static bool SaveOverride(string prefab, CategoryOverride value)
        {
            if (!overridesReadable) return false;
            var next = CategoryOverrides.Load(Overrides.Save());
            next.Set(prefab, value);
            string previous = savedOverrides.Value;
            bool saveOnSet = instance.Config.SaveOnConfigSet;
            try
            {
                // Explicit save permits reporting write failures before applying the edit.
                instance.Config.SaveOnConfigSet = false;
                savedOverrides.Value = next.Save();
                instance.Config.Save();
                Overrides = next;
                return true;
            }
            catch (Exception ex)
            {
                savedOverrides.Value = previous;
                instance.Logger.LogError("Could not save search categories: " + ex.Message);
                return false;
            }
            finally { instance.Config.SaveOnConfigSet = saveOnSet; }
        }
        internal static void ToggleFavorite(string key)
        {
            Favorites.Toggle(key);
            savedFavorites.Value = Favorites.Save();
        }
        private void Awake()
        {
            instance = this;
            savedOverrides = Config.Bind("Search categories", "Overrides", "", "Per-prefab category overrides and custom search terms. Kept when items or mods are absent. Edit in game with Ctrl + middle-click.");
            try { Overrides = CategoryOverrides.Load(savedOverrides.Value); }
            catch (Exception ex)
            {
                overridesReadable = false;
                Logger.LogError("Search category overrides could not be read; saved data will be preserved. " + ex.Message);
            }
            savedFavorites = Config.Bind("Favorites", "Items", "", "Favourite item prefab names. Shared by Craft and Upgrade in this profile.");
            Favorites = new FavoriteSet(savedFavorites.Value);
            harmony = new Harmony(Id);
            harmony.PatchAll();
            Logger.LogInfo("Workstation Search 0.6.4 loaded");
        }
        private void OnDestroy()
        {
            CategoryEditor.Close();
            harmony?.UnpatchSelf();
            if (Panel) Destroy(Panel);
        }

        [HarmonyPatch(typeof(InventoryGui), "Awake")]
        private static class CreatePanel
        {
            private static void Postfix(InventoryGui __instance)
            {
                Panel = __instance.gameObject.AddComponent<SearchPanel>();
                Panel.Initialize(__instance);
            }
        }

        private Player indexedPlayer;
        private void Update()
        {
            if (Player.m_localPlayer != indexedPlayer)
            {
                indexedPlayer = Player.m_localPlayer;
                if (indexedPlayer) SearchCatalog.Rebuild();
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "Show")]
        private static class RefreshCatalogOnOpen
        {
            private static void Prefix() => SearchCatalog.Rebuild();
        }

        [HarmonyPatch(typeof(InventoryGui), "Hide")]
        private static class ClearSearch
        {
            private static void Postfix() { CategoryEditor.Close(); if (Panel) Panel.ResetSearch(); }
        }

        [HarmonyPatch(typeof(TextInput), "IsVisible")]
        private static class TextFocus
        {
            private static void Postfix(ref bool __result) => __result |= SearchPanel.BlockInput;
        }

        // ZInput is separate from TMP's keyboard input. Keep game shortcuts from
        // consuming letters, Tab, Enter or Escape while the field owns focus.
        [HarmonyPatch]
        private static class GameInput
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                foreach (var method in AccessTools.GetDeclaredMethods(typeof(ZInput)))
                    if (method.ReturnType == typeof(bool) &&
                        (method.Name == "GetButtonDown" || method.Name == "GetButton" || method.Name == "GetKeyDown"))
                        yield return method;
            }
            private static bool Prefix(ref bool __result)
            {
                if (!SearchPanel.BlockInput) return true;
                __result = false;
                return false;
            }
        }
    }

    public sealed class SearchPanel : MonoBehaviour
    {
        private static readonly FieldInfo CraftTimer = AccessTools.Field(typeof(InventoryGui), "m_craftTimer");
        internal InventoryGui Gui;
        internal SearchQuery Query = new SearchQuery("");
        private TMP_InputField input;
        private GameObject bar;
        private RectTransform scroll;
        private Vector2 originalOffset;
        private bool pending;
        private bool styled;
        internal void RequestRefresh() => pending = true;
        private static int blockedFrame = -1;
        internal static void SuppressInputFrame() => blockedFrame = Time.frameCount;
        internal static bool BlockInput => CategoryEditor.IsOpen || Time.frameCount == blockedFrame ||
            (Plugin.Panel && Plugin.Panel.input && Plugin.Panel.input.isFocused && Plugin.Panel.input.gameObject.activeInHierarchy);

        internal void Initialize(InventoryGui gui)
        {
            Gui = gui;
            var scrolling = gui.m_recipeListRoot.GetComponentInParent<ScrollRect>();
            scroll = scrolling ? (RectTransform)scrolling.transform : gui.m_recipeListRoot.parent as RectTransform;
            originalOffset = scroll.offsetMax;
            bar = new GameObject("CraftSearch", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)bar.transform;
            rect.SetParent(scroll.parent, false);
            rect.anchorMin = new Vector2(scroll.anchorMin.x, scroll.anchorMax.y);
            rect.anchorMax = scroll.anchorMax;
            const float barHeight = 34;
            const float gap = 6;
            rect.offsetMin = new Vector2(scroll.offsetMin.x, originalOffset.y - gap - barHeight);
            rect.offsetMax = originalOffset - new Vector2(0, gap);
            scroll.offsetMax = originalOffset - new Vector2(0, barHeight + 2 * gap);
            bar.GetComponent<Image>().color = new Color(0.08f, 0.07f, 0.06f, 0.95f);

            var viewport = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            var textArea = (RectTransform)viewport.transform;
            textArea.SetParent(rect, false);
            Stretch(textArea, 9, 38);
            var text = Label("Text", textArea, gui.m_recipeName, "");
            text.richText = false;
            var placeholder = Label("Placeholder", textArea, gui.m_recipeName, "Search recipes...");
            placeholder.color = new Color(0.7f, 0.67f, 0.6f);
            input = bar.AddComponent<TMP_InputField>();
            input.textViewport = textArea;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.targetGraphic = bar.GetComponent<Image>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 128;
            input.onValueChanged.AddListener(_ => pending = true);
            input.onEndEdit.AddListener(_ => blockedFrame = Time.frameCount);
            var clear = new GameObject("Clear", typeof(RectTransform), typeof(Image), typeof(Button));
            var clearRect = (RectTransform)clear.transform;
            clearRect.SetParent(rect, false);
            clearRect.anchorMin = new Vector2(1, 0);
            clearRect.anchorMax = Vector2.one;
            clearRect.offsetMin = new Vector2(-34, 0);
            clearRect.offsetMax = Vector2.zero;
            var clearButton = clear.GetComponent<Button>();
            var nativeButton = gui.m_craftCancelButton;
            CopyImage(nativeButton.targetGraphic as Image, clear.GetComponent<Image>());
            CopySelectable(nativeButton, clearButton);
            clearButton.targetGraphic = clear.GetComponent<Image>();
            var nativeLabel = gui.m_tabCraft.GetComponentInChildren<TMP_Text>(true);
            var clearLabel = Label("Label", clearRect, nativeLabel ? nativeLabel : gui.m_recipeName, "X");
            if (nativeLabel) CopyTextStyle(nativeLabel, clearLabel);
            clearLabel.fontSize = Mathf.Max(26, clearLabel.fontSize) * 0.5f;
            // A single, constant capital X can be visually centered without the
            // changing-glyph baseline problem that affects editable text.
            clearLabel.alignment = TextAlignmentOptions.Center;
            clearLabel.rectTransform.offsetMin = Vector2.zero;
            clearLabel.rectTransform.offsetMax = Vector2.zero;
            var hoverText = clear.AddComponent<ClearButtonText>();
            hoverText.Initialize(clearLabel, gui.m_tabCraft, gui.m_tabUpgrade);
            clearButton.onClick.AddListener(() => input.text = "");
            TryApplyBuildFilterStyle();
        }

        // Hud and InventoryGui can awaken in either order. Retry until the real
        // build filter exists; copy appearance only, never its search callbacks.
        private void TryApplyBuildFilterStyle()
        {
            if (styled || !Hud.instance || !Hud.instance.m_buildUi) return;
            var source = AccessTools.Field(typeof(BuildUi), "m_searchField").GetValue(Hud.instance.m_buildUi) as TMP_InputField;
            if (!source || !source.textComponent) return;
            CopyImage(source.targetGraphic as Image, bar.GetComponent<Image>());
            CopySelectable(source, input);
            input.caretColor = source.caretColor;
            input.customCaretColor = source.customCaretColor;
            input.caretWidth = source.caretWidth;
            input.selectionColor = source.selectionColor;
            CopyTextStyle(source.textComponent, input.textComponent);
            FixBaseline(input.textComponent);
            var placeholder = (TMP_Text)input.placeholder;
            CopyTextStyle(source.placeholder as TMP_Text ?? source.textComponent, placeholder);
            FixBaseline(placeholder);
            styled = true;
        }

        internal static void CopyImage(Image source, Image target)
        {
            if (!source) return;
            target.sprite = source.sprite;
            target.type = source.type;
            target.color = source.color;
            target.material = source.material;
            target.preserveAspect = source.preserveAspect;
            target.fillCenter = source.fillCenter;
            target.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        }

        internal static void CopySelectable(Selectable source, Selectable target)
        {
            target.colors = source.colors;
            target.spriteState = source.spriteState;
            // Native fields/buttons use tint or sprite transitions. An Animator
            // controller cannot be shared without its matching object hierarchy.
            target.transition = source.transition == Selectable.Transition.Animation
                ? Selectable.Transition.ColorTint : source.transition;
            target.navigation = new Navigation { mode = Navigation.Mode.None };
        }

        private static void CopyTextStyle(TMP_Text source, TMP_Text target)
        {
            target.font = source.font;
            target.fontSharedMaterial = source.fontSharedMaterial;
            target.fontSize = source.fontSize;
            target.fontStyle = source.fontStyle;
            target.color = source.color;
            target.characterSpacing = source.characterSpacing;
        }

        private static void FixBaseline(TMP_Text text, bool centered = false)
        {
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.alignment = centered ? TextAlignmentOptions.Baseline : TextAlignmentOptions.BaselineLeft;
            // Baseline alignment is independent of the current glyph bounds.
            // Center the font's full ascender/descender range in the fixed box.
            var face = text.font.faceInfo;
            float scale = text.fontSize * face.scale / face.pointSize;
            float baseline = -(face.ascentLine + face.descentLine) * scale * 0.5f;
            text.rectTransform.offsetMin = new Vector2(0, baseline);
            text.rectTransform.offsetMax = new Vector2(0, baseline);
            text.margin = Vector4.zero;
        }

        internal static void Stretch(RectTransform rect, float left = 0, float right = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, 0);
            rect.offsetMax = new Vector2(-right, 0);
        }
        internal static TextMeshProUGUI Label(string name, Transform parent, TMP_Text source, string value)
        {
            // Assign the game font before TMP awakens and looks for its default font.
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.SetActive(false);
            labelObject.transform.SetParent(parent, false);
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            Stretch(label.rectTransform);
            label.font = source.font;
            label.fontSharedMaterial = source.fontSharedMaterial;
            label.fontSize = 18;
            label.color = new Color(1, 0.88f, 0.65f);
            FixBaseline(label);
            label.raycastTarget = false;
            label.text = value;
            labelObject.SetActive(true);
            return label;
        }
        private void Update()
        {
            TryApplyBuildFilterStyle();
            if (!input || !Gui || !InventoryGui.IsVisible()) return;
            if (CategoryEditor.IsOpen) return;
            bool crafting = (float)CraftTimer.GetValue(Gui) >= 0;
            input.interactable = !crafting;
            if (input.isFocused)
            {
                blockedFrame = Time.frameCount;
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    input.DeactivateInputField();
                    if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
                }
            }
            if (!pending || crafting || !Player.m_localPlayer) return;
            pending = false;
            Query = new SearchQuery(input.text);
            FavoriteRows.Apply(Gui);
            Gui.m_recipeListScroll.value = 1;
        }
        internal void ResetSearch()
        {
            Query = new SearchQuery("");
            pending = false;
            if (!input) return;
            input.DeactivateInputField();
            input.SetTextWithoutNotify("");
        }
        private void OnDestroy()
        {
            CategoryEditor.Close();
            FavoriteRows.Release(Gui);
            if (scroll) scroll.offsetMax = originalOffset;
            if (bar) Destroy(bar);
            if (Plugin.Panel == this) Plugin.Panel = null;
        }
    }

    public sealed class ClearButtonText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly FieldInfo DefaultColor = AccessTools.Field(typeof(ButtonTextColor), "m_defaultMeshColor");
        private TMP_Text label;
        private Button craftTab;
        private Button upgradeTab;
        private bool hovered;

        internal void Initialize(TMP_Text text, Button craft, Button upgrade)
        {
            label = text;
            craftTab = craft;
            upgradeTab = upgrade;
        }

        private void LateUpdate()
        {
            if (!label || !craftTab) return;
            var colors = craftTab.GetComponent<ButtonTextColor>();
            if (colors)
            {
                // Valheim marks the active tab non-interactable. Its disabled
                // text colour is therefore the selected-tab colour requested
                // for hover; its original text colour is the unselected one.
                label.color = hovered ? colors.m_disabledColor : (Color)DefaultColor.GetValue(colors);
                return;
            }
            // Support UI mods that replace ButtonTextColor with other styling.
            var tab = craftTab.interactable == !hovered ? craftTab : upgradeTab;
            var source = tab ? tab.GetComponentInChildren<TMP_Text>(true) : null;
            if (source) label.color = source.color;
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = false;
        private void OnDisable() => hovered = false;
    }
}
