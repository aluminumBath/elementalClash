#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterWorldEventContentGenerator
    {
        private const string BaseDir = "Assets/Elementborn/Generated";
        private const string EncounterDir = BaseDir + "/WorldEvents/Encounters";
        private const string EventDir = BaseDir + "/WorldEvents/Events";

        [MenuItem("Elementborn/Generate Starter Content/World Events")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(EncounterDir);
            Directory.CreateDirectory(EventDir);

            var seaMonsterEncounter = CreateEncounter("NerithaSeaMonsterEncounter", "Reef Gate Sea Monster", "A rare sea monster circles the old reef gate.", DynamicEncounterSpawnMode.MarkerOnly, MapMarkerType.SeaMonsterSighting, "Sailors whisper that something huge circles the old reef gate after sunset.");
            var windCurrentEncounter = CreateEncounter("TemporaryWindCurrentEncounter", "Temporary Wind Current", "A storm has opened a temporary wind current.", DynamicEncounterSpawnMode.MarkerOnly, MapMarkerType.WindCurrent, "The wind is behaving strangely near the reef. A skilled sailor could use it.");
            var factionPatrolEncounter = CreateEncounter("FactionPatrolEncounter", "Faction Patrol", "A faction patrol has appeared near the border.", DynamicEncounterSpawnMode.MarkerOnly, MapMarkerType.EnemyCamp, "Patrol banners were seen near the region border.");
            var merchantEncounter = CreateEncounter("MerchantCaravanEncounter", "Merchant Caravan", "A merchant caravan has arrived with temporary stock.", DynamicEncounterSpawnMode.MarkerOnly, MapMarkerType.VendorNpc, "A caravan has arrived. Their goods may not stay long.");
            var coralRespawnEncounter = CreateEncounter("CoralResourceRespawnEncounter", "Coral Bloom", "Coral resource nodes have refreshed around the reef.", DynamicEncounterSpawnMode.MarkerOnly, MapMarkerType.ResourceNode, "The reef is blooming again. Harvesters are already moving.");
            var bossLairEncounter = CreateEncounter("BossLairAwakeningEncounter", "Awakened Boss Lair", "A boss lair has become active after a quest event.", DynamicEncounterSpawnMode.MarkerOnly, MapMarkerType.BossLair, "Something ancient has awakened below the stone.");

            CreateEvent("neritha_sea_monster", "Sea Monster at Neritha Reefwood", "A rare sea monster appears near Neritha Reefwood and threatens boats that pass the reef gate.", WorldEventType.SeaMonster, "Neritha Reefwood", new Vector3(220f, 0f, 360f), MapMarkerType.SeaMonsterSighting, "Kram says a sea monster has been circling the old reef gate after sunset.", seaMonsterEncounter);
            CreateEvent("temporary_wind_current", "Temporary Wind Current", "A storm opens a temporary wind current that can speed boats or reveal hidden sky paths.", WorldEventType.WindCurrent, "Coastal Waters", new Vector3(140f, 0f, 280f), MapMarkerType.WindCurrent, "The storm left a moving river of wind over the sea.", windCurrentEncounter);
            CreateEvent("border_faction_patrol", "Faction Patrol at the Border", "A faction patrol appears near a contested elemental border.", WorldEventType.FactionPatrol, "Borderlands", new Vector3(80f, 0f, 210f), MapMarkerType.EnemyCamp, "Travelers report a patrol challenging mixed-blood channelers near the border.", factionPatrolEncounter);
            CreateEvent("merchant_caravan_arrival", "Merchant Caravan Arrival", "A traveling merchant caravan arrives with temporary inventory.", WorldEventType.MerchantCaravan, "Neutral City Road", new Vector3(35f, 0f, 90f), MapMarkerType.VendorNpc, "A caravan arrived at the road market. Their prices may shift by morning.", merchantEncounter);
            CreateEvent("coral_resource_bloom", "Coral Resource Bloom", "Coral resource nodes respawn around Neritha Reefwood.", WorldEventType.ResourceRespawn, "Neritha Reefwood", new Vector3(260f, 0f, 325f), MapMarkerType.ResourceNode, "The reef is blooming; coral shards and soft seaweed are easy to find.", coralRespawnEncounter);
            CreateEvent("kram_new_threat_rumor", "Kram's Warning", "Kram shares a rumor about a newly active threat.", WorldEventType.Rumor, "Neritha Reefwood", new Vector3(120f, 0f, 340f), MapMarkerType.GuideNpc, "Kram heard something moving beneath the coral roots.", null);
            CreateEvent("quest_boss_lair_awakens", "Boss Lair Awakens", "A boss lair becomes active after the related quest.", WorldEventType.BossAwakening, "Old Reef Gate", new Vector3(300f, 0f, 390f), MapMarkerType.BossLair, "The old reef gate is open. Something inside is awake.", bossLairEncounter);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated starter Elementborn world events.");
        }

        private static DynamicEncounterDefinition CreateEncounter(string id, string displayName, string description, DynamicEncounterSpawnMode mode, MapMarkerType markerType, string rumor)
        {
            string path = $"{EncounterDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<DynamicEncounterDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DynamicEncounterDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            so.FindProperty("encounterId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("spawnMode").enumValueIndex = (int)mode;
            so.FindProperty("minCount").intValue = 1;
            so.FindProperty("maxCount").intValue = 1;
            so.FindProperty("spawnRadius").floatValue = 12f;
            so.FindProperty("addMapMarker").boolValue = true;
            so.FindProperty("markerType").enumValueIndex = (int)markerType;
            so.FindProperty("markerExpiresInSeconds").floatValue = 600f;
            so.FindProperty("skillPointReward").intValue = 0;
            so.FindProperty("rumorText").stringValue = rumor;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static WorldEventDefinition CreateEvent(string id, string displayName, string description, WorldEventType type, string region, Vector3 position, MapMarkerType markerType, string rumor, DynamicEncounterDefinition encounter)
        {
            string path = $"{EventDir}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<WorldEventDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WorldEventDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }
            var so = new SerializedObject(asset);
            so.FindProperty("eventId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("description").stringValue = description;
            so.FindProperty("eventType").enumValueIndex = (int)type;
            so.FindProperty("defaultTriggerMode").enumValueIndex = (int)WorldEventTriggerMode.Manual;
            so.FindProperty("unique").boolValue = false;
            so.FindProperty("important").boolValue = true;
            so.FindProperty("region").stringValue = region;
            so.FindProperty("hasWorldPosition").boolValue = true;
            so.FindProperty("worldPosition").vector3Value = position;
            so.FindProperty("activationRadius").floatValue = 30f;
            so.FindProperty("activationDelaySeconds").floatValue = 0f;
            so.FindProperty("durationSeconds").floatValue = 600f;
            so.FindProperty("recurrenceSeconds").floatValue = -1f;
            so.FindProperty("addMapMarker").boolValue = true;
            so.FindProperty("markerType").enumValueIndex = (int)markerType;
            so.FindProperty("addRumor").boolValue = true;
            so.FindProperty("rumorText").stringValue = rumor;
            so.FindProperty("addJournalEntry").boolValue = true;
            var encounters = so.FindProperty("encounters");
            encounters.arraySize = encounter == null ? 0 : 1;
            if (encounter != null) encounters.GetArrayElementAtIndex(0).objectReferenceValue = encounter;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
#endif
