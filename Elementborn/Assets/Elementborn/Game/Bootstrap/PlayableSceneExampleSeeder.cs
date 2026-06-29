using UnityEngine;

namespace Elementborn.Game
{
    public sealed class PlayableSceneExampleSeeder : MonoBehaviour
    {
        [SerializeField] private QuestUiDefinition[] starterQuests;
        [SerializeField] private bool startFirstQuestOnAwake = true;
        [SerializeField] private bool fillSpellResourcesOnAwake = true;

        private void Start()
        {
            if (startFirstQuestOnAwake && starterQuests != null && starterQuests.Length > 0 && starterQuests[0] != null)
            {
                QuestUiTracker.StartQuest(starterQuests[0]);
            }

            if (fillSpellResourcesOnAwake)
            {
                foreach (var pool in ElementbornFindUtility.FindAll<SpellResourcePool>())
                {
                    pool.Fill();
                }
            }
        }
    }
}
