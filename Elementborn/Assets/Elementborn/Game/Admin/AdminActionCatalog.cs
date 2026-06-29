using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AdminActionCatalog : MonoBehaviour
    {
        public static AdminActionCatalog Instance { get; private set; }

        [SerializeField] private List<AdminActionDefinition> actions = new List<AdminActionDefinition>();

        public IReadOnlyList<AdminActionDefinition> Actions => actions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            RebuildDefaults();
        }

        public static AdminActionCatalog Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(AdminActionCatalog));
            return go.AddComponent<AdminActionCatalog>();
        }

        public void RebuildDefaults()
        {
            actions.Clear();

            Add("loop.start", "Start Main Gameplay Loop", AdminActionCategory.GameplayLoop, "Start or restart the playable loop.");
            Add("loop.spawn", "Spawn Starter Wave", AdminActionCategory.GameplayLoop, "Spawn configured starter waves.");
            Add("loop.complete", "Complete Loop", AdminActionCategory.GameplayLoop, "Mark the playable loop complete.");

            Add("save.narrative", "Save Narrative Slot", AdminActionCategory.SaveLoad, "Save narrative/story runtime systems.")
                .AddField(new AdminActionFieldDefinition("slot", "Slot", AdminFieldType.Integer, "0"));
            Add("load.narrative", "Load Narrative Slot", AdminActionCategory.SaveLoad, "Load narrative/story runtime systems.")
                .AddField(new AdminActionFieldDefinition("slot", "Slot", AdminFieldType.Integer, "0"));
            Add("delete.narrative", "Delete Narrative Slot", AdminActionCategory.SaveLoad, "Delete narrative/story runtime systems.")
                .AddField(new AdminActionFieldDefinition("slot", "Slot", AdminFieldType.Integer, "0"));

            Add("capital.pressure", "Change Capital Pressure", AdminActionCategory.CapitalWorldState, "Add/subtract a pressure value.")
                .AddField(new AdminActionFieldDefinition("capital", "Capital", AdminFieldType.Capital, "FireCapital"))
                .AddField(new AdminActionFieldDefinition("pressure", "Pressure", AdminFieldType.CapitalPressure, "Unrest"))
                .AddField(new AdminActionFieldDefinition("amount", "Amount", AdminFieldType.Integer, "5"))
                .AddField(new AdminActionFieldDefinition("notes", "Notes", AdminFieldType.Text, "Admin wrist change"));
            Add("capital.stability", "Change Capital Stability", AdminActionCategory.CapitalWorldState, "Add/subtract capital stability.")
                .AddField(new AdminActionFieldDefinition("capital", "Capital", AdminFieldType.Capital, "FireCapital"))
                .AddField(new AdminActionFieldDefinition("amount", "Amount", AdminFieldType.Integer, "5"))
                .AddField(new AdminActionFieldDefinition("notes", "Notes", AdminFieldType.Text, "Admin wrist stability change"));
            Add("capital.sync", "Sync Capital World State", AdminActionCategory.CapitalWorldState, "Pull in dependent regional systems.");

            Add("political.evaluate", "Evaluate Political Events", AdminActionCategory.PoliticalEvents, "Re-evaluate all political event eligibility.");
            Add("political.advance", "Advance Political Days", AdminActionCategory.PoliticalEvents, "Advance world day and re-evaluate events.")
                .AddField(new AdminActionFieldDefinition("days", "Days", AdminFieldType.Integer, "1"));
            Add("political.activate", "Activate Political Event", AdminActionCategory.PoliticalEvents, "Activate an event by id.")
                .AddField(new AdminActionFieldDefinition("eventId", "Event Id", AdminFieldType.Text, "wind_capital_riot"));
            Add("political.resolve", "Resolve Political Event", AdminActionCategory.PoliticalEvents, "Resolve an event by id.")
                .AddField(new AdminActionFieldDefinition("eventId", "Event Id", AdminFieldType.Text, "wind_capital_riot"));

            Add("chain.start", "Start Quest Chain", AdminActionCategory.QuestChains, "Start a quest chain by id.")
                .AddField(new AdminActionFieldDefinition("chainId", "Chain Id", AdminFieldType.Text, "sarah_hidden_past_chain"));
            Add("chain.stage", "Start Quest Chain Stage", AdminActionCategory.QuestChains, "Start a specific stage.")
                .AddField(new AdminActionFieldDefinition("chainId", "Chain Id", AdminFieldType.Text, "sarah_hidden_past_chain"))
                .AddField(new AdminActionFieldDefinition("stageId", "Stage Id", AdminFieldType.Text, "main"));
            Add("chain.complete", "Complete Quest Chain Stage", AdminActionCategory.QuestChains, "Complete a stage.")
                .AddField(new AdminActionFieldDefinition("chainId", "Chain Id", AdminFieldType.Text, "sarah_hidden_past_chain"))
                .AddField(new AdminActionFieldDefinition("stageId", "Stage Id", AdminFieldType.Text, "main"));
            Add("chain.choice", "Apply Quest Chain Choice", AdminActionCategory.QuestChains, "Apply a branching choice.")
                .AddField(new AdminActionFieldDefinition("chainId", "Chain Id", AdminFieldType.Text, "larissa_secret_chain"))
                .AddField(new AdminActionFieldDefinition("stageId", "Stage Id", AdminFieldType.Text, "choice_point"))
                .AddField(new AdminActionFieldDefinition("choiceId", "Choice Id", AdminFieldType.Text, "protect_larissa"));

            Add("fire.hook", "Start Fire Capital Hook", AdminActionCategory.FireCapital, "Start a Fire Capital hook.")
                .AddField(new AdminActionFieldDefinition("hookId", "Hook Id", AdminFieldType.Text, "fire_caldera_audience_hook"));
            Add("fire.resolve", "Resolve Fire Capital Hook", AdminActionCategory.FireCapital, "Resolve a Fire Capital hook.")
                .AddField(new AdminActionFieldDefinition("hookId", "Hook Id", AdminFieldType.Text, "fire_caldera_audience_hook"));
            Add("fire.volcano", "Pulse Fire Volcano", AdminActionCategory.FireCapital, "Increase Fire Capital hidden-threat pressure.");
            Add("fire.calm", "Calm Fire Volcano", AdminActionCategory.FireCapital, "Reduce Fire Capital hidden-threat pressure.");

            Add("social.event", "Trigger Social Group Event", AdminActionCategory.SocialGroups, "Trigger a social group event.")
                .AddField(new AdminActionFieldDefinition("eventId", "Event Id", AdminFieldType.Text, "marie_sleeping_flare_social"));
            Add("social.next", "Trigger Next Social Event", AdminActionCategory.SocialGroups, "Rotate the social schedule once.");

            Add("orphanage.admit", "Admit Creature To Orphanage", AdminActionCategory.CreatureOrphanage, "Send a creature to the orphanage.")
                .AddField(new AdminActionFieldDefinition("creatureId", "Creature Id", AdminFieldType.Text, "emberfox_01"))
                .AddField(new AdminActionFieldDefinition("displayName", "Display Name", AdminFieldType.Text, "Emberfox"))
                .AddField(new AdminActionFieldDefinition("reason", "Reason", AdminFieldType.Dropdown, "RanAway", "Died", "Released", "RanAway", "Mistreatment", "Lost", "StoryEvent"))
                .AddField(new AdminActionFieldDefinition("notes", "Notes", AdminFieldType.Text, "Admin wrist admission"));
            Add("orphanage.buy", "Buy Creature Back", AdminActionCategory.CreatureOrphanage, "Buy back a creature.")
                .AddField(new AdminActionFieldDefinition("creatureId", "Creature Id", AdminFieldType.Text, "emberfox_01"));
            Add("orphanage.lure", "Lure Creature Back", AdminActionCategory.CreatureOrphanage, "Lure back a creature.")
                .AddField(new AdminActionFieldDefinition("creatureId", "Creature Id", AdminFieldType.Text, "emberfox_01"));
            Add("orphanage.rehome", "Permanently Rehome Creature", AdminActionCategory.CreatureOrphanage, "Rehome a creature.")
                .AddField(new AdminActionFieldDefinition("creatureId", "Creature Id", AdminFieldType.Text, "emberfox_01"));

            Add("wolf.defeat", "Defeat Wolf Leader", AdminActionCategory.WolfPack, "Notify the wolf-pack controller that a leader fell.")
                .AddField(new AdminActionFieldDefinition("leader", "Leader", AdminFieldType.Dropdown, "romilus", "romilus", "madrangea"));
            Add("wolf.respawn", "Respawn Wolf Pack", AdminActionCategory.WolfPack, "Force the wolf pack to regroup.");
            Add("wolf.resolve", "Resolve Wolf Pack", AdminActionCategory.WolfPack, "Mark the pack as defeated.");

            Add("encounter.start", "Start Story Encounter", AdminActionCategory.StoryEncounters, "Start encounter progress by id.")
                .AddField(new AdminActionFieldDefinition("encounterId", "Encounter Id", AdminFieldType.Text, "donowl_sleepy_owl"));
            Add("encounter.resolve", "Resolve Story Encounter", AdminActionCategory.StoryEncounters, "Resolve encounter progress by id.")
                .AddField(new AdminActionFieldDefinition("encounterId", "Encounter Id", AdminFieldType.Text, "donowl_sleepy_owl"));

            Add("cheat.apply", "Apply Cheat Code", AdminActionCategory.CheatCodes, "Apply a friendly cheat alias.")
                .AddField(new AdminActionFieldDefinition("cheat", "Cheat", AdminFieldType.Dropdown, "stabilize_fire_capital",
                    "stabilize_fire_capital",
                    "chaos_fire_capital",
                    "start_fire_intro",
                    "spawn_wave",
                    "save_slot_zero",
                    "load_slot_zero",
                    "admit_demo_creature",
                    "resolve_wolf_pack",
                    "pulse_volcano",
                    "calm_volcano"));

            Add("raw.command", "Run Raw Admin Command", AdminActionCategory.RawCommand, "Run any legacy command string.")
                .AddField(new AdminActionFieldDefinition("command", "Command", AdminFieldType.Text, "eventdir.summary"));
        }

        public List<AdminActionDefinition> GetActionsForCategory(AdminActionCategory category)
        {
            return actions.FindAll(a => a != null && a.Category == category);
        }

        public AdminActionDefinition FindAction(string actionId)
        {
            string needle = (actionId ?? "").Trim().ToLowerInvariant();
            return actions.Find(a => a != null && (a.ActionId ?? "").ToLowerInvariant() == needle);
        }

        private AdminActionDefinition Add(string id, string displayName, AdminActionCategory category, string description = "")
        {
            var action = new AdminActionDefinition(id, displayName, category, description);
            actions.Add(action);
            return action;
        }
    }
}
