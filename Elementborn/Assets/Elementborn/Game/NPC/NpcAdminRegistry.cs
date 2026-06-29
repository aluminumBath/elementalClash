using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class NpcAdminRegistry : MonoBehaviour
    {
        public static NpcAdminRegistry Instance { get; private set; }

        [SerializeField] private NpcWorldIntegrationManifest manifest;
        [SerializeField] private List<NpcWorldEntryDefinition> extraNpcs = new List<NpcWorldEntryDefinition>();

        public NpcWorldIntegrationManifest Manifest => manifest;

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

        public static NpcAdminRegistry Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(NpcAdminRegistry));
            return go.AddComponent<NpcAdminRegistry>();
        }

        public void SetManifest(NpcWorldIntegrationManifest value)
        {
            manifest = value;
        }

        public List<NpcWorldEntryDefinition> GetAllNpcs()
        {
            var result = new List<NpcWorldEntryDefinition>();
            if (manifest != null)
            {
                foreach (var entry in manifest.Npcs)
                {
                    if (entry != null && !result.Contains(entry))
                    {
                        result.Add(entry);
                    }
                }
            }

            foreach (var entry in extraNpcs)
            {
                if (entry != null && !result.Contains(entry))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        public List<NpcWorldEntryDefinition> Search(NpcAdminFilter filter)
        {
            filter ??= new NpcAdminFilter();
            var result = new List<NpcWorldEntryDefinition>();
            foreach (var npc in GetAllNpcs())
            {
                if (filter.Matches(npc))
                {
                    result.Add(npc);
                }
            }

            return result;
        }

        public NpcWorldEntryDefinition FindById(string npcId)
        {
            return GetAllNpcs().Find(n => n != null && n.NpcId == npcId);
        }

        public string BuildSummary(NpcAdminFilter filter = null)
        {
            var npcs = filter == null ? GetAllNpcs() : Search(filter);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"NPCs: {npcs.Count}");
            foreach (var npc in npcs)
            {
                if (npc == null)
                {
                    continue;
                }

                sb.AppendLine($"- {npc.DisplayName} [{npc.Role}] {npc.Region} / {npc.LocationName} — {npc.PrimaryElement}{(string.IsNullOrWhiteSpace(npc.SecondaryElement) ? "" : "+" + npc.SecondaryElement)}");
            }
            return sb.ToString();
        }
    }
}
