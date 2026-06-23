using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Scene-side faction tag. Holds this character's <see cref="Faction"/> and optional element, keeps a
    /// short grudge against anyone who attacks it, and answers hostility queries via <see cref="FactionRules"/>.
    /// All live members register themselves so controllers can scan for the nearest hostile target.
    /// </summary>
    public sealed class FactionMember : MonoBehaviour
    {
        [SerializeField] private Faction faction = Faction.Bandit;
        [SerializeField] private bool hasElement = false;
        [SerializeField] private Element element = Element.Fire;
        [Tooltip("Seconds this member stays angry at someone who attacked it.")]
        [SerializeField] private float grudgeDuration = 12f;

        private static readonly List<FactionMember> _all = new List<FactionMember>();
        public static IReadOnlyList<FactionMember> All => _all;

        private readonly Dictionary<FactionMember, float> _provokedUntil = new Dictionary<FactionMember, float>();

        public Faction Faction => faction;
        public Element? Element => hasElement ? element : (Element?)null;

        /// <summary>Set faction and element at runtime (used when spawning element-typed enemies).</summary>
        public void Configure(Faction newFaction, Element? newElement)
        {
            faction = newFaction;
            SetElement(newElement);
        }

        public void SetElement(Element? e)
        {
            hasElement = e.HasValue;
            if (e.HasValue) element = e.Value;
        }

        private void OnEnable() { if (!_all.Contains(this)) _all.Add(this); }
        private void OnDisable() { _all.Remove(this); _provokedUntil.Clear(); }

        /// <summary>Remember that <paramref name="attacker"/> attacked us, for <c>grudgeDuration</c> seconds.</summary>
        public void Provoke(FactionMember attacker)
        {
            if (attacker == null || attacker == this) return;
            _provokedUntil[attacker] = Time.time + grudgeDuration;
        }

        public bool IsProvokedBy(FactionMember other)
            => other != null && _provokedUntil.TryGetValue(other, out float until) && Time.time <= until;

        public Disposition Toward(FactionMember other)
        {
            if (other == null || other == this) return Disposition.Neutral;
            return FactionRules.Resolve(faction, Element, other.faction, other.Element, IsProvokedBy(other));
        }

        public bool IsHostileTo(FactionMember other) => Toward(other) == Disposition.Hostile;

        /// <summary>Nearest member this one is hostile to within range (0 = unlimited); null if none.</summary>
        public FactionMember FindNearestHostile(float maxRange = 0f)
        {
            FactionMember best = null;
            float bestSqr = maxRange > 0f ? maxRange * maxRange : float.MaxValue;
            Vector3 p = transform.position;

            for (int i = 0; i < _all.Count; i++)
            {
                var m = _all[i];
                if (m == null || m == this || !m.isActiveAndEnabled) continue;
                if (!IsHostileTo(m)) continue;

                float d = (m.transform.position - p).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = m; }
            }
            return best;
        }

        /// <summary>Helper for damage sources: provoke the victim against the attacker.</summary>
        public static void RegisterHit(GameObject victim, GameObject attacker)
        {
            if (victim == null || attacker == null) return;
            var v = victim.GetComponentInParent<FactionMember>();
            var a = attacker.GetComponentInParent<FactionMember>();
            if (v != null && a != null) v.Provoke(a);
        }
    }
}
