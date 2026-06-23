using System.Collections.Generic;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// A shopkeeper the player interacts with to open the buy menu. Optionally stocks specific creatures
    /// and vehicles; if both lists are left empty it sells everything purchasable. <see cref="PlayerInteractor"/>
    /// shows an "[E] Shop" prompt near it and calls <see cref="Open"/>. Place one at a market POI.
    /// </summary>
    public sealed class Merchant : MonoBehaviour
    {
        [SerializeField] private string shopName = "Market";
        [SerializeField] private List<CreatureKind> creatureStock = new List<CreatureKind>();
        [SerializeField] private List<VehicleKind> vehicleStock = new List<VehicleKind>();
        [SerializeField] private bool sellEverythingIfEmpty = true;

        public string ShopName => shopName;

        public IEnumerable<CreatureKind> Creatures()
        {
            if (creatureStock.Count > 0 || !sellEverythingIfEmpty) return creatureStock;

            var all = new List<CreatureKind>();
            foreach (CreatureKind k in System.Enum.GetValues(typeof(CreatureKind)))
                if (CreatureCatalog.For(k).Purchasable) all.Add(k);
            return all;
        }

        public IEnumerable<VehicleKind> Vehicles()
        {
            if (vehicleStock.Count > 0 || !sellEverythingIfEmpty) return vehicleStock;

            var all = new List<VehicleKind>();
            foreach (VehicleKind k in System.Enum.GetValues(typeof(VehicleKind))) all.Add(k);
            return all;
        }

        public void Open() => ShopController.EnsureInstance().Open(this);
    }
}
