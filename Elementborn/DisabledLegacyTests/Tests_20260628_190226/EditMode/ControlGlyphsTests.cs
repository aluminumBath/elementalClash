using NUnit.Framework;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class ControlGlyphsTests
    {
        [Test]
        public void GamepadPathsMapToTokens()
        {
            Assert.AreEqual("A", ControlGlyphs.Token("<Gamepad>/buttonSouth"));
            Assert.AreEqual("X", ControlGlyphs.Token("<Gamepad>/buttonWest"));
            Assert.AreEqual("RT", ControlGlyphs.Token("<Gamepad>/rightTrigger"));
            Assert.AreEqual("LB", ControlGlyphs.Token("<Gamepad>/leftShoulder"));
            Assert.AreEqual("D-Up", ControlGlyphs.Token("<Gamepad>/dpad/up"));
            Assert.AreEqual("Start", ControlGlyphs.Token("<Gamepad>/start"));
        }

        [Test]
        public void KeyboardAndMousePathsMapToTokens()
        {
            Assert.AreEqual("E", ControlGlyphs.Token("<Keyboard>/e"));
            Assert.AreEqual("Esc", ControlGlyphs.Token("<Keyboard>/escape"));
            Assert.AreEqual("Space", ControlGlyphs.Token("<Keyboard>/space"));
            Assert.AreEqual("F8", ControlGlyphs.Token("<Keyboard>/f8"));
            Assert.AreEqual("LMB", ControlGlyphs.Token("<Mouse>/leftButton"));
        }

        [Test]
        public void TokenFollowsActionBindingByIndex()
        {
            // Interact defaults: keyboard E (index 0), gamepad X (index 1).
            Assert.AreEqual("E", ControlGlyphs.Token(InputBindings.Interact, 0));
            Assert.AreEqual("X", ControlGlyphs.Token(InputBindings.Interact, 1));
        }

        [Test]
        public void PromptWrapsTokenAndVerb()
        {
            string p = ControlGlyphs.Prompt(InputBindings.Interact, "Ride");
            Assert.IsTrue(p.Contains("Ride"));
            Assert.IsTrue(p.StartsWith("["));
        }

        [Test]
        public void SpriteNameMapsControls()
        {
            Assert.AreEqual("gp_a", ControlGlyphs.SpriteName("<Gamepad>/buttonSouth"));
            Assert.AreEqual("gp_rt", ControlGlyphs.SpriteName("<Gamepad>/rightTrigger"));
            Assert.AreEqual("gp_dup", ControlGlyphs.SpriteName("<Gamepad>/dpad/up"));
            Assert.AreEqual("key_e", ControlGlyphs.SpriteName("<Keyboard>/e"));
            Assert.AreEqual("mouse_left", ControlGlyphs.SpriteName("<Mouse>/leftButton"));
        }

        [Test]
        public void PlayStationBrandRelabelsFaces()
        {
            ControlGlyphs.SetBrand(GamepadBrand.PlayStation);
            try
            {
                Assert.AreEqual("□", ControlGlyphs.Token("<Gamepad>/buttonWest"));
                Assert.AreEqual("✕", ControlGlyphs.Token("<Gamepad>/buttonSouth"));
                Assert.AreEqual("L1", ControlGlyphs.Token("<Gamepad>/leftShoulder"));
                Assert.AreEqual("gp_ps_square", ControlGlyphs.SpriteName("<Gamepad>/buttonWest"));
                Assert.AreEqual("gp_ps_l1", ControlGlyphs.SpriteName("<Gamepad>/leftShoulder"));
                Assert.AreEqual("gp_ps_options", ControlGlyphs.SpriteName("<Gamepad>/start"));
            }
            finally { ControlGlyphs.SetBrand(GamepadBrand.Xbox); }
        }

        [Test]
        public void SwitchBrandUsesPhysicalPositions()
        {
            ControlGlyphs.SetBrand(GamepadBrand.Switch);
            try
            {
                Assert.AreEqual("B", ControlGlyphs.Token("<Gamepad>/buttonSouth"));
                Assert.AreEqual("A", ControlGlyphs.Token("<Gamepad>/buttonEast"));
                Assert.AreEqual("gp_sw_b", ControlGlyphs.SpriteName("<Gamepad>/buttonSouth"));
                Assert.AreEqual("gp_sw_zl", ControlGlyphs.SpriteName("<Gamepad>/leftTrigger"));
                Assert.AreEqual("gp_sw_minus", ControlGlyphs.SpriteName("<Gamepad>/select"));
            }
            finally { ControlGlyphs.SetBrand(GamepadBrand.Xbox); }
        }
    }
}
