# v101 Functionality: Stable Player + Chest Visual Recovery

v101 is a stronger recovery patch after v100 still caused:
- invisible/disappearing chests
- tiny white rectangle chest artifacts
- no visible change to the main character

## What changed

v101 avoids the risky imported chest replacement entirely for now. It uses a clearly visible procedural treasure chest for every chest first, while keeping imported models available for diagnosis.

For the player, v101 tries the exact channeler FBX. If that fails, it creates a procedural robed channeler so the player definitely changes visually. There is also an axolotl option.

## Exact ZIPs extracted

```text
C:\Users\steel\Desktop\Code\elementalClash\Elementborn\Assets\generated_assets\Treasure_Chest_0623200731_texture_fbx.zip

C:\Users\steel\Desktop\Code\elementalClash\Elementborn\Assets\generated_assets\Channeler_Hero_None_3_0624153647_texture_fbx.zip
```

## New menus

```text
Elementborn -> Assets -> V101 Diagnose Exact Model Imports
Elementborn -> Assets -> V101 Apply Stable Visual Fixes
Elementborn -> Assets -> V101 Restore All Chest Visibility
Elementborn -> Assets -> V101 Apply Visible Procedural Chests
Elementborn -> Assets -> V101 Apply Channeler Or Procedural Player
Elementborn -> Assets -> V101 Apply Axolotl Or Procedural Player
Elementborn -> Assets -> V101 Force Procedural Robed Player
Elementborn -> Visuals -> V101 Repair Movement Blocking Decorative Colliders
```

## Recommended Unity order

```text
Elementborn -> Assets -> V101 Diagnose Exact Model Imports
Elementborn -> Assets -> V101 Restore All Chest Visibility
Elementborn -> Assets -> V101 Apply Visible Procedural Chests
Elementborn -> Assets -> V101 Apply Channeler Or Procedural Player
Elementborn -> Visuals -> V101 Repair Movement Blocking Decorative Colliders
File -> Save
File -> Save Project
```

If the imported channeler is still not found or not visible, use:

```text
Elementborn -> Assets -> V101 Force Procedural Robed Player
```

## Why this should work better

The stable chest visual is not imported from FBX. It is simple Unity geometry with brown/gold materials, so it should be visible no matter what happened with model import.

The player visual also has a guaranteed procedural fallback, so the main character should visibly change even if the FBX import fails.
