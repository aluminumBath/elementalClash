using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Owns the player's visible model. Prefers a <b>rigged humanoid</b> (a skinned-mesh prefab that carries its
    /// own <see cref="Animator"/>) and drives its locomotion from the rig's actual movement; if no rigged model
    /// is present it falls back to the static mesh, exactly as before. Put this on the body the model should live
    /// on — <see cref="ThirdPersonRig"/> adds it automatically. The rigged prefab's Animator Controller just needs
    /// a float <c>Speed</c> param (0 idle → 0.5 walk → 1 run) driving a 1D blend tree.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerModelBinder : MonoBehaviour
    {
        [SerializeField] private float walkSpeed = 1.5f;
        [SerializeField] private float runSpeed = 5.5f;
        [SerializeField] private float blendDamp = 12f;
        [SerializeField] private string speedParam = "Speed";

        private Animator _animator;
        private int _speedId;
        private Vector3 _lastPos;
        private float _blend;

        public bool HasRiggedModel => _animator != null;

        private void Start()
        {
            _speedId = Animator.StringToHash(speedParam);
            _lastPos = transform.position;

            // Rigged first — it brings its own Animator. If absent, attach the static mesh (no animation).
            var rigged = ModelLibrary.Attach(PlayerModelNames.RiggedPath(), gameObject, "PlayerRig");
            if (rigged != null)
            {
                _animator = rigged.GetComponentInChildren<Animator>();
                // Give authored animation events (Footstep, AttackImpact, …) a target on the Animator's object.
                if (_animator != null && _animator.GetComponent<AnimationEventReceiver>() == null)
                    _animator.gameObject.AddComponent<AnimationEventReceiver>();
            }
            else
            {
                ModelLibrary.Attach(PlayerModelNames.ResourcePath(), gameObject, "Player");
                // Static mesh has no clips → drive footsteps procedurally so they still happen.
                if (GetComponent<ProceduralFootsteps>() == null) gameObject.AddComponent<ProceduralFootsteps>();
            }
        }

        private void Update()
        {
            if (_animator == null) return; // static fallback: nothing to drive

            Vector3 pos = transform.position;
            Vector3 delta = pos - _lastPos; delta.y = 0f;
            _lastPos = pos;
            float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;

            float target = LocomotionAnimation.Blend(speed, walkSpeed, runSpeed);
            _blend = LocomotionAnimation.Damp(_blend, target, blendDamp, Time.deltaTime);
            _animator.SetFloat(_speedId, _blend);
        }
    }
}
