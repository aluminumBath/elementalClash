#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterHitFeedbackContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string FeedbackDir = BaseDir + "/Feedback";
        private const string SpriteDir = "Assets/Elementborn/Art/VFX/HitFeedback";

        [MenuItem("Elementborn/Generate Starter Content/Hit Feedback and VFX")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(FeedbackDir);
            ConfigureSprites();

            Create("Hit_Normal", HitFeedbackType.NormalHit, AbilityElementType.Neutral, "impact_slash", Color.white, 1f, 0.32f, 0.08f, 0.02f);
            Create("Hit_Critical", HitFeedbackType.CriticalHit, AbilityElementType.Neutral, "impact_critical_star", Color.white, 1.35f, 0.42f, 0.2f, 0.05f);
            Create("Hit_Block", HitFeedbackType.Block, AbilityElementType.Neutral, "impact_block", Color.white, 1.15f, 0.36f, 0.12f, 0.025f);
            Create("Hit_Dodge", HitFeedbackType.Dodge, AbilityElementType.Air, "impact_dodge_puff", Color.white, 1.1f, 0.34f, 0.04f, 0f);
            Create("Hit_Fire", HitFeedbackType.NormalHit, AbilityElementType.Fire, "impact_fire", Color.white, 1.1f, 0.38f, 0.12f, 0.03f);
            Create("Hit_Water", HitFeedbackType.NormalHit, AbilityElementType.Water, "impact_water", Color.white, 1.1f, 0.36f, 0.08f, 0.02f);
            Create("Hit_Earth", HitFeedbackType.NormalHit, AbilityElementType.Earth, "impact_earth", Color.white, 1.15f, 0.38f, 0.14f, 0.035f);
            Create("Hit_Air", HitFeedbackType.NormalHit, AbilityElementType.Air, "impact_air", Color.white, 1.05f, 0.32f, 0.07f, 0.015f);
            Create("Hit_Ice", HitFeedbackType.NormalHit, AbilityElementType.Ice, "impact_ice", Color.white, 1.1f, 0.38f, 0.09f, 0.02f);
            Create("Hit_Lightning", HitFeedbackType.NormalHit, AbilityElementType.Lightning, "impact_lightning", Color.white, 1.2f, 0.30f, 0.18f, 0.035f);
            Create("Hit_Heal", HitFeedbackType.Heal, AbilityElementType.Light, "impact_healing_bloom", Color.white, 1.15f, 0.42f, 0.04f, 0f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter hit feedback definitions.");
        }

        [MenuItem("Elementborn/Feedback Setup/Create Hit Feedback Service In Open Scene")]
        public static void CreateService()
        {
            GenerateAll();

            GameObject service = GameObject.Find("Hit Feedback Service");
            if (service == null)
            {
                service = new GameObject("Hit Feedback Service");
            }

            var hitFeedback = service.GetComponent<HitFeedbackService>();
            if (hitFeedback == null)
            {
                hitFeedback = service.AddComponent<HitFeedbackService>();
            }

            if (service.GetComponent<CameraShakeImpulse>() == null)
            {
                service.AddComponent<CameraShakeImpulse>();
            }

            if (service.GetComponent<HitPauseController>() == null)
            {
                service.AddComponent<HitPauseController>();
            }

            hitFeedback.SetDefinitions(
                Load("Hit_Normal"),
                Load("Hit_Critical"),
                Load("Hit_Block"),
                Load("Hit_Dodge"),
                Load("Hit_Fire"),
                Load("Hit_Water"),
                Load("Hit_Earth"),
                Load("Hit_Air"),
                Load("Hit_Ice"),
                Load("Hit_Lightning"),
                Load("Hit_Heal"));

            EditorUtility.SetDirty(service);
            Debug.Log("Created/updated Hit Feedback Service in open scene.");
        }

        private static void Create(
            string id,
            HitFeedbackType type,
            AbilityElementType element,
            string spriteName,
            Color tint,
            float scale,
            float lifetime,
            float shake,
            float pause)
        {
            string path = $"{FeedbackDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<HitFeedbackDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HitFeedbackDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var so = new SerializedObject(asset);
            so.FindProperty("feedbackId").stringValue = id;
            so.FindProperty("feedbackType").enumValueIndex = (int)type;
            so.FindProperty("element").enumValueIndex = (int)element;
            so.FindProperty("impactSprite").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteDir}/{spriteName}.png");
            so.FindProperty("tint").colorValue = tint;
            so.FindProperty("scale").floatValue = scale;
            so.FindProperty("lifetimeSeconds").floatValue = lifetime;
            so.FindProperty("cameraShakeStrength").floatValue = shake;
            so.FindProperty("hitPauseSeconds").floatValue = pause;
            so.FindProperty("flashTarget").boolValue = true;
            so.FindProperty("faceCamera").boolValue = true;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }

        private static HitFeedbackDefinition Load(string id)
        {
            return AssetDatabase.LoadAssetAtPath<HitFeedbackDefinition>($"{FeedbackDir}/{id}.asset");
        }

        private static void ConfigureSprites()
        {
            string[] names =
            {
                "impact_slash.png",
                "impact_fire.png",
                "impact_water.png",
                "impact_earth.png",
                "impact_air.png",
                "impact_ice.png",
                "impact_lightning.png",
                "impact_block.png",
                "impact_dodge_puff.png",
                "impact_critical_star.png",
                "impact_healing_bloom.png",
                "weapon_trail_swipe.png",
                "hit_feedback_preview.png"
            };

            foreach (string name in names)
            {
                string path = $"{SpriteDir}/{name}";
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
