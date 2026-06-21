using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
        private Text _overlay;
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

            Vector3 home = (PlayerInventory.Instance != null && PlayerInventory.Instance.HasHouse)
                ? PlayerInventory.Instance.HouseLocation
                : _spawnPos;
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
            _overlayRoot = new GameObject("DeathOverlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _overlayRoot.transform.SetParent(transform, false);
            var canvas = _overlayRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = _overlayRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);

            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(_overlayRoot.transform, false);
            var drt = dim.GetComponent<RectTransform>();
            drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one; drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = new Color(0.40f, 0.05f, 0.05f, 0.55f);

            var txt = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(_overlayRoot.transform, false);
            var trt = txt.GetComponent<RectTransform>();
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(700, 200);

            _overlay = txt.GetComponent<Text>();
            _overlay.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _overlay.fontSize = 40;
            _overlay.alignment = TextAnchor.MiddleCenter;
            _overlay.color = Color.white;
            _overlay.text = "You fell.";
        }
    }
}
