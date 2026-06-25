using NUnit.Framework;
using Elementborn.Core;

namespace Elementborn.Tests
{
    public class ToneSynthTests
    {
        [Test]
        public void Render_LengthMatchesDuration()
        {
            var spec = new ToneSpec(Wave.Sine, 440f, 440f, 0.2f, 0f, 0.01f, 0.01f);
            var data = ToneSynth.Render(spec, 22050, 1);
            Assert.AreEqual((int)System.Math.Round(0.2f * 22050), data.Length);
        }

        [Test]
        public void Render_SamplesWithinUnitRange_AndHasSignal()
        {
            var spec = new ToneSpec(Wave.Saw, 200f, 800f, 0.15f, 0.3f, 0.005f, 0.02f);
            var data = ToneSynth.Render(spec, 22050, 7);
            bool any = false;
            foreach (var s in data)
            {
                Assert.LessOrEqual(System.Math.Abs(s), 1f + 1e-4f);
                if (System.Math.Abs(s) > 0.01f) any = true;
            }
            Assert.IsTrue(any, "expected a non-silent buffer");
        }

        [Test]
        public void Render_EnvelopeFadesAtEnds()
        {
            var spec = new ToneSpec(Wave.Sine, 440f, 440f, 0.2f, 0f, 0.02f, 0.02f);
            var data = ToneSynth.Render(spec, 22050, 3);
            Assert.Less(System.Math.Abs(data[0]), 0.2f);
            Assert.Less(System.Math.Abs(data[data.Length - 1]), 0.2f);
        }

        [Test]
        public void Render_IsDeterministic()
        {
            var spec = new ToneSpec(Wave.Noise, 100f, 100f, 0.05f, 1f, 0f, 0f);
            var a = ToneSynth.Render(spec, 22050, 42);
            var b = ToneSynth.Render(spec, 22050, 42);
            Assert.AreEqual(a.Length, b.Length);
            for (int i = 0; i < a.Length; i++) Assert.AreEqual(a[i], b[i]);
        }
    }
}
