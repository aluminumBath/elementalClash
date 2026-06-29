using NUnit.Framework;
using UnityEngine.InputSystem;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class InputBindingsTests
    {
        [Test]
        public void EveryRebindableActionHasKeyboardAndGamepadBindings()
        {
            Assert.IsNotNull(InputBindings.Map);
            Assert.AreEqual(14, InputBindings.Rebindable.Count);
            foreach (var e in InputBindings.Rebindable)
            {
                Assert.GreaterOrEqual(e.Action.bindings.Count, 2, $"{e.Label} should have kbm + gamepad bindings");
                Assert.IsFalse(string.IsNullOrEmpty(e.Action.bindings[0].effectivePath), $"{e.Label} kbm binding missing");
                Assert.IsFalse(string.IsNullOrEmpty(e.Action.bindings[1].effectivePath), $"{e.Label} gamepad binding missing");
                Assert.IsTrue(e.Action.bindings[1].effectivePath.StartsWith("<Gamepad>"), $"{e.Label} second binding should be gamepad");
            }
        }

        [Test]
        public void DefaultsMatchTheDocumentedScheme()
        {
            Assert.AreEqual("<Mouse>/leftButton", InputBindings.PrimaryCast.bindings[0].path);
            Assert.AreEqual("<Gamepad>/rightTrigger", InputBindings.PrimaryCast.bindings[1].path);
            Assert.AreEqual("<Keyboard>/e", InputBindings.Interact.bindings[0].path);
            Assert.AreEqual("<Keyboard>/escape", InputBindings.Menu.bindings[0].path);
            Assert.AreEqual("<Gamepad>/start", InputBindings.Menu.bindings[1].path);
        }

        [Test]
        public void OverrideAppliesAndResetsToDefault()
        {
            var a = InputBindings.Interact;
            string original = a.bindings[0].effectivePath;

            a.ApplyBindingOverride(0, "<Keyboard>/q");
            Assert.AreEqual("<Keyboard>/q", a.bindings[0].effectivePath);

            a.RemoveAllBindingOverrides();
            Assert.AreEqual(original, a.bindings[0].effectivePath);
        }

        [Test]
        public void DisplayIsHumanReadable()
        {
            string s = InputBindings.Display(InputBindings.Dash, 0);
            Assert.IsFalse(string.IsNullOrEmpty(s));
            Assert.AreNotEqual("-", s);
        }
    }
}
