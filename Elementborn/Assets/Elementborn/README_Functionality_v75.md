# v75 Functionality: Tag Repair Menu Compile Fix

v74 introduced a syntax error in:

```text
Assets/Elementborn/Editor/ElementbornTagRepairMenu.cs
```

The log showed:

```text
error CS1039: Unterminated string literal
error CS1056: Unexpected character '\'
```

## Root cause

One `Debug.Log(...)` line used escaped quotes inside an interpolated expression in a way Unity's compiler rejected.

## Change

v75 replaces that line with plain string concatenation and keeps the tag repair functionality.

The tag repair still ensures these tags exist:

```text
Enemy
Interactable
Boat
Boss
Projectile
ResourceNode
QuestObjective
```
