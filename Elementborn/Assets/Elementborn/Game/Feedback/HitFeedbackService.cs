using UnityEngine;

namespace Elementborn.Game
{
    public sealed class HitFeedbackService : MonoBehaviour
    {
        public static HitFeedbackService Instance { get; private set; }

        [SerializeField] private HitFeedbackDefinition defaultHit;
        [SerializeField] private HitFeedbackDefinition criticalHit;
        [SerializeField] private HitFeedbackDefinition blockHit;
        [SerializeField] private HitFeedbackDefinition dodgeHit;
        [SerializeField] private HitFeedbackDefinition fireHit;
        [SerializeField] private HitFeedbackDefinition waterHit;
        [SerializeField] private HitFeedbackDefinition earthHit;
        [SerializeField] private HitFeedbackDefinition airHit;
        [SerializeField] private HitFeedbackDefinition iceHit;
        [SerializeField] private HitFeedbackDefinition lightningHit;
        [SerializeField] private HitFeedbackDefinition healHit;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static HitFeedbackService Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var go = new GameObject(nameof(HitFeedbackService));
            return go.AddComponent<HitFeedbackService>();
        }

        public static void Play(GameObject target, Vector3 position, AbilityElementType element, HitFeedbackType type, bool critical = false)
        {
            Ensure().PlayInternal(target, position, element, type, critical);
        }

        public void PlayInternal(GameObject target, Vector3 position, AbilityElementType element, HitFeedbackType type, bool critical = false)
        {
            HitFeedbackDefinition definition = SelectDefinition(element, type, critical);
            if (definition == null)
            {
                SpawnFallback(position, element, type, critical);
                return;
            }

            SpawnSprite(position, definition);
            if (definition.FlashTarget && target != null)
            {
                HitFlashController flash = target.GetComponent<HitFlashController>();
                if (flash == null)
                {
                    flash = target.AddComponent<HitFlashController>();
                }
                flash.Flash();
            }

            CharacterHitReactionController reaction = target != null ? target.GetComponent<CharacterHitReactionController>() : null;
            if (reaction != null)
            {
                reaction.React(type);
            }

            CameraShakeImpulse.Shake(definition.CameraShakeStrength);
            HitPauseController.Pulse(definition.HitPauseSeconds);
            PlayAudioForFeedback(position, element, type, critical);
        }

        private void PlayAudioForFeedback(Vector3 position, AbilityElementType element, HitFeedbackType type, bool critical)
        {
            if (critical)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.HitCritical, position);
                return;
            }

            if (type == HitFeedbackType.Block)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.BlockClank, position);
                return;
            }

            if (type == HitFeedbackType.PerfectBlock)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.PerfectBlock, position);
                return;
            }

            if (type == HitFeedbackType.Dodge)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.DodgePuff, position);
                return;
            }

            if (type == HitFeedbackType.Heal)
            {
                ElementbornAudioService.PlayAt(ElementbornSoundEventId.HealingBloom, position);
                return;
            }

            switch (element)
            {
                case AbilityElementType.Fire:
                    ElementbornAudioService.PlayAt(ElementbornSoundEventId.SpellFire, position);
                    break;
                case AbilityElementType.Water:
                    ElementbornAudioService.PlayAt(ElementbornSoundEventId.SpellWater, position);
                    break;
                case AbilityElementType.Earth:
                    ElementbornAudioService.PlayAt(ElementbornSoundEventId.SpellEarth, position);
                    break;
                case AbilityElementType.Air:
                    ElementbornAudioService.PlayAt(ElementbornSoundEventId.SpellAir, position);
                    break;
                case AbilityElementType.Ice:
                    ElementbornAudioService.PlayAt(ElementbornSoundEventId.SpellIce, position);
                    break;
                case AbilityElementType.Lightning:
                    ElementbornAudioService.PlayAt(ElementbornSoundEventId.SpellLightning, position);
                    break;
                default:
                    ElementbornAudioService.PlayAt(ElementbornSoundEventId.HitSlash, position);
                    break;
            }
        }

        public void SetDefinitions(
            HitFeedbackDefinition normal,
            HitFeedbackDefinition critical,
            HitFeedbackDefinition block,
            HitFeedbackDefinition dodge,
            HitFeedbackDefinition fire,
            HitFeedbackDefinition water,
            HitFeedbackDefinition earth,
            HitFeedbackDefinition air,
            HitFeedbackDefinition ice,
            HitFeedbackDefinition lightning,
            HitFeedbackDefinition heal)
        {
            defaultHit = normal;
            criticalHit = critical;
            blockHit = block;
            dodgeHit = dodge;
            fireHit = fire;
            waterHit = water;
            earthHit = earth;
            airHit = air;
            iceHit = ice;
            lightningHit = lightning;
            healHit = heal;
        }

        private HitFeedbackDefinition SelectDefinition(AbilityElementType element, HitFeedbackType type, bool critical)
        {
            if (critical && criticalHit != null) return criticalHit;
            if ((type == HitFeedbackType.Block || type == HitFeedbackType.PerfectBlock) && blockHit != null) return blockHit;
            if (type == HitFeedbackType.Dodge && dodgeHit != null) return dodgeHit;
            if (type == HitFeedbackType.Heal && healHit != null) return healHit;

            switch (element)
            {
                case AbilityElementType.Fire: return fireHit != null ? fireHit : defaultHit;
                case AbilityElementType.Water: return waterHit != null ? waterHit : defaultHit;
                case AbilityElementType.Earth: return earthHit != null ? earthHit : defaultHit;
                case AbilityElementType.Air: return airHit != null ? airHit : defaultHit;
                case AbilityElementType.Ice: return iceHit != null ? iceHit : defaultHit;
                case AbilityElementType.Lightning: return lightningHit != null ? lightningHit : defaultHit;
                default: return defaultHit;
            }
        }

        private void SpawnFallback(Vector3 position, AbilityElementType element, HitFeedbackType type, bool critical)
        {
            GameObject go = new GameObject("Impact Feedback Fallback");
            go.transform.position = position + Vector3.up * 1.2f;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.color = critical ? Color.yellow : Color.white;
            var effect = go.AddComponent<ImpactSpriteEffect>();
            effect.Configure(null, renderer.color, critical ? 1.4f : 1f, 0.25f, true);
        }

        private void SpawnSprite(Vector3 position, HitFeedbackDefinition definition)
        {
            GameObject go = new GameObject("Impact Feedback - " + definition.FeedbackId);
            go.transform.position = position + Vector3.up * 1.1f;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 100;
            var effect = go.AddComponent<ImpactSpriteEffect>();
            effect.Configure(definition.ImpactSprite, definition.Tint, definition.Scale, definition.LifetimeSeconds, definition.FaceCamera);
        }
    }
}
