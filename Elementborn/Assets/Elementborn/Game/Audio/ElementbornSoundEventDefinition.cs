using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Audio/Sound Event", fileName = "SoundEvent")]
    public sealed class ElementbornSoundEventDefinition : ScriptableObject
    {
        [SerializeField] private ElementbornSoundEventId eventId = ElementbornSoundEventId.None;
        [SerializeField] private ElementbornSoundCategory category = ElementbornSoundCategory.World;
        [SerializeField] private AudioClip[] clips;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitchMin = 0.96f;
        [SerializeField] private float pitchMax = 1.04f;
        [SerializeField] private bool spatial = false;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 20f;

        public ElementbornSoundEventId EventId => eventId;
        public ElementbornSoundCategory Category => category;
        public float Volume => Mathf.Clamp01(volume);
        public float PitchMin => Mathf.Min(pitchMin, pitchMax);
        public float PitchMax => Mathf.Max(pitchMin, pitchMax);
        public bool Spatial => spatial;
        public float MinDistance => Mathf.Max(0.01f, minDistance);
        public float MaxDistance => Mathf.Max(MinDistance, maxDistance);

        public AudioClip GetRandomClip()
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            return clips[Random.Range(0, clips.Length)];
        }

        private void OnValidate()
        {
            volume = Mathf.Clamp01(volume);
            minDistance = Mathf.Max(0.01f, minDistance);
            maxDistance = Mathf.Max(minDistance, maxDistance);
        }
    }
}
