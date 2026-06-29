using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Elementborn.Game
{
    public sealed class NpcConversationController : MonoBehaviour
    {
        [SerializeField] private NpcConversationProfile profile;
        [SerializeField] private MonoBehaviour responseProviderBehaviour;
        [SerializeField] private int maxHistoryMessages = 12;
        [SerializeField] private bool beginWithGreeting = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onConversationStarted;
        [SerializeField] private UnityEvent onConversationEnded;
        [SerializeField] private StringUnityEvent onNpcText;
        [SerializeField] private StringUnityEvent onPlayerText;

        private readonly List<NpcConversationMessage> history = new List<NpcConversationMessage>();
        private INpcResponseProvider responseProvider;
        private GameObject currentInteractor;

        public NpcConversationProfile Profile => profile;
        public IReadOnlyList<NpcConversationMessage> History => history;

        private void Awake()
        {
            responseProvider = responseProviderBehaviour as INpcResponseProvider;
            if (responseProvider == null)
            {
                responseProvider = GetComponent<INpcResponseProvider>();
            }

            if (responseProvider == null)
            {
                responseProvider = gameObject.AddComponent<RuleBasedNpcResponseProvider>();
            }
        }

        public void BeginConversation(GameObject interactor)
        {
            currentInteractor = interactor;
            onConversationStarted?.Invoke();

            if (beginWithGreeting && profile != null)
            {
                AddNpcMessage(profile.Greeting);
            }
        }

        public void EndConversation()
        {
            currentInteractor = null;
            onConversationEnded?.Invoke();
        }

        public void SubmitPlayerText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            AddPlayerMessage(text);

            var request = new NpcConversationRequest
            {
                Profile = profile,
                PlayerText = text,
                History = new List<NpcConversationMessage>(history)
            };

            responseProvider.GenerateResponse(request, response =>
            {
                string npcText = response != null && !string.IsNullOrWhiteSpace(response.Text)
                    ? response.Text
                    : profile != null ? profile.UnknownResponse : "…";

                AddNpcMessage(npcText);
            });
        }

        private void AddPlayerMessage(string text)
        {
            history.Add(new NpcConversationMessage("Player", text));
            TrimHistory();
            onPlayerText?.Invoke(text);
        }

        private void AddNpcMessage(string text)
        {
            string speaker = profile != null && !string.IsNullOrWhiteSpace(profile.NpcName)
                ? profile.NpcName
                : name;

            history.Add(new NpcConversationMessage(speaker, text));
            TrimHistory();
            onNpcText?.Invoke(text);
        }

        private void TrimHistory()
        {
            while (history.Count > maxHistoryMessages)
            {
                history.RemoveAt(0);
            }
        }
    }

    [System.Serializable]
    public sealed class StringUnityEvent : UnityEvent<string>
    {
    }
}
