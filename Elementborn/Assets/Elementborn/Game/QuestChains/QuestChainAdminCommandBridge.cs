using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Commands:
    /// chain.summary
    /// chain.start chainId
    /// chain.stage chainId|stageId
    /// chain.complete chainId|stageId
    /// chain.choice chainId|stageId|choiceId
    /// </summary>
    public sealed class QuestChainAdminCommandBridge : MonoBehaviour
    {
        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            string trimmed = command.Trim();
            var director = QuestChainDirector.Ensure();

            if (trimmed == "chain.summary")
            {
                Debug.Log(director.BuildSummary());
                return true;
            }

            if (trimmed.StartsWith("chain.start "))
            {
                director.StartChain(trimmed.Substring("chain.start ".Length).Trim());
                return true;
            }

            if (trimmed.StartsWith("chain.stage "))
            {
                string[] parts = trimmed.Substring("chain.stage ".Length).Split('|');
                if (parts.Length >= 2)
                {
                    director.StartStage(parts[0].Trim(), parts[1].Trim());
                }
                return true;
            }

            if (trimmed.StartsWith("chain.complete "))
            {
                string[] parts = trimmed.Substring("chain.complete ".Length).Split('|');
                if (parts.Length >= 2)
                {
                    director.CompleteStage(parts[0].Trim(), parts[1].Trim());
                }
                return true;
            }

            if (trimmed.StartsWith("chain.choice "))
            {
                string[] parts = trimmed.Substring("chain.choice ".Length).Split('|');
                if (parts.Length >= 3)
                {
                    director.ApplyChoice(parts[0].Trim(), parts[1].Trim(), parts[2].Trim());
                }
                return true;
            }

            return false;
        }
    }
}
