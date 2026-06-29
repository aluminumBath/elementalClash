using UnityEngine;

namespace Elementborn.Game
{
    [RequireComponent(typeof(Collider))]
    public sealed class WorldEventTrigger : MonoBehaviour
    {
        [SerializeField] private WorldEventDefinition worldEvent;
        [SerializeField] private WorldEventTriggerMode triggerMode = WorldEventTriggerMode.OnPlayerEnterTrigger;
        [SerializeField] private bool once = true;

        private bool used;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void Start()
        {
            if (worldEvent == null) return;
            if (triggerMode == WorldEventTriggerMode.OnStart) Trigger("OnStart");
            else if (triggerMode == WorldEventTriggerMode.TimedDelay) WorldEventTracker.Schedule(worldEvent, "Timed trigger");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerMode != WorldEventTriggerMode.OnPlayerEnterTrigger) return;
            if (once && used) return;
            if (!other.CompareTag("Player")) return;
            Trigger("Player entered world event trigger");
        }

        public void Trigger(string reason = "")
        {
            if (worldEvent == null) return;
            used = true;
            WorldEventTracker.Activate(worldEvent, reason);
        }
    }
}
