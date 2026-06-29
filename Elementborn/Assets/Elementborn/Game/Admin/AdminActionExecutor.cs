using System;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class AdminActionExecutor : MonoBehaviour
    {
        public static AdminActionExecutor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static AdminActionExecutor Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject(nameof(AdminActionExecutor));
            return go.AddComponent<AdminActionExecutor>();
        }

        public AdminActionResult Execute(AdminActionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ActionId))
            {
                return AdminActionResult.Fail("No admin action selected.");
            }

            try
            {
                switch (request.ActionId)
                {
                    case "loop.start":
                        ElementbornMainGameplayLoopDirector.Ensure().StartGame();
                        return AdminActionResult.Ok("Gameplay loop started.");

                    case "loop.spawn":
                        ElementbornMainGameplayLoopDirector.Ensure().SpawnStarterWaves();
                        return AdminActionResult.Ok("Starter wave spawned.");

                    case "loop.complete":
                        ElementbornMainGameplayLoopDirector.Ensure().CompleteLoop();
                        return AdminActionResult.Ok("Gameplay loop completed.");

                    case "save.narrative":
                        NarrativeBridge().SaveSlot(request.GetInt("slot", 0));
                        return AdminActionResult.Ok("Saved narrative slot " + request.GetInt("slot", 0));

                    case "load.narrative":
                        NarrativeBridge().LoadSlot(request.GetInt("slot", 0));
                        return AdminActionResult.Ok("Loaded narrative slot " + request.GetInt("slot", 0));

                    case "delete.narrative":
                        NarrativeBridge().DeleteSlot(request.GetInt("slot", 0));
                        return AdminActionResult.Ok("Deleted narrative slot " + request.GetInt("slot", 0));

                    case "capital.pressure":
                        CapitalWorldStateTracker.Ensure().AddPressure(
                            ParseCapital(request.Get("capital", "FireCapital")),
                            ParsePressure(request.Get("pressure", "Unrest")),
                            request.GetInt("amount", 0),
                            request.Get("notes", "Admin wrist pressure change"));
                        return AdminActionResult.Ok("Capital pressure changed.");

                    case "capital.stability":
                        CapitalWorldStateTracker.Ensure().AddStability(
                            ParseCapital(request.Get("capital", "FireCapital")),
                            request.GetInt("amount", 0),
                            request.Get("notes", "Admin wrist stability change"));
                        return AdminActionResult.Ok("Capital stability changed.");

                    case "capital.sync":
                        CapitalWorldStateTracker.Ensure().SyncRegionalSystems();
                        return AdminActionResult.Ok("Capital world state synced.");

                    case "political.evaluate":
                        PoliticalWorldEventDirector.Ensure().EvaluateAll();
                        return AdminActionResult.Ok("Political events evaluated.");

                    case "political.advance":
                        for (int i = 0; i < Mathf.Max(1, request.GetInt("days", 1)); i++)
                        {
                            PoliticalWorldEventDirector.Ensure().AdvanceDay();
                        }
                        return AdminActionResult.Ok("Advanced political day(s).");

                    case "political.activate":
                        return PoliticalWorldEventDirector.Ensure().Activate(request.Get("eventId"), "Admin wrist")
                            ? AdminActionResult.Ok("Political event activated.")
                            : AdminActionResult.Fail("Political event not found or could not activate.");

                    case "political.resolve":
                        return PoliticalWorldEventDirector.Ensure().Resolve(request.Get("eventId"))
                            ? AdminActionResult.Ok("Political event resolved.")
                            : AdminActionResult.Fail("Political event not found.");

                    case "chain.start":
                        return QuestChainDirector.Ensure().StartChain(request.Get("chainId"))
                            ? AdminActionResult.Ok("Quest chain started.")
                            : AdminActionResult.Fail("Quest chain not found.");

                    case "chain.stage":
                        return QuestChainDirector.Ensure().StartStage(request.Get("chainId"), request.Get("stageId"))
                            ? AdminActionResult.Ok("Quest chain stage started.")
                            : AdminActionResult.Fail("Quest chain stage not found.");

                    case "chain.complete":
                        return QuestChainDirector.Ensure().CompleteStage(request.Get("chainId"), request.Get("stageId"))
                            ? AdminActionResult.Ok("Quest chain stage completed.")
                            : AdminActionResult.Fail("Quest chain stage not found.");

                    case "chain.choice":
                        return QuestChainDirector.Ensure().ApplyChoice(request.Get("chainId"), request.Get("stageId"), request.Get("choiceId"))
                            ? AdminActionResult.Ok("Quest chain choice applied.")
                            : AdminActionResult.Fail("Quest chain choice not found.");

                    case "fire.hook":
                        FireCapitalRegistry.Ensure().StartHook(request.Get("hookId", "fire_caldera_audience_hook"));
                        return AdminActionResult.Ok("Fire Capital hook started.");

                    case "fire.resolve":
                        FireCapitalRegistry.Ensure().ResolveHook(request.Get("hookId", "fire_caldera_audience_hook"), "Admin wrist");
                        return AdminActionResult.Ok("Fire Capital hook resolved.");

                    case "fire.volcano":
                        FireVolcano().PulseVolcanoPressure();
                        return AdminActionResult.Ok("Fire volcano pulsed.");

                    case "fire.calm":
                        FireVolcano().CalmVolcano();
                        return AdminActionResult.Ok("Fire volcano calmed.");

                    case "social.event":
                        SocialGroupRegistry.Ensure().ActivateEvent(request.Get("eventId", "marie_sleeping_flare_social"));
                        return AdminActionResult.Ok("Social group event triggered.");

                    case "social.next":
                        SocialGroupScheduleDirector schedule = ElementbornFindUtility.FindFirst<SocialGroupScheduleDirector>();
                        if (schedule == null)
                        {
                            schedule = new GameObject(nameof(SocialGroupScheduleDirector)).AddComponent<SocialGroupScheduleDirector>();
                        }
                        schedule.ActivateNextEvent();
                        return AdminActionResult.Ok("Next social group event triggered.");

                    case "orphanage.admit":
                        CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(
                            request.Get("creatureId", "emberfox_01"),
                            request.Get("displayName", "Emberfox"),
                            ParseDepartureReason(request.Get("reason", "RanAway")),
                            request.Get("notes", "Admin wrist admission"));
                        return AdminActionResult.Ok("Creature admitted to orphanage.");

                    case "orphanage.buy":
                        return CreatureOrphanageRecoveryRegistry.Ensure().BuyBack(request.Get("creatureId"))
                            ? AdminActionResult.Ok("Creature bought back.")
                            : AdminActionResult.Fail("Creature could not be bought back.");

                    case "orphanage.lure":
                        return CreatureOrphanageRecoveryRegistry.Ensure().LureBack(request.Get("creatureId"))
                            ? AdminActionResult.Ok("Creature lured back.")
                            : AdminActionResult.Fail("Creature could not be lured back.");

                    case "orphanage.rehome":
                        return CreatureOrphanageRecoveryRegistry.Ensure().PermanentlyRehome(request.Get("creatureId"))
                            ? AdminActionResult.Ok("Creature rehomed.")
                            : AdminActionResult.Fail("Creature could not be rehomed.");

                    case "wolf.defeat":
                        WolfPack().NotifyLeaderDefeated(request.Get("leader", "romilus"));
                        return AdminActionResult.Ok("Wolf leader defeated notification sent.");

                    case "wolf.respawn":
                        WolfPack().ForceRespawnPack();
                        return AdminActionResult.Ok("Wolf pack respawned.");

                    case "wolf.resolve":
                        WolfPack().MarkPackDefeated();
                        return AdminActionResult.Ok("Wolf pack resolved.");

                    case "encounter.start":
                        StoryEncounterProgressTracker.Ensure().StartEncounter(request.Get("encounterId", "donowl_sleepy_owl"), "Admin wrist");
                        return AdminActionResult.Ok("Story encounter started.");

                    case "encounter.resolve":
                        StoryEncounterProgressTracker.Ensure().ResolveEncounter(request.Get("encounterId", "donowl_sleepy_owl"), "Admin wrist");
                        return AdminActionResult.Ok("Story encounter resolved.");

                    case "cheat.apply":
                        return ApplyCheat(request.Get("cheat", "stabilize_fire_capital"));

                    case "raw.command":
                        return AdminRuntimeCommandRouter.Ensure().ExecuteCommand(request.Get("command", ""), out string message)
                            ? AdminActionResult.Ok(message)
                            : AdminActionResult.Fail(message);

                    default:
                        return AdminActionResult.Fail("Unhandled admin action: " + request.ActionId);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return AdminActionResult.Fail(ex.Message);
            }
        }

        private AdminActionResult ApplyCheat(string cheat)
        {
            switch ((cheat ?? "").Trim().ToLowerInvariant())
            {
                case "stabilize_fire_capital":
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, -25, "Cheat: stabilize Fire Capital");
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.HiddenThreat, -25, "Cheat: stabilize Fire Capital");
                    CapitalWorldStateTracker.Ensure().AddStability(CapitalId.FireCapital, 20, "Cheat: stabilize Fire Capital");
                    return AdminActionResult.Ok("Cheat applied: stabilize Fire Capital.");

                case "chaos_fire_capital":
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.Unrest, 25, "Cheat: chaos Fire Capital");
                    CapitalWorldStateTracker.Ensure().AddPressure(CapitalId.FireCapital, CapitalPressureType.HiddenThreat, 20, "Cheat: chaos Fire Capital");
                    return AdminActionResult.Ok("Cheat applied: chaos Fire Capital.");

                case "start_fire_intro":
                    FireCapitalRegistry.Ensure().StartHook("fire_caldera_audience_hook");
                    return AdminActionResult.Ok("Cheat applied: start Fire intro.");

                case "spawn_wave":
                    ElementbornMainGameplayLoopDirector.Ensure().SpawnStarterWaves();
                    return AdminActionResult.Ok("Cheat applied: spawn wave.");

                case "save_slot_zero":
                    NarrativeBridge().SaveSlot(0);
                    return AdminActionResult.Ok("Cheat applied: save slot zero.");

                case "load_slot_zero":
                    NarrativeBridge().LoadSlot(0);
                    return AdminActionResult.Ok("Cheat applied: load slot zero.");

                case "admit_demo_creature":
                    CreatureOrphanageRecoveryRegistry.Ensure().AdmitCreature(
                        "demo_emberfox",
                        "Demo Emberfox",
                        CreatureOrphanageDepartureReason.RanAway,
                        "Cheat demo admission");
                    return AdminActionResult.Ok("Cheat applied: admit demo creature.");

                case "resolve_wolf_pack":
                    WolfPack().MarkPackDefeated();
                    return AdminActionResult.Ok("Cheat applied: resolve wolf pack.");

                case "pulse_volcano":
                    FireVolcano().PulseVolcanoPressure();
                    return AdminActionResult.Ok("Cheat applied: pulse volcano.");

                case "calm_volcano":
                    FireVolcano().CalmVolcano();
                    return AdminActionResult.Ok("Cheat applied: calm volcano.");

                default:
                    return AdminRuntimeCommandRouter.Ensure().ExecuteCommand(cheat, out string message)
                        ? AdminActionResult.Ok(message)
                        : AdminActionResult.Fail("Unknown cheat: " + cheat);
            }
        }

        private NarrativeRuntimeSaveBridge NarrativeBridge()
        {
            NarrativeRuntimeSaveBridge bridge = ElementbornFindUtility.FindFirst<NarrativeRuntimeSaveBridge>();
            if (bridge == null)
            {
                bridge = new GameObject(nameof(NarrativeRuntimeSaveBridge)).AddComponent<NarrativeRuntimeSaveBridge>();
            }

            return bridge;
        }

        private FireCapitalVolcanoHazardController FireVolcano()
        {
            FireCapitalVolcanoHazardController volcano = ElementbornFindUtility.FindFirst<FireCapitalVolcanoHazardController>();
            if (volcano == null)
            {
                volcano = new GameObject(nameof(FireCapitalVolcanoHazardController)).AddComponent<FireCapitalVolcanoHazardController>();
            }

            return volcano;
        }

        private TimedDualLeaderPackRespawnController WolfPack()
        {
            TimedDualLeaderPackRespawnController controller = ElementbornFindUtility.FindFirst<TimedDualLeaderPackRespawnController>();
            if (controller == null)
            {
                controller = new GameObject(nameof(TimedDualLeaderPackRespawnController)).AddComponent<TimedDualLeaderPackRespawnController>();
            }

            return controller;
        }

        private CapitalId ParseCapital(string value)
        {
            return Enum.TryParse(value, true, out CapitalId parsed) ? parsed : CapitalId.FireCapital;
        }

        private CapitalPressureType ParsePressure(string value)
        {
            return Enum.TryParse(value, true, out CapitalPressureType parsed) ? parsed : CapitalPressureType.Unrest;
        }

        private CreatureOrphanageDepartureReason ParseDepartureReason(string value)
        {
            return Enum.TryParse(value, true, out CreatureOrphanageDepartureReason parsed)
                ? parsed
                : CreatureOrphanageDepartureReason.RanAway;
        }
    }
}
