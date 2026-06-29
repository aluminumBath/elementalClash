using UnityEngine;

namespace Elementborn.Game
{
    public sealed class CampInteractable : BaseInteractable
    {
        [SerializeField] private string campName = "Field Camp";
        [SerializeField] private bool markOnMap = true;
        [SerializeField] private bool saveMapMarkersOnUse = true;
        [SerializeField] private MapMarkerSaveBridge saveBridge;

        private void Start()
        {
            if (markOnMap)
            {
                PlayerMapMarkerTracker.ReportCamp(transform.position, campName);
            }
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(campName, "Rest / Save");
        }

        public override void Interact(GameObject interactor)
        {
            if (saveMapMarkersOnUse)
            {
                if (saveBridge == null)
                {
                    saveBridge = ElementbornFindUtility.FindFirst<MapMarkerSaveBridge>();
                }

                if (saveBridge != null)
                {
                    saveBridge.SaveCurrentSlot();
                }
            }

            base.Interact(interactor);
        }
    }
}
