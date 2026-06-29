using System;
using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    [Serializable]
    public class ElementbornAudioBusVolume
    {
        public ElementbornSoundCategory Category = ElementbornSoundCategory.Master;
        [Range(0f, 1f)]
        public float Volume = 1f;
    }

    [CreateAssetMenu(menuName = "Elementborn/Audio/Audio Bus Settings", fileName = "AudioBusSettings")]
    public sealed class ElementbornAudioBusSettings : ScriptableObject
    {
        [SerializeField] private List<ElementbornAudioBusVolume> volumes = new List<ElementbornAudioBusVolume>();

        public float GetVolume(ElementbornSoundCategory category)
        {
            float master = GetRaw(ElementbornSoundCategory.Master, 1f);
            if (category == ElementbornSoundCategory.Master)
            {
                return master;
            }

            return master * GetRaw(category, 1f);
        }

        private float GetRaw(ElementbornSoundCategory category, float fallback)
        {
            var entry = volumes.Find(v => v != null && v.Category == category);
            return entry != null ? Mathf.Clamp01(entry.Volume) : fallback;
        }
    }
}
