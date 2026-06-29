using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class WindCapitalSecretTracker : MonoBehaviour
    {
        public static WindCapitalSecretTracker Instance { get; private set; }

        [SerializeField] private List<string> discoveredHooks = new List<string>();
        [SerializeField] private bool sarahPastKnown;
        [SerializeField] private bool oldLeadersTruthKnown;
        [SerializeField] private bool ruthSteamOmenKnown;

        public bool SarahPastKnown => sarahPastKnown;
        public bool OldLeadersTruthKnown => oldLeadersTruthKnown;
        public bool RuthSteamOmenKnown => ruthSteamOmenKnown;

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

        public static WindCapitalSecretTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(WindCapitalSecretTracker));
            return go.AddComponent<WindCapitalSecretTracker>();
        }

        public void Reveal(WindCapitalIntrigueHookDefinition hook)
        {
            if (hook == null)
            {
                return;
            }

            if (!discoveredHooks.Contains(hook.HookId))
            {
                discoveredHooks.Add(hook.HookId);
            }

            if (hook.HookType == WindCapitalHookType.SarahPast)
            {
                sarahPastKnown = true;
            }
            else if (hook.HookType == WindCapitalHookType.OldLeaders)
            {
                oldLeadersTruthKnown = true;
            }
            else if (hook.HookType == WindCapitalHookType.InfantSteamOmen)
            {
                ruthSteamOmenKnown = true;
            }

            PlayerJournalTracker.AddOrUpdateEntry(
                "wind_secret_" + PlayerJournalTracker.Safe(hook.HookId),
                JournalEntryType.Quest,
                hook.Title,
                hook.SecretTruth,
                "Wind Capital",
                hook.HookId);

            NotificationFeed.Post("Wind Capital secret revealed: " + hook.Title, NotificationType.Quest);
        }

        public bool IsKnown(string hookId)
        {
            return discoveredHooks.Contains(hookId);
        }
    }
}
