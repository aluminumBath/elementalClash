using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Marker interface for objects that can be discovered by PlayerInteractor.
    ///
    /// v50 deliberately makes this a marker interface so older scaffold objects that
    /// already implemented IInteractable do not fail compilation after the newer
    /// prompt-based interaction system was introduced. Runtime invocation is handled
    /// through InteractableCompatibility, which supports both the new BaseInteractable
    /// methods and older no-argument / InteractionArbiter-style objects.
    /// </summary>
    public interface IInteractable
    {
    }
}
