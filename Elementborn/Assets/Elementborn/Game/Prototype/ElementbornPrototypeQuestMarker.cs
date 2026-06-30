using UnityEngine;

namespace Elementborn.Game
{
    public enum ElementbornPrototypeMarkerKind
    {
        Objective,
        Talk,
        Combat,
        Loot,
        Lore,
        Resource,
        Shrine,
        Gate
    }

    public sealed class ElementbornPrototypeQuestMarker : MonoBehaviour
    {
        public ElementbornPrototypeMarkerKind kind = ElementbornPrototypeMarkerKind.Objective;
        public string markerText = "!";
        public Vector3 localOffset = new Vector3(0f, 2.8f, 0f);
        public float bobAmplitude = 0.06f;
        public float bobSpeed = 2.2f;
        public bool faceCamera = true;

        private Transform markerRoot;
        private TextMesh label;
        private Vector3 baseLocalPosition;

        private void Awake()
        {
            EnsureMarker();
        }

        private void OnEnable()
        {
            EnsureMarker();
        }

        private void Update()
        {
            EnsureMarker();

            if (markerRoot == null)
            {
                return;
            }

            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            markerRoot.localPosition = baseLocalPosition + Vector3.up * bob;

            if (faceCamera && Camera.main != null)
            {
                Vector3 toCamera = markerRoot.position - Camera.main.transform.position;
                if (toCamera.sqrMagnitude > 0.01f)
                {
                    markerRoot.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
                }
            }
        }

        public void SetMarker(string text, ElementbornPrototypeMarkerKind newKind)
        {
            markerText = text;
            kind = newKind;
            EnsureMarker();
            ApplyVisuals();
        }

        private void EnsureMarker()
        {
            if (markerRoot == null)
            {
                Transform existing = transform.Find("Prototype Quest Marker");
                GameObject root = existing != null ? existing.gameObject : new GameObject("Prototype Quest Marker");
                root.transform.SetParent(transform, false);
                root.transform.localPosition = localOffset;
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;
                markerRoot = root.transform;
                baseLocalPosition = localOffset;
            }

            if (label == null)
            {
                Transform existingLabel = markerRoot.Find("Marker Label");
                GameObject labelGo = existingLabel != null ? existingLabel.gameObject : new GameObject("Marker Label");
                labelGo.transform.SetParent(markerRoot, false);
                labelGo.transform.localPosition = Vector3.zero;
                labelGo.transform.localRotation = Quaternion.identity;
                labelGo.transform.localScale = Vector3.one * 0.11f;

                label = labelGo.GetComponent<TextMesh>();
                if (label == null)
                {
                    label = labelGo.AddComponent<TextMesh>();
                }

                label.anchor = TextAnchor.MiddleCenter;
                label.alignment = TextAlignment.Center;
                label.fontSize = 48;
                label.characterSize = 0.22f;
            }

            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (label == null)
            {
                return;
            }

            string text = string.IsNullOrWhiteSpace(markerText) ? "!" : markerText;
            if (text == "ART")
            {
                text = "✦";
            }

            label.text = text;
            label.color = GetMarkerColor(kind);
        }

        private Color GetMarkerColor(ElementbornPrototypeMarkerKind markerKind)
        {
            switch (markerKind)
            {
                case ElementbornPrototypeMarkerKind.Talk:
                    return new Color(1f, 0.85f, 0.18f);
                case ElementbornPrototypeMarkerKind.Combat:
                    return new Color(1f, 0.18f, 0.12f);
                case ElementbornPrototypeMarkerKind.Loot:
                    return new Color(1f, 0.65f, 0.14f);
                case ElementbornPrototypeMarkerKind.Lore:
                    return new Color(0.78f, 0.48f, 1f);
                case ElementbornPrototypeMarkerKind.Resource:
                    return new Color(0.2f, 1f, 0.72f);
                case ElementbornPrototypeMarkerKind.Shrine:
                    return new Color(0.55f, 1f, 0.9f);
                case ElementbornPrototypeMarkerKind.Gate:
                    return new Color(0.75f, 0.9f, 1f);
                default:
                    return Color.white;
            }
        }
    }
}
