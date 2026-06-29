using System.Reflection;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Connects generated NPC roster data to an NpcConversationController when one is present.
    /// Uses reflection only for the private profile field so older generated controllers do not need to be rewritten.
    /// </summary>
    public sealed class NpcConversationProfileBinder : MonoBehaviour
    {
        [SerializeField] private NpcWorldEntryDefinition npc;
        [SerializeField] private NpcConversationProfile profile;
        [SerializeField] private bool bindOnAwake = true;

        private void Awake()
        {
            if (bindOnAwake)
            {
                Bind();
            }
        }

        [ContextMenu("Bind Conversation Profile")]
        public void Bind()
        {
            var controller = GetComponent<NpcConversationController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<NpcConversationController>();
            }

            if (profile != null)
            {
                FieldInfo field = typeof(NpcConversationController).GetField("profile", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(controller, profile);
                }
            }

            if (npc != null)
            {
                gameObject.name = npc.DisplayName;
            }
        }

        public void Configure(NpcWorldEntryDefinition entry, NpcConversationProfile conversationProfile)
        {
            npc = entry;
            profile = conversationProfile;
            Bind();
        }
    }
}
