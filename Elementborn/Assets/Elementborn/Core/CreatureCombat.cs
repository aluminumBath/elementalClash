namespace Elementborn.Core
{
    /// <summary>How tough a creature is in a fight — used when one defends itself after being attacked.</summary>
    public readonly struct CreatureCombatStats
    {
        public readonly float MaxHealth;
        public readonly float MoveSpeed;
        public readonly float Damage;

        public CreatureCombatStats(float maxHealth, float moveSpeed, float damage)
        {
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            Damage = damage;
        }
    }

    public static class CreatureCombat
    {
        public static CreatureCombatStats For(CreatureKind kind)
        {
            switch (kind)
            {
                //                                       hp     spd   dmg
                case CreatureKind.FireDragon:   return new CreatureCombatStats(320f, 5.0f, 25f);
                case CreatureKind.WaterDragon:  return new CreatureCombatStats(320f, 5.0f, 25f);
                case CreatureKind.Mermaid:      return new CreatureCombatStats(90f,  3.0f, 12f);
                case CreatureKind.EarthMole:    return new CreatureCombatStats(160f, 3.0f, 18f);
                case CreatureKind.EarthCat:     return new CreatureCombatStats(50f,  4.0f, 8f);
                case CreatureKind.AirDragonfly: return new CreatureCombatStats(110f, 6.0f, 14f);
                case CreatureKind.AirJellyfish: return new CreatureCombatStats(130f, 3.0f, 12f);
                case CreatureKind.Horse:        return new CreatureCombatStats(100f, 6.0f, 6f);

                case CreatureKind.Spider:           return new CreatureCombatStats(60f,  4.0f, 12f);
                case CreatureKind.WaterCat:         return new CreatureCombatStats(45f,  5.0f, 10f);
                case CreatureKind.IceCat:           return new CreatureCombatStats(45f,  5.0f, 10f);
                case CreatureKind.Phoenix:          return new CreatureCombatStats(140f, 6.0f, 20f);
                case CreatureKind.ElectricSquirrel: return new CreatureCombatStats(40f,  6.0f, 12f);
                case CreatureKind.Dog:              return new CreatureCombatStats(70f,  5.0f, 12f);

                default: return new CreatureCombatStats(60f, 4.0f, 10f);
            }
        }
    }
}
