using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    public sealed class MetalCapitalRegistry : MonoBehaviour
    {
        public static MetalCapitalRegistry Instance { get; private set; }

        [SerializeField] private List<MetalCapitalContactDefinition> contacts = new List<MetalCapitalContactDefinition>();
        [SerializeField] private List<MetalCapitalIntrigueHookDefinition> intrigueHooks = new List<MetalCapitalIntrigueHookDefinition>();
        [SerializeField] private bool registerJournalEntriesOnStart = true;
        [SerializeField] private bool registerMapMarkersOnStart = true;

        public IReadOnlyList<MetalCapitalContactDefinition> Contacts => contacts;
        public IReadOnlyList<MetalCapitalIntrigueHookDefinition> IntrigueHooks => intrigueHooks;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (registerJournalEntriesOnStart)
            {
                RegisterJournalEntries();
            }

            if (registerMapMarkersOnStart)
            {
                RegisterMapMarkers();
            }
        }

        public static MetalCapitalRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(MetalCapitalRegistry));
            return go.AddComponent<MetalCapitalRegistry>();
        }

        public void SetData(List<MetalCapitalContactDefinition> contactDefinitions, List<MetalCapitalIntrigueHookDefinition> hookDefinitions)
        {
            contacts = contactDefinitions ?? new List<MetalCapitalContactDefinition>();
            intrigueHooks = hookDefinitions ?? new List<MetalCapitalIntrigueHookDefinition>();
        }

        public MetalCapitalContactDefinition FindContact(string id)
        {
            return contacts.Find(c => c != null && c.ContactId == id);
        }

        public MetalCapitalIntrigueHookDefinition FindHook(string id)
        {
            return intrigueHooks.Find(h => h != null && h.HookId == id);
        }

        public void RegisterJournalEntries()
        {
            foreach (var contact in contacts)
            {
                if (contact == null)
                {
                    continue;
                }

                PlayerJournalTracker.AddOrUpdateEntry(
                    "metal_contact_" + PlayerJournalTracker.Safe(contact.ContactId),
                    JournalEntryType.Character,
                    contact.DisplayName,
                    contact.Description + "\n\nSecret: " + contact.Secret,
                    "Metal Capital",
                    contact.ContactId);
            }

            foreach (var hook in intrigueHooks)
            {
                if (hook == null)
                {
                    continue;
                }

                PlayerJournalTracker.AddOrUpdateEntry(
                    "metal_hook_" + PlayerJournalTracker.Safe(hook.HookId),
                    JournalEntryType.Quest,
                    hook.Title,
                    hook.PlayerFacingRumor + "\n\nHidden GM note: " + hook.SecretTruth,
                    "Metal Capital",
                    hook.HookId);
            }
        }

        public void RegisterMapMarkers()
        {
            foreach (var contact in contacts)
            {
                if (contact == null)
                {
                    continue;
                }

                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "metal_contact_" + PlayerJournalTracker.Safe(contact.ContactId),
                    MapMarkerType.Vendor,
                    contact.WorldPosition,
                    contact.DisplayName,
                    true,
                    contact.District + " — " + contact.Description);
            }

            foreach (var hook in intrigueHooks)
            {
                if (hook == null || !hook.CreatesMapMarker)
                {
                    continue;
                }

                Vector3 position = hook.PrimaryNpc != null ? hook.PrimaryNpc.WorldPosition : Vector3.zero;
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "metal_hook_" + PlayerJournalTracker.Safe(hook.HookId),
                    MapMarkerType.QuestObjective,
                    position,
                    hook.Title,
                    true,
                    hook.PlayerFacingRumor);
            }
        }

        public string BuildSummary()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Metal Capital");
            sb.AppendLine($"Contacts: {contacts.Count}");
            foreach (var contact in contacts)
            {
                if (contact != null)
                {
                    sb.AppendLine($"- {contact.DisplayName} [{contact.ContactType}] {contact.District}");
                }
            }

            sb.AppendLine($"Hooks: {intrigueHooks.Count}");
            foreach (var hook in intrigueHooks)
            {
                if (hook != null)
                {
                    sb.AppendLine($"- {hook.Title} [{hook.District}]");
                }
            }

            return sb.ToString();
        }
    }
}
