# v68 Functionality: Fix Smoke-Test Assembly Definition Duplicate References

The latest Unity log showed Safe Mode caused by duplicate Test Runner references:

```text
Assembly has duplicate references: UnityEngine.TestRunner,UnityEditor.TestRunner
Assets/Tests/ElementbornSmoke/EditMode/Elementborn.EditMode.SmokeTests.asmdef

Assembly has duplicate references: UnityEngine.TestRunner
Assets/Tests/ElementbornSmoke/PlayMode/Elementborn.PlayMode.SmokeTests.asmdef
```

## Root cause

The v67 smoke-test `.asmdef` files included explicit references to Test Runner assemblies and also used:

```text
optionalUnityReferences: ["TestAssemblies"]
```

Unity injects the needed Test Runner references through `TestAssemblies`, so the explicit references became duplicates.

## Change

v68 removes the explicit references and keeps:

```text
"references": [],
"optionalUnityReferences": ["TestAssemblies"]
```

for both smoke-test assembly definitions.

## Expected result

Unity should leave Safe Mode and the Test Runner should discover the smoke tests:

```text
EditMode: at least 2 smoke tests
PlayMode: at least 2 smoke tests
```
