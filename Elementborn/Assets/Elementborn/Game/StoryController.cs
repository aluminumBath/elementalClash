using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The live cursor through the campaign: the current <see cref="StoryChapter"/> and the chosen
    /// <see cref="StoryEnding"/>, persisted with the save. The chapter text and ending hooks live as data in
    /// <see cref="StoryArc"/>; this drives progress through them. Scenes and scripted beats call
    /// <see cref="Advance"/> or <see cref="SetChapter"/> as the story unfolds (e.g. the tower-blast event sets
    /// <see cref="StoryChapter.TheTowerBlast"/>), and <see cref="ChooseEnding"/> records the Reckoning choice.</summary>
    public sealed class StoryController : MonoBehaviour
    {
        public static StoryController Instance { get; private set; }

        public StoryChapter Chapter { get; private set; } = StoryChapter.Arrival;
        public StoryEnding ChosenEnding { get; private set; } = StoryEnding.None;

        /// <summary>Raised whenever the chapter changes — a story log or codex can refresh on this.</summary>
        public static event System.Action<StoryChapter> ChapterChanged;
        /// <summary>Raised when an ending is chosen at the Reckoning.</summary>
        public static event System.Action<StoryEnding> EndingChosen;

        public StoryBeat CurrentBeat => StoryArc.BeatFor(Chapter);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Jump to a specific chapter (scripted beats / story events).</summary>
        public void SetChapter(StoryChapter chapter)
        {
            if (Chapter == chapter) return;
            Chapter = chapter;
            ChapterChanged?.Invoke(Chapter);
        }

        /// <summary>Advance one chapter along the spine (no-op at the final chapter).</summary>
        public void Advance()
        {
            var next = StoryArc.Next(Chapter);
            if (next == Chapter) return;
            Chapter = next;
            ChapterChanged?.Invoke(Chapter);
        }

        /// <summary>Record the ending chosen at the Reckoning.</summary>
        public void ChooseEnding(StoryEnding ending)
        {
            if (ChosenEnding == ending) return;
            ChosenEnding = ending;
            EndingChosen?.Invoke(ending);
        }

        public void CaptureInto(SaveData d)
        {
            if (d == null) return;
            d.storyChapter = (int)Chapter;
            d.storyEnding = (int)ChosenEnding;
        }

        public void RestoreFrom(SaveData d)
        {
            if (d == null) return;
            Chapter = Clamp<StoryChapter>(d.storyChapter);
            ChosenEnding = Clamp<StoryEnding>(d.storyEnding);
            ChapterChanged?.Invoke(Chapter);
        }

        // Guard against an out-of-range saved index if the enums ever change.
        private static T Clamp<T>(int v) where T : struct, System.Enum
        {
            int last = System.Enum.GetValues(typeof(T)).Length - 1;
            if (v < 0) v = 0; else if (v > last) v = last;
            return (T)System.Enum.ToObject(typeof(T), v);
        }
    }
}
