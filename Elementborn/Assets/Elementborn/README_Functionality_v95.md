# v95 Functionality: Assignment Window Compile Fix

v95 fixes compile errors in:

```text
Assets/Elementborn/Editor/ElementbornGeneratedAssetAssignmentWindow.cs
```

The file used `EditorSceneManager` but was missing:

```csharp
using UnityEditor.SceneManagement;
```

This patch adds that import.
