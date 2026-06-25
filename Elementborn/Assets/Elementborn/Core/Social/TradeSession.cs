using System.Collections.Generic;

namespace Elementborn.Core.Social
{
    public enum TradePhase { Building, Locked, Completed, Cancelled }

    /// <summary>One side's side of the table: a multiset of stack keys to quantities. Keys are opaque to the core —
    /// the game layer encodes item ids and currency under distinct keys — so trading stays engine-free and generic.</summary>
    public sealed class TradeOffer
    {
        private readonly Dictionary<string, int> _lines = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> Lines => _lines;
        public bool IsEmpty => _lines.Count == 0;

        internal void Set(string key, int amount)
        {
            if (key == null) return;
            if (amount <= 0) _lines.Remove(key);
            else _lines[key] = amount;
        }

        internal int Get(string key) => key != null && _lines.TryGetValue(key, out var v) ? v : 0;
        internal void Clear() => _lines.Clear();
    }

    /// <summary>A two-party item trade with a tamper-safe lock/confirm handshake: both sides build their offers, then
    /// both lock, then both confirm, and only then does it complete. The safety rule is the whole point — <b>any</b>
    /// change to <b>either</b> offer clears all locks and confirmations, so nobody can alter the deal after the other
    /// side has committed to what they saw. Confirming is only possible once both sides are locked. Either side can
    /// cancel until completion. Engine-free and unit-tested; <c>TradeController</c> validates offers against real
    /// inventory and performs the atomic swap when the phase reaches <see cref="TradePhase.Completed"/>.</summary>
    public sealed class TradeSession
    {
        public string PartyA { get; }
        public string PartyB { get; }
        public TradeOffer OfferA { get; } = new TradeOffer();
        public TradeOffer OfferB { get; } = new TradeOffer();
        public TradePhase Phase { get; private set; } = TradePhase.Building;

        private bool _lockA, _lockB, _confirmA, _confirmB;

        public TradeSession(string partyA, string partyB) { PartyA = partyA; PartyB = partyB; }

        private bool IsParty(string id) => id == PartyA || id == PartyB;
        private TradeOffer OfferOf(string id) => id == PartyA ? OfferA : OfferB;
        private bool Live => Phase != TradePhase.Completed && Phase != TradePhase.Cancelled;

        public bool IsLocked(string id) => id == PartyA ? _lockA : (id == PartyB && _lockB);
        public bool IsConfirmed(string id) => id == PartyA ? _confirmA : (id == PartyB && _confirmB);

        /// <summary>Set how much of a stack key this party is offering (0 removes it). Resets all commitment.</summary>
        public bool SetOffer(string actorId, string key, int amount)
        {
            if (!Live || !IsParty(actorId)) return false;
            OfferOf(actorId).Set(key, amount);
            ResetCommitment();
            return true;
        }

        public bool Lock(string actorId)
        {
            if (!Live || !IsParty(actorId)) return false;
            if (actorId == PartyA) _lockA = true; else _lockB = true;
            Recompute();
            return true;
        }

        public bool Unlock(string actorId)
        {
            if (!Live || !IsParty(actorId)) return false;
            if (actorId == PartyA) _lockA = false; else _lockB = false;
            _confirmA = _confirmB = false; // unlocking voids confirmations
            Recompute();
            return true;
        }

        public bool Confirm(string actorId)
        {
            if (Phase != TradePhase.Locked || !IsParty(actorId)) return false; // both must be locked first
            if (actorId == PartyA) _confirmA = true; else _confirmB = true;
            Recompute();
            return true;
        }

        public void Cancel() { if (Live) Phase = TradePhase.Cancelled; }

        private void ResetCommitment()
        {
            _lockA = _lockB = _confirmA = _confirmB = false;
            Recompute();
        }

        private void Recompute()
        {
            if (!Live) return;
            if (_confirmA && _confirmB) { Phase = TradePhase.Completed; return; }
            Phase = (_lockA && _lockB) ? TradePhase.Locked : TradePhase.Building;
        }
    }
}
