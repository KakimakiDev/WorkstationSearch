using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WorkstationSearch
{
    public sealed class CategoryEditor : MonoBehaviour
    {
        private static CategoryEditor current;
        private static readonly FieldInfo CraftTimer = AccessTools.Field(typeof(InventoryGui), "m_craftTimer");
        internal static bool IsOpen => current;
        private InventoryGui gui;
        private SpellingEntry item;
        private CategoryOverride draft;
        private TMP_InputField terms;
        private TMP_Text status;
        private readonly Dictionary<ItemCategory, TMP_Text> labels = new Dictionary<ItemCategory, TMP_Text>();

        internal static void Open(InventoryGui owner, SpellingEntry entry)
        {
            if (!owner || entry == null || string.IsNullOrEmpty(entry.Prefab) || (float)CraftTimer.GetValue(owner) >= 0) return;
            var canvas = owner.m_recipeListRoot.GetComponentInParent<Canvas>();
            if (!canvas) return;
            Close();
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            var root = new GameObject("WorkstationSearchCategories", typeof(RectTransform), typeof(Image), typeof(CategoryEditor));
            var rect = (RectTransform)root.transform;
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            current = root.GetComponent<CategoryEditor>();
            current.gui = owner;
            current.item = entry;
            current.draft = Plugin.Overrides.Get(entry.Prefab);
            current.Build(canvas);
            SearchPanel.SuppressInputFrame();
        }

        internal static void Close()
        {
            if (!current) return;
            var old = current;
            current = null;
            old.gameObject.SetActive(false);
            Destroy(old.gameObject);
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            SearchPanel.SuppressInputFrame();
        }

        private void Build(Canvas canvas)
        {
            var panel = Rect("Editor", transform, 0, 0, 760, 670);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = Vector2.zero;
            var canvasSize = ((RectTransform)canvas.transform).rect.size;
            float scale = Mathf.Min(1, Mathf.Min((canvasSize.x - 24) / 760, (canvasSize.y - 24) / 670));
            panel.localScale = Vector3.one * Mathf.Max(0.2f, scale);
            panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.105f, 0.085f, 1);
            Text(panel, "Search categories", 20, 14, 720, 32, 24);
            var title = Text(panel, item.Name, 20, 49, 720, 30, 20);
            title.overflowMode = TextOverflowModes.Ellipsis;
            Text(panel, "Click categories to enable or disable them. [x] means enabled; auto means detected.\nChanges apply to this item type in Craft and Upgrade.", 20, 86, 720, 48, 16);

            var view = Rect("Categories", panel, 20, 144, 704, 292);
            view.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
            view.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", view, 0, 0, 704, 0);
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = view;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 35;
            var categories = Enum.GetValues(typeof(ItemCategory)).Cast<ItemCategory>().OrderBy(Friendly).ToArray();
            content.sizeDelta = new Vector2(704, ((categories.Length + 2) / 3) * 42);
            for (int i = 0; i < categories.Length; i++)
            {
                var category = categories[i];
                var label = Button(content, "", (i % 3) * 235, (i / 3) * 42, 228, 38, () =>
                {
                    draft.Toggle(category, item.Categories);
                    RefreshLabels();
                });
                label.fontSize = 14;
                labels.Add(category, label);
            }
            var track = Rect("Scrollbar", panel, 730, 144, 10, 292);
            track.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            var sliding = Rect("Sliding Area", track, 0, 0, 10, 292);
            var handle = Rect("Handle", sliding, 0, 0, 10, 30);
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color(0.8f, 0.58f, 0.25f);
            var scrollbar = track.gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalNormalizedPosition = 1;

            Text(panel, "Custom search words", 20, 449, 720, 24, 18);
            var field = Rect("CustomWords", panel, 20, 480, 720, 36);
            var image = field.gameObject.AddComponent<Image>();
            image.color = new Color(0.25f, 0.23f, 0.2f);
            var area = Rect("Text Area", field, 10, 0, 700, 36);
            area.gameObject.AddComponent<RectMask2D>();
            var value = Text(area, "", 0, 0, 700, 36, 18);
            var placeholder = Text(area, "Example: dragon, winter, sailing", 0, 0, 700, 36, 18);
            placeholder.color = new Color(0.65f, 0.62f, 0.57f);
            terms = field.gameObject.AddComponent<TMP_InputField>();
            terms.textViewport = area;
            terms.textComponent = value;
            terms.placeholder = placeholder;
            terms.targetGraphic = image;
            terms.lineType = TMP_InputField.LineType.SingleLine;
            string savedWords = string.Join(", ", draft.Terms);
            // Formatting saved words with commas must not truncate an existing record.
            terms.characterLimit = Mathf.Max(512, savedWords.Length);
            terms.text = savedWords;
            Text(panel, "Separate words with spaces or commas. Names remain searchable when categories are disabled.\nSettings stay saved even if the item's mod is removed.", 20, 525, 720, 42, 15);
            status = Text(panel, "", 20, 574, 720, 30, 15);
            Button(panel, "Reset to automatic", 20, 620, 240, 34, () =>
            {
                draft = new CategoryOverride();
                terms.text = "";
                status.text = "Automatic settings restored. Save to apply.";
                RefreshLabels();
            });
            Button(panel, "Cancel", 490, 620, 115, 34, Close);
            Button(panel, "Save", 625, 620, 115, 34, Save);
            RefreshLabels();
        }

        private void Save()
        {
            if (!gui || (float)CraftTimer.GetValue(gui) >= 0) return;
            draft.Terms = CategoryOverride.ParseTerms(terms.text);
            if (!Plugin.SaveOverride(item.Prefab, draft))
            {
                status.text = "Could not save. See the BepInEx log; existing settings were kept.";
                return;
            }
            FavoriteRows.RefreshOverrides(item.Prefab);
            Close();
        }
        private void RefreshLabels()
        {
            var effective = new HashSet<ItemCategory>(draft.Apply(item.Categories));
            foreach (var pair in labels)
            {
                bool enabled = effective.Contains(pair.Key);
                pair.Value.text = (enabled ? "[x] " : "[ ] ") + Friendly(pair.Key) +
                    (item.Categories.Contains(pair.Key) ? " (auto)" : "");
                pair.Value.color = enabled ? new Color(1, 0.8f, 0.35f) : new Color(0.68f, 0.66f, 0.62f);
            }
        }
        private static string Friendly(ItemCategory category) => Regex.Replace(category.ToString(), "([a-z])([A-Z])", "$1 $2");
        private TMP_Text Button(Transform parent, string title, float x, float y, float width, float height, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(title.Length == 0 ? "Category" : title, parent, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>();
            var button = rect.gameObject.AddComponent<Button>();
            SearchPanel.CopyImage(gui.m_craftCancelButton.targetGraphic as Image, image);
            SearchPanel.CopySelectable(gui.m_craftCancelButton, button);
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            var label = Text(rect, title, 5, 0, width - 10, height, 16);
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }
        private TMP_Text Text(Transform parent, string value, float x, float y, float width, float height, float size)
        {
            var label = SearchPanel.Label("Label", parent, gui.m_recipeName, value);
            Position(label.rectTransform, x, y, width, height);
            label.fontSize = size;
            label.richText = false;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            return label;
        }
        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            Position(rect, x, y, width, height);
            return rect;
        }
        private static void Position(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
        private void Update()
        {
            if (!gui || !InventoryGui.IsVisible() || (float)CraftTimer.GetValue(gui) >= 0 || Input.GetKeyDown(KeyCode.Escape)) Close();
        }
        private void OnDestroy()
        {
            if (current == this) current = null;
        }
    }
}
