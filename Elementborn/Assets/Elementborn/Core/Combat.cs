using UnityEngine;

namespace Elementborn.Core
{
    public enum OutcomeKind
    {
        None,
        Projectile,
        Melee,
        Barrier,
        Movement,
        Control, // Sanguine Grip on a raycast target
        Sweep,   // wide, multi-target arc in front of the caster
        Heavy    // committed impact zone that lands at a point in front of the caster
    }

    /// <summary>
    /// The specific flavor of an ability, for presentation and status effects.
    /// Distinct from <see cref="SubArt"/>, which tracks rare progression unlocks.
    /// </summary>
    public enum AbilityVariant
    {
        Standard,
        Magmacraft,   // fire sub-art
        Lightning,    // advanced fire channeling
        Ice,          // water channeling technique
        SanguineGrip, // water sub-art
        Oreshaping,   // earth sub-art
        Flight        // air sub-art (comfortable dash/glide)
    }

    public enum StatusKind
    {
        None,
        Slow,    // ice
        Stun,    // lightning
        Burn,    // lava (damage over time)
        Control  // Sanguine Grip immobilize
    }

    public readonly struct StatusEffect
    {
        public StatusKind Kind { get; }
        /// <summary>For Slow: fraction of speed removed (0-1). For Burn: damage per second.</summary>
        public float Magnitude { get; }
        public float Duration { get; }

        public StatusEffect(StatusKind kind, float magnitude, float duration)
        {
            Kind = kind;
            Magnitude = magnitude;
            Duration = duration;
        }

        public bool IsEmpty => Kind == StatusKind.None;
        public static StatusEffect None => new StatusEffect(StatusKind.None, 0f, 0f);
    }

    /// <summary>The resolved effect of an intent, ready for presentation to spawn visuals.</summary>
    public readonly struct AbilityOutcome
    {
        public OutcomeKind Kind { get; }
        public Element Element { get; }
        public AbilityVariant Variant { get; }
        public Vector3 Direction { get; }
        public float Damage { get; }
        public float Speed { get; }
        public StatusEffect Status { get; }
        /// <summary>Impulse applied to the target on hit (air/earth/blood displacement). 0 = none.</summary>
        public float Knockback { get; }

        /// <summary>Cast charge, 0..1 (held duration). Heavy scales its blast by it; most outcomes ignore it.</summary>
        public float Charge { get; }

        public AbilityOutcome(OutcomeKind kind, Element element, AbilityVariant variant,
            Vector3 direction, float damage, float speed, StatusEffect status, float knockback = 0f, float charge = 0f)
        {
            Kind = kind;
            Element = element;
            Variant = variant;
            Direction = direction;
            Damage = damage;
            Speed = speed;
            Status = status;
            Knockback = knockback;
            Charge = charge;
        }

        /// <summary>A copy with damage and knockback scaled (used by weather effects on channeling).</summary>
        public AbilityOutcome Scaled(float multiplier) =>
            new AbilityOutcome(Kind, Element, Variant, Direction, Damage * multiplier, Speed, Status, Knockback * multiplier, Charge);

        /// <summary>A copy carrying a specific element (an elemental arrowhead riding a longbow shot).</summary>
        public AbilityOutcome WithElement(Element element) =>
            new AbilityOutcome(Kind, element, Variant, Direction, Damage, Speed, Status, Knockback, Charge);

        public bool IsEmpty => Kind == OutcomeKind.None;

        public static AbilityOutcome Empty => new AbilityOutcome(
            OutcomeKind.None, default, AbilityVariant.Standard, Vector3.zero, 0f, 0f, StatusEffect.None);
    }

    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public Element Source { get; }
        public AbilityVariant Variant { get; }

        public DamageInfo(float amount, Element source, AbilityVariant variant = AbilityVariant.Standard)
        {
            Amount = amount;
            Source = source;
            Variant = variant;
        }
    }

    /// <summary>Anything in the world that can take damage, status effects, and knockback.</summary>
    public interface IDamageable
    {
        void Apply(DamageInfo damage);
        void ApplyStatus(StatusEffect status);
        void ApplyKnockback(Vector3 impulse);
    }

    /// <summary>Plain hit-point container. No MonoBehaviour, so it unit-tests directly.</summary>
    public sealed class Health
    {
        public float Max { get; private set; }
        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;

        public event System.Action<DamageInfo> Damaged;
        public event System.Action Died;

        public Health(float max)
        {
            Max = max;
            Current = max;
        }

        public void Apply(DamageInfo damage)
        {
            if (IsDead) return;
            Current = Mathf.Max(0f, Current - Mathf.Max(0f, damage.Amount));
            Damaged?.Invoke(damage);
            if (IsDead) Died?.Invoke();
        }

        /// <summary>Heal by an amount, capped at Max.</summary>
        public void Heal(float amount) => Current = Mathf.Min(Max, Current + Mathf.Max(0f, amount));

        /// <summary>Restore to full (used on respawn).</summary>
        public void Revive() => Current = Max;

        /// <summary>Change the maximum <em>in place</em> — keeps this Health object (and every Damaged/Died
        /// subscriber) intact, unlike constructing a new one. Refills to full by default; pass refill:false to
        /// keep the current value (clamped to the new max).</summary>
        public void SetMax(float newMax, bool refill = true)
        {
            Max = Mathf.Max(1f, newMax);
            Current = refill ? Max : Mathf.Min(Current, Max);
        }
    }
}
