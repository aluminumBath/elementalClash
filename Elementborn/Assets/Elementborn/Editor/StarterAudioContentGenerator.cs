#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterAudioContentGenerator
    {
        private const string AudioDir = "Assets/Elementborn/Audio/Generated";
        private const string EventDir = "Assets/Elementborn/Generated/Audio/Events";
        private const string BusDir = "Assets/Elementborn/Generated/Audio";

        [MenuItem("Elementborn/Generate Starter Content/Audio Events and Placeholder Sounds")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(EventDir);
            Directory.CreateDirectory(BusDir);

            ElementbornAudioBusSettings bus = CreateBusSettings();

            CreateEvent(ElementbornSoundEventId.UiConfirm, ElementbornSoundCategory.UI, "ui_confirm_chime", 0.75f, false);
            CreateEvent(ElementbornSoundEventId.UiCancel, ElementbornSoundCategory.UI, "ui_cancel_soft", 0.65f, false);
            CreateEvent(ElementbornSoundEventId.UiTick, ElementbornSoundCategory.UI, "ui_menu_tick", 0.45f, false);
            CreateEvent(ElementbornSoundEventId.QuestStart, ElementbornSoundCategory.UI, "quest_start_fanfare", 0.85f, false);
            CreateEvent(ElementbornSoundEventId.QuestComplete, ElementbornSoundCategory.UI, "quest_complete_fanfare", 0.9f, false);

            CreateEvent(ElementbornSoundEventId.HitSlash, ElementbornSoundCategory.Combat, "hit_slash_wood", 0.8f, true);
            CreateEvent(ElementbornSoundEventId.HitCritical, ElementbornSoundCategory.Combat, "hit_critical_spark", 0.9f, true);
            CreateEvent(ElementbornSoundEventId.BlockClank, ElementbornSoundCategory.Combat, "block_clank", 0.9f, true);
            CreateEvent(ElementbornSoundEventId.PerfectBlock, ElementbornSoundCategory.Combat, "perfect_block_ring", 0.9f, true);
            CreateEvent(ElementbornSoundEventId.DodgePuff, ElementbornSoundCategory.Combat, "dodge_puff", 0.55f, true);

            CreateEvent(ElementbornSoundEventId.SpellFire, ElementbornSoundCategory.Spells, "spell_fire_whoosh", 0.75f, true);
            CreateEvent(ElementbornSoundEventId.SpellWater, ElementbornSoundCategory.Spells, "spell_water_splash", 0.75f, true);
            CreateEvent(ElementbornSoundEventId.SpellAir, ElementbornSoundCategory.Spells, "spell_air_gust", 0.7f, true);
            CreateEvent(ElementbornSoundEventId.SpellEarth, ElementbornSoundCategory.Spells, "spell_earth_thump", 0.85f, true);
            CreateEvent(ElementbornSoundEventId.SpellIce, ElementbornSoundCategory.Spells, "spell_ice_tinkle", 0.7f, true);
            CreateEvent(ElementbornSoundEventId.SpellLightning, ElementbornSoundCategory.Spells, "spell_lightning_zap", 0.8f, true);
            CreateEvent(ElementbornSoundEventId.HealingBloom, ElementbornSoundCategory.Spells, "healing_bloom", 0.75f, true);

            CreateEvent(ElementbornSoundEventId.BoatBoard, ElementbornSoundCategory.Boat, "boat_board", 0.75f, true);
            CreateEvent(ElementbornSoundEventId.BoatWaveCreak, ElementbornSoundCategory.Boat, "boat_wave_creak", 0.55f, true);
            CreateEvent(ElementbornSoundEventId.HarvestPick, ElementbornSoundCategory.World, "harvest_pick", 0.75f, true);
            CreateEvent(ElementbornSoundEventId.ResourcePickup, ElementbornSoundCategory.World, "resource_pickup", 0.65f, false);
            CreateEvent(ElementbornSoundEventId.BossAwaken, ElementbornSoundCategory.Combat, "boss_awaken_rumble", 0.95f, true);
            CreateEvent(ElementbornSoundEventId.BossPhase, ElementbornSoundCategory.Combat, "boss_phase_stinger", 0.9f, true);

            CreateEvent(ElementbornSoundEventId.NpcVoiceWarm, ElementbornSoundCategory.NPC, "npc_voice_placeholder_warm", 0.8f, true);
            CreateEvent(ElementbornSoundEventId.NpcVoiceRoyal, ElementbornSoundCategory.NPC, "npc_voice_placeholder_royal", 0.8f, true);
            CreateEvent(ElementbornSoundEventId.NpcVoiceVillain, ElementbornSoundCategory.NPC, "npc_voice_placeholder_villain", 0.8f, true);
            CreateEvent(ElementbornSoundEventId.NpcVoiceGuard, ElementbornSoundCategory.NPC, "npc_voice_placeholder_guard", 0.8f, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter audio events and placeholder sound definitions.");
        }

        [MenuItem("Elementborn/Audio Setup/Create Audio Service In Open Scene")]
        public static void CreateAudioService()
        {
            GenerateAll();

            GameObject service = GameObject.Find("Elementborn Audio Service");
            if (service == null)
            {
                service = new GameObject("Elementborn Audio Service");
            }

            var audio = service.GetComponent<ElementbornAudioService>();
            if (audio == null)
            {
                audio = service.AddComponent<ElementbornAudioService>();
            }

            if (service.GetComponent<ElementbornAudioEventRouter>() == null)
            {
                service.AddComponent<ElementbornAudioEventRouter>();
            }

            var events = new List<ElementbornSoundEventDefinition>();
            foreach (string guid in AssetDatabase.FindAssets("t:ElementbornSoundEventDefinition", new[] { EventDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var evt = AssetDatabase.LoadAssetAtPath<ElementbornSoundEventDefinition>(path);
                if (evt != null)
                {
                    events.Add(evt);
                }
            }

            var bus = AssetDatabase.LoadAssetAtPath<ElementbornAudioBusSettings>($"{BusDir}/ElementbornAudioBusSettings.asset");
            audio.SetEvents(events, bus);
            EditorUtility.SetDirty(service);
            Debug.Log("Created/updated Elementborn Audio Service in open scene.");
        }

        private static ElementbornAudioBusSettings CreateBusSettings()
        {
            string path = $"{BusDir}/ElementbornAudioBusSettings.asset";
            var bus = AssetDatabase.LoadAssetAtPath<ElementbornAudioBusSettings>(path);
            if (bus == null)
            {
                bus = ScriptableObject.CreateInstance<ElementbornAudioBusSettings>();
                AssetDatabase.CreateAsset(bus, path);
            }

            var so = new SerializedObject(bus);
            var volumes = so.FindProperty("volumes");
            volumes.arraySize = 9;
            for (int i = 0; i < volumes.arraySize; i++)
            {
                var entry = volumes.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("Category").enumValueIndex = i;
                entry.FindPropertyRelative("Volume").floatValue = 1f;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bus);
            return bus;
        }

        private static void CreateEvent(ElementbornSoundEventId id, ElementbornSoundCategory category, string clipName, float volume, bool spatial)
        {
            string path = $"{EventDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ElementbornSoundEventDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ElementbornSoundEventDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioDir}/{clipName}.wav");
            var so = new SerializedObject(asset);
            so.FindProperty("eventId").enumValueIndex = (int)id;
            so.FindProperty("category").enumValueIndex = (int)category;
            var clips = so.FindProperty("clips");
            clips.arraySize = 1;
            clips.GetArrayElementAtIndex(0).objectReferenceValue = clip;
            so.FindProperty("volume").floatValue = volume;
            so.FindProperty("pitchMin").floatValue = 0.96f;
            so.FindProperty("pitchMax").floatValue = 1.04f;
            so.FindProperty("spatial").boolValue = spatial;
            so.FindProperty("minDistance").floatValue = 2f;
            so.FindProperty("maxDistance").floatValue = 24f;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif
