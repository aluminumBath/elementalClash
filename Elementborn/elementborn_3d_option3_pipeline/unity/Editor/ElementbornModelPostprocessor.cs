// ElementbornModelPostprocessor.cs
// Put this file in Assets/Elementborn/Editor/.
using UnityEditor;
using UnityEngine;

public class ElementbornModelPostprocessor : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        var importer = assetImporter as ModelImporter;
        if (importer == null) return;
        if (!assetPath.Contains("Assets/Elementborn/Art/Models")) return;

        importer.globalScale = 1.0f;
        importer.importCameras = false;
        importer.importLights = false;
        importer.meshCompression = ModelImporterMeshCompression.Low;
        importer.isReadable = false;
        importer.optimizeMeshPolygons = true;
        importer.optimizeMeshVertices = true;

        // Use Humanoid manually for humanoid meshes that pass Unity's Avatar validation.
        importer.animationType = ModelImporterAnimationType.Generic;
    }
}
