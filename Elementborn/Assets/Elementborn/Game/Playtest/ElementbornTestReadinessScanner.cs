using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Elementborn.Game
{
    public sealed class ElementbornTestReadinessScanner : MonoBehaviour
    {
        public static ElementbornTestReadinessScanner Instance { get; private set; }

        [SerializeField] private bool includeWarningsForOptionalSystems = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static ElementbornTestReadinessScanner Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(ElementbornTestReadinessScanner));
            return go.AddComponent<ElementbornTestReadinessScanner>();
        }

        public ElementbornTestReadinessReport ScanCurrentScene()
        {
            var report = new ElementbornTestReadinessReport();

            CheckObject(report, "Player", FindPlayer() != null, ElementbornTestReadinessSeverity.Error, "No Player tagged object found.", "Run Build Rounded Playable Scene or Create Player Test Rig.");
            CheckObject(report, "Camera", Camera.main != null, ElementbornTestReadinessSeverity.Error, "No MainCamera found.", "Create a camera tagged MainCamera.");
            CheckObject(report, "EventSystem", ElementbornFindUtility.FindFirst<EventSystem>() != null, ElementbornTestReadinessSeverity.Warning, "No EventSystem found.", "Run Unity Setup → Ensure EventSystem In Open Scene.");

            CheckComponent<ElementbornRuntimeBootstrap>(report, "Runtime Bootstrap", ElementbornTestReadinessSeverity.Warning, "Runtime bootstrap missing.", "Run Playable Setup → Create Runtime Systems Bootstrap.");
            CheckComponent<ElementbornMainGameplayLoopDirector>(report, "Gameplay Loop", ElementbornTestReadinessSeverity.Error, "Main gameplay loop director missing.", "Run Gameplay Loop → Install Gameplay Loop In Open Scene.");
            CheckComponent<ElementbornSpawnRegistry>(report, "Spawn Registry", ElementbornTestReadinessSeverity.Error, "Spawn registry missing.", "Run Gameplay Loop → Install Spawn Points In Open Scene.");
            CheckComponent<StorySystemsDebugDashboard>(report, "Story Dashboard", ElementbornTestReadinessSeverity.Warning, "Story systems dashboard missing.", "Run Debug → Install Story Systems Debug Dashboard.");
            CheckComponent<AdminWristPanelView>(report, "Left Wrist Admin UI", ElementbornTestReadinessSeverity.Warning, "Left wrist admin UI missing.", "Run Admin UI → Install Left Wrist Admin UI In Open Scene.");
            CheckComponent<ElementbornPlaytestHarnessPanel>(report, "Playtest Harness", ElementbornTestReadinessSeverity.Warning, "Playtest harness panel missing.", "Run Playtest → Install Test Harness In Open Scene.");

            CheckComponent<CapitalWorldStateTracker>(report, "Capital World State", ElementbornTestReadinessSeverity.Warning, "Capital state tracker missing.", "Run World State → Install Capital World State In Open Scene.");
            CheckComponent<PoliticalWorldEventDirector>(report, "Political Event Director", ElementbornTestReadinessSeverity.Warning, "Political event director missing.", "Run World State → Install Political World Event Director In Open Scene.");
            CheckComponent<QuestChainDirector>(report, "Quest Chains", ElementbornTestReadinessSeverity.Warning, "Quest chain director missing.", "Run Quest Chains → Install Quest Chain Director In Open Scene.");
            CheckComponent<FireCapitalRegistry>(report, "Fire Capital", ElementbornTestReadinessSeverity.Warning, "Fire Capital registry missing.", "Run Fire Capital → Install Fire Capital Systems In Open Scene.");
            CheckComponent<SocialGroupRegistry>(report, "Social Groups", ElementbornTestReadinessSeverity.Warning, "Social group registry missing.", "Run Social NPCs → Install Social Group Registry In Open Scene.");
            CheckComponent<CreatureOrphanageRecoveryRegistry>(report, "Creature Orphanage", ElementbornTestReadinessSeverity.Warning, "Creature orphanage recovery registry missing.", "Run Save → Install Narrative Runtime Save Bridges or Playable Setup.");
            CheckComponent<TimedDualLeaderPackRespawnController>(report, "Wolf Pack", includeWarningsForOptionalSystems ? ElementbornTestReadinessSeverity.Warning : ElementbornTestReadinessSeverity.Info, "Wolf-pack controller missing.", "Run Playable Setup → Create Wolf Pack Encounter Demo.");

            int spawnCount = ElementbornFindUtility.FindAll<ElementbornSpawnPoint>().Length;
            if (spawnCount <= 0)
            {
                report.Add(new ElementbornTestReadinessIssue(ElementbornTestReadinessSeverity.Error, "Spawn Points", "No ElementbornSpawnPoint components found.", "Run Gameplay Loop → Install Spawn Points In Open Scene."));
            }
            else
            {
                report.Add(new ElementbornTestReadinessIssue(ElementbornTestReadinessSeverity.Info, "Spawn Points", $"{spawnCount} spawn point(s) detected."));
            }

            int landmarks = ElementbornFindUtility.FindAll<CapitalLandmarkDescriptor>().Length;
            if (landmarks <= 0)
            {
                report.Add(new ElementbornTestReadinessIssue(ElementbornTestReadinessSeverity.Warning, "Capital Landmarks", "No capital landmark descriptors found.", "Run Capitals → Install Capital Landmarks In Open Scene."));
            }
            else
            {
                report.Add(new ElementbornTestReadinessIssue(ElementbornTestReadinessSeverity.Info, "Capital Landmarks", $"{landmarks} landmark(s) detected."));
            }

            return report;
        }

        public string WritePersistentReport()
        {
            ElementbornTestReadinessReport report = ScanCurrentScene();
            string directory = Path.Combine(Application.persistentDataPath, "test_readiness");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "Elementborn_TestReadinessReport.md");
            File.WriteAllText(path, report.ToMarkdown());
            Debug.Log("Wrote test readiness report: " + path);
            return path;
        }

        private void CheckComponent<T>(ElementbornTestReadinessReport report, string area, ElementbornTestReadinessSeverity severity, string missingMessage, string fix) where T : Object
        {
            CheckObject(report, area, ElementbornFindUtility.FindFirst<T>() != null, severity, missingMessage, fix);
        }

        private void CheckObject(ElementbornTestReadinessReport report, string area, bool exists, ElementbornTestReadinessSeverity severity, string missingMessage, string fix)
        {
            if (exists)
            {
                report.Add(new ElementbornTestReadinessIssue(ElementbornTestReadinessSeverity.Info, area, "OK"));
                return;
            }

            report.Add(new ElementbornTestReadinessIssue(severity, area, missingMessage, fix));
        }

        private GameObject FindPlayer()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("Player");
            }
            catch
            {
                return GameObject.Find("Player Test Rig");
            }
        }
    }
}
