using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Elementborn.EditorTools
{
    /// <summary>
    /// One-click scaffolding for a playable scene. Because Unity scenes and prefabs are Editor-authored binary
    /// assets (they can't be hand-written as source), this generator builds them programmatically:
    ///
    ///   • <b>Build Player Rig Prefabs</b> — first-person, third-person, and a minimal VR rig, each with the
    ///     real rig/combat/interaction components wired up, saved to Assets/Elementborn/Prefabs.
    ///   • <b>Build Playable Scene</b> — a sun + ambient + ground + spawn point and a GameObject carrying
    ///     <c>GameBootstrap</c> (rig prefabs assigned) and <c>GameFlowController</c>, saved to
    ///     Assets/Elementborn/Scenes/Bootstrap.unity. Press Play and it boots through character creation into
    ///     the world.
    ///   • <b>Build Everything</b> — both, and adds the scene to Build Settings.
    ///
    /// It targets components by <em>name</em> via <see cref="TypeCache"/> and sets fields via
    /// <see cref="SerializedObject"/> by string, so it never hard-references game types: anything it can't find
    /// is logged as a "wire it manually" warning rather than failing. VR still needs the XR plugin enabled and
    /// controller poses bound; see docs/BOOTSTRAP.md.
    /// </summary>
    public static class BootstrapSceneGenerator
    {
        private const string PrefabDir = "Assets/Elementborn/Prefabs";
        private const string SceneDir = "Assets/Elementborn/Scenes";
        private const string ScenePath = SceneDir + "/Bootstrap.unity";
        private const string FpPrefab = PrefabDir + "/PlayerRig_FirstPerson.prefab";
        private const string TpPrefab = PrefabDir + "/PlayerRig_ThirdPerson.prefab";
        private const string VrPrefab = PrefabDir + "/PlayerRig_VR.prefab";

        // ---- menu items -------------------------------------------------------------------------------------

        [MenuItem("Elementborn/Bootstrap/1. Build Player Rig Prefabs")]
        public static void BuildRigs()
        {
            EnsureDir(PrefabDir);
            MaterialGenerator.EnsureMaterials();
            BuildFlatRig(thirdPerson: false, FpPrefab);
            BuildFlatRig(thirdPerson: true, TpPrefab);
            BuildVrRig(VrPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] Player rig prefabs built in " + PrefabDir);
        }

        [MenuItem("Elementborn/Bootstrap/2. Build Playable Scene")]
        public static void BuildScene()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(TpPrefab) == null) BuildRigs();
            MaterialGenerator.EnsureMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = new GameObject("Directional Light");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.62f);

            // Sky + sun tint + ambient + fog at runtime (builds its own ToonSky material).
            var style = new GameObject("SceneStyle");
            Add(style, "SceneStyleController");

            // Ground: the mesh terrain builder if it's available (it builds itself on Start), else a big plane.
            var terrain = new GameObject("Terrain");
            var mtb = Add(terrain, "MeshTerrainBuilder");
            if (mtb == null)
            {
                Object.DestroyImmediate(terrain);
                var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.name = "Ground";
                plane.transform.localScale = new Vector3(60f, 1f, 60f);
                var groundMat = MaterialGenerator.Load(MaterialGenerator.Ground);
                var planeRenderer = plane.GetComponent<MeshRenderer>();
                if (groundMat != null && planeRenderer != null) planeRenderer.sharedMaterial = groundMat;
            }

            var spawn = new GameObject("SpawnPoint");
            spawn.transform.position = new Vector3(0f, 2f, 0f);

            var boot = new GameObject("GameBootstrap");
            var gb = Add(boot, "GameBootstrap");
            Wire(gb, "firstPersonRigPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(FpPrefab));
            Wire(gb, "thirdPersonRigPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(TpPrefab));
            Wire(gb, "vrRigPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(VrPrefab));
            Wire(gb, "spawnPoint", spawn.transform);
            WireEnum(gb, "preferredFlatMode", 2); // Mode { Vr=0, Flat=1, ThirdPerson=2 }

            var flow = Add(boot, "GameFlowController");
            if (mtb != null) Wire(flow, "meshTerrainBuilder", mtb);

            EnsureDir(SceneDir);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Bootstrap] Playable scene saved to " + ScenePath + " — press Play to boot it.");
        }

        [MenuItem("Elementborn/Bootstrap/3. Build Everything + Add To Build Settings")]
        public static void BuildEverything()
        {
            BuildScene();
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == ScenePath))
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log("[Bootstrap] Added Bootstrap.unity to Build Settings (index 0).");
            }
        }

        // ---- rig builders -----------------------------------------------------------------------------------

        private static void BuildFlatRig(bool thirdPerson, string path)
        {
            var root = new GameObject(thirdPerson ? "PlayerRig_ThirdPerson" : "PlayerRig_FirstPerson");
            root.tag = "Player";

            var cc = root.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 2f;
            cc.radius = 0.4f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(body.GetComponent<Collider>()); // CharacterController is the collider
            var bodyMat = MaterialGenerator.Load(MaterialGenerator.Character);
            var bodyRenderer = body.GetComponent<MeshRenderer>();
            if (bodyMat != null && bodyRenderer != null) bodyRenderer.sharedMaterial = bodyMat;

            var camGo = new GameObject("RigCamera");
            camGo.transform.SetParent(root.transform, false);
            camGo.transform.localPosition = thirdPerson ? new Vector3(0f, 1.6f, -4.5f) : new Vector3(0f, 1.6f, 0f);
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();

            var rig = Add(root, thirdPerson ? "ThirdPersonRig" : "FirstPersonRig");
            Wire(rig, "rigCamera", cam);
            if (thirdPerson) Wire(rig, "body", body.transform);

            var input = Add(root, "FlatInputProvider");
            var combat = Add(root, "PlayerCombatController");
            if (input != null) Wire(combat, "inputProviderBehaviour", input);
            Wire(combat, "castOrigin", camGo.transform);
            var weapon = Add(root, "WeaponHolder");
            if (weapon != null) Wire(combat, "weaponHolder", weapon);

            Add(root, "PlayerInteractor");        // adds an InteractionArbiter to the rig automatically
            Add(root, "PlantControlController");

            SavePrefab(root, path);
        }

        private static void BuildVrRig(string path)
        {
            var root = new GameObject("PlayerRig_VR");
            root.tag = "Player";
            var cc = root.AddComponent<CharacterController>();
            cc.center = new Vector3(0f, 1f, 0f);
            cc.height = 1.8f;
            cc.radius = 0.3f;

            var offset = new GameObject("Camera Offset");
            offset.transform.SetParent(root.transform, false);

            var head = new GameObject("Head (Camera)");
            head.transform.SetParent(offset.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            head.tag = "MainCamera";
            head.AddComponent<Camera>();
            head.AddComponent<AudioListener>();
            var vignette = Add(head, "ComfortVignette"); // comfort vignette lives on the HMD camera

            var locomotion = Add(root, "VrComfortLocomotion");
            Wire(locomotion, "cameraOffset", offset.transform);
            Wire(locomotion, "head", head.transform);
            if (vignette != null) Wire(locomotion, "comfort", vignette);

            var input = Add(root, "VrInputProvider");
            var combat = Add(root, "PlayerCombatController");
            if (input != null) Wire(combat, "inputProviderBehaviour", input);
            Wire(combat, "castOrigin", head.transform);

            Add(root, "PlayerInteractor");
            Add(root, "PlantControlController");

            SavePrefab(root, path);
            Debug.LogWarning("[Bootstrap] VR rig is a starting point: enable an XR plugin (Project Settings > " +
                             "XR Plug-in Management, tracking origin Floor) and bind controller poses before it " +
                             "tracks. Comfort locomotion (snap turn, stick move, recenter) is on VrComfortLocomotion " +
                             "— see docs/VR_SETUP.md.");
        }

        // ---- helpers ----------------------------------------------------------------------------------------

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static System.Type Find(string simpleName) =>
            TypeCache.GetTypesDerivedFrom<MonoBehaviour>().FirstOrDefault(t => t.Name == simpleName);

        private static Component Add(GameObject go, string typeName)
        {
            var t = Find(typeName);
            if (t == null)
            {
                Debug.LogWarning("[Bootstrap] component type not found, skipped: " + typeName);
                return null;
            }
            var existing = go.GetComponent(t);
            return existing != null ? existing : go.AddComponent(t);
        }

        private static bool Wire(Object component, string field, Object value)
        {
            if (component == null) return false;
            var so = new SerializedObject(component);
            var p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning("[Bootstrap] field '" + field + "' not found on " +
                                 component.GetType().Name + " — wire it manually in the Inspector.");
                return false;
            }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool WireEnum(Object component, string field, int enumValueIndex)
        {
            if (component == null) return false;
            var so = new SerializedObject(component);
            var p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogWarning("[Bootstrap] enum field '" + field + "' not found — set it manually.");
                return false;
            }
            p.enumValueIndex = enumValueIndex;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static void EnsureDir(string dir)
        {
            if (AssetDatabase.IsValidFolder(dir)) return;
            var parts = dir.Split('/');
            var cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
