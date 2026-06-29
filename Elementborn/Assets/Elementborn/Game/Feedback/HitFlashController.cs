using System.Collections;
using UnityEngine;

namespace Elementborn.Game
{
    public sealed class HitFlashController : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Color flashColor = new Color(1f, 0.95f, 0.55f, 1f);
        [SerializeField] private float flashSeconds = 0.08f;

        private Coroutine routine;
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }
        }

        public void Flash()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            ApplyFlash(1f);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, flashSeconds));
            ApplyFlash(0f);
            routine = null;
        }

        private void ApplyFlash(float amount)
        {
            if (renderers == null)
            {
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(block);
                block.SetColor("_EmissionColor", flashColor * amount);
                block.SetColor("_BaseColor", Color.Lerp(Color.white, flashColor, amount * 0.35f));
                renderer.SetPropertyBlock(block);
            }
        }
    }
}
