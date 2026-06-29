using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class StarterSceneChecklistGenerator : MonoBehaviour
    {
        [SerializeField] private List<BootstrapChecklistItem> items = new List<BootstrapChecklistItem>();

        private void Reset()
        {
            SeedDefaultItems();
        }

        [ContextMenu("Seed Default Checklist")]
        public void SeedDefaultItems()
        {
            items.Clear();
            Add("Runtime systems", "Add ElementbornRuntimeBootstrap and ensure systems exist.");
            Add("Player rig", "Create a Player-tagged object with movement, health, stamina, defense, spells, and interactor.");
            Add("Combat HUD", "Create basic quest, spell, boss, and notification HUD panels.");
            Add("Test enemy", "Create a test enemy with perception, movement, melee/ranged attacks, and health.");
            Add("Boss arena", "Create boss object, arena trigger, arena hazards, and boss UI.");
            Add("Boat test", "Create a test boat with controller, wake, waves, ranged combat, and boarding station.");
            Add("Starter content", "Run starter generators for abilities, combat/status, enemy AI, bosses, quests, items, and spells.");
            Add("Unity compile", "Open Unity and resolve any project-specific compiler errors.");
        }

        [ContextMenu("Write Checklist Markdown")]
        public void WriteChecklistMarkdown()
        {
            string dir = Path.Combine(Application.dataPath, "Elementborn/Generated/Checklists");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "PlayableSceneChecklist.md");
            var lines = new List<string> { "# Elementborn Playable Scene Checklist", "" };
            foreach (var item in items)
            {
                string mark = item.Complete ? "x" : " ";
                lines.Add($"- [{mark}] **{item.Title}** — {item.Description}");
            }
            File.WriteAllLines(path, lines);
            NotificationFeed.Post("Playable scene checklist written.", NotificationType.Info);
        }

        private void Add(string title, string description)
        {
            items.Add(new BootstrapChecklistItem { Title = title, Description = description });
        }
    }
}
