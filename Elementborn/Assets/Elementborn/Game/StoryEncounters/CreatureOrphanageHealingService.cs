using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CreatureOrphanageHealingService : MonoBehaviour
    {
        [SerializeField] private string orphanageName = "Crab-Sign Creature Orphanage";
        [SerializeField] private List<GameObject> creaturesBeingHelped = new List<GameObject>();
        [SerializeField] private int healAmount = 9999;
        [SerializeField] private bool postFunnyNotifications = true;

        public void RegisterCreature(GameObject creature)
        {
            if (creature != null && !creaturesBeingHelped.Contains(creature))
            {
                creaturesBeingHelped.Add(creature);
            }
        }

        public void HealRegisteredCreatures()
        {
            int healed = 0;
            foreach (GameObject creature in creaturesBeingHelped)
            {
                if (creature == null)
                {
                    continue;
                }

                SendMessageOptions options = SendMessageOptions.DontRequireReceiver;
                creature.SendMessage("Heal", healAmount, options);
                creature.SendMessage("RestoreHealth", healAmount, options);
                creature.SendMessage("ClearNegativeStatusEffects", options);
                healed++;
            }

            if (postFunnyNotifications)
            {
                NotificationFeed.Post($"{orphanageName}: Ella and Eloc healed {healed} creature(s), then argued lovingly about crab snacks.", NotificationType.Info);
            }
        }

        public void HealCreature(GameObject creature)
        {
            RegisterCreature(creature);
            HealRegisteredCreatures();
        }
    }
}
