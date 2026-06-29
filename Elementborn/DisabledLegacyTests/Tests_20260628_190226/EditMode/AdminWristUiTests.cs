using NUnit.Framework;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Tests.EditMode
{
    public class AdminWristUiTests
    {
        [SetUp]
        public void SetUp()
        {
            ElementbornEditModeTestUtility.ResetAll();
        }

        [TearDown]
        public void TearDown()
        {
            ElementbornEditModeTestUtility.ResetAll();
        }

        [Test]
        public void Catalog_ContainsCheatAndRawCommandActions()
        {
            AdminActionCatalog catalog = new GameObject("Catalog").AddComponent<AdminActionCatalog>();
            catalog.RebuildDefaults();

            Assert.NotNull(catalog.FindAction("cheat.apply"));
            Assert.NotNull(catalog.FindAction("raw.command"));
            Assert.IsTrue(catalog.GetActionsForCategory(AdminActionCategory.CheatCodes).Count > 0);
            Assert.IsTrue(catalog.GetActionsForCategory(AdminActionCategory.RawCommand).Count > 0);
        }

        [Test]
        public void Executor_CapitalPressureAction_ChangesCapitalState()
        {
            AdminActionExecutor executor = new GameObject("Executor").AddComponent<AdminActionExecutor>();
            AdminActionRequest request = new AdminActionRequest { ActionId = "capital.pressure" };
            request.Values["capital"] = "FireCapital";
            request.Values["pressure"] = "Unrest";
            request.Values["amount"] = "7";
            request.Values["notes"] = "unit test";

            AdminActionResult result = executor.Execute(request);

            Assert.IsTrue(result.Success);
            CapitalRuntimeState state = CapitalWorldStateTracker.Ensure().GetOrCreate(CapitalId.FireCapital);
            Assert.AreEqual(7, state.GetOrCreatePressure(CapitalPressureType.Unrest).Value);
        }

        [Test]
        public void Executor_CheatApply_AdmitsDemoCreature()
        {
            AdminActionExecutor executor = new GameObject("Executor").AddComponent<AdminActionExecutor>();
            AdminActionRequest request = new AdminActionRequest { ActionId = "cheat.apply" };
            request.Values["cheat"] = "admit_demo_creature";

            AdminActionResult result = executor.Execute(request);

            Assert.IsTrue(result.Success);
            Assert.NotNull(CreatureOrphanageRecoveryRegistry.Ensure().Find("demo_emberfox"));
        }

        [Test]
        public void CommandRouter_RawLoopStartShortcut_StartsLoop()
        {
            AdminRuntimeCommandRouter router = new GameObject("Router").AddComponent<AdminRuntimeCommandRouter>();
            bool handled = router.ExecuteCommand("loop.start", out string message);

            Assert.IsTrue(handled, message);
            Assert.AreEqual(ElementbornGameplayLoopState.Explore, ElementbornMainGameplayLoopDirector.Ensure().State);
        }
    }
}
