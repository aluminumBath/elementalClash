using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// VR comfort tunnelling: a camera-facing quad whose periphery darkens as you move, easing motion
    /// sickness during smooth locomotion. The mask is generated in code (clear centre, opaque edge) and
    /// its strength is driven by <see cref="SetIntensity"/> (e.g., from a mount's speed). Put this on the
    /// player's camera object; locomotion scripts find it via <c>GetComponentInChildren</c>.
    /// </summary>
    public sealed class ComfortVignette : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField, Range(0f, 1f)] private float maxStrength = 0.7f;
        [SerializeField] private float responsiveness = 6f;
        [SerializeField] private float innerRadius = 0.55f;
        [SerializeField] private float outerRadius = 0.95f;

        private Material _material;
        private float _current;
        private float _target;
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        private void Awake()
        {
            if (targetCamera == null) targetCamera = GetComponentInChildren<Camera>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) { enabled = false; return; }

            var shader = Shader.Find("Elementborn/ComfortVignette");
            if (shader == null) { enabled = false; return; }

            _material = new Material(shader);
            _material.SetTexture("_MainTex", BuildMask(256));
            _material.SetFloat(IntensityId, 0f);

            BuildQuad();
        }

        /// <summary>Set the desired vignette strength (0..1); it eases toward this each frame.</summary>
        public void SetIntensity(float t) => _target = Mathf.Clamp01(t) * maxStrength;

        private void Update()
        {
            float target = SettingsStore.Current.comfortVignette ? _target : 0f;
            _current = Mathf.MoveTowards(_current, target, responsiveness * Time.deltaTime);
            if (_material != null) _material.SetFloat(IntensityId, _current);
        }

        private void BuildQuad()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "ComfortVignetteQuad";
            var col = quad.GetComponent<Collider>();
            if (col != null) Destroy(col);

            quad.transform.SetParent(targetCamera.transform, false);
            quad.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            quad.transform.localRotation = Quaternion.identity;
            quad.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

            quad.GetComponent<MeshRenderer>().sharedMaterial = _material;
        }

        private Texture2D BuildMask(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy); // 0 centre .. ~1.41 corner
                    float t = Mathf.Clamp01((d - innerRadius) / (outerRadius - innerRadius));
                    float alpha = t * t * (3f - 2f * t); // smoothstep
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
                }
            }
            tex.Apply();
            return tex;
        }
    }
}
