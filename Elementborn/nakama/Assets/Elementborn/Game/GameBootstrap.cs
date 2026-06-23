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
    /// active it boots VR, otherwise it falls back to a non-VR rig chosen by
    /// <c>preferredFlatMode</c> (first-person or third-person). A third-person rig is just
    /// another prefab: assign <c>thirdPersonRigPrefab</c> and set the preference.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public enum Mode { Vr, Flat, ThirdPerson }

        [Header("Rigs (assign prefabs in the inspector)")]
        [SerializeField] private GameObject vrRigPrefab;
        [SerializeField] private GameObject firstPersonRigPrefab;
        [SerializeField] private GameObject thirdPersonRigPrefab;
        [Tooltip("When no XR runtime is active, which non-VR rig to spawn.")]
        [SerializeField] private Mode preferredFlatMode = Mode.Flat;

        [Header("Spawn")]
        [SerializeField] private Transform spawnPoint;

        public Mode ActiveMode { get; private set; }

        private void Awake()
        {
            ActiveMode = IsXrActive() ? Mode.Vr
                : (preferredFlatMode == Mode.ThirdPerson ? Mode.ThirdPerson : Mode.Flat);

            GameObject prefab =
                ActiveMode == Mode.Vr ? vrRigPrefab :
                ActiveMode == Mode.ThirdPerson ? thirdPersonRigPrefab :
                firstPersonRigPrefab;

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
