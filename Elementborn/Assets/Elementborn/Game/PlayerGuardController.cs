using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// The player's guard. Hold the guard input (Left-Ctrl / gamepad B, rebindable) to raise it; the first ~0.2s
    /// after raising is a parry window. A hit in that window is negated and counter-staggers the enemy in front of you
    /// (which sets up a finisher); holding the guard past the window simply blocks — a big damage cut, which also
    /// slows your own poise loss since less damage lands. Drives the pure <see cref="GuardState"/> from input and a
    /// pre-mitigation hook on the player's <see cref="Damageable"/>. Self-bootstrapping; finds the player by tag.
    /// (The VR guard gesture is part of the VR-moves pass.)
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerGuardController : MonoBehaviour
    {
        private const float ParryWindow = 0.22f;
        private const float BlockReduction = 0.70f;
        private const float CounterRange = 6f;
        private const float CounterMinDot = 0.30f; // ~72-degree frontal cone

        private static readonly Color BlockColor = new Color(0.55f, 0.75f, 0.95f, 0.90f);
        private static readonly Color ParryColor = new Color(0.55f, 1.00f, 1.00f, 1.00f);

        private static PlayerGuardController _instance;

        private Damageable _body;
        private GuardState _guard;
        private bool _pendingParry;

        private Canvas _canvas;
        private UiLabel _label;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("PlayerGuardController");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PlayerGuardController>();
            _instance._guard = new GuardState(ParryWindow, BlockReduction, 0.3f);
            _instance.Build();
        }

        private void Build()
        {
            _canvas = UiTheme.Canvas("PlayerGuard", sortOrder: 10);
            DontDestroyOnLoad(_canvas.gameObject);

            var vr = _canvas.gameObject.AddComponent<VrHudAnchor>();
            vr.viewOffset = new Vector3(0f, -0.24f, 1.5f); vr.worldSize = new Vector2(420f, 140f);

            _label = UiTheme.Label(_canvas.transform, "\u25C6 GUARD", 22, BlockColor, TextAnchor.MiddleCenter);
            var r = _label.Rect;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0f);
            r.anchoredPosition = new Vector2(0f, 80f);
            r.sizeDelta = new Vector2(220f, 30f);
            _label.Rect.gameObject.SetActive(false);
        }

        private bool Acquire()
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged == null) return false;
            _body = tagged.GetComponentInParent<Damageable>();
            if (_body == null) return false;
            _body.IncomingModifier = ModifyIncoming;
            return true;
        }

        // Runs inside Damageable.Apply (pre-mitigation). Keep it pure data work — defer all spawning to Update.
        private DamageInfo ModifyIncoming(DamageInfo info)
        {
            if (!_guard.IsGuarding) return info;
            var result = _guard.Resolve();
            if (result == GuardResult.None) return info;
            if (result == GuardResult.Parried) _pendingParry = true;
            return new DamageInfo(info.Amount * _guard.DamageMultiplier(result), info.Source, info.Variant);
        }

        private void Update()
        {
            if (_body == null) { if (!Acquire()) return; }

            bool held = GuardHeld();
            if (held && !_guard.IsGuarding) _guard.Raise();
            else if (!held && _guard.IsGuarding) _guard.Lower();
            _guard.Tick(Time.deltaTime);

            if (_pendingParry) { ResolveParry(); _pendingParry = false; }

            bool guarding = _guard.IsGuarding;
            if (_label.Rect.gameObject.activeSelf != guarding) _label.Rect.gameObject.SetActive(guarding);
            if (guarding) _label.SetColor(_guard.InParryWindow ? ParryColor : BlockColor);
        }

        private void ResolveParry()
        {
            var cam = Camera.main;
            Vector3 origin = cam != null ? cam.transform.position : transform.position;
            Vector3 fwd = cam != null ? cam.transform.forward : transform.forward;

            var enemy = StaggerController.NearestInFront(origin, fwd, CounterRange, CounterMinDot);
            if (enemy != null) enemy.ForceStagger();

            Vector3 flat = fwd; flat.y = 0f;
            Vector3 dir = flat.sqrMagnitude > 1e-4f ? flat.normalized : Vector3.forward;
            Feel.TransientLight.Flash(origin + dir * 2.2f, ParryColor, 9f, 6f, 0.30f);
            Feel.FloatingText.Spawn(origin + dir * 3f, "PARRY!", ParryColor, 30f, 0.12f);
        }

        private static bool GuardHeld() => InputBindings.Guard.IsPressed();

        private void OnDestroy()
        {
            if (_body != null && _body.IncomingModifier == ModifyIncoming) _body.IncomingModifier = null;
        }
    }
}
