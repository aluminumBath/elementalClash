# v44 Static Bug Check

Static checks did not find obvious generated-code hazards.

Additional v44 checks performed:

- old `BaseInteractable.GetPrompt` string override check
- missing `Elementborn.Core` import check for `MapMarkerType`
- missing referenced enum member check
- `NamedShipAssetGenerator` duplicate `party` local variable check

Baseline checks performed:

- C# brace balance
- C# parenthesis balance
- duplicate generated type names
- accidental Python boolean literals
- explicit compile-error marker check

C# files checked: 587

Limitations:

- I cannot run the Unity compiler in this environment.
- Unity may still report project-specific integration issues after import.