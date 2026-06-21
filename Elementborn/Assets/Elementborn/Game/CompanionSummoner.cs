using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Call your tamed companions to fight. One toggle (C by default, or a VR button): spawns a
    /// <see cref="CompanionController"/> for every companion the player owns; toggling again sends them
    /// away. Reuses one placeholder companion prefab for all of them until per-creature art exists.
    /// Put this on the player rig.
    /// </summary>
    public sealed class CompanionSummoner : MonoBehaviour
    {
        [SerializeField] private GameObject companionPrefab;
        [SerializeField] private InputActionReference summonAction;
        [SerializeField] private float spawnRadius = 2.5f;

        private readonly List<GameObject> _summoned = new List<GameObject>();

        private void Update()
        {
            if (Pressed()) Toggle();
        }

        public void Toggle()
        {
            if (_summoned.Count > 0) { Dismiss(); return; }
            SummonAll();
        }

        private void SummonAll()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null || companionPrefab == null) return;

            int i = 0;
            foreach (var k in inv.Owned)
            {
                if (!CreatureCatalog.For(k).IsCompanion) continue;

                float angle = i++ * 1.7f;
                Vector3 at = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
                var go = Instantiate(companionPrefab, at, Quaternion.identity);

                var cc = go.GetComponent<CompanionController>();
                if (cc == null) cc = go.AddComponent<CompanionController>();
                cc.Configure(k);
                _summoned.Add(go);
            }

            if (_summoned.Count == 0) Debug.Log("[Elementborn] No companions owned yet.");
        }

        private void Dismiss()
        {
            foreach (var go in _summoned) if (go != null) Destroy(go);
            _summoned.Clear();
        }

        private bool Pressed()
        {
            if (summonAction != null && summonAction.action != null && summonAction.action.enabled)
                return summonAction.action.WasPressedThisFrame();
            var k = Keyboard.current;
            return k != null && k.cKey.wasPressedThisFrame;
        }
    }
}
