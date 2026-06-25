using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>The basic sound palette. Names map to WAVs under Resources/Audio/.</summary>
    public enum SfxKind
    {
        UiClick, UiConfirm, UiBack,
        FireExplosion, FireBurn, WaterSplash, RockBreak, WindWhoosh,
        IceCrack, ZapLightning, MetalClang, HitSoft, WhooshShort,
        LevelUp, Coin, Pickup, MusicCalm,
        Footstep, FootstepWater, Jump, Land,
        SummonPull, SummonRare, SummonEpic, SummonLegendary
    }

    /// <summary>
    /// Lightweight audio for the sample: a small pool of <see cref="AudioSource"/>s for one-shot SFX plus one
    /// looping ambient channel. Clips are loaded by name from <c>Resources/Audio/</c>; any that are missing are
    /// simply skipped, so the game stays silent-but-fine until audio is added. Volumes come from
    /// <see cref="SettingsStore"/>. Call <see cref="EnsureInstance"/> once (the flow controller does), then use
    /// <see cref="Play"/>/<see cref="PlayAt"/> or the gameplay helpers. Element abilities and impacts are mapped
    /// to clips here, and UI buttons made via <see cref="UiTheme"/> click automatically.
    /// </summary>
    public sealed class AudioController : MonoBehaviour
    {
        public static AudioController Instance { get; private set; }

        private static readonly Dictionary<SfxKind, string> ClipNames = new Dictionary<SfxKind, string>
        {
            { SfxKind.UiClick, "ui_click" }, { SfxKind.UiConfirm, "ui_confirm" }, { SfxKind.UiBack, "ui_back" },
            { SfxKind.FireExplosion, "fire_explosion" }, { SfxKind.FireBurn, "fire_burn" },
            { SfxKind.WaterSplash, "water_splash" }, { SfxKind.RockBreak, "rock_break" },
            { SfxKind.WindWhoosh, "wind_whoosh" }, { SfxKind.IceCrack, "ice_crack" },
            { SfxKind.ZapLightning, "zap_lightning" }, { SfxKind.MetalClang, "metal_clang" },
            { SfxKind.HitSoft, "hit_soft" }, { SfxKind.WhooshShort, "whoosh_short" },
            { SfxKind.LevelUp, "level_up" }, { SfxKind.Coin, "coin" }, { SfxKind.Pickup, "pickup" },
            { SfxKind.MusicCalm, "music_calm" },
            { SfxKind.Footstep, "footstep" }, { SfxKind.FootstepWater, "footstep_water" },
            { SfxKind.Jump, "jump" }, { SfxKind.Land, "land" },
            { SfxKind.SummonPull, "summon_pull" }, { SfxKind.SummonRare, "summon_rare" },
            { SfxKind.SummonEpic, "summon_epic" }, { SfxKind.SummonLegendary, "summon_legendary" },
        };

        private readonly Dictionary<SfxKind, AudioClip> _clips = new Dictionary<SfxKind, AudioClip>();
        private AudioSource[] _pool;
        private int _next;
        private AudioSource _ambient;

        public static AudioController EnsureInstance()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("AudioController");
            DontDestroyOnLoad(go);
            return go.AddComponent<AudioController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (var kv in ClipNames)
            {
                var clip = Resources.Load<AudioClip>("Audio/" + kv.Value);
                if (clip == null) clip = ProceduralSfx.Build(kv.Key); // synthesized placeholder until real audio ships
                if (clip != null) _clips[kv.Key] = clip;
            }

            _pool = new AudioSource[8];
            for (int i = 0; i < _pool.Length; i++) _pool[i] = NewSource(false);
            _ambient = NewSource(true);

            ApplyVolumes();
            SettingsStore.Changed += ApplyVolumes;
        }

        private void OnDestroy()
        {
            SettingsStore.Changed -= ApplyVolumes;
            if (Instance == this) Instance = null;
        }

        private AudioSource NewSource(bool loop)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0f; // 2D by default; PlayAt uses a 3D one-shot
            return s;
        }

        public void ApplyVolumes()
        {
            var s = SettingsStore.Current;
            AudioListener.volume = s.masterVolume;
            if (_ambient != null) _ambient.volume = s.musicVolume;
        }

        // ---- one-shots -------------------------------------------------------------------
        public void Play(SfxKind kind, float volume = 1f)
        {
            if (_pool == null || !_clips.TryGetValue(kind, out var clip) || clip == null) return;
            var src = _pool[_next];
            _next = (_next + 1) % _pool.Length;
            src.spatialBlend = 0f;
            src.PlayOneShot(clip, Mathf.Clamp01(volume) * SettingsStore.Current.sfxVolume);
        }

        public void PlayAt(SfxKind kind, Vector3 position, float volume = 1f)
        {
            if (!_clips.TryGetValue(kind, out var clip) || clip == null) return;
            float v = Mathf.Clamp01(volume) * SettingsStore.Current.sfxVolume * Mathf.Max(0.0001f, AudioListener.volume);
            AudioSource.PlayClipAtPoint(clip, position, v);
        }

        public void Click() => Play(SfxKind.UiClick);
        public void Confirm() => Play(SfxKind.UiConfirm);
        public void Back() => Play(SfxKind.UiBack);
        public void LevelUp() => Play(SfxKind.LevelUp);
        public void Coin() => Play(SfxKind.Coin);
        public void Pickup() => Play(SfxKind.Pickup, 0.8f);
        public void Footstep(Vector3 pos, bool water = false) => PlayAt(water ? SfxKind.FootstepWater : SfxKind.Footstep, pos, 0.7f);
        public void Land(Vector3 pos) => PlayAt(SfxKind.Land, pos, 0.9f);
        public void Jump(Vector3 pos) => PlayAt(SfxKind.Jump, pos, 0.8f);

        /// <summary>The Beacon's "cast" whoosh, played when a summon is rolled.</summary>
        public void SummonPull() => Play(SfxKind.SummonPull);

        /// <summary>The reveal sting for a summon's best tier (louder for a Legendary).</summary>
        public void SummonReveal(SummonRarity rarity) =>
            Play(SfxForSummon(rarity), rarity == SummonRarity.Legendary ? 1f : 0.9f);

        // ---- ambient bed -----------------------------------------------------------------
        public void Ambient(SfxKind kind, float volume = 1f)
        {
            if (_ambient == null) return;
            if (!_clips.TryGetValue(kind, out var clip) || clip == null) { _ambient.Stop(); return; }
            if (_ambient.clip == clip && _ambient.isPlaying) return;
            _ambient.clip = clip;
            _ambient.volume = SettingsStore.Current.musicVolume * Mathf.Clamp01(volume);
            _ambient.Play();
        }

        public void StopAmbient() { if (_ambient != null) _ambient.Stop(); }

        // ---- gameplay mapping ------------------------------------------------------------
        public void PlayAbility(AbilityOutcome outcome, Vector3 position) => PlayAt(SfxForAbility(outcome), position);
        public void PlayImpact(Element source, Vector3 position) => PlayAt(SfxForElement(source), position, 0.8f);

        public static SfxKind SfxForElement(Element e)
        {
            switch (e)
            {
                case Element.Fire:  return SfxKind.FireExplosion;
                case Element.Water: return SfxKind.WaterSplash;
                case Element.Earth: return SfxKind.RockBreak;
                case Element.Air:   return SfxKind.WindWhoosh;
                default:            return SfxKind.HitSoft;
            }
        }

        public static SfxKind SfxForAbility(AbilityOutcome o)
        {
            if (o.Variant == AbilityVariant.Ice) return SfxKind.IceCrack;
            if (o.Variant == AbilityVariant.Lightning) return SfxKind.ZapLightning;
            if (o.Kind == OutcomeKind.Barrier) return SfxKind.MetalClang;
            if (o.Kind == OutcomeKind.Movement) return SfxKind.WhooshShort;
            return SfxForElement(o.Element);
        }

        /// <summary>The reveal sting that matches a summon tier. Pure, so it's unit-tested.</summary>
        public static SfxKind SfxForSummon(SummonRarity rarity)
        {
            switch (rarity)
            {
                case SummonRarity.Legendary: return SfxKind.SummonLegendary;
                case SummonRarity.Epic:      return SfxKind.SummonEpic;
                default:                     return SfxKind.SummonRare;
            }
        }
    }
}
