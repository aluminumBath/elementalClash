using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Prototype input bridge for ability loadout slots.
    /// Assign ability definitions in the inspector while testing; the tracker stores unlock/equip state by id.
    /// </summary>
    public sealed class AbilityInputController : MonoBehaviour
    {
        [SerializeField] private AbilityActivator activator;
        [SerializeField] private List<AbilityDefinition> knownAbilityDefinitions = new List<AbilityDefinition>();

        [Header("Keys")]
        [SerializeField] private KeyCode primaryKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode secondaryKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode utilityKey = KeyCode.R;
        [SerializeField] private KeyCode traversalKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode ultimateKey = KeyCode.F;

        private void Awake()
        {
            if (activator == null)
            {
                activator = GetComponent<AbilityActivator>();
            }

            if (activator == null)
            {
                activator = gameObject.AddComponent<AbilityActivator>();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(primaryKey)) TryActivateSlot(AbilitySlotType.Primary);
            if (Input.GetKeyDown(secondaryKey)) TryActivateSlot(AbilitySlotType.Secondary);
            if (Input.GetKeyDown(utilityKey)) TryActivateSlot(AbilitySlotType.Utility);
            if (Input.GetKeyDown(traversalKey)) TryActivateSlot(AbilitySlotType.Traversal);
            if (Input.GetKeyDown(ultimateKey)) TryActivateSlot(AbilitySlotType.Ultimate);
        }

        public bool TryActivateSlot(AbilitySlotType slot)
        {
            string abilityId = PlayerAbilityTracker.GetEquipped(slot);
            if (string.IsNullOrWhiteSpace(abilityId))
            {
                return false;
            }

            AbilityDefinition definition = FindDefinition(abilityId);
            if (definition == null)
            {
                NotificationFeed.Post($"No AbilityDefinition assigned for {abilityId}.", NotificationType.Warning);
                return false;
            }

            var result = activator.Activate(definition);
            if (!result.Success)
            {
                NotificationFeed.Post(result.Message, NotificationType.Warning);
            }

            return result.Success;
        }

        private AbilityDefinition FindDefinition(string abilityId)
        {
            foreach (var ability in knownAbilityDefinitions)
            {
                if (ability != null && ability.AbilityId == abilityId)
                {
                    return ability;
                }
            }

            return null;
        }
    }
}
