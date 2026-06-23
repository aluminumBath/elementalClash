using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The world-object interactable: mount/dismount a vehicle or creature, tame a weakened creature, claim a
    /// house plot, or open a merchant — whichever is nearest in reach. It no longer reads input or sets the prompt
    /// itself; it offers its best world action to the <see cref="InteractionArbiter"/> (which it adds to the rig
    /// automatically), so it can't clash with NPCs, sidekicks, or plant control. Put this on the player rig.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour, IInteractable
    {
        [SerializeField] private float reach = 3f;

        private MountController _riding;

        private void Awake()
        {
            if (GetComponent<InteractionArbiter>() == null) gameObject.AddComponent<InteractionArbiter>();
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;

            if (_riding != null)
            {
                interaction = new Interaction(0f, 10, "Dismount", () => { _riding.Dismount(); _riding = null; });
                return true;
            }

            float best = reach * reach;
            System.Action act = null;
            string verb = null;
            Vector3 p = playerPosition;

            foreach (var col in Physics.OverlapSphere(p, reach))
            {
                if (col.transform.root == transform.root) continue; // ignore self

                var mount = col.GetComponentInParent<MountController>();
                if (mount != null && !mount.IsRidden)
                {
                    float d = (mount.transform.position - p).sqrMagnitude;
                    if (d < best) { best = d; var m = mount; verb = "Ride"; act = () => { m.Mount(gameObject); _riding = m; }; }
                }

                var tameable = col.GetComponentInParent<Tameable>();
                if (tameable != null)
                {
                    float d = (tameable.transform.position - p).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        var t = tameable;
                        verb = t.CanTame(out _) ? $"Tame {CreatureCatalog.For(t.Kind).Name}" : "Tame";
                        act = () => { var o = t.TryTame(); GameHud.Instance?.Toast(o.Reason); };
                    }
                }

                var plot = col.GetComponentInParent<HousePlot>();
                if (plot != null && !plot.Owned)
                {
                    float d = (plot.transform.position - p).sqrMagnitude;
                    if (d < best) { best = d; var h = plot; verb = "Claim home"; act = () => { bool ok = h.TryClaim(); GameHud.Instance?.Toast(ok ? "Home claimed" : "Couldn't claim it"); }; }
                }

                var merchant = col.GetComponentInParent<Merchant>();
                if (merchant != null)
                {
                    float d = (merchant.transform.position - p).sqrMagnitude;
                    if (d < best) { best = d; var mc = merchant; verb = "Shop"; act = () => mc.Open(); }
                }
            }

            if (act == null || verb == null) return false;
            interaction = new Interaction(Mathf.Sqrt(best), 5, verb, act);
            return true;
        }
    }
}
