#if UNITY_EDITOR
using Elementborn.Game;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Elementborn.Game.EditorTools
{
    public static class ElementbornGeneratedAssetLibraryBuilder
    {
        private const string ExtractedRoot = "Assets/Elementborn/Art/Models/MeshyImported/AutoImported";
        private const string PrefabRoot = "Assets/Elementborn/Generated/Prefabs/ImportedModels/AutoImported";

        public struct Entry
        {
            public string SafeName;
            public string DisplayName;
            public string OldPattern;
            public string RenamedPattern;
            public string Role;
            public string Element;
            public ElementbornPrototypeModelAnimationMode AnimationMode;
            public float TargetHeight;
            public string Notes;

            public Entry(string safeName, string displayName, string oldPattern, string renamedPattern, string role, string element, ElementbornPrototypeModelAnimationMode animationMode, float targetHeight, string notes)
            {
                SafeName = safeName;
                DisplayName = displayName;
                OldPattern = oldPattern;
                RenamedPattern = renamedPattern;
                Role = role;
                Element = element;
                AnimationMode = animationMode;
                TargetHeight = targetHeight;
                Notes = notes;
            }
        }

        public static readonly Entry[] Entries = new Entry[]
        {
            new Entry("ChannelerHeroNone", "Channeler Hero None", "Meshy_AI_Channeler_Hero_None", "Channeler_Hero_None", "Character", "None", ElementbornPrototypeModelAnimationMode.Idle, 2.1f, "Player fallback / neutral channeler"),
            new Entry("ChannelerHeroFire", "Fire Channeler Hero", "Meshy_AI_Channeler_Hero_Fire", "Channeler_Hero_Fire", "Character", "Fire", ElementbornPrototypeModelAnimationMode.Idle, 2.1f, "Fire envoy / fire player visual"),
            new Entry("ChannelerHeroWater", "Water Channeler Hero", "Meshy_AI_Channeler_Hero_Water", "Channeler_Hero_Water", "Character", "Water", ElementbornPrototypeModelAnimationMode.Idle, 2.1f, "Water envoy"),
            new Entry("ChannelerHeroEarth", "Earth Channeler Hero", "Meshy_AI_Channeler_Hero_Earth", "Channeler_Hero_Earth", "Character", "Earth", ElementbornPrototypeModelAnimationMode.Idle, 2.1f, "Earth envoy"),
            new Entry("ChannelerHeroAir", "Air Channeler Hero", "Meshy_AI_Channeler_Hero_Air", "Channeler_Hero_Air", "Character", "Air", ElementbornPrototypeModelAnimationMode.Idle, 2.1f, "Air envoy"),
            new Entry("ChibiPirate", "Chibi Pirate", "Meshy_AI_Chibi_pirate", "Chibi_pirate", "Character", "Water", ElementbornPrototypeModelAnimationMode.Idle, 1.9f, "Water district / pirate NPC"),
            new Entry("CaptainGreenbeak", "Captain Greenbeak", "Meshy_AI_Captain_Greenbeak", "Captain_Greenbeak", "Character", "Air", ElementbornPrototypeModelAnimationMode.Idle, 1.9f, "Air/pirate NPC"),
            new Entry("PinkEyeAxolotl", "Pink Eye Axolotl", "Meshy_AI_Pink_Eye_Axolotl", "Pink_Eye_Axolotl", "Creature", "Water", ElementbornPrototypeModelAnimationMode.Swim, 2.2f, "Water hostile/showcase"),
            new Entry("Skyotter", "Skyotter", "Meshy_AI_Skyotter", "Skyotter", "Creature", "Air", ElementbornPrototypeModelAnimationMode.Hover, 1.8f, "Air creature / future companion"),
            new Entry("SteamFrogTwin", "Steam Frog Twin", "Meshy_AI_Steam_Frog_Twin", "Steam_Frog_Twin", "Creature", "Steam", ElementbornPrototypeModelAnimationMode.Hover, 1.4f, "Steam frog companion"),
            new Entry("HurricaneFrogTwin", "Hurricane Frog Twin", "Meshy_AI_Hurricane_Frog_Twin", "Hurricane_Frog_Twin", "Creature", "Air", ElementbornPrototypeModelAnimationMode.Hover, 1.4f, "Hurricane frog companion"),
            new Entry("Thunderbird", "Thunderbird", "Meshy_AI_Thunderbird", "Thunderbird", "Creature", "Air", ElementbornPrototypeModelAnimationMode.Hover, 2.5f, "Air boss/showcase"),
            new Entry("FirePhoenix", "Fire Phoenix", "Meshy_AI_Fire_Phoenix", "Fire_Phoenix", "Creature", "Fire", ElementbornPrototypeModelAnimationMode.Hover, 2.2f, "Fire creature/showcase"),
            new Entry("NineTailFox", "Nine Tail Fox", "Meshy_AI_Nine_Tail_Fox", "Nine_Tail_Fox", "Creature", "Plant", ElementbornPrototypeModelAnimationMode.Idle, 1.8f, "Forest creature/showcase"),
            new Entry("ShadowWolf", "Shadow Wolf", "Meshy_AI_Shadow_Wolf", "Shadow_Wolf", "Creature", "Shadow", ElementbornPrototypeModelAnimationMode.Combat, 1.8f, "Shadow hostile/showcase"),
            new Entry("StormWyvern", "Storm Wyvern", "Meshy_AI_Storm_Wyvern", "Storm_Wyvern", "Creature", "Air", ElementbornPrototypeModelAnimationMode.Hover, 2.4f, "Storm creature/showcase"),
            new Entry("TideglassHound", "Tideglass Hound", "Meshy_AI_Tideglass_Hound", "Tideglass_Hound", "Creature", "Water", ElementbornPrototypeModelAnimationMode.Combat, 1.8f, "Water hound future hostile"),
            new Entry("BlueDinoMount", "Blue Dino Mount", "Meshy_AI_Blue_Dino_Mount", "Blue_Dino_Mount", "Mount", "Water", ElementbornPrototypeModelAnimationMode.Idle, 2.3f, "Mount showcase"),
            new Entry("TreasureChest", "Treasure Chest", "Meshy_AI_Treasure_Chest", "Treasure_Chest", "Prop", "None", ElementbornPrototypeModelAnimationMode.Idle, 1.1f, "Chest replacement"),
            new Entry("PrismaticGate", "Prismatic Gate", "Meshy_AI_Prismatic_Gate", "Prismatic_Gate", "Prop", "None", ElementbornPrototypeModelAnimationMode.Idle, 3.2f, "Gate decoration/replacement"),
            new Entry("VineGate", "Vine Gate", "Meshy_AI_Vine_Gate", "Vine_Gate", "Prop", "Plant", ElementbornPrototypeModelAnimationMode.Idle, 3.0f, "Plant/earth gate decoration"),
            new Entry("AzureArcPortal", "Azure Arc Portal", "Meshy_AI_Azure_Arc_Portal", "Azure_Arc_Portal", "Prop", "Water", ElementbornPrototypeModelAnimationMode.Idle, 3.0f, "Water gate decoration"),
            new Entry("AncientGlyphMap", "Ancient Glyph Map", "Meshy_AI_Ancient_Glyph_Map", "Ancient_Glyph_Map", "Prop", "None", ElementbornPrototypeModelAnimationMode.Idle, 1.2f, "Lore stone/map replacement"),
            new Entry("LuminescentMushroom", "Luminescent Mushroom", "Meshy_AI_Luminescent_Mushroom", "Luminescent_Mushroom", "Prop", "Plant", ElementbornPrototypeModelAnimationMode.Idle, 1.2f, "Shrine/resource dressing"),
            new Entry("HealingTonic", "Healing Tonic", "Meshy_AI_Healing_Tonic", "Healing_Tonic", "Item", "Water", ElementbornPrototypeModelAnimationMode.Hover, 0.9f, "Healing shrine visual"),
            new Entry("StaminaDraught", "Stamina Draught", "Meshy_AI_Stamina_Draught", "Stamina_Draught", "Item", "Air", ElementbornPrototypeModelAnimationMode.Hover, 0.9f, "Stamina/rest shrine visual"),
            new Entry("VigorElixir", "Vigor Elixir", "Meshy_AI_Vigor_Elixir", "Vigor_Elixir", "Item", "Plant", ElementbornPrototypeModelAnimationMode.Hover, 0.9f, "Reward/consumable"),
            new Entry("PoisonVial", "Poison Vial", "Meshy_AI_Poison_Vial", "Poison_Vial", "Item", "Blood", ElementbornPrototypeModelAnimationMode.Hover, 0.9f, "Hazard consumable"),
            new Entry("Emberblade", "Emberblade", "Meshy_AI_Emberblade", "Emberblade", "Weapon", "Fire", ElementbornPrototypeModelAnimationMode.Hover, 1.2f, "Fire weapon showcase"),
            new Entry("CrimsonLance", "Crimson Lance", "Meshy_AI_Crimson_Lance", "Crimson_Lance", "Weapon", "Fire", ElementbornPrototypeModelAnimationMode.Hover, 1.4f, "Lance reward"),
            new Entry("GildedArcBow", "Gilded Arc Bow", "Meshy_AI_Gilded_Arc_Bow", "Gilded_Arc_Bow", "Weapon", "Air", ElementbornPrototypeModelAnimationMode.Hover, 1.3f, "Bow reward"),
            new Entry("StormcleaverAxe", "Stormcleaver Axe", "Meshy_AI_Stormcleaver_Axe", "Stormcleaver_Axe", "Weapon", "Air", ElementbornPrototypeModelAnimationMode.Hover, 1.3f, "Axe reward"),
            new Entry("StonebreakerHammer", "Stonebreaker Hammer", "Meshy_AI_Stonebreaker_Hammer", "Stonebreaker_Hammer", "Weapon", "Earth", ElementbornPrototypeModelAnimationMode.Hover, 1.3f, "Earth weapon reward"),
            new Entry("AuroraMagnolia", "Aurora Magnolia", "Meshy_AI_Aurora_Magnolia", "Aurora_Magnolia", "Prop", "Plant", ElementbornPrototypeModelAnimationMode.Idle, 1.2f, "Plant region dressing"),
        };

        [MenuItem("Elementborn/Assets/Build Generated Asset Library From Extracted FBXs")]
        public static void BuildGeneratedAssetLibraryFromExtractedFbxs()
        {
            EnsureFolder("Assets/Elementborn/Generated");
            EnsureFolder("Assets/Elementborn/Generated/Prefabs");
            EnsureFolder("Assets/Elementborn/Generated/Prefabs/ImportedModels");
            EnsureFolder(PrefabRoot);

            int built = 0;
            int missing = 0;

            for (int i = 0; i < Entries.Length; i++)
            {
                Entry entry = Entries[i];
                string fbxPath = FindFirstFbxForEntry(entry);
                if (string.IsNullOrWhiteSpace(fbxPath))
                {
                    missing++;
                    continue;
                }

                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (modelAsset == null)
                {
                    missing++;
                    continue;
                }

                BuildPrefab(entry, fbxPath, modelAsset);
                built++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Elementborn generated asset library build complete. Built=" + built + " Missing=" + missing + " Output=" + PrefabRoot);
        }

        [MenuItem("Elementborn/Assets/Report Generated Asset Library Matches")]
        public static void ReportGeneratedAssetLibraryMatches()
        {
            int extracted = 0;
            int prefabs = 0;
            string report = "Elementborn generated asset library fuzzy-match report";

            for (int i = 0; i < Entries.Length; i++)
            {
                Entry entry = Entries[i];
                string fbxPath = FindFirstFbxForEntry(entry);
                string prefabPath = GetPrefabPath(entry);
                bool hasFbx = !string.IsNullOrWhiteSpace(fbxPath);
                bool hasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;

                if (hasFbx) extracted++;
                if (hasPrefab) prefabs++;

                report += "\\n" + entry.SafeName + " | FBX=" + hasFbx + " | Prefab=" + hasPrefab;
                if (hasFbx)
                {
                    report += " | " + fbxPath;
                }
            }

            Debug.Log(report + "\\n\\nExtracted FBX entries=" + extracted + " Prefabs=" + prefabs + " Catalog=" + Entries.Length);
        }

        [MenuItem("Elementborn/Assets/Repair Generated Asset Folder Names")]
        public static void RepairGeneratedAssetFolderNames()
        {
            EnsureFolder("Assets/Elementborn/Art");
            EnsureFolder("Assets/Elementborn/Art/Models");
            EnsureFolder("Assets/Elementborn/Art/Models/MeshyImported");
            EnsureFolder(ExtractedRoot);

            int repaired = 0;
            string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length).Replace("\\\\", "/");
            string absoluteRoot = (projectRoot + ExtractedRoot).Replace("\\\\", "/");

            if (!Directory.Exists(absoluteRoot))
            {
                Debug.LogWarning("AutoImported folder not found: " + ExtractedRoot);
                return;
            }

            for (int i = 0; i < Entries.Length; i++)
            {
                Entry entry = Entries[i];
                string expectedFolder = (absoluteRoot + "/" + entry.SafeName).Replace("\\\\", "/");
                if (Directory.Exists(expectedFolder))
                {
                    continue;
                }

                string match = FindBestExistingFolderForEntry(entry, absoluteRoot);
                if (string.IsNullOrWhiteSpace(match))
                {
                    continue;
                }

                try
                {
                    Directory.Move(match, expectedFolder);
                    repaired++;
                    Debug.Log("Repaired generated asset folder: " + match + " -> " + expectedFolder);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("Could not repair generated asset folder for " + entry.SafeName + ": " + ex.Message);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Generated asset folder repair complete. Repaired=" + repaired);
        }

        public static GameObject LoadAutoPrefab(string safeName)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].SafeName == safeName)
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(GetPrefabPath(Entries[i]));
                }
            }

            return null;
        }

        public static Entry GetEntry(string safeName)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].SafeName == safeName)
                {
                    return Entries[i];
                }
            }

            return new Entry();
        }

        public static string GetPrefabPathBySafeName(string safeName)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].SafeName == safeName)
                {
                    return GetPrefabPath(Entries[i]);
                }
            }

            return "";
        }

        public static string FindFirstFbxForSafeName(string safeName)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].SafeName == safeName)
                {
                    return FindFirstFbxForEntry(Entries[i]);
                }
            }

            return "";
        }

        public static string NormalizeForGeneratedAssetSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string lower = value.ToLowerInvariant();
            string withoutExtension = Path.GetFileNameWithoutExtension(lower);

            string[] removeTokens = new string[]
            {
                "meshy", "ai", "image", "to", "3d", "texture", "fbx", "output",
                "model", "character", "asset", "file"
            };

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < withoutExtension.Length; i++)
            {
                char c = withoutExtension[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append(' ');
                }
            }

            string[] pieces = builder.ToString().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            StringBuilder normalized = new StringBuilder();
            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];
                bool skip = false;

                for (int r = 0; r < removeTokens.Length; r++)
                {
                    if (piece == removeTokens[r])
                    {
                        skip = true;
                        break;
                    }
                }

                if (skip || IsMostlyDigits(piece))
                {
                    continue;
                }

                normalized.Append(piece);
            }

            return normalized.ToString();
        }

        public static bool EntryMatchesName(Entry entry, string name)
        {
            string normalizedName = NormalizeForGeneratedAssetSearch(name);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return false;
            }

            string safe = NormalizeForGeneratedAssetSearch(entry.SafeName);
            string display = NormalizeForGeneratedAssetSearch(entry.DisplayName);
            string oldPattern = NormalizeForGeneratedAssetSearch(entry.OldPattern);
            string renamedPattern = NormalizeForGeneratedAssetSearch(entry.RenamedPattern);

            return
                (!string.IsNullOrWhiteSpace(safe) && normalizedName.Contains(safe)) ||
                (!string.IsNullOrWhiteSpace(display) && normalizedName.Contains(display)) ||
                (!string.IsNullOrWhiteSpace(oldPattern) && normalizedName.Contains(oldPattern)) ||
                (!string.IsNullOrWhiteSpace(renamedPattern) && normalizedName.Contains(renamedPattern)) ||
                (!string.IsNullOrWhiteSpace(safe) && safe.Contains(normalizedName));
        }

        private static void BuildPrefab(Entry entry, string fbxPath, GameObject modelAsset)
        {
            GameObject root = new GameObject(entry.SafeName);
            GameObject model = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
            if (model == null)
            {
                model = Object.Instantiate(modelAsset);
            }

            model.name = "Model";
            model.transform.SetParent(root.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            NormalizeImportedModelScale(model.transform, entry.TargetHeight);

            ElementbornPrototypeImportedModelAnimator animator = root.AddComponent<ElementbornPrototypeImportedModelAnimator>();
            animator.mode = entry.AnimationMode;
            animator.bobAmplitude = entry.Role == "Item" || entry.Role == "Weapon" ? 0.08f : 0.04f;
            animator.boneWiggleDegrees = entry.Role == "Creature" ? 7.5f : 3.0f;

            ElementbornPrototypeImportedModelTag tag = root.AddComponent<ElementbornPrototypeImportedModelTag>();
            tag.sourceAssetPath = fbxPath;
            tag.modelRole = entry.Role + "/" + entry.Element;
            tag.notes = entry.DisplayName + ": " + entry.Notes;

            ElementbornPrototypeGeneratedAssetSlot slot = root.AddComponent<ElementbornPrototypeGeneratedAssetSlot>();
            slot.slotName = entry.DisplayName;
            slot.preferredPrefabName = entry.SafeName;
            slot.role = entry.Role;
            slot.element = entry.Element;
            slot.visualApplied = true;

            PrefabUtility.SaveAsPrefabAsset(root, GetPrefabPath(entry));
            Object.DestroyImmediate(root);
        }

        private static void NormalizeImportedModelScale(Transform modelRoot, float targetHeight)
        {
            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                modelRoot.localScale = Vector3.one;
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            float height = Mathf.Max(0.001f, bounds.size.y);
            float scale = Mathf.Clamp(targetHeight / height, 0.0005f, 20f);
            modelRoot.localScale = Vector3.one * scale;

            renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            modelRoot.position += -bounds.center + Vector3.up * bounds.extents.y;
        }

        private static string FindFirstFbxForEntry(Entry entry)
        {
            string directFolder = ExtractedRoot + "/" + entry.SafeName;
            string direct = FindFirstFbxUnderAssetFolder(directFolder);
            if (!string.IsNullOrWhiteSpace(direct))
            {
                return direct;
            }

            string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length).Replace("\\\\", "/");
            string absoluteRoot = (projectRoot + ExtractedRoot).Replace("\\\\", "/");
            if (!Directory.Exists(absoluteRoot))
            {
                return "";
            }

            string[] files = Directory.GetFiles(absoluteRoot, "*.fbx", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i].Replace("\\\\", "/");
                if (EntryMatchesName(entry, file))
                {
                    return file.Replace(projectRoot, "");
                }
            }

            return "";
        }

        private static string FindFirstFbxUnderAssetFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return "";
            }

            string[] guids = AssetDatabase.FindAssets("t:Model", new string[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return "";
        }

        private static string FindBestExistingFolderForEntry(Entry entry, string absoluteRoot)
        {
            string[] directories = Directory.GetDirectories(absoluteRoot, "*", SearchOption.AllDirectories);
            for (int i = 0; i < directories.Length; i++)
            {
                string directory = directories[i].Replace("\\\\", "/");
                if (EntryMatchesName(entry, directory))
                {
                    return directory;
                }
            }

            return "";
        }

        private static bool IsMostlyDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            int digits = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsDigit(value[i]))
                {
                    digits++;
                }
            }

            return digits >= Mathf.Max(1, value.Length - 1);
        }

        private static string GetPrefabPath(Entry entry)
        {
            return PrefabRoot + "/" + entry.SafeName + ".prefab";
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\\\", "/");
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
