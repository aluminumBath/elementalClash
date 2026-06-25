namespace Elementborn.Core.Social
{
    public enum DuelPhase { Offered, InProgress, Finished, Declined, Cancelled }

    /// <summary>A pure, engine-free 1v1 duel: one player challenges, the other accepts or declines, then it's a
    /// first-to-<see cref="Target"/> contest that finishes the moment someone reaches the target, naming the winner.
    /// Scoring is rejected before the duel is accepted, after it finishes, or from a non-participant; declining and
    /// cancelling are terminal. Deterministic and unit-tested; <c>DuelController</c> drives the arena and feeds round
    /// outcomes in (a simulated opponent offline, real combat over the network).</summary>
    public sealed class DuelMatch
    {
        public string Challenger { get; }
        public string Opponent { get; }
        public int Target { get; }
        public DuelPhase Phase { get; private set; } = DuelPhase.Offered;
        public int ChallengerScore { get; private set; }
        public int OpponentScore { get; private set; }
        public string Winner { get; private set; }

        public DuelMatch(string challenger, string opponent, int target = 3)
        {
            Challenger = challenger;
            Opponent = opponent;
            Target = target < 1 ? 1 : target;
        }

        public bool IsParticipant(string id) => id == Challenger || id == Opponent;

        public bool Accept()
        {
            if (Phase != DuelPhase.Offered) return false;
            Phase = DuelPhase.InProgress;
            return true;
        }

        public bool Decline()
        {
            if (Phase != DuelPhase.Offered) return false;
            Phase = DuelPhase.Declined;
            return true;
        }

        public void Cancel()
        {
            if (Phase == DuelPhase.Offered || Phase == DuelPhase.InProgress) Phase = DuelPhase.Cancelled;
        }

        /// <summary>Credit a round to a participant; reaching the target finishes the duel with them as the winner.</summary>
        public bool Score(string playerId)
        {
            if (Phase != DuelPhase.InProgress || !IsParticipant(playerId)) return false;
            if (playerId == Challenger) ChallengerScore++;
            else OpponentScore++;
            if (ChallengerScore >= Target) Finish(Challenger);
            else if (OpponentScore >= Target) Finish(Opponent);
            return true;
        }

        public int ScoreOf(string playerId) =>
            playerId == Challenger ? ChallengerScore : (playerId == Opponent ? OpponentScore : 0);

        private void Finish(string winner)
        {
            Winner = winner;
            Phase = DuelPhase.Finished;
        }
    }
}
