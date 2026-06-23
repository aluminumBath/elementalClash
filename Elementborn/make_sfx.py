#!/usr/bin/env python3
"""Synthesize Elementborn's placeholder sound effects with numpy.

Regenerates the 16 placeholder SFX (plus a looping ambient music bed) referenced by docs/AUDIO.md, using simple synthesis —
filtered noise, sweeps, and inharmonic partials. They're deliberately rough stand-ins until real sound design.

Usage:
    python3 make_sfx.py [output_dir]
Default output dir is Assets/Elementborn/Resources/Audio next to this script.
"""
import os
import sys
import wave
import numpy as np

SR = 44100
ROOT = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(ROOT, "Assets/Elementborn/Resources/Audio")


def _t(dur):
    return np.linspace(0.0, dur, int(SR * dur), endpoint=False)


def tone(freq, dur):
    return np.sin(2 * np.pi * freq * _t(dur))


def sweep(f0, f1, dur):
    t = _t(dur)
    f = np.linspace(f0, f1, t.size)
    phase = 2 * np.pi * np.cumsum(f) / SR
    return np.sin(phase)


def expdecay(dur, tau):
    return np.exp(-_t(dur) / tau)


def bandnoise(dur, lo, hi):
    """White noise brick-wall filtered to [lo, hi] via FFT (no scipy needed)."""
    n = int(SR * dur)
    x = np.random.randn(n)
    spec = np.fft.rfft(x)
    freqs = np.fft.rfftfreq(n, 1.0 / SR)
    spec[(freqs < lo) | (freqs > hi)] = 0.0
    return np.fft.irfft(spec, n)


def partials(base, ratios, dur, tau):
    out = np.zeros(int(SR * dur))
    for i, r in enumerate(ratios):
        out += (0.7 ** i) * tone(base * r, dur)
    return out * expdecay(dur, tau)


def finish(sig):
    """Normalize, click-free fades, to int16."""
    sig = np.asarray(sig, dtype=np.float64)
    peak = np.max(np.abs(sig)) or 1.0
    sig = 0.7 * sig / peak
    f = min(256, sig.size // 2)
    if f > 0:
        ramp = np.linspace(0.0, 1.0, f)
        sig[:f] *= ramp
        sig[-f:] *= ramp[::-1]
    return (sig * 32767).astype(np.int16)


def finish_loop(sig):
    """Normalize to int16 with no boundary fades, so the clip loops seamlessly."""
    sig = np.asarray(sig, dtype=np.float64)
    peak = np.max(np.abs(sig)) or 1.0
    sig = 0.5 * sig / peak
    return (sig * 32767).astype(np.int16)


def gen():
    s = {}
    np.random.seed(7)  # reproducible synthesis
    # Elemental
    s["fire_explosion"] = bandnoise(0.6, 30, 500) * expdecay(0.6, 0.16) \
        + 0.6 * bandnoise(0.6, 200, 3500) * expdecay(0.6, 0.05)
    crackle = 0.5 + 0.5 * np.abs(bandnoise(1.2, 8, 40))
    s["fire_burn"] = bandnoise(1.2, 120, 2600) * crackle * 0.8
    s["water_splash"] = bandnoise(0.45, 300, 6500) * expdecay(0.45, 0.12) \
        + 0.4 * sweep(1800, 400, 0.45) * expdecay(0.45, 0.1)
    s["rock_break"] = bandnoise(0.4, 40, 600) * expdecay(0.4, 0.12) \
        + 0.6 * bandnoise(0.4, 800, 4000) * expdecay(0.4, 0.03)
    swell = np.sin(np.linspace(0, np.pi, int(SR * 0.7))) ** 1.5
    s["wind_whoosh"] = bandnoise(0.7, 200, 1900) * swell
    ice = bandnoise(0.5, 2200, 9000) * expdecay(0.5, 0.2)
    for _ in range(6):  # sharp transients
        i = np.random.randint(0, ice.size - 600)
        ice[i:i + 600] += bandnoise(600 / SR, 3000, 9000) * expdecay(600 / SR, 0.01)
    s["ice_crack"] = ice + 0.3 * partials(2400, [1, 1.9, 3.1], 0.5, 0.18)
    flicker = 0.6 + 0.4 * (np.random.rand(int(SR * 0.5)) > 0.5)
    s["zap_lightning"] = (partials(180, [1, 2.3, 3.7, 5.1, 6.9], 0.5, 0.22)
                          + 0.5 * bandnoise(0.5, 800, 6000)) * flicker
    s["metal_clang"] = partials(440, [1, 2.76, 5.40, 8.93], 0.8, 0.55) \
        + 0.4 * bandnoise(0.8, 1500, 7000) * expdecay(0.8, 0.02)
    # Generic / movement
    s["whoosh_short"] = bandnoise(0.25, 300, 3200) * np.sin(np.linspace(0, np.pi, int(SR * 0.25)))
    s["hit_soft"] = bandnoise(0.12, 80, 1300) * expdecay(0.12, 0.04)
    # UI
    s["ui_click"] = (tone(1200, 0.04) + 0.3 * bandnoise(0.04, 1500, 6000)) * expdecay(0.04, 0.012)
    s["ui_confirm"] = sweep(660, 990, 0.16) * expdecay(0.16, 0.12)
    s["ui_back"] = sweep(880, 520, 0.16) * expdecay(0.16, 0.12)
    # Reward / progression juice
    notes = [523, 659, 784, 1047]  # C major arpeggio, rising
    lu = np.zeros(int(SR * 0.55))
    for i, fr in enumerate(notes):
        seg = tone(fr, 0.2) * expdecay(0.2, 0.16)
        st = int(SR * 0.1 * i)
        lu[st:st + seg.size] += seg[:lu.size - st]
    s["level_up"] = lu + 0.2 * partials(1047, [1, 2, 3], 0.55, 0.22)
    coin = np.zeros(int(SR * 0.2))
    ca = tone(988, 0.06) * expdecay(0.06, 0.04)
    coin[:ca.size] += ca
    cb = tone(1319, 0.11) * expdecay(0.11, 0.07)
    cs = int(SR * 0.05)
    coin[cs:cs + cb.size] += cb[:coin.size - cs]
    s["coin"] = coin
    s["pickup"] = (tone(740, 0.07) + 0.3 * tone(1110, 0.07)) * expdecay(0.07, 0.05) \
        + 0.2 * bandnoise(0.07, 1500, 5000) * expdecay(0.07, 0.02)
    return s


def gen_music():
    """A calm, seamlessly-looping ambient pad. Frequencies snap to whole cycles over the loop length so the
    waveform is continuous at the loop point (no click)."""
    dur = 8.0
    t = _t(dur)

    def pad(freq):
        k = round(freq * dur)          # whole cycles over the loop
        return np.sin(2 * np.pi * (k / dur) * t)

    chord = (0.6 * pad(110) + 0.4 * pad(165) + 0.35 * pad(220)
             + 0.25 * pad(330) + 0.15 * pad(440))
    swell = 0.6 + 0.4 * np.sin(2 * np.pi * (1 / dur) * t)            # one slow cycle
    shimmer = 0.1 * pad(660) * (0.5 + 0.5 * np.sin(2 * np.pi * (2 / dur) * t))
    return {"music_calm": chord * swell + shimmer}


def main():
    os.makedirs(OUT, exist_ok=True)
    for name, sig in gen().items():
        path = os.path.join(OUT, name + ".wav")
        with wave.open(path, "w") as w:
            w.setnchannels(1)
            w.setsampwidth(2)
            w.setframerate(SR)
            w.writeframes(finish(sig).tobytes())
    for name, sig in gen_music().items():
        path = os.path.join(OUT, name + ".wav")
        with wave.open(path, "w") as w:
            w.setnchannels(1)
            w.setsampwidth(2)
            w.setframerate(SR)
            w.writeframes(finish_loop(sig).tobytes())
    print(f"Wrote {len(gen())} SFX + {len(gen_music())} music to {OUT}")


if __name__ == "__main__":
    main()
