using UnityEngine;
#if UNITY_2020_1_OR_NEWER
using UnityEngine.XR.Management;
#endif

namespace Elementborn.Game
{
    /// <summary>
    /// Single entry point that decides, at launch, whether to run in VR or flat mode,
    /// then spawns the matching rig over the shared logic.
    ///
    /// Quest builds are always VR. The Windows build ships both: if an XR runtime is
    /// active it boots VR, otherwise it falls back to first-person flat. Adding a
    /// third-person rig later is just another prefab + a flat-mode preference.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public enum Mode { Vr, Flat }

        [Header("Rigs (assign prefabs in the inspector)")]
        [SerializeField] private GameObject vrRigPrefab;
        [SerializeField] private GameObject firstPersonRigPrefab;

        [Header("Spawn")]
        [SerializeField] private Transform spawnPoint;

        public Mode ActiveMode { get; private set; }

        private void Awake()
        {
            ActiveMode = IsXrActive() ? Mode.Vr : Mode.Flat;

            GameObject prefab = ActiveMode == Mode.Vr ? vrRigPrefab : firstPersonRigPrefab;
            if (prefab == null)
            {
                Debug.LogError($"GameBootstrap: no rig prefab assigned for {ActiveMode} mode.");
                return;
            }

            Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;
            Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
            Instantiate(prefab, pos, rot);

            Debug.Log($"[Elementborn] Booted in {ActiveMode} mode.");
        }

        private static bool IsXrActive()
        {
#if UNITY_2020_1_OR_NEWER
            var settings = XRGeneralSettings.Instance;
            return settings != null
                && settings.Manager != null
                && settings.Manager.activeLoader != null;
#else
            return false;
#endif
        }
    }
}
