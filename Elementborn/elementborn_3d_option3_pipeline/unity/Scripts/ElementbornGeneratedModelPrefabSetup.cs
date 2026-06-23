// ElementbornGeneratedModelPrefabSetup.cs
// Drop on the root of imported generated models while evaluating them.
using UnityEngine;

[DisallowMultipleComponent]
public class ElementbornGeneratedModelPrefabSetup : MonoBehaviour
{
    public bool addPreviewRotation;
    public bool addCapsuleCollider = true;
    public Vector3 colliderCenter = new Vector3(0, 1, 0);
    public float colliderHeight = 2f;
    public float colliderRadius = 0.35f;

    void Reset()
    {
        if (addCapsuleCollider && GetComponent<CapsuleCollider>() == null)
        {
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.center = colliderCenter;
            col.height = colliderHeight;
            col.radius = colliderRadius;
        }
        if (addPreviewRotation && GetComponent<ModelPreviewRotator>() == null)
        {
            gameObject.AddComponent<ModelPreviewRotator>();
        }
    }
}
