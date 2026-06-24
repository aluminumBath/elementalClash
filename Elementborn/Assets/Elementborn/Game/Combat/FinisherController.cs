using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Turns a staggered enemy into a finisher: when one is within range of the camera it shows a prompt; pressing
    /// the finisher input (keyboard F / gamepad north) starts a <see cref="QuickTimeController"/> quick-time move,
    /// and on success executes that enemy (which fires the defeat poof + score). Self-bootstrapping.
    ///
    /// NOTE: the VR finisher gesture is part of the VR-moves pass — there it'll call
    /// <see cref="QuickTimeController.SubmitAction"/> and start the QTE off a recognized motion instead of F.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FinisherController : MonoBehaviour
    {
        private const float Range = 4.5f;
        private const int QteSteps = 4;

        private static FinisherController _instance;
        private Canvas _canvas;
        private UiLabel _label;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("FinisherController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<FinisherController>();
            _instance.Build();
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("FinisherPrompt", sortOrder: 12);
            DontDestroyOnLoad(_canvas.gameObject);
            _label = UiTheme.Label(_canvas.transform, "Finisher  [F]", 30, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);
            var r = _label.Rect;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0f);
            r.anchoredPosition = new Vector2(0f, 150f);
            r.sizeDelta = new Vector2(420f, 44f);
            _canvas.gameObject.SetActive(false);
        }

        private void ShowPrompt(bool on)
        {
            if (_canvas != null && _canvas.gameObject.activeSelf != on) _canvas.gameObject.SetActive(on);
        }

        private void Update()
        {
            if (QuickTimeController.IsActive) { ShowPrompt(false); return; }

            var cam = Camera.main;
            if (cam == null) { ShowPrompt(false); return; }

            var target = StaggerController.NearestStaggered(cam.transform.position, Range);
            ShowPrompt(target != null);
            if (target == null) return;

            if (FinisherPressed())
            {
                var victim = target.Target;
                ShowPrompt(false);
                QuickTimeController.Begin(QteSteps, QuickTimeSequence.DefaultWindowSeconds,
                    () => Execute(victim), null);
            }
        }

        private static void Execute(Damageable victim)
        {
            if (victim == null || victim.Health == null || victim.Health.IsDead) return;
            float lethal = victim.Health.Max * 4f + 9999f; // guarantee the execution
            victim.Apply(new DamageInfo(lethal, Element.Fire));
        }

        private static bool FinisherPressed()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame) return true;
            var gp = Gamepad.current;
            return gp != null && gp.buttonNorth.wasPressedThisFrame;
        }
    }
}
