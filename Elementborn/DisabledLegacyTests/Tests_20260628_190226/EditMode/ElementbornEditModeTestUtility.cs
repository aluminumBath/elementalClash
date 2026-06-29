
using System.Reflection;
using UnityEngine;

namespace Elementborn.Tests.EditMode
{
    internal static class ElementbornEditModeTestUtility
    {
        public static void ResetAll()
        {
            DestroyAll<Elementborn.Game.NotificationFeed>();
            DestroyAll<Elementborn.Game.PlayerJournalTracker>();
            DestroyAll<Elementborn.Game.PlayerMapMarkerTracker>();
            DestroyAll<Elementborn.Game.CapitalWorldStateTracker>();
            DestroyAll<Elementborn.Game.PoliticalWorldEventDirector>();
            DestroyAll<Elementborn.Game.QuestChainDirector>();
            DestroyAll<Elementborn.Game.SocialGroupRegistry>();
            DestroyAll<Elementborn.Game.CreatureOrphanageRecoveryRegistry>();
            DestroyAll<Elementborn.Game.StoryEncounterProgressTracker>();
            DestroyAll<Elementborn.Game.ReligiousFervorTracker>();
            DestroyAll<Elementborn.Game.ThievesGuildReputationTracker>();
            DestroyAll<Elementborn.Game.SocialNpcDialogueRegistry>();
            DestroyAll<Elementborn.Game.WindCapitalSecretTracker>();
            DestroyAll<Elementborn.Game.HiddenChannelerSecretTracker>();
            DestroyAll<Elementborn.Game.ShipReputationTracker>();
        }

        public static void SetPrivate(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static void DestroyAll<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            foreach (T obj in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
#else
            foreach (T obj in Object.FindObjectsOfType<T>(true))
#endif
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }
    }
}
