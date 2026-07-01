# v103 Functionality: PowerShell Import Script Hotfix

v103 fixes a PowerShell parser error in the exact chest/channeler extraction scripts.

## Error fixed

PowerShell treats this as invalid:

```powershell
"Extracted $Label:"
```

because `$Label:` looks like a scoped variable reference.

v103 changes it to:

```powershell
"Extracted ${Label}:"
```

## Files fixed

```text
IMPORT_V101_EXACT_CHEST_AND_CHANNELER.ps1
IMPORT_V102_EXACT_CHEST_AND_CHANNELER.ps1
IMPORT_V103_EXACT_CHEST_AND_CHANNELER.ps1
```

v103 also includes:

```text
HOTFIX_POWERSHELL_LABEL_COLON.ps1
```

Use that hotfix script if you want to repair an already-extracted v101 or v102 patch folder without re-expanding the whole patch.

## Recommended path

Use the v103 apply script:

```text
FIX_V103_SAFE_VISUAL_RECOVERY.ps1
```

Then in Unity:

```text
Elementborn -> Assets -> V102 Diagnose Exact Imports
Elementborn -> Assets -> V102 Full Safe Visual Recovery
File -> Save
File -> Save Project
```

The Unity menu names remain V102 because v103 is only a packaging/script hotfix.
