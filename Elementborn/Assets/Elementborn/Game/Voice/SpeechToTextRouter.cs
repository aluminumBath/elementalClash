using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Connects a speech-to-text provider to an NPC conversation controller.
    /// Also works with typed input by directly calling SubmitText.
    /// </summary>
    public sealed class SpeechToTextRouter : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour speechProviderBehaviour;
        [SerializeField] private NpcConversationController conversationController;
        [SerializeField] private bool submitFinalTranscripts = true;

        private ISpeechToTextProvider speechProvider;

        private void Awake()
        {
            speechProvider = speechProviderBehaviour as ISpeechToTextProvider;
            if (speechProvider == null)
            {
                speechProvider = GetComponent<ISpeechToTextProvider>();
            }

            if (speechProvider != null)
            {
                speechProvider.FinalTranscript += HandleFinalTranscript;
            }
        }

        private void OnDestroy()
        {
            if (speechProvider != null)
            {
                speechProvider.FinalTranscript -= HandleFinalTranscript;
            }
        }

        public void SetConversation(NpcConversationController conversation)
        {
            conversationController = conversation;
        }

        public void StartListening()
        {
            speechProvider?.StartListening();
        }

        public void StopListening()
        {
            speechProvider?.StopListening();
        }

        public void SubmitText(string text)
        {
            conversationController?.SubmitPlayerText(text);
        }

        private void HandleFinalTranscript(string text)
        {
            if (!submitFinalTranscripts)
            {
                return;
            }

            SubmitText(text);
        }
    }
}
