using System;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    [Serializable]
    public class OwnedCreatureRecord
    {
        public string RecordId = "";
        public string CreatureId = "";
        public string DisplayName = "";
        public string CustomName = "";
        public CreatureTraversalType TraversalType = CreatureTraversalType.Unknown;
        public CreatureTemperament Temperament = CreatureTemperament.Unknown;
        public CreatureRideState State = CreatureRideState.Stable;
        public string StableId = "";
        public bool Favorite = false;

        public int BondXp = 0;
        public int TimesFed = 0;
        public int TimesRidden = 0;

        public float LastX;
        public float LastY;
        public float LastZ;

        public Vector3 LastKnownPosition
        {
            get => new Vector3(LastX, LastY, LastZ);
            set
            {
                LastX = value.x;
                LastY = value.y;
                LastZ = value.z;
            }
        }

        public string NameForDisplay => string.IsNullOrWhiteSpace(CustomName) ? DisplayName : CustomName;

        public CreatureBondStage BondStage
        {
            get
            {
                if (BondXp >= 300) return CreatureBondStage.Soulbonded;
                if (BondXp >= 150) return CreatureBondStage.Loyal;
                if (BondXp >= 60) return CreatureBondStage.Friendly;
                if (BondXp >= 20) return CreatureBondStage.Familiar;
                return CreatureBondStage.Wary;
            }
        }
    }
}
