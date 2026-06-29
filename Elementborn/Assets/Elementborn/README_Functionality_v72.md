# v72 Functionality: EventSystem.current Safety Fix

v71 fixed duplicate EventSystems but could log an error when run from the editor menu:

```text
Failed setting EventSystem.current to unknown EventSystem
No module
Elementborn.Game.ElementbornEventSystemUtility:SetCurrent(...)
```

## Root cause

v71 manually assigned:

```text
EventSystem.current = system
```

Unity can reject that if the candidate EventSystem does not yet have a usable input module.

## Change

v72 removes direct assignment to `EventSystem.current`.

The EventSystem repair now:

1. Chooses one EventSystem to keep.
2. Ensures it is active/enabled.
3. Ensures it has an enabled input module.
4. Removes/disables duplicates.
5. Lets Unity establish `EventSystem.current` naturally.

This should remove the `Failed setting EventSystem.current ... No module` error while preserving the duplicate-EventSystem fix.
