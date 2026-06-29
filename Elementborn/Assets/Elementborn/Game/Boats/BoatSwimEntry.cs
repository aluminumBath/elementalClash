using System.Collections;
using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game
{
    /// <summary>Helper used when the player is knocked out of a boat: positions them in nearby water and applies a short drift.</summary>
    public sealed class BoatSwimEntry : MonoBehaviour
    {
        public static void Launch(Transform player, Vector3 impulse, float waterY)
        {
            if (player == null) return;
            var entry = player.GetComponent<BoatSwimEntry>();
            if (entry == null) entry = player.gameObject.AddComponent<BoatSwimEntry>();
            entry.Begin(impulse, waterY);
        }

        private void Begin(Vector3 impulse, float waterY)
        {
            EnsureSwimComponents();
            Vector3 planar = new Vector3(impulse.x, 0f, impulse.z);
            if (planar.sqrMagnitude < 0.01f) planar = transform.forward;
            transform.position += planar.normalized * 2.5f;
            transform.position = new Vector3(transform.position.x, waterY - 0.65f, transform.position.z);
            StopAllCoroutines();
            StartCoroutine(Drift(planar.normalized * Mathf.Clamp(planar.magnitude, 3f, 9f)));
        }

        private void EnsureSwimComponents()
        {
            if (GetComponent<CharacterController>() == null) gameObject.AddComponent<CharacterController>();
            if (GetComponent<Damageable>() == null) gameObject.AddComponent<Damageable>();
            if (GetComponent<UnderwaterController>() == null) gameObject.AddComponent<UnderwaterController>();
            if (GetComponent<SwimLocomotion>() == null) gameObject.AddComponent<SwimLocomotion>();
        }

        private IEnumerator Drift(Vector3 velocity)
        {
            var cc = GetComponent<CharacterController>();
            float t = 0.45f;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                if (cc != null && cc.enabled) cc.Move(velocity * Time.deltaTime);
                else transform.position += velocity * Time.deltaTime;
                yield return null;
            }
        }
    }
}
