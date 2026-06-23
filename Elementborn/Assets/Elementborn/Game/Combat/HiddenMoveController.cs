using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Applies the four hidden "signature" moves once the <see cref="VrGestureProvider"/> recognises their
    /// special gestures and <see cref="PlayerCombatController"/> routes the <c>Signature</c> intent here:
    /// <list type="bullet">
    /// <item>Water — a forward dash burst (the underwater wrist-spin).</item>
    /// <item>Earth — rock armor: a big damage shield plus a self-slow (crossed arms).</item>
    /// <item>Air — a tornado that damages and launches everyone nearby, the caster included (full-body spin).</item>
    /// <item>Fire — a cone of fire breath that burns (hand at the mouth + a button).</item>
    /// </list>
    /// Effects apply directly to <see cref="Damageable"/>s; numbers live in <see cref="HiddenMoves"/>. The
    /// self-launch is a best-effort upward burst — the full launch/fall-control lands with the mobility moves.
    /// </summary>
    public sealed class HiddenMoveController : MonoBehaviour
    {
        [SerializeField] private Transform head;

        private CharacterController _cc;
        private Damageable _self;
        private ComfortVignette _comfort;
        private Vector3 _burstVel;
        private float _burstTime;

        private static readonly Collider[] _hits = new Collider[32];
        private readonly HashSet<Damageable> _seen = new HashSet<Damageable>();

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _self = GetComponentInParent<Damageable>();
            if (head == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null) head = cam.transform;
            }
            _comfort = head != null ? head.GetComponentInChildren<ComfortVignette>() : null;
            if (_comfort == null) _comfort = GetComponentInChildren<ComfortVignette>();
        }

        private void Update()
        {
            if (_burstTime > 0f && _cc != null)
            {
                _cc.Move(_burstVel * Time.deltaTime);
                _burstTime -= Time.deltaTime;
            }
        }

        public void Perform(Element element, Vector3 facing, Vector3 origin)
        {
            switch (HiddenMoves.For(element))
            {
                case HiddenMove.WaterDash: WaterDash(facing); break;
                case HiddenMove.RockArmor: RockArmor(); break;
                case HiddenMove.AirTornado: AirTornado(origin); break;
                case HiddenMove.FireBreath: FireBreath(origin, facing); break;
            }
        }

        private void WaterDash(Vector3 facing)
        {
            Vector3 dir = facing.sqrMagnitude > 0.001f ? facing.normalized : transform.forward;
            _burstVel = dir * HiddenMoves.WaterDashSpeed;
            _burstTime = HiddenMoves.WaterDashDuration;
            if (_comfort != null) _comfort.SetIntensity(0.6f);
        }

        private void RockArmor()
        {
            _self?.Shield(HiddenMoves.RockArmorDuration, HiddenMoves.RockArmorReduction);
            _self?.ApplyStatus(new StatusEffect(StatusKind.Slow, HiddenMoves.RockArmorSlow, HiddenMoves.RockArmorDuration));
        }

        private void AirTornado(Vector3 origin)
        {
            int n = Physics.OverlapSphereNonAlloc(origin, HiddenMoves.TornadoRadius, _hits);
            _seen.Clear();
            for (int i = 0; i < n; i++)
            {
                var d = _hits[i].GetComponentInParent<Damageable>();
                if (d == null || d == _self || !_seen.Add(d)) continue;
                d.Apply(new DamageInfo(HiddenMoves.TornadoDamage, Element.Air));
                d.ApplyKnockback(Vector3.up * HiddenMoves.TornadoLaunch);
            }

            // Launch the caster too (best-effort upward burst).
            if (_cc != null) { _burstVel = Vector3.up * HiddenMoves.TornadoSelfLaunch; _burstTime = 0.25f; }
            if (_comfort != null) _comfort.SetIntensity(0.8f);
        }

        private void FireBreath(Vector3 origin, Vector3 facing)
        {
            Vector3 fwd = facing.sqrMagnitude > 0.001f ? facing.normalized : transform.forward;
            float cosCone = Mathf.Cos(HiddenMoves.FireBreathConeDegrees * Mathf.Deg2Rad);

            int n = Physics.OverlapSphereNonAlloc(origin, HiddenMoves.FireBreathRange, _hits);
            _seen.Clear();
            for (int i = 0; i < n; i++)
            {
                var d = _hits[i].GetComponentInParent<Damageable>();
                if (d == null || d == _self || !_seen.Add(d)) continue;
                Vector3 to = d.transform.position - origin;
                if (to.sqrMagnitude < 0.0001f || Vector3.Dot(fwd, to.normalized) < cosCone) continue; // outside cone
                d.Apply(new DamageInfo(HiddenMoves.FireBreathDamage, Element.Fire));
                d.ApplyStatus(new StatusEffect(StatusKind.Burn, HiddenMoves.FireBreathBurnDps, HiddenMoves.FireBreathBurnDuration));
            }
        }
    }
}
