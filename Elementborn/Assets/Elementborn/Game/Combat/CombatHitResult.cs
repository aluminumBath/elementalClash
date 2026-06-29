namespace Elementborn.Game
{
    public readonly struct CombatHitResult
    {
        public readonly float FinalDamage;
        public readonly bool Critical;
        public readonly bool Weakness;
        public readonly bool Resisted;
        public readonly string Notes;

        public CombatHitResult(float finalDamage, bool critical, bool weakness, bool resisted, string notes)
        {
            FinalDamage = finalDamage;
            Critical = critical;
            Weakness = weakness;
            Resisted = resisted;
            Notes = notes ?? "";
        }
    }
}
