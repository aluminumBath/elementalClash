using UnityEngine;

namespace Elementborn.Game
{
    public sealed class NpcVoicePlaybackController : MonoBehaviour
    {
        [SerializeField] private NpcWorldEntryDefinition npc;
        [SerializeField] private AudioSource localSource;
        [SerializeField] private bool spatialVoice = true;

        private void Awake()
        {
            if (localSource == null)
            {
                localSource = GetComponent<AudioSource>();
                if (localSource == null)
                {
                    localSource = gameObject.AddComponent<AudioSource>();
                }
            }

            localSource.playOnAwake = false;
            localSource.spatialBlend = spatialVoice ? 1f : 0f;
        }

        public void PlayGreeting()
        {
            Play(NpcVoiceLineType.Greeting);
        }

        public void Play(NpcVoiceLineType type)
        {
            if (npc == null)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.NpcVoiceWarm, transform.position);
                return;
            }

            foreach (var line in npc.VoiceLines)
            {
                if (line == null || line.LineType != type)
                {
                    continue;
                }

                if (line.PlaceholderClip != null && localSource != null)
                {
                    localSource.clip = line.PlaceholderClip;
                    localSource.Play();
                    if (!string.IsNullOrWhiteSpace(line.Subtitle))
                    {
                        NotificationFeed.Post($"{npc.DisplayName}: {line.Subtitle}", NotificationType.Dialogue);
                    }
                    return;
                }
            }

            ElementbornAudioService.PlayAt(npc.DefaultVoiceSound, transform.position);
        }

        public void SetNpc(NpcWorldEntryDefinition value)
        {
            npc = value;
        }
    }
}
