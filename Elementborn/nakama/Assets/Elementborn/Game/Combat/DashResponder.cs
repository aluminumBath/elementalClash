using System.Collections;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Applies the comfortable air dash chosen for VR. A standard dash is a quick slide; the
    /// Flight sub-art produces a longer, faster glide. Moves a CharacterController if present,
    /// else the rig root. Add a comfort vignette during the dash for VR.
    /// </summary>
    public sealed class DashResponder : MonoBehaviour
    {
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private Transform rigRoot;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float glideDuration = 0.5f;
        [Tooltip("Optional: raised during a dash for VR comfort. Auto-found if left empty.")]
        [SerializeField] private ComfortVignette comfortVignette;
        [SerializeField, Range(0f, 1f)] private float dashVignette = 0.6f;
        [SerializeField] private GameObject dashEffectPrefab;

        private CharacterController _cc;
        private Coroutine _dash;

        private void Reset() => combat = GetComponent<PlayerCombatController>();

        private void Awake()
        {
            if (rigRoot == null) rigRoot = transform;
            _cc = rigRoot.GetComponent<CharacterController>();
            if (comfortVignette == null) comfortVignette = FindObjectOfType<ComfortVignette>();
        }

        private void OnEnable()
        {
            if (combat == null) combat = GetComponent<PlayerCombatController>();
            if (combat != null) combat.OutcomeReady += HandleOutcome;
        }

        private void OnDisable()
        {
            if (combat != null) combat.OutcomeReady -= HandleOutcome;
        }

        private void HandleOutcome(AbilityOutcome outcome, Vector3 origin)
        {
            if (outcome.Kind != OutcomeKind.Movement) return;
            float duration = outcome.Variant == AbilityVariant.Flight ? glideDuration : dashDuration;
            if (_dash != null) StopCoroutine(_dash);
            _dash = StartCoroutine(Dash(outcome.Direction, outcome.Speed, duration));
        }

        private IEnumerator Dash(Vector3 direction, float speed, float duration)
        {
            Vector3 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : rigRoot.forward;
            dir.y = 0f;

            if (comfortVignette != null) comfortVignette.SetIntensity(dashVignette); // VR comfort during the rush
            GameObject fx = dashEffectPrefab != null
                ? Instantiate(dashEffectPrefab, rigRoot.position, rigRoot.rotation, rigRoot)
                : null;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                Vector3 step = dir * (speed * Time.deltaTime);
                if (_cc != null) _cc.Move(step);
                else rigRoot.position += step;
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (comfortVignette != null) comfortVignette.SetIntensity(0f);
            if (fx != null) Destroy(fx);
            _dash = null;
        }
    }
}
