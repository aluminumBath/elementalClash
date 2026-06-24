using System;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Per-session event logger. Opens a session at startup, auto-captures gameplay actions off the
    /// <see cref="QuestEvents"/> bus, and exposes typed <c>Log*</c> calls for logins (user details, never
    /// passwords), moves and their math totals, user statuses, spawn / respawn points, and the final leaderboard.
    /// Events accumulate in a pure <see cref="SessionEventLog"/> and flush in batches through an
    /// <see cref="IEventSink"/> (console by default; a Neon-backed, server-routed sink can be installed via
    /// <see cref="SetSink"/>). One is created automatically and survives scene loads.
    /// </summary>
    public sealed class GameEventLogger : MonoBehaviour
    {
        public static GameEventLogger Instance { get; private set; }

        private SessionEventLog _log;
        private IEventSink _sink = new ConsoleEventSink();
        private const int FlushThreshold = 50;
        private const float FlushIntervalSeconds = 15f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("GameEventLogger");
            DontDestroyOnLoad(go);
            go.AddComponent<GameEventLogger>();
        }

        public string SessionId => _log != null ? _log.SessionId : null;

        /// <summary>Swap the destination (e.g. install the Neon/server sink at startup). Buffered events are
        /// kept and routed to the new sink on the next flush, so early events (session start, login) still land.</summary>
        public void SetSink(IEventSink sink)
        {
            if (sink == null) return;
            _sink = sink;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _log = new SessionEventLog(Guid.NewGuid().ToString()); // dashed form → valid Postgres uuid
            Record(GameEventKind.SessionStart, "session_start",
                $"platform={Application.platform};version={Application.version};unity={Application.unityVersion}");
            Subscribe();
            InvokeRepeating(nameof(Flush), FlushIntervalSeconds, FlushIntervalSeconds);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Unsubscribe();
            Flush();
            Instance = null;
        }

        private void OnApplicationPause(bool paused) { if (paused) Flush(); }
        private void OnApplicationQuit() { Record(GameEventKind.Action, "session_end", null); Flush(); }

        // ---- typed entry points (the categories to capture) ----

        /// <summary>Record a sign-in. Pass the user id / display name only — NEVER a password or token.</summary>
        public void LogLogin(string userId, string displayName) =>
            Record(GameEventKind.Login, "login", $"user={userId};name={displayName}");

        /// <summary>A discrete action / function call, with the error text when it failed.</summary>
        public void LogAction(string action, bool ok = true, string error = null) =>
            Record(ok ? GameEventKind.Action : GameEventKind.Error, action, ok ? null : $"error={error}");

        /// <summary>A move's math totals (e.g. damage = base*mult + bonus); pass the breakdown for debugging.</summary>
        public void LogMath(string label, double total, string breakdown = null) =>
            Record(GameEventKind.Math, label, $"total={total}" + (string.IsNullOrEmpty(breakdown) ? "" : ";" + breakdown));

        /// <summary>A user/world status value (health, level, silver, …).</summary>
        public void LogStatus(string stat, double value) =>
            Record(GameEventKind.Status, stat, $"value={value}");

        public void LogSpawn(string point) => Record(GameEventKind.Spawn, "spawn", $"point={point}");
        public void LogRespawn(string point) => Record(GameEventKind.Respawn, "respawn", $"point={point}");

        /// <summary>The final leaderboard — pass a compact, already-formatted summary.</summary>
        public void LogLeaderboard(string summary) => Record(GameEventKind.Leaderboard, "leaderboard", summary);

        // ---- internals ----

        private void Record(GameEventKind kind, string name, string detail)
        {
            if (_log == null) return;
            _log.Record(kind, name, detail, DateTime.UtcNow.Ticks);
            if (_log.PendingCount >= FlushThreshold) Flush();
        }

        /// <summary>Ship whatever has accumulated. Safe to call often; no-ops when empty.</summary>
        public void Flush()
        {
            if (_log == null || _log.PendingCount == 0) return;
            var batch = _log.Drain();
            try { _sink.Send(_log.SessionId, SessionEventLog.ToJsonBatch(_log.SessionId, batch), batch.Count); }
            catch (Exception e) { Debug.LogWarning("[events] sink failed: " + e.Message); }
        }

        // ---- auto-capture off the existing gameplay bus ----

        private void Subscribe()
        {
            QuestEvents.AbilityCast += OnCast;
            QuestEvents.CreatureDefeated += OnDefeated;
            QuestEvents.CreatureTamed += OnTamed;
            QuestEvents.SummonPerformed += OnSummon;
            QuestEvents.ItemCrafted += OnCrafted;
            QuestEvents.ItemEquipped += OnEquipped;
            QuestEvents.FeaturedClaimed += OnFeatured;
            QuestEvents.CurrencyGained += OnCurrency;
            QuestEvents.ItemCollected += OnItem;
            QuestEvents.TalkedToNpc += OnTalk;
            QuestEvents.QuestCompleted += OnQuest;
        }

        private void Unsubscribe()
        {
            QuestEvents.AbilityCast -= OnCast;
            QuestEvents.CreatureDefeated -= OnDefeated;
            QuestEvents.CreatureTamed -= OnTamed;
            QuestEvents.SummonPerformed -= OnSummon;
            QuestEvents.ItemCrafted -= OnCrafted;
            QuestEvents.ItemEquipped -= OnEquipped;
            QuestEvents.FeaturedClaimed -= OnFeatured;
            QuestEvents.CurrencyGained -= OnCurrency;
            QuestEvents.ItemCollected -= OnItem;
            QuestEvents.TalkedToNpc -= OnTalk;
            QuestEvents.QuestCompleted -= OnQuest;
        }

        private void OnCast(string element, string intent) => Record(GameEventKind.Action, "cast", $"element={element};intent={intent}");
        private void OnDefeated(string kind) => Record(GameEventKind.Action, "defeat", $"creature={kind}");
        private void OnTamed(string kind) => Record(GameEventKind.Action, "tame", $"creature={kind}");
        private void OnSummon(string kind) => Record(GameEventKind.Action, "summon", $"creature={kind}");
        private void OnCrafted(string item) => Record(GameEventKind.Action, "craft", $"item={item}");
        private void OnEquipped(string item) => Record(GameEventKind.Action, "equip", $"item={item}");
        private void OnFeatured(string kind) => Record(GameEventKind.Action, "claim_featured", $"creature={kind}");
        private void OnCurrency(string currency, int amt) => Record(GameEventKind.Action, "currency", $"currency={currency};amount={amt}");
        private void OnItem(string item, int amt) => Record(GameEventKind.Action, "item", $"item={item};amount={amt}");
        private void OnTalk(string npc) => Record(GameEventKind.Action, "talk", $"npc={npc}");
        private void OnQuest(string quest) => Record(GameEventKind.Action, "quest_complete", $"quest={quest}");
    }
}
