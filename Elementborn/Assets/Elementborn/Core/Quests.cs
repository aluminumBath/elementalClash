using System;
using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>What a quest objective tracks. Targets are matched by name (a CreatureKind, a Currency, or an
    /// NpcId); an empty target means "any".</summary>
    public enum ObjectiveKind { DefeatCreature, TameCreature, CollectCurrency, CollectItem, TalkToNpc, CastAbility, SummonCreature, EquipItem, CraftItem, ClaimFeatured }

    public sealed class QuestObjective
    {
        public ObjectiveKind Kind { get; }
        public string Target { get; }   // "" = any
        public int Required { get; }
        public string Description { get; }

        public QuestObjective(ObjectiveKind kind, string target, int required, string description)
        {
            Kind = kind; Target = target ?? ""; Required = required < 1 ? 1 : required; Description = description;
        }
    }

    public sealed class QuestReward
    {
        public Currency Currency { get; }
        public int Amount { get; }
        public int Sigils { get; }     // optional Summon Beacon pull currency (0 = none)
        public string Note { get; }   // optional flavour line shown on turn-in

        public QuestReward(Currency currency, int amount, string note = null, int sigils = 0)
        {
            Currency = currency; Amount = amount < 0 ? 0 : amount; Sigils = sigils < 0 ? 0 : sigils; Note = note;
        }
    }

    public sealed class QuestDefinition
    {
        public string Id { get; }
        public string Title => Loc.T(_title);       // localized on read; English is the fallback key
        public string Summary => Loc.T(_summary);
        public string GiverNpcId { get; }   // GuideNpcId.ToString()
        public IReadOnlyList<QuestObjective> Objectives { get; }
        public QuestReward Reward { get; }
        public IReadOnlyList<string> Prerequisites { get; }   // quest ids that must be Completed first (empty = none)

        private readonly string _title;
        private readonly string _summary;

        public QuestDefinition(string id, string title, string summary, string giverNpcId,
                               IReadOnlyList<QuestObjective> objectives, QuestReward reward,
                               IReadOnlyList<string> prerequisites = null)
        {
            Id = id; _title = title; _summary = summary; GiverNpcId = giverNpcId;
            Objectives = objectives; Reward = reward;
            Prerequisites = prerequisites ?? Array.Empty<string>();
        }
    }

    public enum QuestStatus { NotStarted, Active, ReadyToTurnIn, Completed }

    public sealed class QuestState
    {
        public QuestDefinition Definition { get; }
        public QuestStatus Status { get; internal set; }
        public int[] Progress { get; }

        public QuestState(QuestDefinition def)
        {
            Definition = def;
            Progress = new int[def.Objectives.Count];
            Status = QuestStatus.NotStarted;
        }

        public bool ObjectiveMet(int i) => Progress[i] >= Definition.Objectives[i].Required;

        public bool AllMet()
        {
            for (int i = 0; i < Definition.Objectives.Count; i++)
                if (Progress[i] < Definition.Objectives[i].Required) return false;
            return true;
        }
    }

    /// <summary>
    /// The pure quest tracker. Seed it with definitions; <see cref="Start"/> a quest; feed gameplay events through
    /// <see cref="Record"/>. It advances active quests, flips them to <see cref="QuestStatus.ReadyToTurnIn"/> when
    /// every objective is met, and <see cref="TurnIn"/> returns the reward (granting it is the caller's job).
    /// Raises <see cref="Changed"/> for UI. No Unity types — unit-testable on its own.
    /// </summary>
    public sealed class QuestLog
    {
        private readonly Dictionary<string, QuestState> _states = new Dictionary<string, QuestState>();
        public event Action Changed;

        public QuestLog(IEnumerable<QuestDefinition> definitions)
        {
            foreach (var d in definitions) _states[d.Id] = new QuestState(d);
        }

        public QuestState Get(string id) => _states.TryGetValue(id, out var s) ? s : null;
        public IReadOnlyList<QuestState> All() => new List<QuestState>(_states.Values);

        public IReadOnlyList<QuestState> AvailableFrom(string npcId)
        {
            var list = new List<QuestState>();
            foreach (var s in _states.Values)
                if (s.Definition.GiverNpcId == npcId && s.Status == QuestStatus.NotStarted && PrerequisitesMet(s.Definition.Id))
                    list.Add(s);
            return list;
        }

        /// <summary>True if every prerequisite quest of <paramref name="id"/> is Completed (or it has none).</summary>
        public bool PrerequisitesMet(string id)
        {
            var s = Get(id);
            if (s == null) return false;
            foreach (var pre in s.Definition.Prerequisites)
            {
                var ps = Get(pre);
                if (ps == null || ps.Status != QuestStatus.Completed) return false;
            }
            return true;
        }
        public IReadOnlyList<QuestState> ActiveFrom(string npcId) => Filter(npcId, QuestStatus.Active);
        public IReadOnlyList<QuestState> TurnInsFor(string npcId) => Filter(npcId, QuestStatus.ReadyToTurnIn);

        /// <summary>Quests worth showing in a quest log: active or ready to turn in.</summary>
        public IReadOnlyList<QuestState> Tracked()
        {
            var list = new List<QuestState>();
            foreach (var s in _states.Values)
                if (s.Status == QuestStatus.Active || s.Status == QuestStatus.ReadyToTurnIn) list.Add(s);
            return list;
        }

        private IReadOnlyList<QuestState> Filter(string npcId, QuestStatus status)
        {
            var list = new List<QuestState>();
            foreach (var s in _states.Values)
                if (s.Definition.GiverNpcId == npcId && s.Status == status) list.Add(s);
            return list;
        }

        public bool Start(string id)
        {
            var s = Get(id);
            if (s == null || s.Status != QuestStatus.NotStarted) return false;
            if (!PrerequisitesMet(id)) return false;
            s.Status = QuestStatus.Active;
            Changed?.Invoke();
            return true;
        }

        public void Record(ObjectiveKind kind, string target, int amount = 1)
        {
            if (amount <= 0) return;
            bool changed = false;
            foreach (var s in _states.Values)
            {
                if (s.Status != QuestStatus.Active) continue;
                var objs = s.Definition.Objectives;
                for (int i = 0; i < objs.Count; i++)
                {
                    var o = objs[i];
                    if (o.Kind != kind) continue;
                    if (o.Target.Length > 0 && o.Target != target) continue;   // "" = any
                    if (s.Progress[i] >= o.Required) continue;
                    s.Progress[i] = Math.Min(o.Required, s.Progress[i] + amount);
                    changed = true;
                }
                if (s.AllMet()) { s.Status = QuestStatus.ReadyToTurnIn; changed = true; }
            }
            if (changed) Changed?.Invoke();
        }

        /// <summary>Completes a ready quest and returns its reward (or null if it wasn't ready).</summary>
        public QuestReward TurnIn(string id)
        {
            var s = Get(id);
            if (s == null || s.Status != QuestStatus.ReadyToTurnIn) return null;
            s.Status = QuestStatus.Completed;
            Changed?.Invoke();
            return s.Definition.Reward;
        }

        /// <summary>Restores a quest's status and objective progress from a save (ignores unknown ids).</summary>
        public void Restore(string id, QuestStatus status, int[] progress)
        {
            var s = Get(id);
            if (s == null) return;
            s.Status = status;
            if (progress != null)
                for (int i = 0; i < progress.Length && i < s.Progress.Length; i++) s.Progress[i] = progress[i];
        }
    }
}
