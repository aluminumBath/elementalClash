using System;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class ElementbornSpawnWaveEntry
    {
        public string EntryId = "";
        public GameObject Prefab;
        public ElementbornSpawnRole SpawnRole = ElementbornSpawnRole.EnemyWave;
        public int Count = 1;
        public float Radius = 3f;
        public bool ParentToDirector = true;
    }
}
