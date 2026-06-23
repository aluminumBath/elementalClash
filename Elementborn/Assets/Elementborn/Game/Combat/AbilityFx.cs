using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>
    /// Procedural, self-cleaning VFX for the channeled melee shapes — the Sweep fan and the Heavy telegraph ring +
    /// impact burst — built entirely in code so there is no binary prefab to ship (the same approach as
    /// <see cref="ProceduralProjectiles"/>). Colours come from <see cref="AbilityPalette"/>: element primary with
    /// the sub-art as the secondary/accent. Hand-made VFX can replace these later; this just makes the moves
    /// visible now. (Appearance is a placeholder — tune freely.)
    /// </summary>
    public static class AbilityFx
    {
        private static Material SpriteMat() => new Material(Shader.Find("Sprites/Default"));

        private static Vector3 SafeDir(Vector3 d) => d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;

        /// <summary>A fast, wide fan of particles sweeping forward across the arc (instant, self-destructs).</summary>
        public static void SpawnSweepFan(Vector3 origin, Vector3 dir, Element element, AbilityVariant variant)
        {
            Color primary = AbilityPalette.Primary(element);
            Color accent = AbilityPalette.Secondary(variant, element);

            var go = new GameObject($"SweepFan_{element}");
            go.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(SafeDir(dir)));

            var light = go.AddComponent<Light>();
            light.color = primary;
            light.range = SweepArc.Range * 3f;
            light.intensity = 2.5f;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(primary, accent);
            main.startLifetime = 0.25f;
            main.startSpeed = SweepArc.Range / 0.18f; // reach the arc edge fast
            main.startSize = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)40) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = SweepArc.HalfAngleDegrees; // matches the 120-degree fan
            shape.radius = 0.2f;

            Object.Destroy(go, 0.4f);
        }

        /// <summary>
        /// Creates the ground telegraph ring at the impact point. It starts empty; drive it with
        /// <see cref="SetRingFill"/> each frame so the circle sweeps closed (fills) as the strike arcs in.
        /// </summary>
        public static GameObject SpawnTelegraphRing(Vector3 point, Element element)
        {
            var go = new GameObject($"HeavyTelegraph_{element}");
            go.transform.position = point;

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = false;
            lr.widthMultiplier = 0.14f;
            lr.material = SpriteMat();
            lr.startColor = lr.endColor = AbilityPalette.Primary(element);
            lr.positionCount = 0;
            return go;
        }

        /// <summary>Redraws the telegraph ring to show <paramref name="fill01"/> (0..1) of the circle — a clock-sweep fill.</summary>
        public static void SetRingFill(GameObject ring, float radius, float fill01)
        {
            if (ring == null) return;
            var lr = ring.GetComponent<LineRenderer>();
            if (lr == null) return;

            fill01 = Mathf.Clamp01(fill01);
            const int total = 64;
            int n = Mathf.Max(2, Mathf.RoundToInt(total * fill01));
            lr.positionCount = n;
            for (int i = 0; i < n; i++)
            {
                float a = (i / (float)total) * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0.03f, Mathf.Sin(a) * radius));
            }
            lr.loop = fill01 >= 0.999f; // close the loop once full
        }

        /// <summary>A glowing projectile that the caller moves along the strike's arc toward the impact point.</summary>
        public static GameObject SpawnHeavyTravel(Vector3 start, Element element, AbilityVariant variant)
        {
            Color primary = AbilityPalette.Primary(element);
            Color accent = AbilityPalette.Secondary(variant, element);

            var go = new GameObject($"HeavyTravel_{element}");
            go.transform.position = start;

            var light = go.AddComponent<Light>();
            light.color = primary;
            light.range = 5f;
            light.intensity = 3f;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(primary, accent);
            main.startLifetime = 0.3f;
            main.startSpeed = 0.4f;
            main.startSize = 0.45f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.5f;
            trail.endWidth = 0f;
            trail.material = SpriteMat();
            trail.startColor = primary;
            trail.endColor = new Color(primary.r, primary.g, primary.b, 0f);

            return go;
        }

        /// <summary>The flash + burst when a Heavy lands (self-destructs).</summary>
        public static void SpawnImpactBurst(Vector3 point, float radius, Element element, AbilityVariant variant)
        {
            Color primary = AbilityPalette.Primary(element);
            Color accent = AbilityPalette.Secondary(variant, element);

            var go = new GameObject($"HeavyImpact_{element}");
            go.transform.position = point;

            var light = go.AddComponent<Light>();
            light.color = primary;
            light.range = radius * 8f;
            light.intensity = 4f;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(primary, accent);
            main.startLifetime = 0.45f;
            main.startSpeed = radius * 4f;
            main.startSize = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)60) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = radius * 0.5f;

            Object.Destroy(go, 0.6f);
        }
    }
}
