using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Drives the whole sample loop in code: Boot -> Character creation -> World map -> Spawned
    /// world. It builds the (code-built) creation and map screens itself, gates player control
    /// between stages, and populates the world on entry.
    ///
    /// Minimal setup: drop this on an empty GameObject. It will find the player rig's
    /// PlayerCombatController / FirstPersonRig / WeaponHolder at runtime (or you can assign them, plus
    /// a VR locomotion behaviour for rigMovement). Assign a WorldSpawnPlacer with pickup/enemy
    /// prefabs if you want the world populated; without it the flow still runs end to end.
    /// </summary>
    public sealed class GameFlowController : MonoBehaviour
    {
        public enum Stage { Boot, Menu, Creation, Map, World }

        [Header("Player rig (assigned, or found at runtime)")]
        [SerializeField] private PlayerCombatController player;
        [Tooltip("Movement/look behaviour disabled during menus (FirstPersonRig, or a VR locomotion provider).")]
        [SerializeField] private Behaviour rigMovement;
        [SerializeField] private WeaponHolder weaponHolder;

        [Header("World")]
        [Tooltip("Optional: builds terrain from the world on entry.")]
        [SerializeField] private TerrainBuilder terrainBuilder;
        [Tooltip("Optional drop-in alternative to the Unity TerrainBuilder. Assign one or the other.")]
        [SerializeField] private MeshTerrainBuilder meshTerrainBuilder;
        [Tooltip("Optional: builds POI-driven structures on entry.")]
        [SerializeField] private StructurePlacer structurePlacer;
        [Tooltip("Optional: populates the world with pickups/enemies on entry (needs prefabs assigned on it).")]
        [SerializeField] private WorldSpawnPlacer spawnPlacer;
        [SerializeField] private int worldSeed = 12345;
        [SerializeField] private int regionCount = 14;

        public Stage Current { get; private set; } = Stage.Boot;
        public CharacterCreationResult? Result { get; private set; }
        public WorldMap World { get; private set; }
        public WorldRegion StartRegion { get; private set; }

        private GameObject _creationGo;
        private GameObject _mapGo;
        private GameObject _menuGo;
        private bool _firstRunTutorial;

        private void Start()
        {
            if (player == null) player = FindObjectOfType<PlayerCombatController>();
            if (weaponHolder == null) weaponHolder = FindObjectOfType<WeaponHolder>();
            if (rigMovement == null) rigMovement = FindObjectOfType<FirstPersonRig>();
            if (rigMovement == null) rigMovement = FindObjectOfType<ThirdPersonRig>();
            EnsureEventSystem();
            AudioController.EnsureInstance();
            SettingsController.EnsureInstance();
            RebindController.EnsureInstance();
            ControlsLegendController.EnsureInstance();
            InputBindings.Enable();
            ControlGlyphs.EnsureMonitor();
            ModLoader.LoadAll();

            // The title screen is the front door: it decides whether to start fresh or continue a save.
            EnterMenu();
        }

        /// <summary>Applies a loaded character (loadout + faction element) to the live player rig.</summary>
        private void ApplyLoadedCharacter()
        {
            var inv = PlayerInventory.Instance;
            if (inv == null) return;
            if (player != null && inv.Loadout != null) player.SetLoadout(inv.Loadout);
            if (player != null)
            {
                var fm = player.GetComponent<FactionMember>();
                if (fm == null) fm = player.gameObject.AddComponent<FactionMember>();
                fm.Configure(Faction.Player,
                    inv.Loadout != null && inv.Loadout.IsChanneler ? inv.PlayerElement : (Element?)null);
            }
            Result = inv.Loadout != null
                ? new CharacterCreationResult(inv.Loadout, inv.RevealTier, inv.PlayerElement ?? default)
                : (CharacterCreationResult?)null;
        }

        // ---- stages ------------------------------------------------------------------------

        private void EnterMenu()
        {
            Current = Stage.Menu;
            GatePlayer(false);

            var save = SaveSystem.Load();
            bool hasSave = save != null && save.created;

            _menuGo = new GameObject("MainMenu");
            _menuGo.transform.SetParent(transform, false);
            _menuGo.AddComponent<MainMenuController>()
                .Show(hasSave, NewGame, ContinueGame, OpenSettings, QuitGame);

            Debug.Log("[Elementborn] Flow: main menu.");
        }

        // New Game starts a fresh character. An existing save on disk is left untouched until the new
        // character saves over it (a confirm-before-overwrite prompt is a later polish item).
        private void NewGame()
        {
            if (_menuGo != null) Destroy(_menuGo);
            _firstRunTutorial = true;
            EnterCreation();
        }

        private void ContinueGame()
        {
            if (_menuGo != null) Destroy(_menuGo);
            _firstRunTutorial = false;
            var save = SaveSystem.Load();
            if (save != null && save.created && PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.LoadFrom(save);
                ApplyLoadedCharacter();
                EnterMap();
            }
            else EnterCreation(); // no valid save to resume — fall back to a fresh start
        }

        private void OpenSettings() => SettingsController.EnsureInstance().Open();

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnterCreation()
        {
            Current = Stage.Creation;
            GatePlayer(false);

            _creationGo = new GameObject("CharacterCreation");
            _creationGo.transform.SetParent(transform, false);

            var controller = _creationGo.AddComponent<CharacterCreationController>();
            controller.Bind(player, weaponHolder);
            controller.OnResult.AddListener(r =>
            {
                Result = r;
                if (player != null)
                {
                    var fm = player.GetComponent<FactionMember>();
                    if (fm == null) fm = player.gameObject.AddComponent<FactionMember>();
                    fm.Configure(Faction.Player,
                        r.Loadout != null && r.Loadout.IsChanneler ? r.ChosenElement : (Element?)null);
                }

                if (PlayerInventory.Instance != null)
                {
                    PlayerInventory.Instance.PlayerElement =
                        r.Loadout != null && r.Loadout.IsChanneler ? r.ChosenElement : (Element?)null;
                    PlayerInventory.Instance.PlayerIsConfluence = r.Loadout != null && r.Loadout.IsConfluence;
                    PlayerInventory.Instance.Loadout = r.Loadout;
                    PlayerInventory.Instance.RevealTier = r.Tier;
                    PlayerInventory.Instance.CharacterCreated = true;
                }
            });
            _creationGo.AddComponent<CharacterCreationUI>().OnComplete.AddListener(EnterMap);

            Debug.Log("[Elementborn] Flow: character creation.");
        }

        private void EnterMap()
        {
            Current = Stage.Map;
            GatePlayer(false); // the map is a menu: disable the rig and free the cursor so its buttons are clickable
            if (_creationGo != null) Destroy(_creationGo);

            _mapGo = new GameObject("WorldMap");
            _mapGo.transform.SetParent(transform, false);

            var controller = _mapGo.AddComponent<WorldMapController>();
            World = controller.Configure(worldSeed, regionCount);
            _mapGo.AddComponent<WorldMapView>().EnterWorldRequested += EnterWorld;

            Debug.Log("[Elementborn] Flow: world map.");
        }

        private void EnterWorld(WorldRegion start)
        {
            StartCoroutine(EnterWorldRoutine(start));
        }

        private System.Collections.IEnumerator EnterWorldRoutine(WorldRegion start)
        {
            Current = Stage.World;
            StartRegion = start;
            if (_mapGo != null) Destroy(_mapGo);

            LoadingScreen.Instance?.Show(Localization.T("hud.loadingWorld"));
            yield return null; // let the loading overlay render before the blocking build
            yield return null;

            GatePlayer(true);
            if (World != null)
            {
                if (terrainBuilder != null) terrainBuilder.Build(World);
                LoadingScreen.Instance?.SetProgress(0.4f); yield return null;
                if (meshTerrainBuilder != null) meshTerrainBuilder.Build(World);
                LoadingScreen.Instance?.SetProgress(0.7f); yield return null;
                if (structurePlacer != null) structurePlacer.Place(World);
                LoadingScreen.Instance?.SetProgress(0.88f); yield return null;
                if (spawnPlacer != null) spawnPlacer.Place(World);
                LoadingScreen.Instance?.SetProgress(1f); yield return null;
            }

            // Cricket wakes with the player and stays for the whole game; the tutorial runs on a fresh start.
            new GameObject("Cricket").AddComponent<CricketCompanion>().Spawn(_firstRunTutorial);
            yield return null;

            LoadingScreen.Instance?.Hide();
            Debug.Log($"[Elementborn] Flow: entering world at {(start != null ? start.Name : "(none)")}.");
        }

        // ---- helpers -----------------------------------------------------------------------

        private void GatePlayer(bool playing)
        {
            if (player != null) player.enabled = playing;
            if (rigMovement != null) rigMovement.enabled = playing;
            Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !playing;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.transform.SetParent(transform, false);
        }
    }
}
