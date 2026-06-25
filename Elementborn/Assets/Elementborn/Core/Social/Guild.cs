using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    public enum GuildRank { Member, Officer, Leader }

    public sealed class GuildMember
    {
        public string Id { get; }
        public GuildRank Rank { get; internal set; }
        public GuildMember(string id, GuildRank rank) { Id = id; Rank = rank; }
    }

    /// <summary>Pure, engine-free player guild: a persistent named group with a ranked roster (Leader > Officer >
    /// Member) and rank-gated actions — join, leave (with automatic leader succession), kick (only ranks strictly
    /// below you), promote/demote (leader only, with single-leader transfer of command), and rename. Distinct from
    /// the NPC lore factions. Deterministic and unit-tested; <c>GuildController</c> wraps it, saves it with the game,
    /// and wires chat + UI.</summary>
    public sealed class Guild
    {
        public string Id { get; }
        public string Name { get; private set; }

        private readonly List<GuildMember> _members = new List<GuildMember>();
        public IReadOnlyList<GuildMember> Members => _members;
        public int Count => _members.Count;

        public Guild(string id, string name, string founderId)
        {
            Id = id;
            Name = name;
            _members.Add(new GuildMember(founderId, GuildRank.Leader));
        }

        private Guild(string id, string name) { Id = id; Name = name; } // empty roster, for Restore

        /// <summary>Rebuild a guild from a saved roster, preserving each member's rank exactly.</summary>
        public static Guild Restore(string id, string name, IReadOnlyList<string> ids, IReadOnlyList<GuildRank> ranks)
        {
            var g = new Guild(id, name);
            int n = ids.Count < ranks.Count ? ids.Count : ranks.Count;
            for (int i = 0; i < n; i++) g._members.Add(new GuildMember(ids[i], ranks[i]));
            return g;
        }

        public GuildMember Find(string id)
        {
            foreach (var m in _members) if (m.Id == id) return m;
            return null;
        }

        public bool Contains(string id) => Find(id) != null;

        public string LeaderId
        {
            get { foreach (var m in _members) if (m.Rank == GuildRank.Leader) return m.Id; return null; }
        }

        public bool IsLeader(string id) { var m = Find(id); return m != null && m.Rank == GuildRank.Leader; }
        public bool CanManage(string id) { var m = Find(id); return m != null && (m.Rank == GuildRank.Officer || m.Rank == GuildRank.Leader); }

        public bool Join(string id)
        {
            if (Contains(id)) return false;
            _members.Add(new GuildMember(id, GuildRank.Member));
            return true;
        }

        public bool Leave(string id)
        {
            var m = Find(id);
            if (m == null) return false;
            bool wasLeader = m.Rank == GuildRank.Leader;
            _members.Remove(m);
            if (wasLeader) PromoteSuccessor();
            return true;
        }

        /// <summary>Remove a member who ranks strictly below the actor.</summary>
        public bool Kick(string actorId, string targetId)
        {
            if (actorId == targetId) return false;
            var actor = Find(actorId);
            var target = Find(targetId);
            if (actor == null || target == null) return false;
            if (actor.Rank <= target.Rank) return false;
            _members.Remove(target);
            return true;
        }

        /// <summary>Leader raises a member one step. Officer → Leader transfers command (actor steps down to Officer).</summary>
        public bool Promote(string actorId, string targetId)
        {
            if (!IsLeader(actorId) || actorId == targetId) return false;
            var target = Find(targetId);
            if (target == null) return false;
            if (target.Rank == GuildRank.Member) { target.Rank = GuildRank.Officer; return true; }
            if (target.Rank == GuildRank.Officer)
            {
                var actor = Find(actorId);
                target.Rank = GuildRank.Leader;
                if (actor != null) actor.Rank = GuildRank.Officer; // single-leader invariant
                return true;
            }
            return false;
        }

        /// <summary>Leader lowers an Officer back to Member.</summary>
        public bool Demote(string actorId, string targetId)
        {
            if (!IsLeader(actorId) || actorId == targetId) return false;
            var target = Find(targetId);
            if (target == null || target.Rank != GuildRank.Officer) return false;
            target.Rank = GuildRank.Member;
            return true;
        }

        public bool Rename(string actorId, string newName)
        {
            if (!IsLeader(actorId) || string.IsNullOrWhiteSpace(newName)) return false;
            Name = newName;
            return true;
        }

        private void PromoteSuccessor()
        {
            if (_members.Count == 0 || LeaderId != null) return;
            GuildMember best = _members[0];
            foreach (var m in _members) if (m.Rank > best.Rank) best = m;
            best.Rank = GuildRank.Leader;
        }
    }
}
