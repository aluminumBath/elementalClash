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

                // Wildlife
                case CreatureKind.Eel:        return new CreatureCombatStats(40f,  4.0f,  8f);
                case CreatureKind.Crab:       return new CreatureCombatStats(35f,  2.0f,  6f);
                case CreatureKind.Monkey:     return new CreatureCombatStats(30f,  5.0f,  5f);
                case CreatureKind.Crocodile:  return new CreatureCombatStats(140f, 2.5f, 22f);
                case CreatureKind.Snake:      return new CreatureCombatStats(45f,  3.5f, 14f);
                case CreatureKind.Roc:        return new CreatureCombatStats(180f, 6.0f, 20f);
                case CreatureKind.Thunderbird:return new CreatureCombatStats(160f, 6.5f, 22f);
                case CreatureKind.Rhino:      return new CreatureCombatStats(220f, 4.0f, 26f);
                case CreatureKind.Tiger:      return new CreatureCombatStats(110f, 6.5f, 24f);

                // Exotic apex creatures — strong; the apex flyers/sea-beasts are formidable.
                case CreatureKind.Ridgewing:   return new CreatureCombatStats(170f, 7.0f, 22f);
                case CreatureKind.Glidewisp:   return new CreatureCombatStats(70f,  7.5f, 12f);
                case CreatureKind.Skytyrant:   return new CreatureCombatStats(420f, 7.5f, 40f);
                case CreatureKind.Goldkoi:     return new CreatureCombatStats(150f, 6.0f, 20f);
                case CreatureKind.Direstalker: return new CreatureCombatStats(300f, 7.0f, 36f);
                case CreatureKind.Skimfin:     return new CreatureCombatStats(140f, 8.0f, 22f);
                case CreatureKind.Gillcloak:   return new CreatureCombatStats(180f, 5.5f, 24f);
                case CreatureKind.Tidewarden:  return new CreatureCombatStats(600f, 5.0f, 44f);

                default: return new CreatureCombatStats(60f, 4.0f, 10f);
            }
        }
    }
}
