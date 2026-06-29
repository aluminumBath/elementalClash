#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class CapitalWorldStateAssetGenerator
    {
        private const string CapitalDir = "Assets/Elementborn/Generated/WorldState/Capitals";
        private const string EventDir = "Assets/Elementborn/Generated/WorldState/Events";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/WorldState";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/World State/Generate Capital World State Assets")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(CapitalDir);
            Directory.CreateDirectory(EventDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            CreateCapital(CapitalId.EarthCapital, "Earth Capital", ElementbornFactionId.EarthChannelers, CapitalControlStatus.StableRule, new Vector3(40f, 0f, 22f),
                "Queen Alexis Malachite and Chrief Gover provide kind, intelligent, stabilizing rule.",
                new[] { (CapitalPressureType.RoyalFamilyStability, 80, "The Malachite-Gover family stabilizes the court."), (CapitalPressureType.Unrest, 15, "Public unrest is low."), (CapitalPressureType.ChannelerTension, 20, "Tibidi's plant channeling may matter later.") });

            CreateCapital(CapitalId.MetalCapital, "Metal Capital", ElementbornFactionId.MetalChannelers, CapitalControlStatus.ShadowControlled, new Vector3(610f, 0f, 128f),
                "Queen Rhonda and King Randy's laissez-faire rule allows the black market and thieves guild to flourish.",
                new[] { (CapitalPressureType.BlackMarketPressure, 75, "The black market is a defining force."), (CapitalPressureType.ThievesGuildInfluence, 70, "The thieves guild has deep influence."), (CapitalPressureType.HiddenThreat, 60, "Howlj and Larissa's secret could explode.") });

            CreateCapital(CapitalId.WindCapital, "Wind Capital", ElementbornFactionId.WindTheocracy, CapitalControlStatus.Usurped, new Vector3(720f, 25f, 420f),
                "High Priest Redbeard and Priestess Lizkota rule after a recent theocratic usurpation, while locals like Rekr, Manon, Johna, Amy, and Marie keep the lower terraces alive with rumor, smoke, and domestic chaos.",
                new[] { (CapitalPressureType.ReligiousFervor, 80, "The capital is chaotic with religious fervor."), (CapitalPressureType.RulerLegitimacy, 30, "The regime is powerful but not broadly legitimate."), (CapitalPressureType.HiddenThreat, 75, "Sarah's past, Ruth's steam omen, and neighborhood tensions are volatile.") });

            CreateCapital(CapitalId.NerithaReefwood, "Neritha Reefwood", ElementbornFactionId.ReefwoodVillagers, CapitalControlStatus.Contested, new Vector3(265f, 0f, 338f),
                "Neritha Reefwood is shaped by reefborn diplomacy, pirate routes, and the Raucous Tide.",
                new[] { (CapitalPressureType.PirateActivity, 70, "The Raucous Tide shapes local politics."), (CapitalPressureType.EconomicStrain, 35, "Raid economics and reef trade collide."), (CapitalPressureType.FactionControl, 45, "Villagers, pirates, and water channelers compete.") });

            CreateCapital(CapitalId.FireCapital, "Fire Capital", ElementbornFactionId.FireChannelers, CapitalControlStatus.Unknown, new Vector3(520f, 0f, -210f),
                "The Fire Capital still needs named rulers and broad pressure hooks, but locals like Kelly and visiting fire-channelers like Marie already create plenty of neighborhood drama.",
                new[] { (CapitalPressureType.Unrest, 35, "Neighborhood flare-ups still occur while leadership remains undefined."), (CapitalPressureType.ChannelerTension, 40, "Kelly tempers some conflicts, but fire politics still need rulers and factions.") });

            CreateCapital(CapitalId.NeutralCentralCity, "Neutral Central City", ElementbornFactionId.NeutralCity, CapitalControlStatus.Contested, new Vector3(0f, 0f, 0f),
                "The central city is a convergence point for all elemental politics.",
                new[] { (CapitalPressureType.FactionControl, 50, "All factions want influence in the center."), (CapitalPressureType.HiddenThreat, 45, "Cross-capital secrets move through neutral channels.") });

            CreateEvent("wind_sermon_surge", "Redbeard's Sermon Surge", CapitalId.WindCapital,
                new[] { (CapitalPressureType.ReligiousFervor, 10, "A sermon draws more faithful into the streets."), (CapitalPressureType.Unrest, 5, "Crowds overwhelm skybridge patrols.") }, -5, -3);

            CreateEvent("metal_guild_crackdown", "Metal Guild Crackdown", CapitalId.MetalCapital,
                new[] { (CapitalPressureType.ThievesGuildInfluence, -10, "A raid disrupts guild routes."), (CapitalPressureType.BlackMarketPressure, -5, "Vendors go quiet."), (CapitalPressureType.Unrest, 8, "The black market resents interference.") }, 3, 2);

            CreateEvent("earth_court_audience", "Earth Court Audience", CapitalId.EarthCapital,
                new[] { (CapitalPressureType.RoyalFamilyStability, 5, "Alexis and Chrief reassure the court."), (CapitalPressureType.Unrest, -5, "Public confidence improves.") }, 5, 5);

            CreateEvent("raucous_tide_raid_news", "Raucous Tide Raid News", CapitalId.NerithaReefwood,
                new[] { (CapitalPressureType.PirateActivity, 8, "The Raucous Tide celebrates another raid."), (CapitalPressureType.EconomicStrain, 4, "Trade routes adjust to pirate noise.") }, -2, 0);

            CreateEvent("wind_neighbor_gossip", "Wind Terrace Gossip Spiral", CapitalId.WindCapital,
                new[] { (CapitalPressureType.Unrest, 4, "Amy and Marie accidentally spread rumors through the lower terraces."), (CapitalPressureType.HiddenThreat, 3, "Small domestic scandals become public spectacle.") }, -1, 0);

            CreateEvent("wind_pipe_counsel", "Johna's Pipe Counsel", CapitalId.WindCapital,
                new[] { (CapitalPressureType.Unrest, -4, "Johna Rold calms worried locals with quiet advice."), (CapitalPressureType.ReligiousFervor, -2, "His grounded counsel steadies anxious neighbors.") }, 2, 1);

            CreateEvent("fire_sleeping_flare", "Marie's Sleeping Flare", CapitalId.FireCapital,
                new[] { (CapitalPressureType.Unrest, 6, "Marie Conflag nods off and sets something ablaze again."), (CapitalPressureType.ChannelerTension, 2, "Cleanup crews grumble about careless fire channeling.") }, -2, -1);

            CreateEvent("kelly_neighborhood_watch", "Kelly's Neighborhood Watch", CapitalId.FireCapital,
                new[] { (CapitalPressureType.Unrest, -5, "Kelly protects neighbors before tempers get out of hand."), (CapitalPressureType.ChannelerTension, -2, "Her kindness eases local strain.") }, 3, 1);

            CreatePrefab();
            WriteReport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated capital world-state assets.");
        }

        [MenuItem("Elementborn/World State/Install Capital World State In Open Scene")]
        public static void InstallSystems()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Capital World State");
            if (root == null)
            {
                root = new GameObject("Elementborn Capital World State");
            }

            var tracker = root.GetComponent<CapitalWorldStateTracker>();
            if (tracker == null)
            {
                tracker = root.AddComponent<CapitalWorldStateTracker>();
            }

            tracker.SetDefinitions(LoadAllCapitals());

            if (root.GetComponent<CapitalWorldStateAdminCommandBridge>() == null)
            {
                root.AddComponent<CapitalWorldStateAdminCommandBridge>();
            }

            tracker.SyncRegionalSystems();
            EditorUtility.SetDirty(root);
            Debug.Log("Installed capital world-state systems in open scene.");
        }

        private static void CreateCapital(CapitalId id, string displayName, ElementbornFactionId faction, CapitalControlStatus status, Vector3 position, string summary, (CapitalPressureType type, int value, string notes)[] pressures)
        {
            string path = $"{CapitalDir}/{id}.asset";
            var capital = AssetDatabase.LoadAssetAtPath<CapitalWorldStateDefinition>(path);
            if (capital == null)
            {
                capital = ScriptableObject.CreateInstance<CapitalWorldStateDefinition>();
                AssetDatabase.CreateAsset(capital, path);
            }

            var so = new SerializedObject(capital);
            so.FindProperty("capitalId").enumValueIndex = (int)id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("rulingFaction").enumValueIndex = (int)faction;
            so.FindProperty("controlStatus").enumValueIndex = (int)status;
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("summary").stringValue = summary;

            var pressureArray = so.FindProperty("startingPressures");
            pressureArray.arraySize = pressures.Length;
            for (int i = 0; i < pressures.Length; i++)
            {
                var entry = pressureArray.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Type").enumValueIndex = (int)pressures[i].type;
                entry.FindPropertyRelative("Value").intValue = pressures[i].value;
                entry.FindPropertyRelative("Notes").stringValue = pressures[i].notes;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(capital);
        }

        private static void CreateEvent(string id, string displayName, CapitalId capital, (CapitalPressureType type, int delta, string reason)[] changes, int stabilityDelta, int legitimacyDelta)
        {
            string path = $"{EventDir}/{id}.asset";
            var evt = AssetDatabase.LoadAssetAtPath<CapitalPressureEventDefinition>(path);
            if (evt == null)
            {
                evt = ScriptableObject.CreateInstance<CapitalPressureEventDefinition>();
                AssetDatabase.CreateAsset(evt, path);
            }

            var so = new SerializedObject(evt);
            so.FindProperty("eventId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("targetCapital").enumValueIndex = (int)capital;
            so.FindProperty("stabilityDelta").intValue = stabilityDelta;
            so.FindProperty("legitimacyDelta").intValue = legitimacyDelta;
            so.FindProperty("journalText").stringValue = displayName + " changed the political pressure in " + capital + ".";
            so.FindProperty("notifyPlayer").boolValue = true;

            var pressureChanges = so.FindProperty("pressureChanges");
            pressureChanges.arraySize = changes.Length;
            for (int i = 0; i < changes.Length; i++)
            {
                var entry = pressureChanges.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("PressureType").enumValueIndex = (int)changes[i].type;
                entry.FindPropertyRelative("Delta").intValue = changes[i].delta;
                entry.FindPropertyRelative("Reason").stringValue = changes[i].reason;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(evt);
        }

        private static void CreatePrefab()
        {
            GameObject go = new GameObject("Elementborn_CapitalWorldState");
            var tracker = go.AddComponent<CapitalWorldStateTracker>();
            tracker.SetDefinitions(LoadAllCapitals());
            go.AddComponent<CapitalWorldStateAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_CapitalWorldState.prefab");
            Object.DestroyImmediate(go);
        }

        private static List<CapitalWorldStateDefinition> LoadAllCapitals()
        {
            var results = new List<CapitalWorldStateDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:CapitalWorldStateDefinition", new[] { CapitalDir }))
            {
                var item = AssetDatabase.LoadAssetAtPath<CapitalWorldStateDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null)
                {
                    results.Add(item);
                }
            }
            return results;
        }

        private static void WriteReport()
        {
            string path = $"{ReportDir}/CapitalWorldStateReport.md";
            File.WriteAllText(path,
@"# Capital World State Report

Generated by v34.

## Capitals

```text
Earth Capital
Metal Capital
Wind Capital
Neritha Reefwood
Fire Capital
Neutral Central City
```

## Runtime commands

```text
world.capitals
world.capital CapitalId
world.sync
world.pressure CapitalId|PressureType|delta
world.legitimacy CapitalId|delta
world.stability CapitalId|delta
```
");
        }
    }
}
#endif
