using System.Collections;
using UnityEngine;

namespace Elementborn.Game
{
    /// <summary>
    /// Player death + respawn. Watches the player's <see cref="Damageable"/>; on death it disables
    /// control, shows a countdown overlay, then revives the player at the spawn point and restores
    /// control (combo resets too). Sets the player's Damageable to not destroy on death.
    /// </summary>
    public sealed class RespawnController : MonoBehaviour
    {
        [SerializeField] private Damageable player;
        [SerializeField] private PlayerCombatController combat;
        [SerializeField] private Behaviour rigMovement;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float respawnDelay = 3f;

        private Vector3 _spawnPos;
        private Quaternion _spawnRot;
        private GameObject _overlayRoot;
        private UiLabel _overlay;
        private bool _respawning;

        private void Start()
        {
            if (player == null)
            {
                var tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null) player = tagged.GetComponentInParent<Damageable>();
            }
            if (player == null) { enabled = false; return; }

            if (combat == null) combat = player.GetComponentInChildren<PlayerCombatController>();
            if (rigMovement == null) rigMovement = player.GetComponent<FirstPersonRig>();

            player.DestroyOnDeath = false;
            player.Health.Died += OnDied;

            _spawnPos = spawnPoint ? spawnPoint.position : player.transform.position;
            _spawnRot = spawnPoint ? spawnPoint.rotation : player.transform.rotation;

            BuildOverlay();
            _overlayRoot.SetActive(false);
        }

        private void OnDied()
        {
            if (_respawning) return;
            _respawning = true;
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            SetControl(false);
            _overlayRoot.SetActive(true);
            ScoreController.Instance?.ResetCombo();

            float t = respawnDelay;
            while (t > 0f)
            {
                _overlay.text = $"You fell.\nRespawning in {Mathf.CeilToInt(t)}...";
                t -= Time.deltaTime;
                yield return null;
            }

            // Respawn priority: an activated checkpoint (the player's most recent "respawn here") wins, then a
            // claimed house, then the scene's spawn point.
            Vector3 home;
            if (CheckpointState.Instance != null && CheckpointState.Instance.TryActivePosition(out var cp))
                home = cp;
            else if (PlayerInventory.Instance != null && PlayerInventory.Instance.HasHouse)
                home = PlayerInventory.Instance.HouseLocation;
            else
                home = _spawnPos;
            Teleport(home, _spawnRot);
            player.Health.Revive();
            _overlayRoot.SetActive(false);
            SetControl(true);
            _respawning = false;
        }

        private void Teleport(Vector3 pos, Quaternion rot)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.SetPositionAndRotation(pos, rot);
            if (cc != null) cc.enabled = true;
        }

        private void SetControl(bool on)
        {
            if (combat != null) combat.enabled = on;
            if (rigMovement != null) rigMovement.enabled = on;
            Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !on;
        }

        private void BuildOverlay()
        {
            var canvas = UiTheme.Canvas("DeathOverlay", sortOrder: 100);
            canvas.transform.SetParent(transform, false);
            _overlayRoot = canvas.gameObject;

            var dim = UiTheme.Panel(canvas.transform, new Color(0.40f, 0.05f, 0.05f, 0.55f), "overlay_dim");
            var drt = dim.rectTransform;
            drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one; drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;

            _overlay = UiTheme.Label(canvas.transform, "You fell.", 40, Color.white, TextAnchor.MiddleCenter);
            var trt = _overlay.Rect;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(700, 200);
        }
    }
}
