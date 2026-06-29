using UnityEngine;

namespace Elementborn.Game
{
    public sealed class KellyMoodFlameController : MonoBehaviour
    {
        [SerializeField] private KellyMoodState mood = KellyMoodState.Calm;
        [SerializeField] private string currentFlameColor = "steady gold";

        public KellyMoodState Mood => mood;
        public string CurrentFlameColor => currentFlameColor;

        public void SetMood(KellyMoodState value)
        {
            mood = value;
            currentFlameColor = ColorForMood(value);
            NotificationFeed.Post($"Kelly's hair burns {currentFlameColor}. Mood: {mood}.", NotificationType.Info);
        }

        public void SetMoodByName(string value)
        {
            if (System.Enum.TryParse(value, true, out KellyMoodState parsed))
            {
                SetMood(parsed);
            }
        }

        public void ProtectNeighborhood()
        {
            SetMood(KellyMoodState.Protective);
            CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, -5, "Kelly protects her friends and neighbors.");
            NotificationFeed.Post("Kelly steps in with a mischievous grin and a wall of lava-bright protection.", NotificationType.Quest);
        }

        private string ColorForMood(KellyMoodState value)
        {
            switch (value)
            {
                case KellyMoodState.Happy: return "warm sunrise yellow";
                case KellyMoodState.Protective: return "deep loyal blue-orange";
                case KellyMoodState.Angry: return "violent crimson";
                case KellyMoodState.Mischievous: return "sparkling violet";
                case KellyMoodState.Worried: return "ashen gray-blue";
                default: return "steady gold";
            }
        }
    }
}
