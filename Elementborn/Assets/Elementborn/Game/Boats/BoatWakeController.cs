using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Simple procedural wake. Drop it on the boat and assign two stern points, or let it create a default
    /// single wake. Emission scales with boat speed and shuts off at rest.
    /// </summary>
    public sealed class BoatWakeController : MonoBehaviour
    {
        [SerializeField] private BoatController boat;
        [SerializeField] private ParticleSystem wakeParticles;
        [SerializeField] private Transform sternPoint;
        [SerializeField] private float minSpeedForWake = 0.8f;
        [SerializeField] private float maxEmissionRate = 90f;
        [SerializeField] private float waterOffset = 0.03f;

        private ParticleSystem.EmissionModule _emission;

        private void Reset()
        {
            boat = GetComponent<BoatController>();
            sternPoint = transform;
        }

        private void Awake()
        {
            if (boat == null) boat = GetComponent<BoatController>();
            if (sternPoint == null) sternPoint = transform;
            if (wakeParticles == null) wakeParticles = BuildDefaultWake();
            _emission = wakeParticles.emission;
        }

        private void LateUpdate()
        {
            if (wakeParticles == null) return;
            float speed = boat != null ? boat.PlanarSpeed : 0f;
            float speed01 = boat != null ? boat.NormalizedSpeed : Mathf.Clamp01(speed / 10f);
            bool moving = speed > minSpeedForWake;

            _emission.rateOverTime = moving ? maxEmissionRate * speed01 : 0f;
            if (boat != null)
            {
                var pos = sternPoint.position;
                pos.y = boat.WaterY + waterOffset;
                wakeParticles.transform.position = pos;
                wakeParticles.transform.rotation = Quaternion.LookRotation(-boat.transform.forward, Vector3.up);
            }
        }

        private ParticleSystem BuildDefaultWake()
        {
            var go = new GameObject("WakeParticles");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, -2.7f);
            go.transform.localRotation = Quaternion.identity;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 1.1f;
            main.startSpeed = 1.9f;
            main.startSize = 0.32f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 600;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.5f;

            var color = ps.colorOverLifetime;
            color.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(new Color(0.75f, 0.95f, 1f), 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.55f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = g;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1.8f));
            ps.Play();
            return ps;
        }
    }
}
