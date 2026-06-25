using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Makes the main-quest spine legible: announces the current story beat once the player is in the
    /// world, and again whenever the chapter changes — so there's always a sense of where the story stands and
    /// what it's about. Reads the beats from <see cref="StoryArc"/> via <see cref="StoryController"/>. Spawned by
    /// the bootstrap alongside the StoryController.</summary>
    public sealed class StoryDirector : MonoBehaviour
    {
        private bool _announced;

        private void OnEnable() { StoryController.ChapterChanged += OnChapterChanged; }
        private void OnDisable() { StoryController.ChapterChanged -= OnChapterChanged; }

        private void Update()
        {
            // Announce the opening beat once, after the HUD and story state exist.
            if (_announced) return;
            if (GameHud.Instance == null || StoryController.Instance == null) return;
            _announced = true;
            Announce(StoryController.Instance.CurrentBeat);
        }

        private void OnChapterChanged(StoryChapter chapter) => Announce(StoryArc.BeatFor(chapter));

        private void Announce(StoryBeat beat)
        {
            GameHud.Instance?.Toast(beat.Title + "\n" + beat.Summary);
        }
    }
}
