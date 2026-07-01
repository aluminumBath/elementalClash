using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game.Social;

namespace Elementborn.Game
{
    /// <summary>
    /// The player's holdings: a <see cref="Wallet"/>, taming lures, the creatures they own, and their one
    /// house (which becomes their respawn point). Buying and taming check the player's element so a user
    /// only gets creatures appropriate to them. A lightweight singleton so drops and shops can find it.
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        public Wallet Wallet { get; } = new Wallet();
        public Inventory Items { get; } = new Inventory();
        public Element? PlayerElement { get; set; }
        public bool PlayerIsConfluence { get; set; }

        /// <summary>The chosen character, kept so it can be persisted and rebuilt on load.</summary>
        public ChannelerLoadout Loadout { get; set; }
        public RevealTier RevealTier { get; set; }
        public bool CharacterCreated { get; set; }

        public bool HasHouse { get; private set; }
        public Vector3 HouseLocation { get; private set; }
        public Homestead Home { get; } = new Homestead();
        public Wardrobe Wardrobe { get; } = new Wardrobe();
        public HomeGarden Garden { get; } = new HomeGarden();

        private readonly Dictionary<CreatureKind, int> _lures = new Dictionary<CreatureKind, int>();
        private readonly HashSet<CreatureKind> _owned = new HashSet<CreatureKind>();
        private readonly HashSet<VehicleKind> _ownedVehicles = new HashSet<VehicleKind>();

        public event System.Action WalletChanged;
        public event System.Action OwnedChanged;
        public event System.Action HouseChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // --- currency ---
        public void AddCurrency(Currency c, int count)
        {
            Wallet.Add(c, count);
            WalletChanged?.Invoke();
            QuestEvents.RaiseCurrencyGained(c.ToString(), count);
        }

        public void AddItem(string itemId, int count = 1)
        {
            // Unified inventory: items live in the Tracker now. The legacy Items bag is no longer written to
            // (kept only until the save migration in the next step). Quest + audio hooks are preserved so every
            // caller (crafting output, creature drops, world pickups, trade) still fires collection events.
            PlayerInventoryTracker.AddItemId(itemId, count);
            QuestEvents.RaiseItemCollected(itemId, count);
            AudioController.Instance?.Pickup();
        }

        // --- lures (the per-creature taming items) ---
        public int Lures(CreatureKind kind) => _lures.TryGetValue(kind, out int n) ? n : 0;
        public void AddLure(CreatureKind kind, int count = 1)
        {
            if (count <= 0) return;
            _lures[kind] = Lures(kind) + count;
        }

        // --- ownership ---
        public bool Owns(CreatureKind kind) => _owned.Contains(kind);
        public IEnumerable<CreatureKind> Owned => _owned;

        /// <summary>Revoke ownership of a creature (e.g. Kiana confiscating a mistreated one) — it must be tamed
        /// again from scratch. Returns true if it was owned.</summary>
        public bool RemoveOwned(CreatureKind kind)
        {
            if (!_owned.Remove(kind)) return false;
            OwnedChanged?.Invoke();
            return true;
        }

        /// <summary>Grant ownership of a creature directly (used by the Summon Beacon). Returns true if it was new
        /// (a duplicate returns false, letting the caller refund Motes instead).</summary>
        public bool GrantOwned(CreatureKind kind)
        {
            if (!_owned.Add(kind)) return false;
            OwnedChanged?.Invoke();
            return true;
        }

        /// <summary>Whether the player's element lets them use this creature.</summary>
        public bool CanUse(CreatureInfo info)
        {
            if (info.RequiredElement == null) return true;
            if (PlayerIsConfluence) return true;
            return PlayerElement.HasValue && PlayerElement.Value == info.RequiredElement.Value;
        }

        public bool TryBuy(CreatureKind kind, out string reason)
        {
            var info = CreatureCatalog.For(kind);
            if (!info.Purchasable) { reason = "Not for sale"; return false; }
            if (!CanUse(info)) { reason = "Wrong element"; return false; }
            if (Owns(kind)) { reason = "Already owned"; return false; }
            if (!Wallet.CanAfford(info.Price)) { reason = "Can't afford it"; return false; }

            Wallet.Spend(info.Price);
            _owned.Add(kind);
            WalletChanged?.Invoke();
            OwnedChanged?.Invoke();
            reason = "Purchased";
            return true;
        }

        // --- vehicle ownership ---
        public bool OwnsVehicle(VehicleKind kind) => _ownedVehicles.Contains(kind);
        public IEnumerable<VehicleKind> OwnedVehicles => _ownedVehicles;

        /// <summary>Whether the player's element lets them use this vehicle (boats are open to all).</summary>
        public bool CanUseVehicle(VehicleInfo info)
        {
            if (info.RequiredElement == null) return true;
            if (PlayerIsConfluence) return true;
            return PlayerElement.HasValue && PlayerElement.Value == info.RequiredElement.Value;
        }

        public bool TryBuyVehicle(VehicleKind kind, out string reason)
        {
            var info = VehicleCatalog.For(kind);
            if (!CanUseVehicle(info)) { reason = "Wrong element"; return false; }
            if (OwnsVehicle(kind)) { reason = "Already owned"; return false; }
            if (!Wallet.CanAfford(info.Price)) { reason = "Can't afford it"; return false; }

            Wallet.Spend(info.Price);
            _ownedVehicles.Add(kind);
            WalletChanged?.Invoke();
            OwnedChanged?.Invoke();
            reason = "Purchased";
            return true;
        }

        public TameOutcome TryTame(CreatureKind kind, float healthFraction, IRandomSource rng)
        {
            var info = CreatureCatalog.For(kind);
            if (!CanUse(info)) return TameOutcome.Fail("Wrong element");
            if (Owns(kind)) return TameOutcome.Fail("Already owned");

            bool hasLure = Lures(kind) > 0;
            var outcome = TamingRules.Resolve(info, healthFraction, hasLure, rng);

            if (outcome.LureConsumed && hasLure) _lures[kind] = Lures(kind) - 1;
            if (outcome.Success) { _owned.Add(kind); OwnedChanged?.Invoke(); }
            return outcome;
        }

        // --- house / respawn home ---
        public bool TryClaimHouse(Vector3 position, long price, out string reason)
        {
            if (price > 0 && !Wallet.CanAfford(price)) { reason = "Can't afford it"; return false; }
            if (price > 0) Wallet.Spend(price);
            HasHouse = true;
            HouseLocation = position; // one house per player — claiming relocates home
            WalletChanged?.Invoke();
            HouseChanged?.Invoke();
            reason = "Home claimed";
            return true;
        }

        // --- save / load ---
        public SaveData ToSave()
        {
            var d = new SaveData
            {
                silver = Wallet.CountOf(Currency.Silver),
                ruby = Wallet.CountOf(Currency.Ruby),
                emerald = Wallet.CountOf(Currency.Emerald),
                sapphire = Wallet.CountOf(Currency.Sapphire),
                diamond = Wallet.CountOf(Currency.Diamond),
                hasHouse = HasHouse,
                wardrobeLook = Wardrobe.Save(),
                gardenAccrued = Garden.Save(),
                houseX = HouseLocation.x,
                houseY = HouseLocation.y,
                houseZ = HouseLocation.z,
                playerElement = PlayerElement.HasValue ? PlayerElement.Value.ToString() : "",
                isConfluence = PlayerIsConfluence,
                created = CharacterCreated,
                revealTier = (int)RevealTier,
                loadoutWeapon = Loadout != null ? Loadout.Weapon.ToString() : "",
            };
            foreach (var kv in _lures) { d.lureKinds.Add(kv.Key.ToString()); d.lureCounts.Add(kv.Value); }
            foreach (var k in _owned) d.ownedKinds.Add(k.ToString());
            foreach (var v in _ownedVehicles) d.ownedVehicles.Add(v.ToString());
            foreach (var a in Home.SaveAdditions()) d.houseAdditions.Add(a);
            if (Loadout != null)
            {
                foreach (var el in Loadout.Elements) d.loadoutElements.Add(el.ToString());
                foreach (var sa in Loadout.SubArts) d.loadoutSubArts.Add(sa.ToString());
            }
            foreach (var e in Items.Entries()) { d.itemIds.Add(e.Key); d.itemCounts.Add(e.Value); }
            QuestController.Instance?.CaptureInto(d);
            ProgressionController.Instance?.CaptureInto(d);
            GrimoireController.Instance?.CaptureInto(d);
            PlayerAttunementHud.Instance?.CaptureInto(d);
            MapState.Instance?.CaptureInto(d);
            CheckpointState.Instance?.CaptureInto(d);
            AchievementController.Instance?.CaptureInto(d);
            EquipmentController.Instance?.CaptureInto(d);
            GuildController.Instance?.CaptureInto(d);
            SummonController.Instance?.CaptureInto(d);
            StoryController.Instance?.CaptureInto(d);
            return d;
        }

        public void LoadFrom(SaveData d)
        {
            if (d == null) return;

            Wallet.Clear();
            Wallet.Add(Currency.Silver, d.silver);
            Wallet.Add(Currency.Ruby, d.ruby);
            Wallet.Add(Currency.Emerald, d.emerald);
            Wallet.Add(Currency.Sapphire, d.sapphire);
            Wallet.Add(Currency.Diamond, d.diamond);

            _lures.Clear();
            int n = Mathf.Min(d.lureKinds.Count, d.lureCounts.Count);
            for (int i = 0; i < n; i++)
                if (System.Enum.TryParse(d.lureKinds[i], out CreatureKind kind))
                    _lures[kind] = d.lureCounts[i];

            _owned.Clear();
            foreach (var name in d.ownedKinds)
                if (System.Enum.TryParse(name, out CreatureKind kind))
                    _owned.Add(kind);

            _ownedVehicles.Clear();
            foreach (var name in d.ownedVehicles)
                if (System.Enum.TryParse(name, out VehicleKind vk))
                    _ownedVehicles.Add(vk);

            HasHouse = d.hasHouse;
            HouseLocation = new Vector3(d.houseX, d.houseY, d.houseZ);
            Home.Restore(d.houseAdditions);
            Wardrobe.Restore(d.wardrobeLook);
            Garden.Restore(d.gardenAccrued);

            PlayerElement = !string.IsNullOrEmpty(d.playerElement) && System.Enum.TryParse(d.playerElement, out Element e)
                ? (Element?)e : null;
            PlayerIsConfluence = d.isConfluence;

            CharacterCreated = d.created;
            RevealTier = (RevealTier)Mathf.Clamp(d.revealTier, 0, 3);

            var els = new List<Element>();
            foreach (var name in d.loadoutElements)
                if (System.Enum.TryParse(name, out Element el)) els.Add(el);
            var subs = new List<SubArt>();
            foreach (var name in d.loadoutSubArts)
                if (System.Enum.TryParse(name, out SubArt sa)) subs.Add(sa);
            WeaponType weapon = WeaponType.None;
            if (!string.IsNullOrEmpty(d.loadoutWeapon)) System.Enum.TryParse(d.loadoutWeapon, out weapon);
            Loadout = (els.Count > 0 || weapon != WeaponType.None)
                ? ChannelerLoadout.FromState(els, subs, weapon)
                : null;

            Items.Clear();
            int itemCount = Mathf.Min(d.itemIds.Count, d.itemCounts.Count);
            for (int i = 0; i < itemCount; i++) Items.Add(d.itemIds[i], d.itemCounts[i]);
            QuestController.Instance?.RestoreFrom(d);
            ProgressionController.Instance?.RestoreFrom(d);
            GrimoireController.Instance?.RestoreFrom(d);
            PlayerAttunementHud.Instance?.RestoreFrom(d);
            MapState.Instance?.RestoreFrom(d);
            CheckpointState.Instance?.RestoreFrom(d);
            AchievementController.Instance?.RestoreFrom(d);
            EquipmentController.Instance?.RestoreFrom(d);
            GuildController.Instance?.RestoreFrom(d);
            SummonController.Instance?.RestoreFrom(d);
            StoryController.Instance?.RestoreFrom(d);

            WalletChanged?.Invoke();
            OwnedChanged?.Invoke();
            HouseChanged?.Invoke();
        }
    }
}
