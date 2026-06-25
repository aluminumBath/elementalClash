using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Synthesizes a characterful placeholder AudioClip for each <see cref="SfxKind"/> so the whole audio
    /// system is audible the moment you press play — UI blips, elemental booms, footsteps, summon stings, and a calm
    /// ambient pad — before any real audio assets exist. AudioController falls back to these whenever a clip is
    /// missing from <c>Resources/Audio</c>; dropping a real file in overrides the placeholder. The waveform math
    /// lives in the engine-free, unit-tested <see cref="ToneSynth"/>.</summary>
    public static class ProceduralSfx
    {
        private const int SampleRate = 22050;

        public static AudioClip Build(SfxKind kind)
        {
            var spec = SpecFor(kind);
            float[] data = ToneSynth.Render(spec, SampleRate, (int)kind + 1);
            var clip = AudioClip.Create("synth_" + kind, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static ToneSpec SpecFor(SfxKind kind)
        {
            switch (kind)
            {
                // UI
                case SfxKind.UiClick:   return new ToneSpec(Wave.Square, 880f, 700f, 0.05f, 0.1f, 0.002f, 0.03f);
                case SfxKind.UiConfirm: return new ToneSpec(Wave.Sine, 660f, 990f, 0.14f, 0f, 0.005f, 0.05f);
                case SfxKind.UiBack:    return new ToneSpec(Wave.Sine, 660f, 440f, 0.14f, 0f, 0.005f, 0.05f);
                // Elemental impacts
                case SfxKind.FireExplosion: return new ToneSpec(Wave.Noise, 200f, 60f, 0.40f, 0.9f, 0.002f, 0.25f);
                case SfxKind.FireBurn:      return new ToneSpec(Wave.Noise, 300f, 200f, 0.50f, 0.85f, 0.05f, 0.20f);
                case SfxKind.WaterSplash:   return new ToneSpec(Wave.Noise, 700f, 300f, 0.30f, 0.8f, 0.002f, 0.20f);
                case SfxKind.RockBreak:     return new ToneSpec(Wave.Noise, 160f, 80f, 0.25f, 0.95f, 0.002f, 0.15f);
                case SfxKind.WindWhoosh:    return new ToneSpec(Wave.Noise, 500f, 900f, 0.40f, 0.85f, 0.05f, 0.20f);
                case SfxKind.IceCrack:      return new ToneSpec(Wave.Square, 1600f, 1200f, 0.15f, 0.5f, 0.002f, 0.10f);
                case SfxKind.ZapLightning:  return new ToneSpec(Wave.Saw, 1200f, 400f, 0.20f, 0.4f, 0.002f, 0.12f);
                case SfxKind.MetalClang:    return new ToneSpec(Wave.Square, 520f, 500f, 0.30f, 0.2f, 0.002f, 0.25f);
                case SfxKind.HitSoft:       return new ToneSpec(Wave.Sine, 220f, 160f, 0.10f, 0.35f, 0.002f, 0.07f);
                case SfxKind.WhooshShort:   return new ToneSpec(Wave.Noise, 400f, 800f, 0.18f, 0.85f, 0.01f, 0.10f);
                // Progression / pickups
                case SfxKind.LevelUp: return new ToneSpec(Wave.Sine, 523f, 1046f, 0.50f, 0f, 0.01f, 0.15f);
                case SfxKind.Coin:    return new ToneSpec(Wave.Square, 988f, 1318f, 0.12f, 0f, 0.002f, 0.06f);
                case SfxKind.Pickup:  return new ToneSpec(Wave.Sine, 784f, 1175f, 0.10f, 0f, 0.002f, 0.05f);
                // Ambient pad (loops)
                case SfxKind.MusicCalm: return new ToneSpec(Wave.Sine, 196f, 196f, 2.0f, 0f, 0.02f, 0.02f);
                // Locomotion
                case SfxKind.Footstep:      return new ToneSpec(Wave.Noise, 180f, 120f, 0.08f, 0.7f, 0.002f, 0.05f);
                case SfxKind.FootstepWater: return new ToneSpec(Wave.Noise, 600f, 300f, 0.10f, 0.8f, 0.002f, 0.07f);
                case SfxKind.Jump:          return new ToneSpec(Wave.Sine, 330f, 660f, 0.15f, 0.1f, 0.002f, 0.08f);
                case SfxKind.Land:          return new ToneSpec(Wave.Sine, 200f, 120f, 0.12f, 0.4f, 0.002f, 0.08f);
                // Summon stings
                case SfxKind.SummonPull:      return new ToneSpec(Wave.Saw, 220f, 520f, 0.60f, 0.2f, 0.05f, 0.20f);
                case SfxKind.SummonRare:      return new ToneSpec(Wave.Sine, 523f, 784f, 0.40f, 0f, 0.01f, 0.15f);
                case SfxKind.SummonEpic:      return new ToneSpec(Wave.Sine, 659f, 988f, 0.50f, 0f, 0.01f, 0.18f);
                case SfxKind.SummonLegendary: return new ToneSpec(Wave.Sine, 784f, 1318f, 0.80f, 0f, 0.02f, 0.25f);
                default:                      return new ToneSpec(Wave.Sine, 440f, 440f, 0.10f, 0f, 0.005f, 0.05f);
            }
        }
    }
}
