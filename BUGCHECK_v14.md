# v14 Static Bug Check

Static checks did not find obvious generated-code hazards.

Checks performed:

- C# brace balance
- C# parenthesis balance
- duplicate generated type names
- accidental Python boolean literals
- explicit compile-error marker check
- equipment icon PNG generation count

Limitations:

- I cannot run the Unity compiler in this environment.
- Unity may still report project-specific integration issues after import.
- PNG import settings may need to be changed to Sprite in Unity for UI use.