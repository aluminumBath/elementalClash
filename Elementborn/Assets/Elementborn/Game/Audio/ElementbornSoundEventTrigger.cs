using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornSoundEventTrigger : MonoBehaviour
    {
        [SerializeField] private ElementbornSoundEventId eventId = ElementbornSoundEventId.UiConfirm;
        [SerializeField] private bool playAtTransform = false;

        public void Play()
        {
            if (playAtTransform)
            {
                ElementbornAudioService.PlayAt(eventId, transform.position);
            }
            else
            {
                ElementbornAudioService.Play(eventId);
            }
        }

        public void PlayAtSelf()
        {
            ElementbornAudioService.PlayAt(eventId, transform.position);
        }

        public void SetEvent(ElementbornSoundEventId value)
        {
            eventId = value;
        }
    }
}
