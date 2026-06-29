using UnityEngine;

namespace Elementborn.Game
{
    public sealed class DoorInteractable : BaseInteractable
    {
        [SerializeField] private Transform doorVisual;
        [SerializeField] private Vector3 openEuler = new Vector3(0f, 90f, 0f);
        [SerializeField] private float openSpeed = 8f;
        [SerializeField] private bool isOpen;
        [SerializeField] private bool locked;
        [SerializeField] private string lockedMessage = "Locked";

        private Quaternion closedRotation;
        private Quaternion openRotation;

        private void Awake()
        {
            if (doorVisual == null)
            {
                doorVisual = transform;
            }

            closedRotation = doorVisual.localRotation;
            openRotation = closedRotation * Quaternion.Euler(openEuler);
        }

        private void Update()
        {
            if (doorVisual == null)
            {
                return;
            }

            Quaternion target = isOpen ? openRotation : closedRotation;
            doorVisual.localRotation = Quaternion.Slerp(doorVisual.localRotation, target, Time.deltaTime * openSpeed);
        }

        public override bool CanInteract(GameObject interactor)
        {
            return base.CanInteract(interactor);
        }

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            if (locked)
            {
                return InteractionPromptData.Simple("Door", lockedMessage);
            }

            return InteractionPromptData.Simple("Door", isOpen ? "Close" : "Open");
        }

        public override void Interact(GameObject interactor)
        {
            if (locked)
            {
                Debug.Log(lockedMessage);
                return;
            }

            isOpen = !isOpen;
            base.Interact(interactor);
        }

        public void SetLocked(bool value)
        {
            locked = value;
        }
    }
}
