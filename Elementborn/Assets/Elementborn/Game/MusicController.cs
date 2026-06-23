using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Starts the looping ambient music bed on <see cref="AudioController"/>'s ambient channel (driven by the
    /// music-volume setting), and stops it when destroyed. Put one on a bootstrap object (the scene adds it).
    /// </summary>
    public sealed class MusicController : MonoBehaviour
    {
        [SerializeField] private SfxKind track = SfxKind.MusicCalm;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        private void Start()
        {
            AudioController.EnsureInstance().Ambient(track, volume);
        }

        private void OnDestroy()
        {
            if (AudioController.Instance != null) AudioController.Instance.StopAmbient();
        }
    }
}
