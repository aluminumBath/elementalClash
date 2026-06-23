using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Elementborn.Game
{
    /// <summary>
    /// A code-built save-slot menu (toggle with F8): shows each slot's character and timestamp, and lets the
    /// player switch the active slot, load it into <see cref="PlayerInventory"/>, or delete it. Selecting a slot
    /// sets <see cref="SaveSystem.CurrentSlot"/>, so subsequent F5/F9 and quit-saves use it. Standalone — drop it
    /// on a persistent object (or call <see cref="EnsureInstance"/>); it raises <see cref="SlotSelected"/> for a
    /// front-end that wants to react (e.g., reload the world).
    /// </summary>
    public sealed class SaveSlotController : MonoBehaviour
    {
        public static SaveSlotController Instance { get; private set; }

        /// <summary>Raised with the slot index after the player picks (load or select-empty).</summary>
        public event Action<int> SlotSelected;

        private Canvas _canvas;
        private GameObject _root;
        private Transform _list;
        private bool _open;

        public static SaveSlotController EnsureInstance()
        {
            if (Instance != null) return Instance;
            return new GameObject("SaveSlotController").AddComponent<SaveSlotController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            if (InputBindings.Slots.WasPressedThisFrame()) Toggle();
        }

        public void Toggle() { if (_open) Hide(); else Show(); }

        public void Show()
        {
            if (_root == null) Build();
            Populate();
            _root.SetActive(true);
            _open = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
            _open = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("SaveSlotCanvas", sortOrder: 190);
            _canvas.transform.SetParent(transform, false);

            _root = new GameObject("SaveSlotRoot", typeof(RectTransform));
            _root.transform.SetParent(_canvas.transform, false);
            var rootRt = (RectTransform)_root.transform;
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;

            var dim = UiTheme.Panel(_root.transform, new Color(0f, 0f, 0f, 0.5f), "overlay_dim");
            var dr = dim.rectTransform; dr.anchorMin = Vector2.zero; dr.anchorMax = Vector2.one;
            dr.offsetMin = Vector2.zero; dr.offsetMax = Vector2.zero;

            var panel = UiTheme.Panel(_root.transform);
            var pr = panel.rectTransform;
            pr.sizeDelta = new Vector2(760, 560);
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.anchoredPosition = Vector2.zero;

            var title = UiTheme.Label(panel.transform, "Save Slots", 34, Color.white, TextAnchor.UpperCenter);
            var tr = title.Rect; tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f); tr.sizeDelta = new Vector2(-40, 48); tr.anchoredPosition = new Vector2(0, -18);

            var listGo = new GameObject("Slots", typeof(RectTransform));
            listGo.transform.SetParent(panel.transform, false);
            _list = listGo.transform;
            var lr = (RectTransform)listGo.transform;
            lr.anchorMin = new Vector2(0.05f, 0.16f); lr.anchorMax = new Vector2(0.95f, 0.85f);
            lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
            var vlg = listGo.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.spacing = 12; vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            var close = UiTheme.Button(panel.transform, "Close", Hide, 200, 52);
            var br = (RectTransform)close.transform;
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 0f);
            br.anchoredPosition = new Vector2(0, 40);
        }

        private void Populate()
        {
            for (int i = _list.childCount - 1; i >= 0; i--) Destroy(_list.GetChild(i).gameObject);

            for (int slot = 0; slot < SaveSystem.SlotCount; slot++)
            {
                int captured = slot;
                var data = SaveSystem.LoadSlot(slot);

                var row = new GameObject($"Slot{slot}", typeof(RectTransform));
                row.transform.SetParent(_list, false);
                var le = row.AddComponent<UnityEngine.UI.LayoutElement>();
                le.minHeight = 92; le.preferredHeight = 92;
                var bg = row.AddComponent<UnityEngine.UI.Image>();
                bg.color = slot == SaveSystem.CurrentSlot ? new Color(0.18f, 0.26f, 0.36f, 1f) : UiTheme.TrackColor;

                bool isCurrent = slot == SaveSystem.CurrentSlot;
                string head = $"Slot {slot + 1}{(isCurrent ? "  (active)" : "")}";
                var headLbl = UiTheme.Label(row.transform, head, 24, Color.white);
                var hr = headLbl.Rect; hr.anchorMin = new Vector2(0.03f, 0.5f); hr.anchorMax = new Vector2(0.6f, 1f);
                hr.offsetMin = Vector2.zero; hr.offsetMax = Vector2.zero;

                var subLbl = UiTheme.Label(row.transform, Summary(data), 18, UiTheme.TextColor);
                var sr = subLbl.Rect; sr.anchorMin = new Vector2(0.03f, 0.05f); sr.anchorMax = new Vector2(0.6f, 0.5f);
                sr.offsetMin = Vector2.zero; sr.offsetMax = Vector2.zero;

                if (data != null)
                {
                    var load = UiTheme.Button(row.transform, "Load", () => LoadSlot(captured), 130, 44);
                    Place(load.transform, 0.62f);
                    var del = UiTheme.Button(row.transform, "Delete", () => DeleteSlot(captured), 130, 44);
                    del.GetComponent<UnityEngine.UI.Image>().color = new Color(0.7f, 0.3f, 0.32f, 1f);
                    Place(del.transform, 0.85f);
                }
                else
                {
                    var pick = UiTheme.Button(row.transform, "Use", () => SelectEmpty(captured), 130, 44);
                    Place(pick.transform, 0.85f);
                }
            }
        }

        private static void Place(Transform t, float anchorX)
        {
            var rt = (RectTransform)t;
            rt.anchorMin = rt.anchorMax = new Vector2(anchorX, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        private void LoadSlot(int slot)
        {
            SaveSystem.CurrentSlot = slot;
            var data = SaveSystem.LoadSlot(slot);
            if (data != null) PlayerInventory.Instance?.LoadFrom(data);
            GameHud.Instance?.Toast($"Loaded slot {slot + 1}");
            SlotSelected?.Invoke(slot);
            Hide();
        }

        private void SelectEmpty(int slot)
        {
            SaveSystem.CurrentSlot = slot;
            GameHud.Instance?.Toast($"Slot {slot + 1} selected");
            SlotSelected?.Invoke(slot);
            Hide();
        }

        private void DeleteSlot(int slot)
        {
            SaveSystem.DeleteSlot(slot);
            GameHud.Instance?.Toast($"Deleted slot {slot + 1}");
            Populate();
        }

        private static string Summary(SaveData d)
        {
            if (d == null) return "Empty";
            string who =
                d.isConfluence ? "Confluence" :
                !string.IsNullOrEmpty(d.playerElement) ? d.playerElement + (d.loadoutSubArts.Count > 0 ? " (sub-art)" : "") :
                (!string.IsNullOrEmpty(d.loadoutWeapon) && d.loadoutWeapon != "None") ? d.loadoutWeapon + " user" :
                (d.created ? "Channeler" : "In progress");

            string when = d.savedUnixSeconds > 0
                ? DateTimeOffset.FromUnixTimeSeconds(d.savedUnixSeconds).LocalDateTime.ToString("yyyy-MM-dd HH:mm")
                : "saved";
            string home = d.hasHouse ? " · home set" : "";
            return $"{who} · {when}{home}";
        }
    }
}
