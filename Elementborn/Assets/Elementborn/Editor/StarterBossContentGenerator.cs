#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Core;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterBossContentGenerator
    {
        private const string BossDir = "Assets/Elementborn/Generated/Bosses";
        private const string LootDir = "Assets/Elementborn/Generated/Combat/LootTables";
        private const string IconDir = "Assets/Elementborn/Art/UI/BossIcons";

        [MenuItem("Elementborn/Generate Starter Content/Bosses and Arenas")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(BossDir);
            CreateBoss("ReefGuardian", "Reef Guardian", "A massive coral-armored guardian beneath Neritha Reefwood.", "Neritha Reefwood", new Vector3(300f,0f,390f), "boss_reef_guardian", "The reef trembles as the Guardian rises.", "The coral quiets. The Reef Guardian has fallen.", "ReefCrabLoot", 150, 1,
                Phase("Shell Wake", 1f, "The Reef Guardian raises its coral shell.", "boss_phase_transition", Action(BossPhaseActionType.Announce, "Coral plates lock into place.")),
                Phase("Tidal Rage", 0.65f, "The Reef Guardian floods the arena.", "boss_arena_hazard", Action(BossPhaseActionType.EnableHazards, "Water hazards activate.")),
                Phase("Cracked Crown", 0.30f, "The Reef Guardian summons reeflings.", "boss_summon_adds", Action(BossPhaseActionType.SummonAdds, "Reeflings answer the call.")));
            CreateBoss("EmberTitan", "Ember Titan", "A volcanic titan that awakens in the fire region.", "Fire Region", new Vector3(520f,0f,190f), "boss_ember_titan", "The basalt cracks. The Ember Titan stands.", "Ash settles across the arena.", "EmberWispLoot", 200, 2,
                Phase("Molten Armor", 1f, "The Ember Titan hardens its molten armor.", "boss_phase_transition", Action(BossPhaseActionType.SwitchProfile, "The titan becomes aggressive.")),
                Phase("Lava Heart", 0.55f, "The arena burns hotter.", "boss_arena_hazard", Action(BossPhaseActionType.EnableHazards, "Lava vents open.")),
                Phase("Ashfall", 0.25f, "Ash spirits gather around the titan.", "boss_summon_adds", Action(BossPhaseActionType.SummonAdds, "Ash spirits join the fight.")));
            CreateBoss("StormSerpent", "Storm Serpent", "A sky-and-sea serpent that coils around storm currents.", "Storm Coast", new Vector3(180f,0f,520f), "boss_storm_serpent", "Thunder rolls as the Storm Serpent descends.", "The storm breaks.", "StormlingLoot", 250, 2,
                Phase("Coiling Winds", 1f, "The Storm Serpent wraps the arena in wind.", "boss_phase_transition", Action(BossPhaseActionType.Announce, "The wind reverses direction.")),
                Phase("Lightning Molt", 0.60f, "Lightning crawls across the serpent's scales.", "boss_arena_hazard", Action(BossPhaseActionType.EnableHazards, "Lightning hazards activate.")),
                Phase("Eye of the Storm", 0.25f, "The serpent calls stormlings.", "boss_summon_adds", Action(BossPhaseActionType.SummonAdds, "Stormlings appear.")));
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); Debug.Log("Generated starter Elementborn bosses.");
        }
        private static BossPhaseDefinition Phase(string name, float health, string ann, string icon, params BossPhaseAction[] actions){ var p=new BossPhaseDefinition{PhaseName=name,StartAtHealthPercent=health,PhaseAnnouncement=ann,PhaseIcon=AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{icon}.png")}; if(actions!=null) p.Actions.AddRange(actions); return p; }
        private static BossPhaseAction Action(BossPhaseActionType type, string msg){ return new BossPhaseAction{ActionType=type,Message=msg,SpawnCount=2,SpawnRadius=7f}; }
        private static void CreateBoss(string id,string name,string desc,string region,Vector3 pos,string icon,string intro,string defeat,string loot,int currency,int points,params BossPhaseDefinition[] phases)
        { string path=$"{BossDir}/{id}.asset"; var asset=AssetDatabase.LoadAssetAtPath<BossDefinition>(path); if(asset==null){ asset=ScriptableObject.CreateInstance<BossDefinition>(); AssetDatabase.CreateAsset(asset,path);} var so=new SerializedObject(asset); so.FindProperty("bossId").stringValue=id; so.FindProperty("displayName").stringValue=name; so.FindProperty("description").stringValue=desc; so.FindProperty("region").stringValue=region; so.FindProperty("mapPosition").vector3Value=pos; so.FindProperty("addMapMarker").boolValue=true; so.FindProperty("mapMarkerType").enumValueIndex=(int)MapMarkerType.BossLair; so.FindProperty("lootTable").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LootDropTableDefinition>($"{LootDir}/{loot}.asset"); so.FindProperty("currencyReward").intValue=currency; so.FindProperty("skillPointReward").intValue=points; so.FindProperty("questIdOnDefeat").stringValue=""; so.FindProperty("objectiveIdOnDefeat").stringValue=""; so.FindProperty("icon").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{icon}.png"); so.FindProperty("introMessage").stringValue=intro; so.FindProperty("defeatMessage").stringValue=defeat; var arr=so.FindProperty("phases"); arr.arraySize=phases.Length; for(int i=0;i<phases.Length;i++){ var src=phases[i]; var dst=arr.GetArrayElementAtIndex(i); dst.FindPropertyRelative("StartAtHealthPercent").floatValue=src.StartAtHealthPercent; dst.FindPropertyRelative("PhaseName").stringValue=src.PhaseName; dst.FindPropertyRelative("PhaseAnnouncement").stringValue=src.PhaseAnnouncement; dst.FindPropertyRelative("PhaseIcon").objectReferenceValue=src.PhaseIcon; var actions=dst.FindPropertyRelative("Actions"); actions.arraySize=src.Actions.Count; for(int a=0;a<src.Actions.Count;a++){ var action=src.Actions[a]; var ad=actions.GetArrayElementAtIndex(a); ad.FindPropertyRelative("ActionType").enumValueIndex=(int)action.ActionType; ad.FindPropertyRelative("Message").stringValue=action.Message; ad.FindPropertyRelative("SpawnCount").intValue=action.SpawnCount; ad.FindPropertyRelative("SpawnRadius").floatValue=action.SpawnRadius; }} so.ApplyModifiedProperties(); EditorUtility.SetDirty(asset); }
    }
}
#endif
