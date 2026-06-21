using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// One interact button that does the contextual thing nearby: mount/dismount a vehicle or creature,
    /// attempt to tame a weakened creature, or claim a house plot. Each frame it finds the nearest thing
    /// in reach and shows a prompt on the <see cref="GameHud"/> ("[E] Ride", "[E] Tame Horse", "Weaken it
    /// first", …); pressing the button acts on it and toasts the result. Put this on the player rig; reads
    /// the E key by default, or assign an action for a VR controller button.
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float reach = 3f;
        [SerializeField] private InputActionReference interactAction;

        private MountController _riding;

        private void Update()
        {
            bool pressed = InteractPressed();

            if (_riding != null)
            {
                GameHud.Instance?.SetPrompt("[E] Dismount");
                if (pressed)
                {
                    _riding.Dismount();
                    _riding = null;
                    GameHud.Instance?.SetPrompt("");
                }
                return;
            }

            float best = reach * reach;
            System.Action act = null;
            string prompt = null;
            Vector3 p = transform.position;

            foreach (var col in Physics.OverlapSphere(p, reach))
            {
                if (col.transform.root == transform.root) continue; // ignore self

                var mount = col.GetComponentInParent<MountController>();
                if (mount != null && !mount.IsRidden)
                {
                    float d = (mount.transform.position - p).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        var m = mount;
                        prompt = "[E] Ride";
                        act = () => { m.Mount(gameObject); _riding = m; GameHud.Instance?.SetPrompt(""); };
                    }
                }

                var tameable = col.GetComponentInParent<Tameable>();
                if (tameable != null)
                {
                    float d = (tameable.transform.position - p).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        var t = tameable;
                        prompt = t.CanTame(out string reason)
                            ? $"[E] Tame {CreatureCatalog.For(t.Kind).Name}"
                            : reason;
                        act = () => { var o = t.TryTame(); GameHud.Instance?.Toast(o.Reason); };
                    }
                }

                var plot = col.GetComponentInParent<HousePlot>();
                if (plot != null && !plot.Owned)
                {
                    float d = (plot.transform.position - p).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        var h = plot;
                        prompt = "[E] Claim home";
                        act = () => { bool ok = h.TryClaim(); GameHud.Instance?.Toast(ok ? "Home claimed" : "Couldn't claim it"); };
                    }
                }

                var merchant = col.GetComponentInParent<Merchant>();
                if (merchant != null)
                {
                    float d = (merchant.transform.position - p).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        var mc = merchant;
                        prompt = "[E] Shop";
                        act = () => mc.Open();
                    }
                }
            }

            GameHud.Instance?.SetPrompt(prompt ?? "");
            if (pressed && act != null) act();
        }

        private bool InteractPressed()
        {
            if (interactAction != null && interactAction.action != null && interactAction.action.enabled)
                return interactAction.action.WasPressedThisFrame();
            var k = Keyboard.current;
            return k != null && k.eKey.wasPressedThisFrame;
        }
    }
}
