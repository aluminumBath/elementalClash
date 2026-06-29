using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornAudioService : MonoBehaviour
    {
        public static ElementbornAudioService Instance { get; private set; }

        [SerializeField] private ElementbornAudioBusSettings busSettings;
        [SerializeField] private List<ElementbornSoundEventDefinition> events = new List<ElementbornSoundEventDefinition>();
        [SerializeField] private AudioSource twoDimensionalSource;
        [SerializeField] private int pooledSources = 12;

        private readonly List<AudioSource> pool = new List<AudioSource>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
        }

        public static ElementbornAudioService Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(ElementbornAudioService));
            return go.AddComponent<ElementbornAudioService>();
        }

        public static void Play(ElementbornSoundEventId eventId)
        {
            Ensure().Play2D(eventId);
        }

        public static void PlayAt(ElementbornSoundEventId eventId, Vector3 position)
        {
            Ensure().Play3D(eventId, position);
        }

        public void SetEvents(List<ElementbornSoundEventDefinition> definitions, ElementbornAudioBusSettings settings)
        {
            events = definitions ?? new List<ElementbornSoundEventDefinition>();
            busSettings = settings;
            EnsureSources();
        }

        public void Play2D(ElementbornSoundEventId eventId)
        {
            var def = Find(eventId);
            if (def == null)
            {
                return;
            }

            AudioClip clip = def.GetRandomClip();
            if (clip == null)
            {
                return;
            }

            EnsureSources();
            twoDimensionalSource.pitch = Random.Range(def.PitchMin, def.PitchMax);
            twoDimensionalSource.spatialBlend = 0f;
            twoDimensionalSource.PlayOneShot(clip, def.Volume * GetBus(def.Category));
        }

        public void Play3D(ElementbornSoundEventId eventId, Vector3 position)
        {
            var def = Find(eventId);
            if (def == null)
            {
                return;
            }

            AudioClip clip = def.GetRandomClip();
            if (clip == null)
            {
                return;
            }

            AudioSource source = GetPooledSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = def.Volume * GetBus(def.Category);
            source.pitch = Random.Range(def.PitchMin, def.PitchMax);
            source.spatialBlend = def.Spatial ? 1f : 0f;
            source.minDistance = def.MinDistance;
            source.maxDistance = def.MaxDistance;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.Play();
        }

        private ElementbornSoundEventDefinition Find(ElementbornSoundEventId eventId)
        {
            return events.Find(e => e != null && e.EventId == eventId);
        }

        private float GetBus(ElementbornSoundCategory category)
        {
            return busSettings != null ? busSettings.GetVolume(category) : 1f;
        }

        private void EnsureSources()
        {
            if (twoDimensionalSource == null)
            {
                twoDimensionalSource = gameObject.GetComponent<AudioSource>();
                if (twoDimensionalSource == null)
                {
                    twoDimensionalSource = gameObject.AddComponent<AudioSource>();
                }
                twoDimensionalSource.spatialBlend = 0f;
                twoDimensionalSource.playOnAwake = false;
            }

            while (pool.Count < Mathf.Max(1, pooledSources))
            {
                var child = new GameObject("Pooled Audio Source");
                child.transform.SetParent(transform, false);
                var source = child.AddComponent<AudioSource>();
                source.playOnAwake = false;
                pool.Add(source);
            }
        }

        private AudioSource GetPooledSource()
        {
            EnsureSources();

            foreach (AudioSource source in pool)
            {
                if (source != null && !source.isPlaying)
                {
                    return source;
                }
            }

            return pool[0];
        }
    }
}
