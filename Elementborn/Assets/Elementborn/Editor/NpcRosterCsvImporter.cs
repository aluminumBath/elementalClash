#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class NpcRosterCsvImporter
    {
        private const string RosterDir = "Assets/Elementborn/Generated/NPC/Rosters";
        private const string OutputDir = "Assets/Elementborn/Generated/NPC/WorldEntries";

        [MenuItem("Elementborn/NPC Tools/Import Royal Family NPC Roster Template")]
        public static void ImportRoyalTemplate()
        {
            ImportCsv($"{RosterDir}/royal_family_npcs_template.csv");
        }

        [MenuItem("Elementborn/NPC Tools/Import Villain NPC Roster Template")]
        public static void ImportVillainTemplate()
        {
            ImportCsv($"{RosterDir}/villain_npcs_template.csv");
        }

        [MenuItem("Elementborn/NPC Tools/Import All NPC Roster CSVs")]
        public static void ImportAllRosters()
        {
            Directory.CreateDirectory(OutputDir);
            foreach (string path in Directory.GetFiles(RosterDir, "*.csv", SearchOption.TopDirectoryOnly))
            {
                ImportCsv(path);
            }
        }

        public static void ImportCsv(string csvPath)
        {
            Directory.CreateDirectory(OutputDir);

            if (!File.Exists(csvPath))
            {
                Debug.LogWarning($"NPC roster CSV not found: {csvPath}");
                return;
            }

            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length <= 1)
            {
                Debug.LogWarning($"NPC roster CSV had no data rows: {csvPath}");
                return;
            }

            var headerMap = BuildHeaderMap(lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                List<string> cells = SplitCsvLine(lines[i]);
                string npcId = Cell(cells, headerMap, "npcId", $"npc_{i}");
                string displayName = Cell(cells, headerMap, "displayName", npcId);
                string roleText = Cell(cells, headerMap, "role", "Unknown");
                string title = Cell(cells, headerMap, "titleOrRank", "");
                string region = Cell(cells, headerMap, "region", "");
                string location = Cell(cells, headerMap, "locationName", "");
                float x = ParseFloat(Cell(cells, headerMap, "x", "0"));
                float y = ParseFloat(Cell(cells, headerMap, "y", "0"));
                float z = ParseFloat(Cell(cells, headerMap, "z", "0"));
                string primaryElement = Cell(cells, headerMap, "primaryElement", "");
                string secondaryElement = Cell(cells, headerMap, "secondaryElement", "");
                string aliases = Cell(cells, headerMap, "aliases", "");
                string origin = Cell(cells, headerMap, "origin", "");
                string appearanceNotes = Cell(cells, headerMap, "appearanceNotes", "");
                string personalityNotes = Cell(cells, headerMap, "personalityNotes", "");
                string relationshipSummary = Cell(cells, headerMap, "relationshipSummary", "");
                string notes = Cell(cells, headerMap, "notes", "");
                string factionText = Cell(cells, headerMap, "faction", "NeutralCity");

                if (!Enum.TryParse(roleText, true, out NpcWorldRole role))
                {
                    role = NpcWorldRole.Unknown;
                }

                if (!Enum.TryParse(factionText, true, out ElementbornFactionId faction))
                {
                    faction = ElementbornFactionId.NeutralCity;
                }

                CreateNpc(
                    npcId,
                    displayName,
                    role,
                    title,
                    region,
                    location,
                    new Vector3(x, y, z),
                    aliases,
                    primaryElement,
                    secondaryElement,
                    origin,
                    appearanceNotes,
                    personalityNotes,
                    relationshipSummary,
                    notes,
                    faction);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Imported NPC roster: {csvPath}");
        }

        private static void CreateNpc(
            string id,
            string displayName,
            NpcWorldRole role,
            string title,
            string region,
            string location,
            Vector3 position,
            string aliases,
            string primaryElement,
            string secondaryElement,
            string origin,
            string appearanceNotes,
            string personalityNotes,
            string relationshipSummary,
            string notes,
            ElementbornFactionId faction)
        {
            string path = $"{OutputDir}/{Sanitize(id)}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<NpcWorldEntryDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<NpcWorldEntryDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("npcId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("role").enumValueIndex = (int)role;
            so.FindProperty("titleOrRank").stringValue = title;
            so.FindProperty("region").stringValue = region;
            so.FindProperty("locationName").stringValue = location;
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("aliases").stringValue = aliases;
            so.FindProperty("primaryElement").stringValue = primaryElement;
            so.FindProperty("secondaryElement").stringValue = secondaryElement;
            so.FindProperty("origin").stringValue = origin;
            so.FindProperty("appearanceNotes").stringValue = appearanceNotes;
            so.FindProperty("personalityNotes").stringValue = personalityNotes;
            so.FindProperty("relationshipSummary").stringValue = relationshipSummary;
            so.FindProperty("notes").stringValue = notes;
            so.FindProperty("faction").enumValueIndex = (int)faction;
            so.FindProperty("defaultVoiceSound").enumValueIndex =
                role == NpcWorldRole.RoyalFamily ? (int)ElementbornSoundEventId.NpcVoiceRoyal :
                role == NpcWorldRole.Villain ? (int)ElementbornSoundEventId.NpcVoiceVillain :
                role == NpcWorldRole.Guard ? (int)ElementbornSoundEventId.NpcVoiceGuard :
                (int)ElementbornSoundEventId.NpcVoiceWarm;

            var voiceLines = so.FindProperty("voiceLines");
            voiceLines.arraySize = 1;
            var line = voiceLines.GetArrayElementAtIndex(0);
            line.FindPropertyRelative("LineType").enumValueIndex = (int)NpcVoiceLineType.Greeting;
            line.FindPropertyRelative("Subtitle").stringValue = $"Greetings. I am {displayName}.";
            line.FindPropertyRelative("FutureVoiceActorClipName").stringValue = $"{Sanitize(id)}_greeting.wav";

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private static Dictionary<string, int> BuildHeaderMap(string headerLine)
        {
            List<string> headers = SplitCsvLine(headerLine);
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                string header = headers[i].Trim();
                if (!map.ContainsKey(header))
                {
                    map.Add(header, i);
                }
            }
            return map;
        }

        private static List<string> SplitCsvLine(string line)
        {
            var cells = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    cells.Add(current.ToString().Trim());
                    current.Length = 0;
                }
                else
                {
                    current.Append(c);
                }
            }

            cells.Add(current.ToString().Trim());
            return cells;
        }

        private static string Cell(List<string> cells, Dictionary<string, int> headerMap, string key, string fallback)
        {
            if (headerMap.TryGetValue(key, out int index) && index >= 0 && index < cells.Count)
            {
                string value = cells[index].Trim();
                return string.IsNullOrWhiteSpace(value) ? fallback : value;
            }
            return fallback;
        }

        private static float ParseFloat(string value)
        {
            return float.TryParse(value, out float result) ? result : 0f;
        }

        private static string Sanitize(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }

            return value.Replace(" ", "_").ToLowerInvariant();
        }
    }
}
#endif
