# v32 Static Bug Check

Static checks did not find obvious generated-code hazards.

Checks performed:

- C# brace balance
- C# parenthesis balance
- duplicate generated type names
- accidental Python boolean literals
- explicit compile-error marker check

C# files checked: 469

Limitations:

- I cannot run the Unity compiler in this environment.
- Unity may still report project-specific integration issues after import.