using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>One offered interaction: how far, how important, the prompt verb, and what to do.</summary>
    public readonly struct Interaction
    {
        public readonly float Distance;
        public readonly int Priority;
        public readonly string Verb;
        public readonly System.Action Act;

        public Interaction(float distance, int priority, string verb, System.Action act)
        {
            Distance = distance; Priority = priority; Verb = verb; Act = act;
        }

        public bool IsValid => Act != null && !string.IsNullOrEmpty(Verb);
        public static readonly Interaction None = default;
    }

    /// <summary>A thing the player can interact with. Given the player's position, it reports whether it currently
    /// offers an interaction (and its distance/verb/action). The <see cref="InteractionArbiter"/> does the input
    /// and prompt; implementers never poll Interact or touch the HUD themselves.</summary>
    public interface IInteractable
    {
        bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction);
    }

    /// <summary>
    /// The single owner of the Interact button and the interaction prompt. Each frame it asks every registered
    /// <see cref="IInteractable"/> for an offer, picks the best (highest priority, ties to nearest), shows one
    /// prompt, and routes one press — so overlapping interactables no longer multi-fire or fight over the HUD.
    /// Lives on the player rig (<see cref="PlayerInteractor"/> adds one automatically).
    /// </summary>
    public sealed class InteractionArbiter : MonoBehaviour
    {
        private static readonly List<IInteractable> Registered = new List<IInteractable>();

        public static void Register(IInteractable interactable)
        {
            if (interactable != null && !Registered.Contains(interactable)) Registered.Add(interactable);
        }

        public static void Unregister(IInteractable interactable) => Registered.Remove(interactable);

        // A non-keyboard interact source (e.g. a VR grip) can request that the current best interaction fire this
        // frame, routing through the same selection/prompt logic the desktop Interact key uses.
        private static bool _signal;
        public static void SignalInteract() => _signal = true;

        [SerializeField] private Transform player;

        private readonly List<Interaction> _offers = new List<Interaction>();

        private void Awake() { if (player == null) player = transform; }
        private void OnEnable() => InputBindings.Enable();

        private void Update()
        {
            Vector3 p = player != null ? player.position : transform.position;

            _offers.Clear();
            for (int i = 0; i < Registered.Count; i++)
            {
                var interactable = Registered[i];
                if (interactable == null) continue;
                if (interactable.TryGetInteraction(p, out var offer) && offer.IsValid) _offers.Add(offer);
            }

            var best = PickBest(_offers);
            bool fire = InputBindings.Interact.WasPressedThisFrame() || _signal;
            _signal = false; // consumed every frame, whether or not an interaction is offered
            if (best.IsValid)
            {
                GameHud.Instance?.SetPrompt(InputBindings.Interact, best.Verb);
                if (fire) best.Act();
            }
            else
            {
                GameHud.Instance?.SetPrompt("");
            }
        }

        /// <summary>Pure selection: highest priority wins; ties break to the nearest. Unit-tested.</summary>
        public static Interaction PickBest(IReadOnlyList<Interaction> offers)
        {
            Interaction best = Interaction.None;
            bool has = false;
            for (int i = 0; i < offers.Count; i++)
            {
                var o = offers[i];
                if (!o.IsValid) continue;
                if (!has || o.Priority > best.Priority ||
                    (o.Priority == best.Priority && o.Distance < best.Distance))
                {
                    best = o; has = true;
                }
            }
            return best;
        }
    }
}
