using UnityEngine;
using UnityEngine.UI;

namespace Elementborn.Game
{
    /// <summary>
    /// Minimal typed-dialogue panel for testing NPC conversations without voice.
    /// </summary>
    public sealed class NpcConversationTextPanel : MonoBehaviour
    {
        [SerializeField] private NpcConversationController targetConversation;
        [SerializeField] private InputField inputField;
        [SerializeField] private Text transcriptText;
        [SerializeField] private Button sendButton;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(Send);
            }

            Hide();
        }

        public void Bind(NpcConversationController conversation)
        {
            targetConversation = conversation;
            Show();
        }

        public void Send()
        {
            if (targetConversation == null || inputField == null)
            {
                return;
            }

            string text = inputField.text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Append("You", text);
            targetConversation.SubmitPlayerText(text);
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        public void AppendNpc(string text)
        {
            string speaker = targetConversation != null && targetConversation.Profile != null
                ? targetConversation.Profile.NpcName
                : "NPC";

            Append(speaker, text);
        }

        public void Append(string speaker, string text)
        {
            if (transcriptText == null)
            {
                return;
            }

            transcriptText.text += $"{speaker}: {text}\n";
        }

        public void Show()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
