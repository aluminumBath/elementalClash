using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Elementborn.Core;

namespace Elementborn.nakama.Game
{
    /// <summary>
    /// A code-built top-down world map screen (uGUI, no prefabs): region nodes coloured by biome,
    /// connection lines, name labels, and a detail panel that lists a region's POIs when clicked. Text and
    /// the Enter button go through <see cref="UiTheme"/> (TextMeshPro with a legacy fallback); the coloured
    /// nodes/links stay plain Images. Flat works out of the box; in VR add the XRI raycaster + input module.
    /// </summary>
    public sealed class WorldMapView : MonoBehaviour
    {
        [SerializeField] private WorldMapController worldSource;
        [SerializeField] private Vector2 mapAreaSize = new Vector2(760f, 760f);

        private RectTransform _mapArea;
        private UiLabel _detailText;
        private WorldMap _world;
        private WorldRegion _selected;

        /// <summary>Raised when the player presses "Enter the world" (with the selected region, or the capital).</summary>
        public event System.Action<WorldRegion> EnterWorldRequested;

        private void Start()
        {
            if (worldSource == null) worldSource = FindObjectOfType<WorldMapController>();
            EnsureEventSystem();
            Build();
            if (worldSource != null)
            {
                worldSource.WorldReady += Render;
                if (worldSource.World != null) Render(worldSource.World);
            }
        }

        private void OnDestroy()
        {
            if (worldSource != null) worldSource.WorldReady -= Render;
        }

        private static Color BiomeColor(BiomeType b) => b switch
        {
            BiomeType.CapitalCity  => new Color(0.95f, 0.85f, 0.30f),
            BiomeType.CloudTemple  => new Color(0.80f, 0.90f, 1.00f),
            BiomeType.Volcano      => new Color(0.85f, 0.25f, 0.15f),
            BiomeType.Desert       => new Color(0.90f, 0.78f, 0.45f),
            BiomeType.Island       => new Color(0.30f, 0.75f, 0.70f),
            BiomeType.Beach        => new Color(0.95f, 0.90f, 0.65f),
            BiomeType.Swamp        => new Color(0.35f, 0.45f, 0.25f),
            BiomeType.Marsh        => new Color(0.45f, 0.55f, 0.35f),
            BiomeType.ForestTemple => new Color(0.25f, 0.60f, 0.35f),
            BiomeType.Mountains    => new Color(0.55f, 0.55f, 0.60f),
            _                      => new Color(0.55f, 0.70f, 0.40f) // Plains
        };

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                es.transform.SetParent(transform, false);
            }
        }

        private void Build()
        {
            var canvas = UiTheme.Canvas("WorldMapCanvas", sortOrder: 0);
            canvas.transform.SetParent(transform, false);
            var canvasGo = canvas.gameObject;

            var bg = NewImage("Background", canvasGo.transform, new Color(0.07f, 0.08f, 0.10f, 1f));
            Stretch(bg.rectTransform);

            var title = UiTheme.Label(canvasGo.transform, "WORLD MAP", 30, Color.white, TextAnchor.UpperLeft);
            var trt = title.Rect;
            trt.anchorMin = trt.anchorMax = new Vector2(0, 1); trt.pivot = new Vector2(0, 1);
            trt.anchoredPosition = new Vector2(28, -20); trt.sizeDelta = new Vector2(420, 44);

            var area = NewImage("MapArea", canvasGo.transform, new Color(0.10f, 0.12f, 0.15f, 1f));
            _mapArea = area.rectTransform;
            _mapArea.anchorMin = _mapArea.anchorMax = new Vector2(0, 0.5f); _mapArea.pivot = new Vector2(0, 0.5f);
            _mapArea.sizeDelta = mapAreaSize; _mapArea.anchoredPosition = new Vector2(28, -10);

            var panel = UiTheme.Panel(canvasGo.transform, new Color(0.12f, 0.14f, 0.18f, 0.95f));
            var prt = panel.rectTransform;
            prt.anchorMin = prt.anchorMax = new Vector2(1, 0.5f); prt.pivot = new Vector2(1, 0.5f);
            prt.sizeDelta = new Vector2(420, 760); prt.anchoredPosition = new Vector2(-28, -10);

            _detailText = UiTheme.Label(panel.transform, "Select a region.", 18, new Color(0.90f, 0.92f, 0.95f), TextAnchor.UpperLeft);
            var dtr = _detailText.Rect;
            Stretch(dtr);
            dtr.offsetMin = new Vector2(18, 18);
            dtr.offsetMax = new Vector2(-18, -18);

            // Enter-world button (uses the selected region, or the capital if none picked yet)
            var enter = UiTheme.Button(canvasGo.transform, "Enter the world  >",
                () => EnterWorldRequested?.Invoke(_selected ?? (_world != null ? _world.Capital() : null)), 300, 54);
            var ert = (RectTransform)enter.transform;
            ert.anchorMin = ert.anchorMax = new Vector2(0.5f, 0f); ert.pivot = new Vector2(0.5f, 0f);
            ert.sizeDelta = new Vector2(300, 54); ert.anchoredPosition = new Vector2(0, 22);
        }

        private void Render(WorldMap world)
        {
            _world = world;
            for (int i = _mapArea.childCount - 1; i >= 0; i--) Destroy(_mapArea.GetChild(i).gameObject);

            var bounds = world.Bounds();
            var drawn = new System.Collections.Generic.HashSet<string>();
            foreach (var r in world.Regions)
                foreach (var nId in r.Neighbors)
                {
                    string key = string.CompareOrdinal(r.Id, nId) < 0 ? r.Id + "|" + nId : nId + "|" + r.Id;
                    if (!drawn.Add(key)) continue;
                    var n = world.Get(nId);
                    if (n != null) DrawLine(ToLocal(r.MapPosition, bounds), ToLocal(n.MapPosition, bounds));
                }

            foreach (var r in world.Regions) DrawNode(r, ToLocal(r.MapPosition, bounds));
        }

        private Vector2 ToLocal(Vector2 mapPos, Rect bounds)
        {
            float nx = bounds.width  > 0 ? (mapPos.x - bounds.xMin) / bounds.width  : 0.5f;
            float ny = bounds.height > 0 ? (mapPos.y - bounds.yMin) / bounds.height : 0.5f;
            return new Vector2((nx - 0.5f) * mapAreaSize.x * 0.9f, (ny - 0.5f) * mapAreaSize.y * 0.9f);
        }

        private void DrawLine(Vector2 a, Vector2 b)
        {
            var img = NewImage("Link", _mapArea, new Color(0.40f, 0.45f, 0.50f, 0.55f));
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            Vector2 dir = b - a;
            rt.sizeDelta = new Vector2(dir.magnitude, 3f);
            rt.anchoredPosition = (a + b) * 0.5f;
            rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }

        private void DrawNode(WorldRegion region, Vector2 local)
        {
            var node = NewImage("Node_" + region.Id, _mapArea, BiomeColor(region.Biome));
            var rt = node.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            float size = region.Biome == BiomeType.CapitalCity ? 26f : 18f;
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = local;

            var captured = region;
            node.gameObject.AddComponent<Button>().onClick.AddListener(() => ShowDetail(captured));

            var label = UiTheme.Label(_mapArea, region.Name, 13, new Color(0.85f, 0.88f, 0.92f), TextAnchor.LowerCenter);
            var lrt = label.Rect;
            lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f); lrt.pivot = new Vector2(0.5f, 0f);
            lrt.sizeDelta = new Vector2(160, 18);
            lrt.anchoredPosition = local + new Vector2(0, size * 0.5f + 2f);
        }

        private void ShowDetail(WorldRegion r)
        {
            _selected = r;
            var sb = new StringBuilder();
            sb.AppendLine(r.Name.ToUpper());
            sb.AppendLine($"{Pretty(r.Biome)}   .   danger {r.DangerLevel}");
            sb.AppendLine();
            sb.AppendLine($"Points of interest ({r.Pois.Count}):");
            foreach (var p in r.Pois)
            {
                sb.Append($"  - {p.Name}  ({Pretty(p.Type)})");
                if (p.EnemySpawnCount > 0) sb.Append($"  -  {p.EnemySpawnCount} foes");
                if (p.HasWeaponCache) sb.Append($"  -  {p.WeaponMaterial} {p.WeaponType}");
                sb.AppendLine();
            }
            sb.AppendLine();
            var connected = string.Join(", ", _world.NeighborsOf(r).Select(n => n.Name));
            sb.AppendLine("Connects to: " + (string.IsNullOrEmpty(connected) ? "-" : connected));
            _detailText.text = sb.ToString();
        }

        private static string Pretty(System.Enum e)
        {
            string s = e.ToString();
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (i > 0 && char.IsUpper(s[i])) sb.Append(' ');
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        private Image NewImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
