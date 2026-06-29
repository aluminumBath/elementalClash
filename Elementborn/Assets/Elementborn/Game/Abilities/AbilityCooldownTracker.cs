using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AbilityCooldownTracker : MonoBehaviour
    {
        public static AbilityCooldownTracker Instance { get; private set; }

        private readonly Dictionary<string, float> readyTimes = new Dictionary<string, float>();

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

        public static AbilityCooldownTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(AbilityCooldownTracker));
            return go.AddComponent<AbilityCooldownTracker>();
        }

        public static bool IsReady(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return false;
            }

            var tracker = Ensure();
            return !tracker.readyTimes.TryGetValue(abilityId, out float readyAt) || Time.unscaledTime >= readyAt;
        }

        public static float Remaining(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return 0f;
            }

            var tracker = Ensure();
            return tracker.readyTimes.TryGetValue(abilityId, out float readyAt)
                ? Mathf.Max(0f, readyAt - Time.unscaledTime)
                : 0f;
        }

        public static void StartCooldown(string abilityId, float seconds)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return;
            }

            Ensure().readyTimes[abilityId] = Time.unscaledTime + Mathf.Max(0f, seconds);
        }

        public static void Clear(string abilityId)
        {
            Ensure().readyTimes.Remove(abilityId);
        }

        public static void ClearAll()
        {
            Ensure().readyTimes.Clear();
        }
    }
}
