# v46 Static Bug Check

Static checks did not find obvious generated-code hazards.

Checks performed:

- C# brace balance
- C# parenthesis balance
- duplicate type names
- accidental Python boolean literals
- obsolete BaseInteractable.GetPrompt return type check
- bad ElementbornFindUtility reference check
- referenced enum member check
- required v46 file presence check

C# files checked: 619

Limitations:

- I cannot run the Unity compiler or Unity Test Runner in this environment.
- Unity may still surface serialized-field or test-runner issues after import.