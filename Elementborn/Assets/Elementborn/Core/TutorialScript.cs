using System.Collections.Generic;

namespace Elementborn.Core
{
    /// <summary>One line of Cricket's first-run guidance.</summary>
    public struct TutorialStep
    {
        public string Id;
        public string Text;
        public TutorialStep(string id, string text) { Id = id; Text = text; }
    }

    /// <summary>The first-run tutorial: Cricket's ordered lines walking a new Channeler through the basics,
    /// plus a simple cursor (advance / skip / restart). Engine-free so the sequence is unit-testable; the
    /// Game-layer <c>CricketCompanion</c> renders each line and advances on the player's input.</summary>
    public sealed class TutorialScript
    {
        private readonly List<TutorialStep> _steps;
        private int _index;

        public TutorialScript(List<TutorialStep> steps) { _steps = steps ?? new List<TutorialStep>(); }

        public static TutorialScript Default()
        {
            return new TutorialScript(new List<TutorialStep>
            {
                new TutorialStep("wake",    "Rise and shine, Channeler! I'm Cricket — your guide. Stick close and I'll show you the ropes."),
                new TutorialStep("look",    "Take a look around. This whole realm runs on the four elements you'll learn to command."),
                new TutorialStep("move",    "Let's get moving — walk with the thumbstick (or WASD on a keyboard). Go on, try it!"),
                new TutorialStep("channel", "Now reach for your element and channel it. A flick of the wrist and it answers your call."),
                new TutorialStep("attack",  "Strike a foe with your channel. Remember: each element bests another in a cycle — use it."),
                new TutorialStep("jump",    "Need height? Jump — and hold it to glide. The heights of this world are yours to reach."),
                new TutorialStep("explore", "That's the basics! Seek rifts to travel, beacons to summon, and the Convergence itself. I'll be right beside you."),
            });
        }

        public int Count => _steps.Count;
        public int Index => _index;
        public bool IsComplete => _index >= _steps.Count;

        public bool TryGetCurrent(out TutorialStep step)
        {
            if (_index < 0 || _index >= _steps.Count) { step = default; return false; }
            step = _steps[_index];
            return true;
        }

        public bool IsLast => _index == _steps.Count - 1;

        public void Advance() { if (_index < _steps.Count) _index++; }
        public void Skip() { _index = _steps.Count; }
        public void Restart() { _index = 0; }
    }
}
