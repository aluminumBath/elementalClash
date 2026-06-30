# v97 Functionality: Generated Asset Review + Approval Workflow

v97 adds a safety layer before generated models are used broadly in the prototype.

## Why

Some generated FBX imports may be:
- huge
- white/untextured
- incorrectly oriented
- visually messy
- missing/corrupt texture files
- unsuitable for the current role

v97 lets you review and approve assets before presets use them.

## New files

```text
Assets/Elementborn/Game/Prototype/ElementbornPrototypeGeneratedAssetReviewTag.cs
Assets/Elementborn/Editor/ElementbornGeneratedAssetApprovalDatabase.cs
Assets/Elementborn/Editor/ElementbornGeneratedAssetReviewGalleryBuilder.cs
Assets/Elementborn/Editor/ElementbornGeneratedAssetReviewWindow.cs
Assets/Elementborn/Data/GeneratedAssets/ElementbornGeneratedAssetApproval_v97.json
```

## New menus

```text
Elementborn -> Assets -> Generated Asset Review Window
Elementborn -> Assets -> Create Generated Asset Review Gallery
Elementborn -> Assets -> Clear Generated Asset Review Gallery
Elementborn -> Assets -> Toggle Presets Require Approved Assets
```

## Review workflow

```text
Elementborn -> Assets -> Sanitize Generated Asset Imports
Elementborn -> Assets -> Build Generated Asset Library From Extracted FBXs
Elementborn -> Assets -> Generated Asset Review Window
```

Then:
1. Search or scroll assets.
2. Click **Create Review Gallery**.
3. Inspect the models in-scene.
4. Select an asset in the window.
5. Add a note.
6. Click **Approve** or **Reject**.

## Presets now use approved assets

The generated visual preset system now defaults to:

```text
Require Approved Assets = true
```

That means visual presets only apply assets you approved.

You can toggle it in:

```text
Elementborn -> Assets -> Generated Visual Presets Window
```

or through:

```text
Elementborn -> Assets -> Toggle Presets Require Approved Assets
```

## Recommended next steps

Approve a tiny set first:
- TreasureChest
- HealingTonic
- StaminaDraught
- AncientGlyphMap
- PinkEyeAxolotl only if it looks good
- one ChannelerHero if it looks good

Then apply:

```text
PropsOnly
CharactersOnly
CreatureShowcaseSmall
```

This prevents another over-filled or ugly scene.
