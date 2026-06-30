# v96 Functionality: Generated Visual Presets

v96 adds curated generated-asset visual presets.

## Why

v91's auto-decoration was too aggressive. v94 added one-at-a-time assignment. v96 adds the middle ground:

```text
small curated presets
preview/report before applying
safe cleanup
no auto-dumping
```

## New runtime marker

```text
ElementbornPrototypeVisualPresetTag.cs
```

This marks objects added by visual presets so cleanup can remove them safely and restore the original renderer.

## New Unity window

```text
Elementborn -> Assets -> Generated Visual Presets Window
```

The window lets you:
- select a visual preset
- refresh a report
- see which prefabs/targets are available
- build prefabs
- sanitize imports
- clear existing generated decorations
- apply only the selected preset

## Presets

```text
PropsOnly
CharactersOnly
CreatureShowcaseSmall
GameplayReplacementsSmall
FullSafeVisualPass
```

Recommended order:

```text
PropsOnly
CharactersOnly
CreatureShowcaseSmall
```

Avoid FullSafeVisualPass until the smaller presets look good.

## Existing menus updated

```text
Elementborn -> Assets -> Decorate Open Prototype Scene With Generated Assets
Elementborn -> Assets -> Decorate Open Prototype Scene With Safe Generated Assets
Elementborn -> Assets -> Create Small Generated Asset Showcase Gallery
```

These now use the preset system.

## Workflow

```text
Elementborn -> Assets -> Sanitize Generated Asset Imports
Elementborn -> Assets -> Report Generated Asset Library Matches
Elementborn -> Assets -> Build Generated Asset Library From Extracted FBXs
Elementborn -> Assets -> Generated Visual Presets Window
```

Then apply one preset at a time.
