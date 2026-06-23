namespace Elementborn.Core
{
    /// <summary>
    /// A character's path in the evolution mode: start with one element, optionally take a second, and the pair
    /// yields a specialty (the <see cref="Specialties"/> table). <see cref="ToLoadout"/> turns the current state
    /// into a <see cref="ChannelerLoadout"/> — both elements plus the specialty sub-art — so it plugs straight
    /// into combat and the specialty gates (plant, steam, etc.). Pure, so it unit-tests directly.
    /// </summary>
    public sealed class ElementalEvolution
    {
        public Element Primary { get; private set; }
        public Element? Secondary { get; private set; }

        public bool HasSecondary => Secondary.HasValue;
        public SubArt Specialty => Secondary.HasValue ? Specialties.For(Primary, Secondary.Value) : SubArt.None;
        public bool HasSpecialty => Specialty != SubArt.None;

        public ElementalEvolution(Element primary) => Primary = primary;

        /// <summary>Take a second element (must differ from the first, and only once). Returns false if invalid.</summary>
        public bool Evolve(Element second)
        {
            if (Secondary.HasValue || second == Primary) return false;
            Secondary = second;
            return true;
        }

        public ChannelerLoadout ToLoadout()
        {
            var elements = Secondary.HasValue ? new[] { Primary, Secondary.Value } : new[] { Primary };
            var subArts = HasSpecialty ? new[] { Specialty } : System.Array.Empty<SubArt>();
            return ChannelerLoadout.FromState(elements, subArts, WeaponType.None);
        }
    }
}
