using UnityEngine;
using UnityEngine.Events;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A guide NPC in the world. Stand near and press Interact to talk; what they say depends on their role —
    /// Willow names where to find a creature and how to reach it, Kiana gives keeping advice, Parfa points you to
    /// items and people. Lines surface through the <see cref="Spoke"/> event (for a dialogue UI) and the log.
    /// Put on the NPC object and set its <see cref="id"/>. Offers its "Talk" interaction through the
    /// <see cref="InteractionArbiter"/>, which owns the prompt and the button.
    /// </summary>
    public sealed class GuideNpcController : MonoBehaviour, IInteractable
    {
        [SerializeField] private GuideNpcId id = GuideNpcId.Willow;
        [SerializeField] private float talkRange = 3.5f;

        [System.Serializable] public class SpeakEvent : UnityEvent<string> { }
        public SpeakEvent Spoke;

        private GuideNpcInfo _info;

        private static readonly CreatureKind[] Notable =
        {
            CreatureKind.Skytyrant, CreatureKind.Ridgewing, CreatureKind.Tidewarden, CreatureKind.Direstalker,
            CreatureKind.Goldkoi, CreatureKind.Gillcloak, CreatureKind.Skimfin, CreatureKind.Glidewisp,
            CreatureKind.Roc, CreatureKind.Thunderbird, CreatureKind.Phoenix
        };

        public GuideNpcInfo Info => _info;

        private void Start()
        {
            _info = NpcCatalog.For(id);
            ModelLibrary.Attach(NpcModelNames.ResourcePath(id), gameObject, "Npc");
        }

        private void OnEnable() { InputBindings.Enable(); InteractionArbiter.Register(this); }
        private void OnDisable() => InteractionArbiter.Unregister(this);

        public bool TryGetInteraction(Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            float d = Vector3.Distance(playerPosition, transform.position);
            if (d > talkRange) return false;
            interaction = new Interaction(d, 0, "Talk", Talk);
            return true;
        }

        private void Talk()
        {
            if (string.IsNullOrEmpty(_info.Name)) _info = NpcCatalog.For(id);
            QuestEvents.RaiseTalkedToNpc(id.ToString());
            string line = $"{_info.Greeting} {ServiceLine()}";
            Spoke?.Invoke(line);
            DialogueController.Instance?.Open(id.ToString(), _info.Name, line);
            Debug.Log($"[{_info.Name}] {line}");
        }

        private string ServiceLine()
        {
            if (_info.Role == NpcRole.CreatureFinder)
            {
                if (SidekickFeedingController.Instance != null && SidekickFeedingController.Instance.HintUnlocked)
                    return HiddenAbilityHint.Text; // earned by tending all her companions
                return CreatureHints.WhereToFind(Notable[Random.Range(0, Notable.Length)]);
            }
            return _info.ServiceLine; // Keeper / Locator: their standing advice
        }
    }
}
