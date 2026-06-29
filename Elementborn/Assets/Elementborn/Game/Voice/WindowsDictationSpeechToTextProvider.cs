using System;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

namespace Elementborn.Game
{
    /// <summary>
    /// Windows-only dictation provider using UnityEngine.Windows.Speech.DictationRecognizer.
    /// For cross-platform shipping, use this as a dev fallback and plug in a backend STT provider later.
    /// </summary>
    public sealed class WindowsDictationSpeechToTextProvider : MonoBehaviour, ISpeechToTextProvider
    {
        public bool IsListening { get; private set; }

        public event Action<string> PartialTranscript;
        public event Action<string> FinalTranscript;
        public event Action<string> Error;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private DictationRecognizer recognizer;
#endif

        private void Awake()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            recognizer = new DictationRecognizer();

            recognizer.DictationHypothesis += text =>
            {
                PartialTranscript?.Invoke(text);
            };

            recognizer.DictationResult += (text, confidence) =>
            {
                FinalTranscript?.Invoke(text);
            };

            recognizer.DictationError += (error, hresult) =>
            {
                Error?.Invoke($"{error} ({hresult})");
            };

            recognizer.DictationComplete += cause =>
            {
                IsListening = false;
            };
#else
            Error?.Invoke("Windows dictation is only available on Windows builds/editor.");
#endif
        }

        public void StartListening()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (recognizer == null)
            {
                return;
            }

            if (IsListening)
            {
                return;
            }

            recognizer.Start();
            IsListening = true;
#else
            Error?.Invoke("Windows dictation is only available on Windows builds/editor.");
#endif
        }

        public void StopListening()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (recognizer == null || !IsListening)
            {
                return;
            }

            recognizer.Stop();
            IsListening = false;
#endif
        }

        private void OnDestroy()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (recognizer != null)
            {
                if (IsListening)
                {
                    recognizer.Stop();
                }

                recognizer.Dispose();
                recognizer = null;
            }
#endif
        }
    }
}
