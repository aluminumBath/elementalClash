#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Elementborn.Game;

namespace Elementborn.Game.EditorTools
{
    public static class StarterQuestUiContentGenerator
    {
        private const string QuestDir = "Assets/Elementborn/Generated/QuestUI";
        private const string IconDir = "Assets/Elementborn/Art/UI/QuestIcons";
        [MenuItem("Elementborn/Generate Starter Content/Quest UI Examples")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(QuestDir);
            CreateQuest("FindKramAtNeritha","Find Kram at Neritha Reefwood","Kram was last seen near the coral forest above the sea.","Neritha Reefwood","Kram","quest_scroll", Reward("Creature Treat","CreatureTreat",2,20,0), Objective("reach_reefwood","Reach Neritha Reefwood","Follow the coast to the coral forest.",new Vector3(300f,0f,390f)), Objective("speak_to_kram","Speak to Kram","Find Kram near the water-channeler market.",new Vector3(322f,0f,404f)));
            CreateQuest("RepairTheBoat","Repair the Boat","Gather enough material to patch the boat and return to sea.","Coastal Waters","Dockwright","quest_reward_chest", Reward("Boat Repair Kit","BoatRepairKit",1,35,0), Objective("collect_planks","Collect boat planks","Harvest driftwood or buy planks from the dockwright.",new Vector3(160f,0f,210f)), Objective("patch_boat","Patch the boat","Use the repair station at the dock.",new Vector3(140f,0f,190f)));
            CreateQuest("BossLairAwakens","Boss Lair Awakens","A strange tremor reveals a dangerous lair.","Storm Coast","Kram","quest_star", Reward("Skill Point","",0,100,1), Objective("find_lair","Find the awakened lair","Track the tremor toward the storm coast.",new Vector3(180f,0f,520f)), Objective("defeat_guardian","Defeat the guardian","Enter the arena and survive the phase changes.",new Vector3(190f,0f,530f)));
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); Debug.Log("Generated starter quest UI examples.");
        }
        private static QuestObjectiveUiDefinition Objective(string id,string title,string description,Vector3 position){ return new QuestObjectiveUiDefinition{ ObjectiveId=id, Title=title, Description=description, WorldPosition=position, CreateWaypoint=true, Required=true }; }
        private static QuestRewardPreviewDefinition Reward(string displayName,string itemId,int quantity,int currency,int skillPoints){ return new QuestRewardPreviewDefinition{ RewardId=displayName.Replace(" ","_"), DisplayName=displayName, ItemId=itemId, Quantity=Mathf.Max(1,quantity), Currency=currency, SkillPoints=skillPoints }; }
        private static void CreateQuest(string id,string title,string description,string region,string giver,string iconName,QuestRewardPreviewDefinition reward,params QuestObjectiveUiDefinition[] objectives)
        {
            string path=$"{QuestDir}/{id}.asset"; var asset=AssetDatabase.LoadAssetAtPath<QuestUiDefinition>(path); if(asset==null){ asset=ScriptableObject.CreateInstance<QuestUiDefinition>(); AssetDatabase.CreateAsset(asset,path); }
            var so=new SerializedObject(asset); so.FindProperty("questId").stringValue=id; so.FindProperty("title").stringValue=title; so.FindProperty("description").stringValue=description; so.FindProperty("region").stringValue=region; so.FindProperty("giverName").stringValue=giver; so.FindProperty("autoTrack").boolValue=true; so.FindProperty("icon").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>($"{IconDir}/{iconName}.png");
            var objArray=so.FindProperty("objectives"); objArray.arraySize=objectives.Length; for(int i=0;i<objectives.Length;i++){ var src=objectives[i]; var dst=objArray.GetArrayElementAtIndex(i); dst.FindPropertyRelative("ObjectiveId").stringValue=src.ObjectiveId; dst.FindPropertyRelative("Title").stringValue=src.Title; dst.FindPropertyRelative("Description").stringValue=src.Description; dst.FindPropertyRelative("WorldPosition").vector3Value=src.WorldPosition; dst.FindPropertyRelative("CreateWaypoint").boolValue=src.CreateWaypoint; dst.FindPropertyRelative("Required").boolValue=src.Required; }
            var rewards=so.FindProperty("rewards"); rewards.arraySize=1; var r=rewards.GetArrayElementAtIndex(0); r.FindPropertyRelative("RewardId").stringValue=reward.RewardId; r.FindPropertyRelative("DisplayName").stringValue=reward.DisplayName; r.FindPropertyRelative("ItemId").stringValue=reward.ItemId; r.FindPropertyRelative("Quantity").intValue=reward.Quantity; r.FindPropertyRelative("Currency").intValue=reward.Currency; r.FindPropertyRelative("SkillPoints").intValue=reward.SkillPoints;
            so.ApplyModifiedProperties(); EditorUtility.SetDirty(asset);
        }
    }
}
#endif
