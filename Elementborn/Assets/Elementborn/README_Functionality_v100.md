# v100 Functionality: Exact Channeler/Axolotl Player Visual + Safe Chest Recovery

v100 uses the exact paths you provided:

```text
C:\Users\steel\Desktop\Code\elementalClash\Elementborn\Assets\generated_assets\Treasure_Chest_0623200731_texture_fbx.zip
C:\Users\steel\Desktop\Code\elementalClash\Elementborn\Assets\generated_assets\Channeler_Hero_None_3_0624153647_texture_fbx.zip
```

New menus:

```text
Elementborn -> Assets -> V100 Build Exact Chest And Channeler Prefabs
Elementborn -> Assets -> V100 Restore Visible Fallback Chests
Elementborn -> Assets -> V100 Apply Exact Chest Visuals
Elementborn -> Assets -> V100 Apply Channeler Visual To Player
Elementborn -> Assets -> V100 Apply Axolotl Visual To Player
Elementborn -> Assets -> V100 Restore Blocky Player
Elementborn -> Assets -> V100 Apply Channeler Player And Chest Visuals
Elementborn -> Assets -> V100 Apply Axolotl Player And Chest Visuals
```

The rule is now: **never hide the blocky fallback unless the imported visual is confirmed to have visible renderers.**

The player controller stays on `Prototype Player`; the FBX is only a child visual.
