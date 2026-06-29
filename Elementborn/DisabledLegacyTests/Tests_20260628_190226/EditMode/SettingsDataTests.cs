using NUnit.Framework;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class SettingsDataTests
    {
        [Test]
        public void ClampsEveryFieldIntoRange()
        {
            var s = new SettingsData
            {
                masterVolume = 5f,
                musicVolume = -1f,
                sfxVolume = 2f,
                mouseSensitivity = 99f,
                fieldOfView = 10f,
            };
            s.Clamped();

            Assert.AreEqual(1f, s.masterVolume, 1e-4f);
            Assert.AreEqual(0f, s.musicVolume, 1e-4f);
            Assert.AreEqual(1f, s.sfxVolume, 1e-4f);
            Assert.AreEqual(5f, s.mouseSensitivity, 1e-4f);   // upper clamp
            Assert.AreEqual(50f, s.fieldOfView, 1e-4f);       // lower clamp
        }

        [Test]
        public void DefaultsAreReasonable()
        {
            var s = new SettingsData();
            Assert.IsTrue(s.comfortVignette);
            Assert.IsFalse(s.invertY);
            Assert.AreEqual(70f, s.fieldOfView, 1e-4f);
            Assert.AreEqual(1f, s.masterVolume, 1e-4f);
        }

        [Test]
        public void CopyIsIndependent()
        {
            var a = new SettingsData { masterVolume = 0.3f };
            var b = a.Copy();
            b.masterVolume = 0.9f;
            Assert.AreEqual(0.3f, a.masterVolume, 1e-4f);
        }
    }
}
