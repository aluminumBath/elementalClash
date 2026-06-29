using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Attach to tavern boards, gossiping NPCs, message bottles, wanted posters, etc.
    /// </summary>
    public sealed class RumorSourceInteractable : BaseInteractable
    {
        [SerializeField] private string sourceName = "Rumor Source";
        [TextArea]
        [SerializeField] private string rumorText = "There is something strange nearby.";
        [SerializeField] private RumorType rumorType = RumorType.Unknown;
        [SerializeField] private string region = "";
        [SerializeField] private bool important;
        [SerializeField] private bool hasMapLocation;
        [SerializeField] private Transform mapLocation;

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple(sourceName, "Listen");
        }

        public override void Interact(GameObject interactor)
        {
            Vector3 position = mapLocation != null ? mapLocation.position : transform.position;

            RumorTracker.AddRumor(
                rumorText,
                rumorType,
                sourceName,
                region,
                important,
                isTrue: true,
                worldPosition: position,
                hasWorldPosition: hasMapLocation);

            Debug.Log($"Rumor learned: {rumorText}");
            base.Interact(interactor);
        }
    }
}
