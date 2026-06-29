using UnityEngine;

namespace Elementborn.Game
{
    public static class WorldEventEncounterSpawner
    {
        public static void SpawnEncounter(DynamicEncounterDefinition encounter, Vector3 origin, string region, string eventId)
        {
            if (encounter == null) return;

            if (encounter.AddMapMarker)
            {
                PlayerMapMarkerTracker.ReportOrUpdateMarker(
                    "encounter_" + PlayerJournalTracker.Safe(eventId + "_" + encounter.EncounterId),
                    encounter.MarkerType,
                    origin,
                    encounter.DisplayName,
                    isPersistent: false,
                    expiresInSeconds: encounter.MarkerExpiresInSeconds,
                    notes: encounter.Description);
            }

            if (!string.IsNullOrWhiteSpace(encounter.RumorText))
            {
                RumorTracker.AddRumor(encounter.RumorText, RumorType.Threat, encounter.DisplayName, region, true, true, origin, true);
            }

            if (encounter.SkillPointReward > 0)
            {
                DialogueMemoryTracker.Remember(
                    DialogueMemoryType.WorldFact,
                    encounter.DisplayName,
                    $"Resolving this encounter may grant {encounter.SkillPointReward} skill point(s).",
                    "World Event",
                    region);
            }

            if (encounter.SpawnMode == DynamicEncounterSpawnMode.LogOnly)
            {
                Debug.Log($"Encounter activated: {encounter.DisplayName}");
                return;
            }

            if (encounter.SpawnMode == DynamicEncounterSpawnMode.MarkerOnly || encounter.SpawnMode == DynamicEncounterSpawnMode.None)
            {
                return;
            }

            if (encounter.SpawnMode != DynamicEncounterSpawnMode.SpawnPrefab || encounter.Prefab == null)
            {
                return;
            }

            int count = Random.Range(encounter.MinCount, encounter.MaxCount + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 circle = Random.insideUnitCircle * encounter.SpawnRadius;
                Vector3 position = origin + new Vector3(circle.x, 0f, circle.y);
                Object.Instantiate(encounter.Prefab, position, Quaternion.identity);
            }
        }
    }
}
