using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Cheat/admin command bridge for enemy AI.
    ///
    /// Commands:
    /// ai.state state
    /// ai.target.player
    /// ai.clear.target
    /// ai.stun
    /// ai.resume
    /// ai.kill
    /// </summary>
    public sealed class AdminEnemyAiCommandBridge : MonoBehaviour
    {
        [SerializeField] private EnemyCombatBrain targetBrain;

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return false;
            }

            if (targetBrain == null)
            {
                targetBrain = GetComponent<EnemyCombatBrain>();
            }

            if (targetBrain == null)
            {
                return false;
            }

            string trimmed = command.Trim();

            if (trimmed.StartsWith("ai.state "))
            {
                string stateText = trimmed.Substring("ai.state ".Length).Trim();
                if (System.Enum.TryParse(stateText, true, out EnemyAiState state))
                {
                    targetBrain.ForceState(state);
                }
                return true;
            }

            if (trimmed == "ai.target.player")
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                targetBrain.ForceTarget(player != null ? player.transform : null);
                return true;
            }

            if (trimmed == "ai.clear.target")
            {
                targetBrain.ForceTarget(null);
                return true;
            }

            if (trimmed == "ai.stun")
            {
                targetBrain.ForceState(EnemyAiState.Stunned);
                return true;
            }

            if (trimmed == "ai.resume")
            {
                targetBrain.ForceState(EnemyAiState.Idle);
                return true;
            }

            if (trimmed == "ai.kill")
            {
                SimpleCombatHealth health = targetBrain.GetComponent<SimpleCombatHealth>();
                if (health != null)
                {
                    health.ApplyDamage(health.CurrentHealth + 99999f);
                }
                return true;
            }

            return false;
        }
    }
}
