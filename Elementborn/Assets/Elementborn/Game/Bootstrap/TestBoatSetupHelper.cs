using UnityEngine;

namespace Elementborn.Game
{
    public sealed class TestBoatSetupHelper : MonoBehaviour
    {
        [SerializeField] private bool buildOnStart = false;

        private void Start()
        {
            if (buildOnStart)
            {
                BuildBoat();
            }
        }

        [ContextMenu("Build Test Boat")]
        public GameObject BuildBoat()
        {
            GameObject boat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boat.name = "Test Boat";
            boat.transform.position = transform.position;
            boat.transform.localScale = new Vector3(3f, 0.6f, 5f);
            boat.AddComponent<Rigidbody>().isKinematic = true;
            boat.AddComponent<BoatController>();
            boat.AddComponent<BoatWaveVisuals>();
            boat.AddComponent<BoatWakeController>();
            boat.AddComponent<BoatRangedCombat>();
            boat.AddComponent<BoatCombatHook>();

            GameObject station = new GameObject("Boat Boarding Station");
            station.transform.SetParent(boat.transform);
            station.transform.localPosition = new Vector3(0f, 1f, -2f);
            BoxCollider collider = station.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(2f, 2f, 2f);
            station.AddComponent<BoatBoardingStation>();

            return boat;
        }
    }
}
