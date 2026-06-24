namespace Elementborn.Core
{
    /// <summary>The special move a mount grants its rider, keyed by how the mount travels. Pure data; the Game
    /// layer (MountController) reads this and executes the burst on a button while mounted. Ground mounts charge,
    /// water mounts surge, flyers divebomb.</summary>
    public enum MountSkill { None, Charge, Surge, Divebomb }

    public static class MountAbilities
    {
        public static MountSkill ForLocomotion(LocomotionType loco)
        {
            switch (loco)
            {
                case LocomotionType.Ground:  return MountSkill.Charge;
                case LocomotionType.Water:   return MountSkill.Surge;
                case LocomotionType.Flying:  return MountSkill.Divebomb;
                default:                     return MountSkill.None;
            }
        }

        public static string DisplayName(MountSkill skill)
        {
            switch (skill)
            {
                case MountSkill.Charge:   return "Charge";
                case MountSkill.Surge:    return "Surge";
                case MountSkill.Divebomb: return "Divebomb";
                default:                  return "";
            }
        }

        /// <summary>Seconds before the skill can be used again.</summary>
        public static float Cooldown(MountSkill skill) => skill == MountSkill.None ? 0f : 4f;

        /// <summary>Extra speed (m/s) applied along the burst direction while the skill is active.</summary>
        public static float BurstSpeed(MountSkill skill)
        {
            switch (skill)
            {
                case MountSkill.Charge:   return 18f;
                case MountSkill.Surge:    return 16f;
                case MountSkill.Divebomb: return 20f;
                default:                  return 0f;
            }
        }

        /// <summary>How long the burst lasts (seconds).</summary>
        public static float BurstDuration(MountSkill skill) => skill == MountSkill.None ? 0f : 0.5f;

        /// <summary>True for skills that stagger nearby foes as the mount drives through them.</summary>
        public static bool HasImpact(MountSkill skill) => skill == MountSkill.Charge || skill == MountSkill.Divebomb;
    }
}
