using UnityEngine;

namespace Elementborn.Game
{
    [CreateAssetMenu(menuName = "Elementborn/Feedback/Hit Feedback Definition", fileName = "HitFeedback")]
    public sealed class HitFeedbackDefinition : ScriptableObject
    {
        [SerializeField] private string feedbackId = "";
        [SerializeField] private HitFeedbackType feedbackType = HitFeedbackType.NormalHit;
        [SerializeField] private AbilityElementType element = AbilityElementType.Neutral;
        [SerializeField] private Sprite impactSprite;
        [SerializeField] private Color tint = Color.white;
        [SerializeField] private float scale = 1f;
        [SerializeField] private float lifetimeSeconds = 0.35f;
        [SerializeField] private float cameraShakeStrength = 0.12f;
        [SerializeField] private float hitPauseSeconds = 0.035f;
        [SerializeField] private bool flashTarget = true;
        [SerializeField] private bool faceCamera = true;

        public string FeedbackId => string.IsNullOrWhiteSpace(feedbackId) ? name : feedbackId;
        public HitFeedbackType FeedbackType => feedbackType;
        public AbilityElementType Element => element;
        public Sprite ImpactSprite => impactSprite;
        public Color Tint => tint;
        public float Scale => Mathf.Max(0.01f, scale);
        public float LifetimeSeconds => Mathf.Max(0.01f, lifetimeSeconds);
        public float CameraShakeStrength => Mathf.Max(0f, cameraShakeStrength);
        public float HitPauseSeconds => Mathf.Max(0f, hitPauseSeconds);
        public bool FlashTarget => flashTarget;
        public bool FaceCamera => faceCamera;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(feedbackId))
            {
                feedbackId = name;
            }

            scale = Mathf.Max(0.01f, scale);
            lifetimeSeconds = Mathf.Max(0.01f, lifetimeSeconds);
            cameraShakeStrength = Mathf.Max(0f, cameraShakeStrength);
            hitPauseSeconds = Mathf.Max(0f, hitPauseSeconds);
        }
    }
}
