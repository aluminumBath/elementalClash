using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A physical entrance to a hidden landmark. On interact it reads the player's real approach — altitude above
    /// sea level, depth below the surface, whether they can fly (Flight sub-art), whether they can breathe under
    /// water (a Water Channeler or an active Tideglass boon), and whether they carry a recognition relic — into a
    /// <see cref="LandmarkApproach"/>, asks <see cref="LandmarkAccessGate"/> whether the way opens, records the
    /// discovery in the Grimoire, and toasts the gate's reason.
    ///
    /// Discovery is progressive: reaching the threshold at all reveals the rumour (Glimpsed); a successful entry
    /// reveals what the place is (Known) and — until interiors exist — marks it explored (Mastered). Drop this on a
    /// GameObject with a (trigger) collider, or let <see cref="LandmarkPortalPlacer"/> spawn the four.
    /// </summary>
    public sealed class LandmarkPortal : BaseInteractable
    {
        [Header("Landmark")]
        [SerializeField] private Landmark landmark = Landmark.ThalenVeyr;
        [Tooltip("World Y treated as sea level, for the altitude/depth checks (used by Thalen'Veyr).")]
        [SerializeField] private float seaLevelY = 0f;
        [Tooltip("Until interiors exist, a successful entry also marks the location fully explored (Mastered).")]
        [SerializeField] private bool markExploredOnEntry = true;

        public Landmark Landmark => landmark;

        /// <summary>Used by the placer to set which landmark this portal leads to.</summary>
        public void Configure(Landmark value, float seaLevel)
        {
            landmark = value;
            seaLevelY = seaLevel;
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(LandmarkCatalog.For(landmark).DisplayName, "Enter");
        }

        public override void Interact(GameObject interactor)
        {
            if (interactor == null || !CanInteract(interactor)) return;

            LandmarkApproach approach = BuildApproach(interactor);
            AccessResult result = LandmarkAccessGate.Evaluate(landmark, approach);

            // Standing at the threshold at all means the player now knows the place exists.
            GrimoireController.Instance?.NoteLocationHeard(landmark);

            if (!string.IsNullOrEmpty(result.Reason))
                GameHud.Instance?.Toast(result.Reason);

            if (result.Allowed)
            {
                GrimoireController.Instance?.NoteLocationFound(landmark);
                if (markExploredOnEntry) GrimoireController.Instance?.NoteLocationExplored(landmark);
                OnEntered(interactor, LandmarkCatalog.For(landmark));
            }
        }

        /// <summary>Hook for the interior-load slice. For now the gate, discovery and toast are the whole interaction.</summary>
        private void OnEntered(GameObject interactor, LandmarkInfo info) { }

        private LandmarkApproach BuildApproach(GameObject interactor)
        {
            Vector3 pos = interactor.transform.position;
            float y = pos.y;

            float altitude = Mathf.Max(0f, y - seaLevelY);

            float depth = 0f;
            var volume = WaterVolume.Submerged(pos);
            if (volume != null) depth = Mathf.Max(0f, volume.Surface - y);
            else if (y < seaLevelY) depth = seaLevelY - y;

            var loadout = PlayerInventory.Instance != null ? PlayerInventory.Instance.Loadout : null;
            bool canFly = loadout != null && loadout.HasSubArt(SubArt.Flight);

            var underwater = interactor.GetComponentInParent<UnderwaterController>();
            bool canBreathe = (loadout != null && loadout.HasElement(Element.Water))
                              || (underwater != null && underwater.HasWaterBreathingBoon);

            bool hasToken = PlayerInventoryTracker.HasItemId("stormwardens_token", 1)
                            || PlayerInventoryTracker.HasItemId("keelwood_splinter", 1);

            return new LandmarkApproach(altitude, depth, canFly, canBreathe, true, hasToken);
        }
    }
}
