using System;

namespace Elementborn.Game
{
    public interface ISpeechToTextProvider
    {
        bool IsListening { get; }
        event Action<string> PartialTranscript;
        event Action<string> FinalTranscript;
        event Action<string> Error;

        void StartListening();
        void StopListening();
    }
}
