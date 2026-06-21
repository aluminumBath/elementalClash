using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Builds ability projectiles entirely in code, so there is no binary prefab to ship and
    /// the loop works the moment you press play. Color and scale follow the ability variant
    /// and element. Assign a hand-made prefab in <see cref="AbilityVfxBinder"/> to replace
    /// this with designer art later.
    /// </summary>
    public static class ProceduralProjectiles
    {
        public static GameObject BuildProjectile(AbilityOutcome outcome)
        {
            Color color = ColorFor(outcome.Variant, outcome.Element);
            float radius = RadiusFor(outcome.Variant, outcome.Element);

            var go = new GameObject($"Projectile_{outcome.Element}_{outcome.Variant}");

            var sphere = go.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = radius;

            var body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            var light = go.AddComponent<Light>();
            light.color = color;
            light.range = radius * 12f;
            light.intensity = 2.2f;

            var ps = go.AddComponent<ParticleSystem>();
            ConfigureParticles(ps, color, radius, outcome.Variant);

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = radius * 1.5f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);

            return go;
        }

        private static void ConfigureParticles(ParticleSystem ps, Color color, float radius, AbilityVariant variant)
        {
            var main = ps.main;
            main.startColor = color;
            main.startSize = radius * 2f;
            main.startLifetime = 0.35f;
            main.startSpeed = 0.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = variant == AbilityVariant.Lightning ? 120f : 60f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius * 0.5f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default")) { color = color };
        }

        private static Color ColorFor(AbilityVariant variant, Element element)
        {
            switch (variant)
            {
                case AbilityVariant.Magmacraft: return new Color(1f, 0.35f, 0.05f);
                case AbilityVariant.Lightning: return new Color(0.6f, 0.8f, 1f);
                case AbilityVariant.Ice: return new Color(0.6f, 0.9f, 1f);
                case AbilityVariant.Oreshaping: return new Color(0.7f, 0.72f, 0.78f);
            }
            switch (element)
            {
                case Element.Fire: return new Color(1f, 0.55f, 0.15f);
                case Element.Water: return new Color(0.2f, 0.5f, 1f);
                case Element.Earth: return new Color(0.6f, 0.45f, 0.25f);
                case Element.Air: return new Color(0.85f, 0.95f, 0.9f);
                default: return Color.white;
            }
        }

        private static float RadiusFor(AbilityVariant variant, Element element)
        {
            switch (variant)
            {
                case AbilityVariant.Lightning: return 0.12f;
                case AbilityVariant.Ice: return 0.18f;
                case AbilityVariant.Magmacraft: return 0.28f;
                case AbilityVariant.Oreshaping: return 0.14f;
            }
            return element == Element.Earth ? 0.3f : 0.22f;
        }
    }
}
