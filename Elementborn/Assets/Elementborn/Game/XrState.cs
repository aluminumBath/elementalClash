using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>Whether an XR (VR) loader is active this run — used by the code-built UI to decide between a flat
    /// screen canvas and a world-space one. Mirrors the check the creation UI uses.</summary>
    public static class XrState
    {
        public static bool Active
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                var s = UnityEngine.XR.Management.XRGeneralSettings.Instance;
                return s != null && s.Manager != null && s.Manager.activeLoader != null;
#else
                return false;
#endif
            }
        }
    }
}
