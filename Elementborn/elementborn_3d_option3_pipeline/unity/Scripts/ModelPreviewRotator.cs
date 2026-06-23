// ModelPreviewRotator.cs
using UnityEngine;

public class ModelPreviewRotator : MonoBehaviour
{
    public float degreesPerSecond = 25f;
    void Update()
    {
        transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
    }
}
