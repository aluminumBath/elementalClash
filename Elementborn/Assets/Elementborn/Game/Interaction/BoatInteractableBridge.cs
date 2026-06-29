using System.Reflection;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Generic bridge to a BoatController without assuming exact method names.
    /// It tries common boarding method names via reflection, then falls back to UnityEvent behavior from BaseInteractable.
    /// </summary>
    public sealed class BoatInteractableBridge : BaseInteractable
    {
        [SerializeField] private MonoBehaviour boatController;
        [SerializeField] private string[] possibleBoardMethodNames =
        {
            "Board",
            "EnterBoat",
            "BeginPiloting",
            "SetPilot",
            "Mount",
            "Interact"
        };

        public override InteractionPromptData GetPrompt(GameObject interactor)
        {
            return InteractionPromptData.Simple("Boat", "Board");
        }

        public override void Interact(GameObject interactor)
        {
            bool handled = TryInvokeBoat(interactor);
            PlayerMapMarkerTracker.ReportBoat(transform.position, currentlyRidden: handled);
            base.Interact(interactor);
        }

        private bool TryInvokeBoat(GameObject interactor)
        {
            if (boatController == null)
            {
                boatController = GetComponent<MonoBehaviour>();
            }

            if (boatController == null)
            {
                return false;
            }

            var type = boatController.GetType();

            foreach (string methodName in possibleBoardMethodNames)
            {
                var methodWithGameObject = type.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(GameObject) },
                    null);

                if (methodWithGameObject != null)
                {
                    methodWithGameObject.Invoke(boatController, new object[] { interactor });
                    return true;
                }

                var methodNoArgs = type.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    System.Type.EmptyTypes,
                    null);

                if (methodNoArgs != null)
                {
                    methodNoArgs.Invoke(boatController, null);
                    return true;
                }
            }

            return false;
        }
    }
}
