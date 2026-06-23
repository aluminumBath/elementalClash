using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Lets a plant user command the flora around them: when the player has the Verdancy specialty (or is a
    /// steam/healer, for lilies), the <see cref="InteractionArbiter"/> offers a "Tend plants" action that lashes
    /// nearby vines, puffs nearby spore caps, and harvests nearby lilies. Put on the player rig. Offered at low
    /// priority, so a nearby NPC or rideable wins the prompt when both are in reach.
    /// </summary>
    public sealed class PlantControlController : MonoBehaviour, IInteractable
    {
        [SerializeField] private float commandRange = 8f;

        private PlayerCombatController _combat;

        private void Awake()
        {
            _combat = GetComponentInParent<PlayerCombatController>();
            if (_combat == null) _combat = FindObjectOfType<PlayerCombatController>();
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            if (_combat == null) return false;
            var loadout = _combat.Loadout;
            bool plant = PlantControl.IsPlantUser(loadout);
            bool tend = PlantControl.CanTendLily(loadout); // plant users + steam/healers
            if (!plant && !tend) return false;
            interaction = new Interaction(0f, -1, "Tend plants", () => Command(plant, tend));
            return true;
        }

        private void Command(bool plant, bool tend)
        {
            Vector3 pos = transform.position;
            float r2 = commandRange * commandRange;

            if (plant)
            {
                foreach (var v in FindObjectsOfType<VinePatch>())
                    if ((v.transform.position - pos).sqrMagnitude <= r2) v.Snare();

                foreach (var s in FindObjectsOfType<MushroomCluster>())
                    if ((s.transform.position - pos).sqrMagnitude <= r2) s.Puff();
            }

            if (tend) // plant users and steam/healers alike can coax fruit from a Gleamlily
            {
                foreach (var l in FindObjectsOfType<GleamLily>())
                    if ((l.transform.position - pos).sqrMagnitude <= r2) l.Harvest();
            }
        }
    }
}
