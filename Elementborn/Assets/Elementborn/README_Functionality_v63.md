# v63 Functionality: PlayMode UI Null-Safety

This patch addresses PlayMode runtime errors:

```text
NullReferenceException
Elementborn.Game.PlayerAttunementHud.RefreshIfChanged(...)
Elementborn.Game.PlayerAttunementHud.Update()
```

and repeated TextMeshPro errors:

```text
NullReferenceException
TMPro.TMP_Settings.get_defaultFontAsset()
Elementborn.Game.UiTheme.Label(...)
Elementborn.Game.PlayerStaggerController.Build()
Elementborn.Game.PlayerStaggerController.Bootstrap()
```

## Changes

### PlayerAttunementHud

Replaced stale/local `PlayerAttunementHud.cs` with a null-safe HUD that:

- auto-creates minimal UI if serialized references are missing
- does not crash if no attunement tracker exists
- uses reflection safely to discover an attunement/channel/element tracker when present
- handles missing label, swatch, canvas, and canvas group references

### TextMeshPro default font

Added:

```text
ElementbornTmpFontUtility.GetDefaultFontAsset()
ElementbornTmpFontUtility.ApplyDefaultFont(...)
```

The helper avoids `TMP_Settings.defaultFontAsset`, which can throw when TMP settings are present but no default font asset is configured.

The apply script patches any remaining local stale references from:

```text
TMP_Settings.defaultFontAsset
```

to:

```text
Elementborn.Game.ElementbornTmpFontUtility.GetDefaultFontAsset()
```

## Note on EditMode tests

The exported EditMode XML showed zero discovered tests. That means the EditMode export passing is not meaningful yet; it ran 0 tests.
