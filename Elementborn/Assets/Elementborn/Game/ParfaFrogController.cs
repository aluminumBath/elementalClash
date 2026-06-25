using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Parfa's two frogs argue air vs water forever. The trick to make them agree: show up embodying <em>both</em>
    /// — a player whose loadout holds Air and Water is common ground they can't squabble with. Pull it off and
    /// Parfa pays a <see cref="Currency.Diamond"/>, once. Put on the frogs object near Parfa. Offers its
    /// interaction through the <see cref="InteractionArbiter"/>.
    /// </summary>
    public sealed class ParfaFrogController : MonoBehaviour, IInteractable
    {
        [SerializeField] private float reach = 3.5f;

        private readonly FrogAccord _accord = new FrogAccord();
        private PlayerCombatController _combat;

        public bool Agreed => _accord.Agreed;

        private void Start()
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _combat = p.GetComponentInParent<PlayerCombatController>();
            BuildFrogs();
        }

        // The two frogs as side-by-side visuals turned toward each other, mid-squabble. Each gets a tinted
        // placeholder (visible until its Meshy model is imported) and a ProceduralAnimator for an idle bob — the
        // models are static, so the bob is what gives them life, and the pair de-sync by position so they bicker
        // out of step. ModelLibrary.Attach swaps in the real model and hides the placeholder once it exists.
        private void BuildFrogs()
        {
            if (transform.Find("Hurricane Frog") != null) return; // idempotent
            MakeFrog("Hurricane Frog", FrogModelNames.Hurricane, new Vector3(-0.55f, 0f, 0f),  22f, Element.Air);
            MakeFrog("Steam Frog",     FrogModelNames.Steam,     new Vector3( 0.55f, 0f, 0f), -22f, Element.Water);
        }

        private void MakeFrog(string label, string resourcePath, Vector3 offset, float yaw, Element tint)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = offset;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            placeholder.name = "Placeholder";
            placeholder.transform.SetParent(go.transform, false);
            placeholder.transform.localScale = new Vector3(0.5f, 0.36f, 0.5f);
            var col = placeholder.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var mr = placeholder.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(ElementColor.For(tint));

            ModelLibrary.Attach(resourcePath, go, "Frog");      // real model hides the placeholder when present
            if (go.GetComponent<ProceduralAnimator>() == null)
                go.AddComponent<ProceduralAnimator>();
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            if (_accord.Agreed) return false;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > reach) return false;
            interaction = new Interaction(d, 0, "Talk to the frogs", Attempt);
            return true;
        }

        private void Attempt()
        {
            var loadout = _combat != null ? _combat.Loadout : null;
            bool harmonizesBoth = loadout != null && loadout.HasElement(Element.Air) && loadout.HasElement(Element.Water);
            if (harmonizesBoth) Trick();
            else Debug.Log("[Frogs] *the two frogs go right on bickering about air and water*");
        }

        /// <summary>The trick worked — the frogs agree and Parfa awards a diamond (once).</summary>
        public void Trick()
        {
            if (!_accord.Reconcile()) return;
            PlayerInventory.Instance?.AddCurrency(Currency.Diamond, 1);
            _accord.MarkRewardGiven();
            Debug.Log("[Parfa] You got those two to agree?! Astonishing. Here — a diamond. You've earned it.");
        }
    }
}
