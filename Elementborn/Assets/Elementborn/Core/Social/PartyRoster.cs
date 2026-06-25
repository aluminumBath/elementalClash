using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    public enum PartyJoinResult { Joined, AlreadyMember, PartyFull, NoParty }

    /// <summary>Pure, engine-free party membership and leadership: a small group of players who've teamed up for
    /// co-op — one leader, a capped roster, and join / leave / kick / promote / disband with automatic leader
    /// succession when the leader departs. Deterministic and unit-tested; <c>PartyController</c> wraps this and wires
    /// it to the invite system, presence, and the UI.</summary>
    public sealed class PartyRoster
    {
        public const int MaxMembers = 4;

        private readonly List<string> _members = new List<string>();

        public string LeaderId { get; private set; }
        public bool Active => _members.Count > 0;
        public int Count => _members.Count;
        public IReadOnlyList<string> Members => _members;
        public bool Contains(string id) => _members.Contains(id);
        public bool IsLeader(string id) => Active && LeaderId == id;
        public bool IsFull => _members.Count >= MaxMembers;

        /// <summary>Start a fresh party led by the founder. Resets any existing roster.</summary>
        public void Form(string founderId)
        {
            _members.Clear();
            _members.Add(founderId);
            LeaderId = founderId;
        }

        public PartyJoinResult Join(string id)
        {
            if (!Active) return PartyJoinResult.NoParty;
            if (_members.Contains(id)) return PartyJoinResult.AlreadyMember;
            if (_members.Count >= MaxMembers) return PartyJoinResult.PartyFull;
            _members.Add(id);
            return PartyJoinResult.Joined;
        }

        /// <summary>A member leaves (or is removed). The leader leaving hands off to the next member; emptying the
        /// roster disbands it.</summary>
        public void Leave(string id)
        {
            if (!_members.Remove(id)) return;
            if (_members.Count == 0) { LeaderId = null; return; }
            if (LeaderId == id) LeaderId = _members[0]; // succession to the next member
        }

        public bool Kick(string actorId, string targetId)
        {
            if (!IsLeader(actorId) || actorId == targetId || !_members.Contains(targetId)) return false;
            Leave(targetId);
            return true;
        }

        public bool Promote(string actorId, string targetId)
        {
            if (!IsLeader(actorId) || !_members.Contains(targetId)) return false;
            LeaderId = targetId;
            return true;
        }

        public void Disband()
        {
            _members.Clear();
            LeaderId = null;
        }
    }
}
