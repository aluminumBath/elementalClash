using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class SpellCooldownTracker : MonoBehaviour
    {
        public static SpellCooldownTracker Instance { get; private set; }

        [SerializeField] private List<SpellCooldownRecord> records = new List<SpellCooldownRecord>();

        public IReadOnlyList<SpellCooldownRecord> Records => records;

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

        public static SpellCooldownTracker Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(SpellCooldownTracker));
            return go.AddComponent<SpellCooldownTracker>();
        }

        public static bool IsReady(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return false;
            }

            SpellCooldownRecord record = Ensure().GetOrCreate(spellId);
            return Time.unscaledTime >= record.ReadyAtUnscaledTime;
        }

        public static float Remaining(string spellId)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return 0f;
            }

            SpellCooldownRecord record = Ensure().GetOrCreate(spellId);
            return Mathf.Max(0f, record.ReadyAtUnscaledTime - Time.unscaledTime);
        }

        public static void StartCooldown(string spellId, float seconds)
        {
            if (string.IsNullOrWhiteSpace(spellId))
            {
                return;
            }

            SpellCooldownRecord record = Ensure().GetOrCreate(spellId);
            record.ReadyAtUnscaledTime = Time.unscaledTime + Mathf.Max(0f, seconds);
            record.TimesCast++;
        }

        public static bool HasCastBefore(string spellId)
        {
            return Ensure().GetOrCreate(spellId).TimesCast > 0;
        }

        public static void MarkFirstCastRewardGranted(string spellId)
        {
            Ensure().GetOrCreate(spellId).FirstCastRewardGranted = true;
        }

        public static bool FirstCastRewardGranted(string spellId)
        {
            return Ensure().GetOrCreate(spellId).FirstCastRewardGranted;
        }

        public static void ClearCooldown(string spellId)
        {
            Ensure().GetOrCreate(spellId).ReadyAtUnscaledTime = 0f;
        }

        public static void ClearAll()
        {
            foreach (var record in Ensure().records)
            {
                if (record != null)
                {
                    record.ReadyAtUnscaledTime = 0f;
                }
            }
        }

        public void ImportRecord(SpellCooldownRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.SpellId))
            {
                return;
            }

            records.RemoveAll(r => r.SpellId == record.SpellId);
            records.Add(record);
        }

        public void ClearRecords()
        {
            records.Clear();
        }

        private SpellCooldownRecord GetOrCreate(string spellId)
        {
            SpellCooldownRecord record = records.Find(r => r.SpellId == spellId);
            if (record != null)
            {
                return record;
            }

            record = new SpellCooldownRecord { SpellId = spellId };
            records.Add(record);
            return record;
        }
    }
}
