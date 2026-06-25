namespace Elementborn.Core
{
    /// <summary>Oscillator shape for a synthesized placeholder sound.</summary>
    public enum Wave { Sine, Square, Saw, Triangle, Noise }

    /// <summary>A compact recipe for one synthesized sound: an oscillator swept from <see cref="StartFreq"/> to
    /// <see cref="EndFreq"/>, blended toward white noise by <see cref="NoiseMix"/>, under an attack/sustain/release
    /// amplitude envelope.</summary>
    public readonly struct ToneSpec
    {
        public readonly Wave Wave;
        public readonly float StartFreq;
        public readonly float EndFreq;
        public readonly float Duration; // seconds
        public readonly float NoiseMix; // 0..1 blend toward white noise
        public readonly float Attack;   // seconds, fade-in
        public readonly float Release;  // seconds, fade-out

        public ToneSpec(Wave wave, float startFreq, float endFreq, float duration,
                        float noiseMix, float attack, float release)
        {
            Wave = wave;
            StartFreq = startFreq;
            EndFreq = endFreq;
            Duration = duration;
            NoiseMix = noiseMix;
            Attack = attack;
            Release = release;
        }
    }

    /// <summary>Pure software synth that renders a mono PCM buffer (samples in [-1, 1]) from a <see cref="ToneSpec"/>
    /// — a frequency-swept oscillator blended with seeded white noise under an attack/sustain/release envelope.
    /// Engine-free and deterministic, so it's unit-tested; <c>ProceduralSfx</c> wraps each rendered buffer in a Unity
    /// AudioClip so the game is audible before any real audio assets are dropped in.</summary>
    public static class ToneSynth
    {
        public static float[] Render(ToneSpec spec, int sampleRate, int seed)
        {
            if (sampleRate < 1) sampleRate = 1;
            int n = (int)System.Math.Round(spec.Duration * sampleRate);
            if (n < 1) n = 1;
            var data = new float[n];

            uint rng = (uint)(seed * 2654435761u + 1013904223u);
            double phase = 0.0;
            double twoPi = 2.0 * System.Math.PI;
            int attackN = (int)(spec.Attack * sampleRate);
            int releaseN = (int)(spec.Release * sampleRate);

            for (int i = 0; i < n; i++)
            {
                float frac = n > 1 ? (float)i / (n - 1) : 0f;
                double freq = spec.StartFreq + (spec.EndFreq - spec.StartFreq) * frac;
                phase += twoPi * freq / sampleRate;

                rng = rng * 1664525u + 1013904223u; // LCG white noise, range minus-one to one
                float noise = (float)((rng >> 8) & 0xFFFFFF) / 8388608f - 1f;

                float osc = Oscillator(spec.Wave, phase, noise);
                float v = osc * (1f - spec.NoiseMix) + noise * spec.NoiseMix;

                float env = 1f;
                if (attackN > 0 && i < attackN) env = (float)i / attackN;
                if (releaseN > 0 && i > n - releaseN) env *= (float)(n - i) / releaseN;

                float s = v * env * 0.9f;
                if (s > 1f) s = 1f; else if (s < -1f) s = -1f;
                data[i] = s;
            }
            return data;
        }

        private static float Oscillator(Wave wave, double phase, float noise)
        {
            switch (wave)
            {
                case Wave.Sine:
                    return (float)System.Math.Sin(phase);
                case Wave.Square:
                    return System.Math.Sin(phase) >= 0.0 ? 1f : -1f;
                case Wave.Saw:
                {
                    double p = phase / (2.0 * System.Math.PI);
                    return (float)(2.0 * (p - System.Math.Floor(p + 0.5)));
                }
                case Wave.Triangle:
                {
                    double p = phase / (2.0 * System.Math.PI);
                    double saw = 2.0 * (p - System.Math.Floor(p + 0.5));
                    return (float)(2.0 * System.Math.Abs(saw) - 1.0);
                }
                case Wave.Noise:
                    return noise;
                default:
                    return 0f;
            }
        }
    }
}
