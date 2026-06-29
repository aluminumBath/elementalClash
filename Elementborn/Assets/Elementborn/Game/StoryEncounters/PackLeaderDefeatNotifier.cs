using UnityEngine;

namespace Elementborn.Game
{
    public sealed class PackLeaderDefeatNotifier : MonoBehaviour
    {
        [SerializeField] private TimedDualLeaderPackRespawnController packController;
        [SerializeField] private string leaderId = "romilus";
        [SerializeField] private bool notifyOnDisable = false;

        public void NotifyDefeated()
        {
            if (packController != null)
            {
                packController.NotifyLeaderDefeated(leaderId);
            }
        }

        private void OnDisable()
        {
            if (notifyOnDisable)
            {
                NotifyDefeated();
            }
        }
    }
}
