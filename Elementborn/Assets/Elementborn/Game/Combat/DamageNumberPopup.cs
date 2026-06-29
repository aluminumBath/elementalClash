using UnityEngine;

namespace Elementborn.Game
{
    public sealed class DamageNumberPopup : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1.1f;
        [SerializeField] private float riseSpeed = 1.2f;
        private TextMesh textMesh;

        private void Awake()
        {
            textMesh = gameObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.1f;
            textMesh.fontSize = 56;
        }

        public void Initialize(string text, Color color)
        {
            if (textMesh == null) textMesh = GetComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;
        }

        private void Update()
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            if (Camera.main != null) transform.forward = Camera.main.transform.forward;
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Destroy(gameObject);
        }
    }
}
