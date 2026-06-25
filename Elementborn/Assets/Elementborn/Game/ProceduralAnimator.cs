using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Brings creatures, enemies, and bosses to life instead of sliding as static meshes: a breathing idle
    /// bob and sway on the visual, a crouch wind-up when an attack telegraphs, and a forward lunge when it lands.
    /// Touches only the visual child's local position and rotation — leaving HitReaction's squash (which owns the
    /// child's <c>localScale</c>) and the controller's facing (which owns the root's rotation) untouched, so the
    /// three compose cleanly. Self-attached by EnemyController and CreatureController, so every body gets it.</summary>
    public sealed class ProceduralAnimator : MonoBehaviour
    {
        [SerializeField] private float bobAmplitude = 0.06f;
        [SerializeField] private float bobSpeed = 3.2f;
        [SerializeField] private float swayDeg = 3f;

        private const float LungeDur = 0.35f;
        private const float ThrustReach = 0.25f;

        private Transform _visual;
        private Vector3 _baseLocalPos;
        private Quaternion _baseLocalRot;
        private float _phase;
        private float _lungeT = -1f;   // <0 = not lunging
        private float _lungeDir = 1f;  // +1 thrust forward (strike), -1 pull back (wind-up)
        private bool _captured;
        private EnemyController _enemy;

        private void Start()
        {
            _phase = (transform.position.x + transform.position.z) * 0.6f; // de-sync neighbours
            _enemy = GetComponent<EnemyController>();
            if (_enemy != null)
            {
                _enemy.AttackTelegraphed += OnWindup;
                _enemy.AttackLanded += OnLunge;
            }
        }

        private void OnDestroy()
        {
            if (_enemy != null)
            {
                _enemy.AttackTelegraphed -= OnWindup;
                _enemy.AttackLanded -= OnLunge;
            }
        }

        // Models are attached at runtime (placeholder hidden, model added as a sibling), so capture lazily and
        // pick whichever child is actually visible.
        private void Capture()
        {
            _visual = PickVisual();
            _baseLocalPos = _visual.localPosition;
            _baseLocalRot = _visual.localRotation;
            _captured = true;
        }

        private Transform PickVisual()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var r = transform.GetChild(i).GetComponentInChildren<Renderer>();
                if (r != null && r.enabled) return transform.GetChild(i);
            }
            return transform.childCount > 0 ? transform.GetChild(0) : transform;
        }

        private void OnWindup() { _lungeT = 0f; _lungeDir = -1f; }
        private void OnLunge() { _lungeT = 0f; _lungeDir = 1f; }

        private void Update()
        {
            if (!_captured || _visual == null) Capture();
            if (_visual == null || _visual == transform) return; // no separate visual to animate locally

            float t = Time.time + _phase;
            float bob = BodyAnimation.Bob(t, bobAmplitude, bobSpeed);
            float sway = BodyAnimation.Sway(t, swayDeg, bobSpeed);

            float thrust = 0f;
            if (_lungeT >= 0f)
            {
                _lungeT += Time.deltaTime / LungeDur;
                thrust = BodyAnimation.LungeCurve(_lungeT) * _lungeDir * ThrustReach;
                if (_lungeT >= 1f) _lungeT = -1f;
            }

            _visual.localPosition = _baseLocalPos + new Vector3(0f, bob, thrust);
            _visual.localRotation = _baseLocalRot * Quaternion.Euler(thrust * -40f, 0f, sway);
        }
    }
}
