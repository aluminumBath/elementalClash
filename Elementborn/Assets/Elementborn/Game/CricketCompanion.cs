using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Cricket — a tiny bat who wakes with the player, floats at their shoulder for the whole game, and
    /// delivers the first-run tutorial. She scales uniformly for whatever a scene needs — and can shrink to an
    /// earring tucked at the player's ear when hiding — always keeping her proportions (no distortion). She is a
    /// pure companion: no Damageable and no colliders, so she has no attacks and can never take damage. Spawned
    /// by <see cref="GameFlowController"/> on world entry; the tutorial panel only runs on a fresh start. Visual
    /// is code-built primitives; the lines come from the pure <see cref="TutorialScript"/>.</summary>
    public sealed class CricketCompanion : MonoBehaviour
    {
        public static CricketCompanion Instance { get; private set; }

        public enum CricketMode { Companion, Earring }

        // Where Cricket rides relative to the player, and how small the earring form is.
        private static readonly Vector3 CompanionOffset = new Vector3(0.85f, 1.6f, 0.55f); // right / up / forward
        private static readonly Vector3 EarringOffset = new Vector3(0.17f, 1.62f, 0.05f);  // close beside the head
        private const float EarringScale = 0.22f; // tiny; uniform, so her dimensions are preserved

        private Transform _player;
        private TutorialScript _script;
        private Canvas _panelCanvas;
        private Transform _panelContent;
        private float _bob;
        private CricketMode _mode = CricketMode.Companion;
        private float _baseScale = 1f; // scene-driven size; applied as a uniform scale so proportions never change

        /// <summary>Build Cricket and (optionally) start the first-run tutorial.</summary>
        public void Spawn(bool runTutorial)
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildBat();
            if (runTutorial) StartTutorial();
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>Scale Cricket uniformly for the scene — only her size changes, never her proportions.</summary>
        public void SetSize(float multiplier) { _baseScale = Mathf.Max(0.05f, multiplier); }

        /// <summary>Shrink to an earring at the player's ear (hiding), or return to the shoulder as a companion.</summary>
        public void SetEarringMode(bool on) { _mode = on ? CricketMode.Earring : CricketMode.Companion; }
        public void ToggleEarring() { SetEarringMode(_mode == CricketMode.Companion); }

        private void Update()
        {
            if (_player == null) _player = RigTeleporter.Rig;
            if (_player == null) return;

            bool earring = _mode == CricketMode.Earring;
            Vector3 offset = earring ? EarringOffset : CompanionOffset;
            float scale = _baseScale * (earring ? EarringScale : 1f);
            float bobAmp = earring ? 0.015f : 0.12f;

            _bob += Time.deltaTime * 2.6f;
            Vector3 target = _player.position
                + _player.right * offset.x
                + Vector3.up * (offset.y + Mathf.Sin(_bob) * bobAmp)
                + _player.forward * offset.z;

            float follow = 1f - Mathf.Exp(-(earring ? 14f : 6f) * Time.deltaTime); // snappier when perched
            transform.position = Vector3.Lerp(transform.position, target, follow);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scale, 1f - Mathf.Exp(-8f * Time.deltaTime));

            Vector3 look = (_player.position + Vector3.up * 1.4f) - transform.position;
            if (look.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), earring ? 0.25f : 0.12f);
        }

        // ---- tutorial ----------------------------------------------------------------------

        private void StartTutorial()
        {
            _script = TutorialScript.Default();
            ShowStep();
        }

        private void ShowStep()
        {
            if (_script == null || !_script.TryGetCurrent(out var step)) { CloseTutorial(); return; }

            if (_panelCanvas == null)
            {
                var p = OverlayUi.Panel("CricketTutorial", "Cricket", 230, new Vector2(560, 320), Skip);
                _panelCanvas = p.canvas;
                _panelContent = p.content;
                _panelCanvas.transform.SetParent(transform, false);
            }

            OverlayUi.Clear(_panelContent);
            OverlayUi.Body(_panelContent, step.Text, 20);
            OverlayUi.Body(_panelContent, " ", 6);
            UiTheme.Button(_panelContent, _script.IsLast ? "Got it!" : "Next \u25B6", Next, 240, 52);
            if (!_script.IsLast) UiTheme.Button(_panelContent, "Skip tutorial", Skip, 240, 44);
            AudioController.Instance?.Confirm();
        }

        private void Next()
        {
            if (_script == null) { CloseTutorial(); return; }
            _script.Advance();
            if (_script.IsComplete) CloseTutorial();
            else ShowStep();
        }

        private void Skip()
        {
            _script?.Skip();
            CloseTutorial();
        }

        private void CloseTutorial()
        {
            if (_panelCanvas != null) { Destroy(_panelCanvas.gameObject); _panelCanvas = null; _panelContent = null; }
            GameHud.Instance?.Toast("Cricket flutters to your side.");
        }

        // ---- visual ------------------------------------------------------------------------

        private void BuildBat()
        {
            gameObject.name = "Cricket";
            var dark = new Color(0.16f, 0.13f, 0.22f);
            var eye = new Color(0.78f, 0.95f, 1f);

            Part(PrimitiveType.Sphere, new Vector3(0f, 0f, 0f), new Vector3(0.26f, 0.20f, 0.30f), dark, Quaternion.identity);
            Part(PrimitiveType.Quad, new Vector3(-0.22f, 0.02f, 0f), new Vector3(0.34f, 0.24f, 1f), dark, Quaternion.Euler(0f, 35f, 18f));
            Part(PrimitiveType.Quad, new Vector3(0.22f, 0.02f, 0f), new Vector3(0.34f, 0.24f, 1f), dark, Quaternion.Euler(0f, -35f, -18f));
            Part(PrimitiveType.Sphere, new Vector3(-0.05f, 0.04f, 0.14f), Vector3.one * 0.05f, eye, Quaternion.identity);
            Part(PrimitiveType.Sphere, new Vector3(0.05f, 0.04f, 0.14f), Vector3.one * 0.05f, eye, Quaternion.identity);
        }

        private void Part(PrimitiveType type, Vector3 lp, Vector3 ls, Color c, Quaternion lr)
        {
            var go = GameObject.CreatePrimitive(type);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = lp;
            go.transform.localScale = ls;
            go.transform.localRotation = lr;
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = ToonPalette.Tinted(c);
        }
    }
}
