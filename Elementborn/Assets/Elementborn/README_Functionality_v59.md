# v59 Functionality: UI Prefab Factory Compile Fix

This patch addresses:

```text
Assets\Elementborn\Editor\ElementbornUiPrefabFactory.cs(21,13): error CS0103: The name 'CreateThemeAsset' does not exist in the current context
```

## Change

`ElementbornUiPrefabFactory.CreateAll()` calls:

```text
CreateThemeAsset()
```

but the actual implementation was named:

```text
LoadOrCreateTheme()
```

v59 adds a compatibility wrapper:

```text
public static ElementbornUiTheme CreateThemeAsset()
{
    return LoadOrCreateTheme();
}
```

It also exposes the wrapper as an editor menu item:

```text
Elementborn/UI Prefabs/Create Default UI Theme Asset
```
