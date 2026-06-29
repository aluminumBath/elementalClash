using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Prototype input for combat defense. Replace with the new Input System later if desired.
    /// </summary>
    [RequireComponent(typeof(CombatDefenseController))]
    public sealed class PlayerCombatInputController : MonoBehaviour
    {
        [SerializeField] private CombatDefenseController defense;
        [SerializeField] private KeyCode blockKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode dodgeKey = KeyCode.LeftAlt;
        [SerializeField] private bool useWasdDodgeDirection = true;

        private void Awake()
        {
            if (defense == null)
            {
                defense = GetComponent<CombatDefenseController>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(blockKey))
            {
                defense.BeginBlock();
            }

            if (Input.GetKeyUp(blockKey))
            {
                defense.EndBlock();
            }

            if (Input.GetKeyDown(dodgeKey))
            {
                defense.TryDodge(GetDodgeDirection());
            }
        }

        private Vector3 GetDodgeDirection()
        {
            if (!useWasdDodgeDirection)
            {
                return transform.forward;
            }

            Vector3 direction = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) direction += transform.forward;
            if (Input.GetKey(KeyCode.S)) direction -= transform.forward;
            if (Input.GetKey(KeyCode.D)) direction += transform.right;
            if (Input.GetKey(KeyCode.A)) direction -= transform.right;

            return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        }
    }
}
