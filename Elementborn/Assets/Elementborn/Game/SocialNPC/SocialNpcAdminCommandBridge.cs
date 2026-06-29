using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// social.summary
    /// social.cue npcId|cueOrKeyword
    /// marie.fire
    /// marie.extinguish
    /// kelly.mood mood
    /// kelly.protect
    /// johna.advice
    /// rekr.cough
    /// rekr.remedy
    /// manon.clean
    /// manon.mess
    /// </summary>
    public sealed class SocialNpcAdminCommandBridge : MonoBehaviour
    {
        [SerializeField] private MarieAccidentalFireController marie;
        [SerializeField] private KellyMoodFlameController kelly;
        [SerializeField] private JohnaAdviceController johna;
        [SerializeField] private RekrGrossnessController rekr;
        [SerializeField] private ManonCleanlinessController manon;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed == "social.summary")
            {
                Debug.Log(SocialNpcDialogueRegistry.Ensure().BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("social.cue "))
            {
                string[] parts = trimmed.Substring("social.cue ".Length).Split('|');
                if (parts.Length >= 2)
                {
                    var profile = SocialNpcDialogueRegistry.Ensure().FindProfile(parts[0].Trim());
                    var cue = profile != null ? profile.FindCue(parts[1].Trim()) : null;
                    Debug.Log(cue != null ? $"{profile.DisplayName}: {cue.Line}" : "Social cue not found.");
                }
                return true;
            }

            if (trimmed == "marie.fire") { if (marie != null) marie.TriggerAccidentalFlare(); return true; }
            if (trimmed == "marie.extinguish") { if (marie != null) marie.ExtinguishFlare(); return true; }
            if (trimmed.StartsWith("kelly.mood ")) { if (kelly != null) kelly.SetMoodByName(trimmed.Substring("kelly.mood ".Length).Trim()); return true; }
            if (trimmed == "kelly.protect") { if (kelly != null) kelly.ProtectNeighborhood(); return true; }
            if (trimmed == "johna.advice") { if (johna != null) johna.GiveAdvice(); return true; }
            if (trimmed == "rekr.cough") { if (rekr != null) rekr.CoughLavaSmoke(); return true; }
            if (trimmed == "rekr.remedy") { if (rekr != null) rekr.ReceiveRemedy(); return true; }
            if (trimmed == "manon.clean") { if (manon != null) manon.CleanUpChaos(); return true; }
            if (trimmed == "manon.mess") { if (manon != null) manon.NoticeMess(); return true; }

            return false;
        }
    }
}
