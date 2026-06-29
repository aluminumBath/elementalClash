#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class PoliticalWorldEventAssetGenerator
    {
        private const string EventDir = "Assets/Elementborn/Generated/WorldState/PoliticalEvents";
        private const string PrefabDir = "Assets/Elementborn/Generated/Prefabs/WorldState";
        private const string ReportDir = "Assets/Elementborn/Generated/Reports";

        [MenuItem("Elementborn/World State/Generate Political World Events")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(EventDir);
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(ReportDir);

            CapitalWorldStateAssetGenerator.GenerateAll();

            CreateEvent(
                "wind_capital_riot",
                "Skybridge Riot",
                PoliticalWorldEventCategory.Riot,
                CapitalId.WindCapital,
                new Vector3(720f, 25f, 420f),
                new[] { (CapitalPressureType.ReligiousFervor, 75, true), (CapitalPressureType.Unrest, 55, true) },
                new[] { (CapitalPressureType.Unrest, 10, -8, -4, "Religious crowds surge across the skybridges.") },
                "Religious fervor in the Wind Capital spills into a skybridge riot.",
                "Can lead to Redbeard tightening theocracy control or Sarah finding old-regime allies.",
                ElementbornSoundEventId.BossPhase,
                true);

            CreateEvent(
                "metal_black_market_surge",
                "Black-Market Surge",
                PoliticalWorldEventCategory.MarketShift,
                CapitalId.MetalCapital,
                new Vector3(602f, 0f, 122f),
                new[] { (CapitalPressureType.BlackMarketPressure, 70, true) },
                new[] { (CapitalPressureType.ThievesGuildInfluence, 8, -3, -2, "The market floods with forbidden metalwork.") },
                "Black-market activity in the Metal Capital intensifies.",
                "Can expose Bubba's prototype, Larissa's secret, or a thieves guild debt chain.",
                ElementbornSoundEventId.UiTick,
                false);

            CreateEvent(
                "larissa_secret_blackmail",
                "Larissa Blackmail Attempt",
                PoliticalWorldEventCategory.HiddenThreat,
                CapitalId.MetalCapital,
                new Vector3(592f, 0f, 120f),
                new[] { (CapitalPressureType.HiddenThreat, 55, true), (CapitalPressureType.AntiChannelerSentiment, 40, true) },
                new[] { (CapitalPressureType.HiddenThreat, 12, -4, -3, "Someone moves to expose Larissa's metal channeling.") },
                "A blackmailer threatens to expose Larissa's secret metal channeling.",
                "Howlj's hatred can turn this from personal danger into political violence.",
                ElementbornSoundEventId.NpcVoiceVillain,
                true);

            CreateEvent(
                "raucous_tide_port_emergency",
                "Raucous Tide Port Emergency",
                PoliticalWorldEventCategory.PirateIncident,
                CapitalId.NerithaReefwood,
                new Vector3(266f, 0f, 340f),
                new[] { (CapitalPressureType.PirateActivity, 65, true) },
                new[] { (CapitalPressureType.PirateActivity, 7, -2, 0, "The Raucous Tide drags a port dispute into a festival.") },
                "A Raucous Tide celebration risks turning into a port-wide incident.",
                "A useful bridge between Ramón, Sarah, and regional politics.",
                ElementbornSoundEventId.BoatWaveCreak,
                false);

            CreateEvent(
                "earth_court_stabilizes",
                "Malachite Court Stabilizes",
                PoliticalWorldEventCategory.CourtAudience,
                CapitalId.EarthCapital,
                new Vector3(42f, 0f, 24f),
                new[] { (CapitalPressureType.RoyalFamilyStability, 60, true) },
                new[] { (CapitalPressureType.Unrest, -8, 5, 5, "Alexis and Chrief calm the court.") },
                "The Malachite court holds a stabilizing audience.",
                "A positive counterweight to other capitals collapsing into pressure.",
                ElementbornSoundEventId.QuestComplete,
                false);

            CreateEvent(
                "neutral_cult_whispers",
                "Neutral City Cult Whispers",
                PoliticalWorldEventCategory.CultActivity,
                CapitalId.NeutralCentralCity,
                new Vector3(0f, 0f, 0f),
                new[] { (CapitalPressureType.HiddenThreat, 40, true) },
                new[] { (CapitalPressureType.HiddenThreat, 8, -3, -2, "Cross-capital rumors feed cult recruitment.") },
                "Whispers from multiple capitals converge in the neutral city.",
                "A future hook for unification groups, supremacist factions, or cult conspiracies.",
                ElementbornSoundEventId.NpcVoiceWarm,
                false);

            CreatePrefab();
            WriteReport();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated political world events.");
        }

        [MenuItem("Elementborn/World State/Install Political World Event Director In Open Scene")]
        public static void InstallDirector()
        {
            GenerateAll();

            GameObject root = GameObject.Find("Elementborn Political World Event Director");
            if (root == null)
            {
                root = new GameObject("Elementborn Political World Event Director");
            }

            var director = root.GetComponent<PoliticalWorldEventDirector>();
            if (director == null)
            {
                director = root.AddComponent<PoliticalWorldEventDirector>();
            }

            director.SetEvents(LoadAllEvents());

            if (root.GetComponent<PoliticalWorldEventAdminCommandBridge>() == null)
            {
                root.AddComponent<PoliticalWorldEventAdminCommandBridge>();
            }

            director.EvaluateAll();
            EditorUtility.SetDirty(root);
            Debug.Log("Installed political world event director in open scene.");
        }
        private static void CreateEvent(
            string id,
            string title,
            PoliticalWorldEventCategory category,
            CapitalId capital,
            Vector3 position,
            (CapitalPressureType type, int threshold, bool atOrAbove)[] conditions,
            (CapitalPressureType type, int pressureDelta, int stabilityDelta, int legitimacyDelta, string reason)[] consequences,
            string summary,
            string notes,
            ElementbornSoundEventId sound,
            bool autoActivate)
        {
            string path = $"{EventDir}/{id}.asset";
            var evt = AssetDatabase.LoadAssetAtPath<PoliticalWorldEventDefinition>(path);
            if (evt == null)
            {
                evt = ScriptableObject.CreateInstance<PoliticalWorldEventDefinition>();
                AssetDatabase.CreateAsset(evt, path);
            }

            var so = new SerializedObject(evt);
            so.FindProperty("eventId").stringValue = id;
            so.FindProperty("displayName").stringValue = title;
            so.FindProperty("category").enumValueIndex = (int)category;
            so.FindProperty("primaryCapital").enumValueIndex = (int)capital;
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("sound").enumValueIndex = (int)sound;
            so.FindProperty("cooldownDays").intValue = 2;
            so.FindProperty("autoActivateWhenEligible").boolValue = autoActivate;
            so.FindProperty("createMapMarker").boolValue = true;
            so.FindProperty("playerFacingSummary").stringValue = summary;
            so.FindProperty("hiddenDirectorNotes").stringValue = notes;

            var conditionArray = so.FindProperty("conditions");
            conditionArray.arraySize = conditions.Length;
            for (int i = 0; i < conditions.Length; i++)
            {
                var condition = conditionArray.GetArrayElementAtIndex(i);
                condition.FindPropertyRelative("Capital").enumValueIndex = (int)capital;
                condition.FindPropertyRelative("PressureType").enumValueIndex = (int)conditions[i].type;
                condition.FindPropertyRelative("MinimumValue").intValue = conditions[i].threshold;
                condition.FindPropertyRelative("RequireAtOrAbove").boolValue = conditions[i].atOrAbove;
            }

            var consequenceArray = so.FindProperty("consequences");
            consequenceArray.arraySize = consequences.Length;
            for (int i = 0; i < consequences.Length; i++)
            {
                var consequence = consequenceArray.GetArrayElementAtIndex(i);
                consequence.FindPropertyRelative("PressureType").enumValueIndex = (int)consequences[i].type;
                consequence.FindPropertyRelative("PressureDelta").intValue = consequences[i].pressureDelta;
                consequence.FindPropertyRelative("StabilityDelta").intValue = consequences[i].stabilityDelta;
                consequence.FindPropertyRelative("LegitimacyDelta").intValue = consequences[i].legitimacyDelta;
                consequence.FindPropertyRelative("Reason").stringValue = consequences[i].reason;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(evt);
        }

        private static void CreatePrefab()
        {
            GameObject go = new GameObject("Elementborn_PoliticalWorldEventDirector");
            var director = go.AddComponent<PoliticalWorldEventDirector>();
            director.SetEvents(LoadAllEvents());
            go.AddComponent<PoliticalWorldEventAdminCommandBridge>();
            PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/Elementborn_PoliticalWorldEventDirector.prefab");
            Object.DestroyImmediate(go);
        }

        private static List<PoliticalWorldEventDefinition> LoadAllEvents()
        {
            var results = new List<PoliticalWorldEventDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:PoliticalWorldEventDefinition", new[] { EventDir }))
            {
                var item = AssetDatabase.LoadAssetAtPath<PoliticalWorldEventDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (item != null)
                {
                    results.Add(item);
                }
            }
            return results;
        }

        private static void WriteReport()
        {
            string path = $"{ReportDir}/PoliticalWorldEventDirectorReport.md";
            File.WriteAllText(path,
@"# Political World Event Director Report

Generated by v35.

## Events

```text
Skybridge Riot
Black-Market Surge
Larissa Blackmail Attempt
Raucous Tide Port Emergency
Malachite Court Stabilizes
Neutral City Cult Whispers
```

## Runtime commands

```text
eventdir.summary
eventdir.eval
eventdir.day
eventdir.advance days
eventdir.activate eventId
eventdir.resolve eventId
```
");
        }
    }
}
#endif
