using System.Collections.Generic;
using UnityEngine;

namespace Elementborn.Game
{
    public enum ElementbornPrototypeModelAnimationMode
    {
        Idle,
        Swim,
        Hover,
        Combat,
        HitReact
    }

    /// <summary>
    /// Lightweight procedural animation layer for imported Meshy/FBX/GLB-style models.
    /// This does not require pre-authored clips. If a model has bones, it gently animates
    /// the hierarchy. If it has no useful bones, it still animates the root with bob/tilt.
    /// </summary>
    public sealed class ElementbornPrototypeImportedModelAnimator : MonoBehaviour
    {
        public ElementbornPrototypeModelAnimationMode mode = ElementbornPrototypeModelAnimationMode.Idle;

        [Header("Root Motion Feel")]
        public float bobAmplitude = 0.08f;
        public float bobSpeed = 1.8f;
        public float yawAmplitude = 3.5f;
        public float pitchAmplitude = 2.5f;
        public float rollAmplitude = 2.5f;

        [Header("Bone Motion Feel")]
        public float boneWiggleDegrees = 5.5f;
        public float boneWaveSpeed = 2.6f;
        public int maxAnimatedBones = 48;

        private readonly List<Transform> animatedBones = new List<Transform>();
        private readonly Dictionary<Transform, Quaternion> initialRotations = new Dictionary<Transform, Quaternion>();
        private Vector3 initialLocalPosition;
        private Quaternion initialLocalRotation;
        private float hitReactUntil;

        private void Awake()
        {
            Capture();
        }

        private void OnEnable()
        {
            Capture();
        }

        private void LateUpdate()
        {
            Capture();

            ElementbornPrototypeModelAnimationMode activeMode = Time.time < hitReactUntil
                ? ElementbornPrototypeModelAnimationMode.HitReact
                : mode;

            float speed = boneWaveSpeed;
            float rootBob = bobAmplitude;
            float boneDegrees = boneWiggleDegrees;
            float rootYaw = yawAmplitude;
            float rootPitch = pitchAmplitude;
            float rootRoll = rollAmplitude;

            switch (activeMode)
            {
                case ElementbornPrototypeModelAnimationMode.Swim:
                    speed *= 1.3f;
                    rootBob *= 1.25f;
                    boneDegrees *= 1.35f;
                    rootYaw *= 1.35f;
                    break;
                case ElementbornPrototypeModelAnimationMode.Hover:
                    speed *= 0.9f;
                    rootBob *= 1.8f;
                    boneDegrees *= 0.7f;
                    break;
                case ElementbornPrototypeModelAnimationMode.Combat:
                    speed *= 1.8f;
                    rootBob *= 0.85f;
                    boneDegrees *= 1.6f;
                    rootYaw *= 1.2f;
                    break;
                case ElementbornPrototypeModelAnimationMode.HitReact:
                    speed *= 3.0f;
                    rootBob *= 0.5f;
                    boneDegrees *= 2.4f;
                    rootPitch *= 2.6f;
                    rootRoll *= 2.0f;
                    break;
            }

            float t = Time.time * speed;
            transform.localPosition = initialLocalPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * rootBob);

            Quaternion rootOffset = Quaternion.Euler(
                Mathf.Sin(t * 0.77f) * rootPitch,
                Mathf.Sin(t * 0.49f) * rootYaw,
                Mathf.Sin(t * 0.63f) * rootRoll);

            transform.localRotation = initialLocalRotation * rootOffset;

            for (int i = 0; i < animatedBones.Count; i++)
            {
                Transform bone = animatedBones[i];
                if (bone == null || !initialRotations.ContainsKey(bone))
                {
                    continue;
                }

                float phase = t + i * 0.47f;
                float wave = Mathf.Sin(phase);
                float secondary = Mathf.Cos(phase * 0.73f);
                float weight = Mathf.Lerp(1f, 0.35f, i / Mathf.Max(1f, (float)animatedBones.Count));

                Quaternion offset = Quaternion.Euler(
                    wave * boneDegrees * weight,
                    secondary * boneDegrees * 0.55f * weight,
                    Mathf.Sin(phase * 1.17f) * boneDegrees * 0.35f * weight);

                bone.localRotation = initialRotations[bone] * offset;
            }
        }

        public void PlayHitReact()
        {
            hitReactUntil = Time.time + 0.35f;
        }

        public void SetMode(ElementbornPrototypeModelAnimationMode newMode)
        {
            mode = newMode;
        }

        private void Capture()
        {
            if (initialRotations.Count > 0)
            {
                return;
            }

            initialLocalPosition = transform.localPosition;
            initialLocalRotation = transform.localRotation;
            animatedBones.Clear();

            Transform[] all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform candidate = all[i];
                if (candidate == null || candidate == transform)
                {
                    continue;
                }

                // Avoid animating the visible mesh root too aggressively. Bone names from Meshy are
                // often Bone_###, but this also catches common rig naming.
                string lower = candidate.name.ToLowerInvariant();
                bool looksLikeBone =
                    lower.Contains("bone") ||
                    lower.Contains("joint") ||
                    lower.Contains("spine") ||
                    lower.Contains("tail") ||
                    lower.Contains("head") ||
                    lower.Contains("neck") ||
                    lower.Contains("arm") ||
                    lower.Contains("leg") ||
                    lower.Contains("fin") ||
                    lower.Contains("wing");

                if (!looksLikeBone)
                {
                    continue;
                }

                animatedBones.Add(candidate);
                initialRotations[candidate] = candidate.localRotation;

                if (animatedBones.Count >= maxAnimatedBones)
                {
                    break;
                }
            }

            // Fallback: animate a limited number of child transforms if the importer uses opaque names.
            if (animatedBones.Count == 0)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    Transform candidate = all[i];
                    if (candidate == null || candidate == transform)
                    {
                        continue;
                    }

                    if (candidate.GetComponent<Renderer>() != null)
                    {
                        continue;
                    }

                    animatedBones.Add(candidate);
                    initialRotations[candidate] = candidate.localRotation;

                    if (animatedBones.Count >= Mathf.Min(16, maxAnimatedBones))
                    {
                        break;
                    }
                }
            }
        }
    }
}
