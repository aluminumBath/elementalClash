
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Registers a visible capital landmark as a journal + map entry so the primitive landmark prefabs
    /// act as playable world anchors instead of decoration only.
    /// </summary>
    public sealed class CapitalLandmarkDescriptor : MonoBehaviour
    {
        [SerializeField] private CapitalId capitalId = CapitalId.Unknown;
        [SerializeField] private string displayName = "Capital Landmark";
        [TextArea]
        [SerializeField] private string summary = "";
        [SerializeField] private bool registerOnStart = true;
        [SerializeField] private Vector3 markerOffset = new Vector3(0f, 3f, 0f);

        public CapitalId CapitalId => capitalId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? gameObject.name : displayName;
        public string Summary => summary ?? string.Empty;

        private void Start()
        {
            if (registerOnStart)
            {
                Register();
            }
        }

        public void Configure(CapitalId capital, string title, string body)
        {
            capitalId = capital;
            displayName = title;
            summary = body;
        }

        // v52 compatibility overload for older scene/site generators.
        public void Configure(CapitalId capital, string title, string body, Vector3 offset)
        {
            Configure(capital, title, body);
            markerOffset = offset;
        }

        [ContextMenu("Register Landmark")]
        public void Register()
        {
            string safeId = PlayerJournalTracker.Safe(capitalId + "_" + DisplayName);
            PlayerJournalTracker.AddOrUpdateEntry(
                "capital_landmark_" + safeId,
                JournalEntryType.Location,
                DisplayName,
                Summary,
                capitalId.ToString(),
                capitalId.ToString());

            PlayerMapMarkerTracker.ReportOrUpdateMarker(
                "capital_landmark_" + safeId,
                MapMarkerType.GuideNpc,
                transform.position + markerOffset,
                DisplayName,
                true,
                notes: Summary);
        }
    }
}
