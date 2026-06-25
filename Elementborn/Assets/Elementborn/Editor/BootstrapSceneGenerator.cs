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
        private const string CreaturePrefab = PrefabDir + "/WildCreature.prefab";
        private const string EnemyPrefab = PrefabDir + "/Enemy.prefab";
        private const string CivilianPrefab = PrefabDir + "/Civilian.prefab";

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

            // Platform: per-device frame-rate target at boot + a dev frame-time overlay (toggle with F3).
            var platform = new GameObject("Platform");
            Add(platform, "Localization");
            Add(platform, "PerformanceController");
            Add(platform, "PerformanceHud");
            Add(platform, "PauseMenu");
            Add(platform, "LoadingScreen");

            // Social overlay (press J in play mode): notifications / friends / chat / feedback / moderation.
            var social = new GameObject("Social");
            Add(social, "SocialMenuController");
            Add(social, "PartyController");
            Add(social, "TradeController");
            Add(social, "GuildController");
            Add(social, "DuelController");

            // Quest loop: tracks objectives from gameplay events and grants rewards on turn-in.
            var quests = new GameObject("Quests");
            Add(quests, "QuestController");
            Add(quests, "DialogueController");   // NPC conversations: accept / turn in quests
            Add(quests, "QuestLogController");   // press L for the quest log
            Add(quests, "InventoryController");   // press I for the inventory
            Add(quests, "ProgressionController"); // XP / levels from combat + quests
            Add(quests, "CharacterScreenController"); // press C for level / XP
            Add(quests, "ProgressionHud");        // always-on level / XP bar

            // Grimoire overlay (press G in play mode): discovery-driven Bestiary / Attacks / Bloodlines tome.
            var grimoire = new GameObject("Grimoire");
            Add(grimoire, "GrimoireController");

            // Achievements: a milestone tracker off the QuestEvents bus + a viewer (press K).
            var achievements = new GameObject("Achievements");
            Add(achievements, "AchievementController");
            Add(achievements, "AchievementsViewer");

            // Story mode: the live cursor through the campaign (current chapter + chosen ending), saved with the game.
            var story = new GameObject("Story");
            Add(story, "StoryController");
            Add(story, "StoryDirector");

            // Environmental hazards: altitude cold + underwater pressure (with element/enchant exemptions),
            // applied to the player on a tick. Dormant on the flat surface; bites in the heights and deep water.
            var hazards = new GameObject("Hazards");
            Add(hazards, "EnvironmentHazardController");

            // Admin / cheat console (dev tool; the backquote key toggles it).
            var adminConsole = new GameObject("Admin Console");
            Add(adminConsole, "AdminConsole");

            // Crafting overlay (press B): turn loot materials into gear/consumables via recipes.
            var crafting = new GameObject("Crafting");
            Add(crafting, "CraftingViewer");

            // Equipment (press V): wear crafted/looted gear for max-health + power bonuses.
            var equipment = new GameObject("Equipment");
            Add(equipment, "EquipmentController");
            Add(equipment, "EquipmentViewer");

            // Summon Beacon (press U): currency-gated gacha — spend Sigils to roll companions & mounts into the
            // roster, dust duplicates into Motes, claim a featured creature with Motes.
            var summon = new GameObject("Summon");
            Add(summon, "SummonController");
            Add(summon, "SummonViewer");

            // Map systems: leyline state + always-on minimap + full map viewer (press M, also opened from a rift).
            var map = new GameObject("Map");
            Add(map, "MapState");
            Add(map, "CoopAllies"); // renders sharing friends as visible in-world figures, not just map dots
            Add(map, "MinimapHud");
            Add(map, "MapViewerController");
            Add(map, "CheckpointState"); // respawn shrines: activation + active anchor, drawn on the map

            BuildDemoContent();

            // World population: the flow's EnterWorld calls spawnPlacer.Place(World), scattering creatures,
            // enemies, and town civilians across the generated regions (snapped to the mesh terrain).
            BuildWorldPrefabs();
            var worldPop = new GameObject("World Population");
            var placer = Add(worldPop, "WorldSpawnPlacer");
            Wire(placer, "creaturePrefab", AssetDatabase.LoadAssetAtPath<GameObject>(CreaturePrefab));
            Wire(placer, "enemyPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefab));
            Wire(placer, "civilianPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(CivilianPrefab));
            Wire(flow, "spawnPlacer", placer);

            // Site interiors: the instanced pocket spaces that site entrances open into.
            var siteInteriors = new GameObject("Site Interiors");
            var interior = Add(siteInteriors, "SiteInteriorController");
            Wire(interior, "enemyPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefab));
            Wire(interior, "creaturePrefab", AssetDatabase.LoadAssetAtPath<GameObject>(CreaturePrefab));

            // Leyline rifts: spawns a discoverable, fast-travelable rift for each canonical WorldMap node.
            var rifts = new GameObject("Leyline Rifts");
            Add(rifts, "LeylineRiftSpawner");

            // Respawn shrines: a checkpoint obelisk for each canonical WorldMap waystone. Touching one's Interact
            // sets it as the player's respawn point (see docs/MAP.md).
            var checkpoints = new GameObject("Checkpoints");
            Add(checkpoints, "CheckpointSpawner");

            // Demo friend presence (sandbox only): seeds one ally and reports it orbiting the player so the map's
            // friend markers are visible without a server or a second client. Remove the object for a real build —
            // the networked build registers a Nakama presence source instead. See docs/MAP.md.
            var demoFriend = new GameObject("Demo Friend (presence sim)");
            Add(demoFriend, "SimulatedFriendPresence");

            var music = new GameObject("Music");
            Add(music, "MusicController");        // looping ambient bed

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

            // Combat presentation: each listens to PlayerCombatController.OutcomeReady and presents one OutcomeKind.
            Add(root, "AbilityVfxBinder");        // Projectile — procedural visuals + audio + a damaging Projectile
            Add(root, "MeleeController");         // Melee — single-target weapon swing
            Add(root, "SweepController");         // Sweep — wide multi-target arc (crowd control)
            Add(root, "HeavyController");         // Heavy — committed impact zone at range
            Add(root, "BarrierResponder");        // Barrier — Defend raises a brief damage-reducing shield
            Add(root, "DashResponder");           // Movement — Dash / Flight glide
            Add(root, "SanguineGripController");  // Control — Sanguine Grip (water sub-art)

            Add(root, "PlayerInteractor");        // adds an InteractionArbiter to the rig automatically
            Add(root, "PlantControlController");
            Add(root, "Damageable");              // the player can take damage; level bonus raises its max health

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

            // Combat presentation: each listens to PlayerCombatController.OutcomeReady and presents one OutcomeKind.
            Add(root, "AbilityVfxBinder");        // Projectile — procedural visuals + audio + a damaging Projectile
            Add(root, "MeleeController");         // Melee — single-target weapon swing
            Add(root, "SweepController");         // Sweep — wide multi-target arc (crowd control)
            Add(root, "HeavyController");         // Heavy — committed impact zone at range
            Add(root, "BarrierResponder");        // Barrier — Defend raises a brief damage-reducing shield
            Add(root, "DashResponder");           // Movement — Dash / Flight glide
            Add(root, "SanguineGripController");  // Control — Sanguine Grip (water sub-art)

            Add(root, "PlayerInteractor");
            Add(root, "VrInteractInput");        // VR Interact: right grip -> InteractionArbiter (NPCs, pickups, mount, tame)
            Add(root, "VrOverlayHub");           // VR menus: left menu button -> hub that opens each overlay
            Add(root, "PlantControlController");
            Add(root, "Damageable");

            SavePrefab(root, path);
            Debug.LogWarning("[Bootstrap] VR rig is a starting point: enable an XR plugin (Project Settings > " +
                             "XR Plug-in Management, tracking origin Floor) and bind controller poses before it " +
                             "tracks. Comfort locomotion (snap turn, stick move, recenter) is on VrComfortLocomotion " +
                             "— see docs/VR_SETUP.md.");
        }

        // ---- helpers ----------------------------------------------------------------------------------------

        private static void BuildDemoContent()
        {
            var demo = new GameObject("Demo Content");

            Npc(demo.transform, "Willow", 0, new Vector3(4f, 1f, 4f));
            Npc(demo.transform, "Kiana", 1, new Vector3(-4f, 1f, 4f));
            Npc(demo.transform, "Parfa", 2, new Vector3(0f, 1f, 6f));

            var market = GameObject.CreatePrimitive(PrimitiveType.Cube);
            market.name = "Merchant";
            market.transform.SetParent(demo.transform, false);
            market.transform.position = new Vector3(6f, 1f, -2f);
            Add(market, "Merchant");                // sells everything by default
            Paint(market, MaterialGenerator.Stone);

            Creature(demo.transform, "Wild Creature A", new Vector3(8f, 1f, 2f));
            Creature(demo.transform, "Wild Creature B", new Vector3(8f, 1f, -4f));
        }

        private static void Npc(Transform parent, string who, int idIndex, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "NPC " + who;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var npc = Add(go, "GuideNpcController");
            WireEnum(npc, "id", idIndex);            // GuideNpcId: Willow=0, Kiana=1, Parfa=2
            Paint(go, MaterialGenerator.Character);
        }

        private static void Creature(Transform parent, string name, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Add(go, "CreatureController");           // RequireComponent adds Damageable
            Add(go, "Tameable");
            Add(go, "FactionMember");
            Paint(go, MaterialGenerator.Foliage);
        }

        private static void Paint(GameObject go, string materialPath)
        {
            var mat = MaterialGenerator.Load(materialPath);
            var r = go.GetComponent<MeshRenderer>();
            if (mat != null && r != null) r.sharedMaterial = mat;
        }

        private static void BuildWorldPrefabs()
        {
            EnsureDir(PrefabDir);
            MaterialGenerator.EnsureMaterials();

            if (AssetDatabase.LoadAssetAtPath<GameObject>(CreaturePrefab) == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = "WildCreature";
                Add(go, "CreatureController");   // adds Damageable
                Add(go, "Tameable");
                Add(go, "FactionMember");
                Paint(go, MaterialGenerator.Foliage);
                SavePrefab(go, CreaturePrefab);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefab) == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = "Enemy";
                Object.DestroyImmediate(go.GetComponent<Collider>()); // CharacterController (added below) is the collider
                Add(go, "EnemyController");      // adds Damageable + FactionMember + CharacterController
                Paint(go, MaterialGenerator.Fire);
                SavePrefab(go, EnemyPrefab);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(CivilianPrefab) == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.name = "Civilian";
                Add(go, "FactionMember");
                Paint(go, MaterialGenerator.Air);
                SavePrefab(go, CivilianPrefab);
            }

            AssetDatabase.SaveAssets();
        }

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
