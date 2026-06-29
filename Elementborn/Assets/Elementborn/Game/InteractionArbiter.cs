using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Compatibility interaction result used by older scaffold interactables.
    /// Newer prompt-based interactables can use BaseInteractable/PlayerInteractor directly.
    /// </summary>
    public readonly struct Interaction
    {
        public readonly float Distance;
        public readonly int Priority;
        public readonly string Label;
        public readonly Action Callback;

        public Interaction(float distance, int priority, string label, Action callback)
        {
            Distance = distance;
            Priority = priority;
            Label = label ?? "";
            Callback = callback;
        }

        public bool IsValid => Callback != null;

        public void Execute()
        {
            Callback?.Invoke();
        }

        public static Interaction None => new Interaction(float.PositiveInfinity, int.MinValue, "", null);
    }

    /// <summary>
    /// v51 canonical arbiter. It intentionally does NOT define IInteractable.
    /// IInteractable lives in Game/Interaction/IInteractable.cs as a marker interface.
    /// </summary>
    public static class InteractionArbiter
    {
        private static readonly HashSet<IInteractable> Registered = new HashSet<IInteractable>();

        public static void Register(IInteractable interactable)
        {
            if (interactable != null)
            {
                Registered.Add(interactable);
            }
        }

        public static void Unregister(IInteractable interactable)
        {
            if (interactable != null)
            {
                Registered.Remove(interactable);
            }
        }

        public static bool TryGetBestInteraction(Vector3 playerPosition, out Interaction best)
        {
            best = Interaction.None;
            bool found = false;

            foreach (IInteractable interactable in Registered)
            {
                if (TryGetInteractionFrom(interactable, playerPosition, out Interaction candidate) && candidate.IsValid)
                {
                    if (!found || candidate.Priority > best.Priority || 
                        (candidate.Priority == best.Priority && candidate.Distance < best.Distance))
                    {
                        best = candidate;
                        found = true;
                    }
                }
            }

            return found;
        }


        // v52 compatibility entry points used by older keyboard/VR input scripts.
        public static bool SignalInteract()
        {
            GameObject player = FindPlayer();
            if (player != null)
            {
                return SignalInteract(player.transform.position);
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                return SignalInteract(camera.transform.position);
            }

            return false;
        }

        public static bool SignalInteract(GameObject interactor)
        {
            if (interactor == null)
            {
                return SignalInteract();
            }

            return SignalInteract(interactor.transform.position);
        }

        public static bool SignalInteract(Transform interactor)
        {
            if (interactor == null)
            {
                return SignalInteract();
            }

            return SignalInteract(interactor.position);
        }

        public static bool SignalInteract(Vector3 playerPosition)
        {
            return TryInteractClosest(playerPosition);
        }

        private static GameObject FindPlayer()
        {
            try
            {
                GameObject tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null)
                {
                    return tagged;
                }
            }
            catch { }

            return GameObject.Find("Player Test Rig");
        }

        public static bool TryInteractClosest(Vector3 playerPosition)
        {
            if (TryGetBestInteraction(playerPosition, out Interaction interaction))
            {
                interaction.Execute();
                return true;
            }

            return false;
        }

        private static bool TryGetInteractionFrom(IInteractable interactable, Vector3 playerPosition, out Interaction interaction)
        {
            interaction = Interaction.None;
            if (interactable == null)
            {
                return false;
            }

            MethodInfo method = interactable.GetType().GetMethod(
                "TryGetInteraction",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(Vector3), typeof(Interaction).MakeByRefType() },
                null);

            if (method == null)
            {
                return false;
            }

            object[] args = { playerPosition, Interaction.None };
            bool result = false;
            try
            {
                object raw = method.Invoke(interactable, args);
                if (raw is bool ok)
                {
                    result = ok;
                }

                if (args.Length > 1 && args[1] is Interaction found)
                {
                    interaction = found;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("InteractionArbiter failed to query " + interactable.GetType().Name + ": " + ex.Message);
                return false;
            }

            return result;
        }
    }
}
