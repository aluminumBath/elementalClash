# Asset import — Meshy models (Willow's Parrot + Parfa's Frogs)

Three Meshy exports are now wired in code. Each loads through `Resources.Load`, so once the
prefab named below exists at its path, the game picks it up — no further code changes. Until then
each falls back safely to a tinted placeholder primitive.

> The raw exports are large (parrot ~167 MB, frogs ~67 MB and ~103 MB). They are **not** committed
> to the scaffold zip (which stays ~4 MB). Keep them in your local project only — when you re-extract
> a new scaffold zip over your project, your local model files are untouched.

| Asset | Binds to | Folder (under `Assets/Elementborn/Resources/`) | Prefab to create |
|---|---|---|---|
| Raven-Parrot (rigged, 5 clips) | `WillowSidekick.Parrot` | `Models/Sidekicks/Raven_Parrot/` | `Raven_Parrot.prefab` |
| Hurricane Frog (static) | Parfa's frogs (air) | `Models/Npcs/Hurricane_Frog/` | `Hurricane_Frog.prefab` |
| Steam Frog (static) | Parfa's frogs (water) | `Models/Npcs/Steam_Frog/` | `Steam_Frog.prefab` |

Each folder ships a `_DROP_FILES_HERE.txt` marker you can delete once its prefab is in.

---

## Parfa's Frogs (static — the easy ones)

Each frog is a single textured FBX. No animation, no AnimatorController — `ParfaFrogController`
already gives each frog an idle bob via `ProceduralAnimator`, and spawns the pair beside Parfa
facing each other (the air-vs-water squabble).

1. **Extract** the FBX + PNG into the folder above (e.g. `Models/Npcs/Hurricane_Frog/`).
2. Select the FBX → if the material/texture didn't auto-assign, drag the `*image-to-3d-texture.png`
   onto the model's material (Base Map). Frogs are unlit-friendly; the toon material is fine too.
3. Drag the FBX into a scene, rename its root to exactly **`Hurricane_Frog`** (or **`Steam_Frog`**),
   drag it back into the folder to save **`Hurricane_Frog.prefab`**, then delete it from the scene.
4. Play → walk to Parfa. Her two frogs now show the real models, bobbing out of step. Approach with
   a loadout holding **both Air and Water** and "Talk to the frogs" pays a Diamond (once).

That's it for the frogs — repeat for both.

---

## Willow's Parrot (rigged — needs an AnimatorController)

The export is 5 FBX, each the **same** skinned mesh + ONE baked clip, plus 2 textures:

| File | Role |
|---|---|
| `..._Animation_Walking_withSkin.fbx` | mesh + `Walking` clip |
| `..._Animation_Running_withSkin.fbx` | `Running` clip |
| `..._Animation_Hop_with_Arms_Raised_withSkin.fbx` | `Hop_with_Arms_Raised` |
| `..._Animation_Angry_Ground_Stomp_withSkin.fbx` | `Angry_Ground_Stomp` |
| `..._Animation_jumping_jacks_withSkin.fbx` | `jumping_jacks` |
| `..._texture_0.png` | base colour |
| `..._texture_0_normal.png` | normal map |

1. **Extract** all 5 FBX + 2 PNG into `Models/Sidekicks/Raven_Parrot/`.
2. **Normal map:** select `..._texture_0_normal.png` → Texture Type = **Normal map** → Apply.
3. **Rig:** select all 5 FBX → Rig tab → Animation Type = **Generic** → Apply. (This makes each FBX
   expose its clip as a sub-asset.)
4. **Loop:** expand each FBX, select its clip → tick **Loop Time** (Walking/Running/Hop read best looped).
5. **AnimatorController:** create `Raven_Parrot.controller` in the folder, open the Animator window,
   drag each clip in as a state, and set **Walking** as the default (a calm ambient loop for an
   orbiting sidekick — Meshy didn't bake a pure idle).
6. **Prefab:** drag any one FBX into a scene (all share the mesh), add an **Animator** if absent and
   assign `Raven_Parrot.controller`, confirm base + normal are on the material, rename the root to
   **`Raven_Parrot`**, drag back into the folder to save `Raven_Parrot.prefab`, delete from scene.
7. Play. The procedural body-animator auto-detects the Animator and **stands down**, so the baked
   clip plays with no bob/clip conflict.

### Optional — build the parrot controller + prefab in code
Prefer to skip the manual graph wiring? Drop this one-time Editor utility in
`Assets/Elementborn/Editor/` and run **Elementborn ▸ Build Raven Parrot Prefab** after step 3.
(It's shown here rather than shipped as a `.cs`, so an untested Editor script can never block the
project's compilation — paste it in only if you want it.)

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Elementborn.EditorTools
{
    public static class RavenParrotSetup
    {
        const string Dir = "Assets/Elementborn/Resources/Models/Sidekicks/Raven_Parrot";
        const string Prefix = "Meshy_AI_Raven_Parrot_3D_biped_Animation_";

        [MenuItem("Elementborn/Build Raven Parrot Prefab")]
        public static void Build()
        {
            string[] takes = { "Walking", "Running", "Hop_with_Arms_Raised",
                               "Angry_Ground_Stomp", "jumping_jacks" };
            string Fbx(string t) => Dir + "/" + Prefix + t + "_withSkin.fbx";

            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(Dir + "/Raven_Parrot.controller");
            var sm = ctrl.layers[0].stateMachine;
            foreach (var t in takes)
            {
                var clip = LoadClip(Fbx(t));
                if (clip == null) { Debug.LogWarning("[RavenParrot] no clip in " + Fbx(t)); continue; }
                var st = sm.AddState(t);
                st.motion = clip;
                if (t == "Walking") sm.defaultState = st;
            }

            var src = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx("Walking"));
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(src);
            var anim = inst.GetComponentInChildren<Animator>();
            if (anim == null) anim = inst.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
            PrefabUtility.SaveAsPrefabAsset(inst, Dir + "/Raven_Parrot.prefab");
            Object.DestroyImmediate(inst);
            AssetDatabase.SaveAssets();
            Debug.Log("[RavenParrot] Built Raven_Parrot.prefab + controller.");
        }

        static AnimationClip LoadClip(string fbxPath)
        {
            foreach (var o in AssetDatabase.LoadAllAssetRepresentationsAtPath(fbxPath))
            {
                var c = o as AnimationClip;
                if (c != null && !c.name.StartsWith("__preview")) return c;
            }
            return null;
        }
    }
}
#endif
```

Requires Rig = Generic (step 3) first, so each FBX exposes its clip.

---

## Optimisation note (later, optional)
The parrot ships the same mesh 5× (one per clip FBX) — fine to import, wasteful to ship. When you're
ready to trim build size: keep one FBX as the mesh, extract the other four clips, and compress the
23 MB / 17 MB PNGs. Same idea for the frogs' large textures.
