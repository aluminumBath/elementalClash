using System.Collections.Generic;
using NUnit.Framework;
using Elementborn.Core;
using Elementborn.Core.Social;

namespace Elementborn.Tests.EditMode
{
    public class SignatureCharacterTests
    {
        [Test]
        public void AshHasThreeElementsAndTheirSubArts()
        {
            var lo = SignatureCharacter.Loadout();
            Assert.IsTrue(lo.IsChanneler);
            Assert.IsFalse(lo.IsConfluence); // three, not the full four
            CollectionAssert.AreEquivalent(new[] { Element.Air, Element.Water, Element.Fire }, lo.Elements);
            Assert.IsTrue(lo.HasSubArt(SubArt.Flight));
            Assert.IsTrue(lo.HasSubArt(SubArt.SanguineGrip));
            Assert.IsTrue(lo.HasSubArt(SubArt.Magmacraft));
            Assert.AreEqual(WeaponType.None, lo.Weapon);
        }

        [Test]
        public void AshHasThreeNamedSidekicks()
        {
            Assert.AreEqual(3, SignatureCharacter.Sidekicks.Length);
            var names = new List<string>();
            foreach (var s in SignatureCharacter.Sidekicks) names.Add(s.Name);
            CollectionAssert.AreEquivalent(new[] { "Apollo", "Artemis", "Iago" }, names);
        }

        [Test]
        public void ArtemisCanBlinkAndIagoIsReborn()
        {
            Assert.IsTrue(SignatureCharacter.Artemis.Combat.CanBlink);   // shadow-teleport
            Assert.IsTrue(SignatureCharacter.Iago.Combat.CanRebirth);    // phoenix
        }

        [Test]
        public void IagoMirrorsTheHeroElements()
        {
            CollectionAssert.AreEquivalent(SignatureCharacter.Loadout().Elements, SignatureCharacter.IagoElements);
        }

        [Test]
        public void DragonFormIsDefined()
        {
            Assert.AreEqual(DragonForm.Shadowthorn, SignatureCharacter.Dragon);
        }

        // ---- Owner admin allow-list ----

        [Test]
        public void OwnerEmailAndItsGmailAliasesAreAdmin()
        {
            Assert.IsTrue(AdminAccounts.IsAdminEmail("steeleschauer@gmail.com"));
            Assert.IsTrue(AdminAccounts.IsAdminEmail("STEELESCHAUER@Gmail.com"));    // case
            Assert.IsTrue(AdminAccounts.IsAdminEmail("steele.schauer@gmail.com"));   // dots ignored
            Assert.IsTrue(AdminAccounts.IsAdminEmail("steeleschauer+dev@gmail.com"));// +suffix ignored
        }

        [Test]
        public void OtherAddressesAreNotAdmin()
        {
            Assert.IsFalse(AdminAccounts.IsAdminEmail("someoneelse@gmail.com"));
            Assert.IsFalse(AdminAccounts.IsAdminEmail("steeleschauer@outlook.com")); // different domain
            Assert.IsFalse(AdminAccounts.IsAdminEmail(""));
            Assert.IsFalse(AdminAccounts.IsAdminEmail(null));
        }

        [Test]
        public void ProvisioningTheOwnerRegistersAGlobalAdmin()
        {
            var dir = new LocalUserDirectory();
            var owner = AdminAccounts.ProvisionOwner(dir, "owner-1");
            Assert.IsTrue(owner.IsAdmin);
            Assert.AreEqual(UserRole.Admin, owner.Role);
            Assert.AreEqual(SignatureCharacter.Name, owner.DisplayName);
            Assert.IsTrue(dir.TryGet("owner-1", out var fetched) && fetched.IsAdmin);
        }
    }
}
