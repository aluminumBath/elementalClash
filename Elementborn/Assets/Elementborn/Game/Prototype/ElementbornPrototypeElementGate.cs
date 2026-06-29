using UnityEngine;

namespace Elementborn.Game
{
    public sealed class ElementbornPrototypeElementGate : MonoBehaviour
    {
        public ElementbornPrototypeElementType element = ElementbornPrototypeElementType.Fire;
        public bool isOpen;
        public Transform leftPillar;
        public Transform rightPillar;
        public Transform topBeam;
        public TextMesh label;

        private Vector3 leftClosed;
        private Vector3 rightClosed;
        private Vector3 topClosed;
        private bool captured;

        private void Awake()
        {
            CaptureClosedPose();
            ApplyVisualState(false);
        }

        private void OnEnable()
        {
            CaptureClosedPose();
            ApplyVisualState(false);
        }

        public void OpenGate()
        {
            CaptureClosedPose();

            isOpen = true;

            if (leftPillar != null)
            {
                leftPillar.localPosition = leftClosed + Vector3.left * 0.9f;
            }

            if (rightPillar != null)
            {
                rightPillar.localPosition = rightClosed + Vector3.right * 0.9f;
            }

            if (topBeam != null)
            {
                topBeam.localPosition = topClosed + Vector3.up * 2.2f;
            }

            ApplyVisualState(true);
        }

        public void CloseGate()
        {
            CaptureClosedPose();

            isOpen = false;

            if (leftPillar != null)
            {
                leftPillar.localPosition = leftClosed;
            }

            if (rightPillar != null)
            {
                rightPillar.localPosition = rightClosed;
            }

            if (topBeam != null)
            {
                topBeam.localPosition = topClosed;
            }

            ApplyVisualState(false);
        }

        private void CaptureClosedPose()
        {
            if (captured)
            {
                return;
            }

            if (leftPillar == null)
            {
                Transform found = transform.Find("Left Pillar");
                if (found != null)
                {
                    leftPillar = found;
                }
            }

            if (rightPillar == null)
            {
                Transform found = transform.Find("Right Pillar");
                if (found != null)
                {
                    rightPillar = found;
                }
            }

            if (topBeam == null)
            {
                Transform found = transform.Find("Top Beam");
                if (found != null)
                {
                    topBeam = found;
                }
            }

            if (label == null)
            {
                label = GetComponentInChildren<TextMesh>(true);
            }

            leftClosed = leftPillar != null ? leftPillar.localPosition : Vector3.zero;
            rightClosed = rightPillar != null ? rightPillar.localPosition : Vector3.zero;
            topClosed = topBeam != null ? topBeam.localPosition : Vector3.zero;
            captured = true;
        }

        private void ApplyVisualState(bool open)
        {
            if (label != null)
            {
                label.text = ElementbornPrototypeVisualUtility.GetElementName(element) + (open ? "\nOPEN" : "\nGATE");
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            Color color = ElementbornPrototypeVisualUtility.GetElementColor(element);
            if (open)
            {
                color = Color.Lerp(color, Color.white, 0.35f);
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && renderer.GetComponent<TextMesh>() == null)
                {
                    renderer.sharedMaterial = ElementbornPrototypeVisualUtility.CreateRuntimeMaterial(name + " Material", color);
                }
            }
        }
    }
}
