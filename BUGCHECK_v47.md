# v47 Static Bug Check

Static checks did not find obvious generated-code hazards.

Checks performed:

- C# brace balance
- C# parenthesis balance
- duplicate type names
- accidental Python boolean literals
- obsolete BaseInteractable.GetPrompt return type check
- bad ElementbornFindUtility reference check
- referenced enum member check
- admin action/category enum reference check

C# files checked: 633

Limitations:

- I cannot run the Unity compiler or Unity Test Runner in this environment.
- Unity may still surface prefab/UI template or test-runner issues after import.