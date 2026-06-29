using UnityEngine;

namespace Elementborn.Game
{
    public sealed class LeftWristAdminPanelController : MonoBehaviour
    {
        [SerializeField] private Transform leftWristAnchor;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private KeyCode toggleKey = KeyCode.F8;
        [SerializeField] private bool visibleOnStart = false;
        [SerializeField] private bool followMainCameraIfNoWrist = true;
        [SerializeField] private Vector3 wristLocalOffset = new Vector3(0.05f, 0.08f, 0.12f);
        [SerializeField] private Vector3 wristLocalEuler = new Vector3(65f, 0f, 0f);
        [SerializeField] private Vector3 cameraOffset = new Vector3(-0.42f, -0.22f, 0.85f);
        [SerializeField] private Vector3 cameraEulerOffset = new Vector3(12f, 0f, 0f);

        private void Start()
        {
            SetVisible(visibleOnStart);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                Toggle();
            }

            FollowAnchor();
        }

        public void SetLeftWristAnchor(Transform anchor)
        {
            leftWristAnchor = anchor;
        }

        public void Toggle()
        {
            SetVisible(panelRoot == null || !panelRoot.activeSelf);
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            panelRoot.SetActive(visible);
        }

        private void FollowAnchor()
        {
            if (leftWristAnchor != null)
            {
                transform.position = leftWristAnchor.TransformPoint(wristLocalOffset);
                transform.rotation = leftWristAnchor.rotation * Quaternion.Euler(wristLocalEuler);
                return;
            }

            if (!followMainCameraIfNoWrist || Camera.main == null)
            {
                return;
            }

            Transform cam = Camera.main.transform;
            transform.position = cam.TransformPoint(cameraOffset);
            transform.rotation = cam.rotation * Quaternion.Euler(cameraEulerOffset);
        }
    }
}
