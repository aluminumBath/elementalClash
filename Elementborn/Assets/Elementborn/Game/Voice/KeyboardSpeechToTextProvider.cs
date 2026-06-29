using System;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Debug provider for testing the speech pipeline without a microphone.
    /// Call SubmitDebugText from UI.
    /// </summary>
    public sealed class KeyboardSpeechToTextProvider : MonoBehaviour, ISpeechToTextProvider
    {
        public bool IsListening { get; private set; }

        public event Action<string> PartialTranscript;
        public event Action<string> FinalTranscript;
        public event Action<string> Error;

        public void StartListening()
        {
            IsListening = true;
        }

        public void StopListening()
        {
            IsListening = false;
        }

        public void SubmitDebugText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            PartialTranscript?.Invoke(text);
            FinalTranscript?.Invoke(text);
        }

        public void RaiseError(string message)
        {
            Error?.Invoke(message);
        }
    }
}
