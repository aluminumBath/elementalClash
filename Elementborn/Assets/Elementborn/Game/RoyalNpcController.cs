using UnityEngine;
using UnityEngine.Events;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A member of the royal houses in the world (King Ronald, Queen Renee, the Windwyrms, the Flowers, and their
    /// heirs). Stand near and press Interact to hear them speak; their identity and voice come from
    /// <see cref="RoyalCatalog"/>. Put this on the NPC object and set its <see cref="id"/>. Offers a "Speak"
    /// interaction through the <see cref="InteractionArbiter"/>, exactly like <see cref="GuideNpcController"/>.
    /// </summary>
    public sealed class RoyalNpcController : MonoBehaviour, IInteractable
    {
        [SerializeField] private Royal id = Royal.KingRonald;
        [SerializeField] private float talkRange = 3.5f;

        [System.Serializable] public class SpeakEvent : UnityEvent<string> { }
        public SpeakEvent Spoke;

        private RoyalInfo _info;

        public RoyalInfo Info => _info;

        private void Start()
        {
            _info = RoyalCatalog.For(id);
            ModelLibrary.Attach(RoyalModelNames.ResourcePath(id), gameObject, "Royal"); // element-hero stand-in; placeholder hides when found
        }

        /// <summary>Set which royal this is at runtime (used by the spawners), refreshing the cached profile.</summary>
        public void Configure(Royal newId) { id = newId; _info = RoyalCatalog.For(newId); }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > talkRange) return false;
            interaction = new Interaction(d, 0, "Speak", Talk);
            return true;
        }

        private void Talk()
        {
            if (string.IsNullOrEmpty(_info.Name)) _info = RoyalCatalog.For(id);
            QuestEvents.RaiseTalkedToNpc(id.ToString());
            string line = Loc.T(_info.Greeting);
            Spoke?.Invoke(line);
            DialogueController.Instance?.Open(id.ToString(), _info.Name, line);
            Debug.Log($"[{_info.Name}] {line}");
        }
    }
}
