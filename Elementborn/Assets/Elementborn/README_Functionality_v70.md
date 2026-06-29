# v70 Functionality: Timestamp Normalization + TMP Font Warning Cleanup

The latest editor log did not show compile errors. The important messages were:

```text
Cannot trust contents of Assets\Tests\ElementbornSmoke\PlayMode\ElementbornPlayModeSmokeTests.cs because its timestamp is in the future
Cannot trust contents of Assets\Tests\ElementbornSmoke\EditMode\ElementbornEditModeSmokeTests.cs because its timestamp is in the future
```

and repeated warnings from:

```text
ElementbornTmpFontUtility.GetDefaultFontAsset()
TMP_FontAsset.CreateFontAsset(...)
Unable to load font face for [LegacyRuntime]
```

## Changes

### Timestamp normalization

The v70 apply script normalizes timestamps under:

```text
Assets\Elementborn
Assets\Tests
```

to five minutes before the local machine time.

This should stop Unity/Bee from rebuilding because it distrusts future-dated files.

### TextMeshPro font helper

`ElementbornTmpFontUtility` no longer tries to create a TMP font asset from Unity's built-in `LegacyRuntime` font. It now tries:

```text
TMP Resources: LiberationSans SDF
OS font: Segoe UI
OS font: Arial
OS font: Liberation Sans
```

and quietly returns `null` if none are available.

### Visual material utility

The full pink-material repair no longer forces an immediate `AssetDatabase.Refresh()` after saving, which should reduce unnecessary import/memory churn.
