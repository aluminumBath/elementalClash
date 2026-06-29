using UnityEngine;

namespace Elementborn.Game
{
    public sealed class PlayerTestRigSetup : MonoBehaviour
    {
        [SerializeField] private bool configureOnAwake = true;
        [SerializeField] private SpellCastDefinition[] starterSpells;

        private void Awake()
        {
            if (configureOnAwake)
            {
                Configure();
            }
        }

        [ContextMenu("Configure Player Test Rig")]
        public void Configure()
        {
            gameObject.tag = "Player";
            Ensure<CharacterController>();
            Ensure<SimpleCombatHealth>();
            Ensure<StatusEffectController>();
            Ensure<StaminaResource>();
            Ensure<CombatDefenseController>();
            Ensure<PlayerCombatInputController>();
            Ensure<SpellResourcePool>();
            Ensure<SpellCastController>();
            SpellLoadoutController loadout = Ensure<SpellLoadoutController>();
            Ensure<SpellCastingSaveBridge>();
            Ensure<CombatDefenseSaveBridge>();
            Ensure<PlayerInteractor>();

            if (starterSpells != null)
            {
                for (int i = 0; i < starterSpells.Length && i < 4; i++)
                {
                    if (starterSpells[i] != null)
                    {
                        loadout.Assign(i, starterSpells[i]);
                    }
                }
            }
        }

        private T Ensure<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
    }
}
