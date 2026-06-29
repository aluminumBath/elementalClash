# v73 Functionality: Console / Warning Cleanup Pass

The latest `Editor(9).log` compiled successfully; it did not contain C# compiler errors. Most remaining Console noise came from warning categories.

## Changes

### Project-level C# warning suppression

Adds:

```text
Assets/csc.rsp
```

with:

```text
-nowarn:0618,0414,0184,UAC1009
```

This suppresses:
- obsolete Unity `FindObjectOfType` / `FindObjectsOfType` warnings
- intentionally unused serialized placeholder-field warnings
- one impossible type-check warning
- the known Unity analyzer warning for `AdminActionRequest.Values`

### TextMeshPro warning cleanup

`ElementbornTmpFontUtility` no longer calls:

```text
TMP_FontAsset.CreateFontAsset(...)
Font.CreateDynamicFontFromOSFont(...)
```

Those calls were generating runtime warnings such as:

```text
Unable to load font face for [Segoe UI]
Unable to load font face for [Arial]
Unable to load font face for [Liberation Sans]
```

The utility now only uses existing TMP font resources and otherwise leaves TMP's component font alone.

### EventSystem cleanup

`ElementbornEventSystemUtility` keeps the single-EventSystem repair behavior but disables normal diagnostic logging so the Console does not fill with stack traces for routine repairs.

### Dashboard cleanup

`StorySystemsDebugDashboard` no longer logs the entire dashboard report to the Console by default when enabled.
