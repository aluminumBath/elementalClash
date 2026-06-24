using UnityEngine;
using Elementborn.Core;

namespace Elementborn.Game.Feel
{
    /// <summary>
    /// Self-bootstrapping global service that bursts a handful of element-tinted shards when any
    /// <see cref="Damageable"/> is defeated (listening on <see cref="CombatFeedback.Defeated"/>). Shards are
    /// asset-free <see cref="ToonPalette"/>-tinted cubes flung along the deterministic <see cref="ShardBurst"/>
    /// spread; they scatter, tumble, and shrink away. Pairs with the defeat light burst in
    /// <see cref="FlashFeedback"/> and the gold score popup that enemies raise on death.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DeathPoof : MonoBehaviour
    {
        private const int ShardCount = 7;
        private static DeathPoof _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("DeathPoof");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<DeathPoof>();
        }

        private void OnEnable() { CombatFeedback.Defeated += OnDefeated; }
        private void OnDisable() { CombatFeedback.Defeated -= OnDefeated; }

        private void OnDefeated(Vector3 pos, Element element)
        {
            var mat = ToonPalette.Tinted(ElementColor.For(element));
            Vector3 origin = pos + Vector3.up * 0.8f;

            for (int i = 0; i < ShardCount; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var col = go.GetComponent<Collider>();
                if (col != null) Destroy(col);

                go.transform.position = origin;
                go.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);
                go.GetComponent<MeshRenderer>().sharedMaterial = mat;

                ShardBurst.Direction(i, ShardCount, 0.6f, out float x, out float y, out float z);
                float speed = Random.Range(2.5f, 4.5f);
                go.AddComponent<Shard>().Launch(new Vector3(x, y, z) * speed, Random.Range(0.5f, 0.8f));
            }
        }
    }
}
