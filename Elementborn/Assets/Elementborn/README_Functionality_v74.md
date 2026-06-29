# v74 Functionality: Interactable Tag Repair

The latest `Editor(10).log` shows repeated runtime errors:

```text
Tag: Interactable is not defined.
Elementborn.Game.NpcWorldIntegrationManager:TryAssignTag(...)
Elementborn.Game.NpcWorldIntegrationManager:SpawnPlaceholder(...)
```

This happened 45 times because placeholder NPC spawning attempted to assign the `Interactable` tag before the Unity project tag list contained it.

## Changes

### Project settings repair

The v74 apply script directly patches:

```text
ProjectSettings/TagManager.asset
```

to ensure required custom tags exist:

```text
Enemy
Interactable
Boat
Boss
Projectile
ResourceNode
QuestObjective
```

### Runtime guard

`NpcWorldIntegrationManager.TryAssignTag(...)` now checks whether a tag is defined before assigning it in the editor. If a tag is missing, it logs a single warning instead of triggering repeated Unity tag errors.

### Editor menu

v74 adds:

```text
Elementborn -> Unity Setup -> Repair Required Tags Now
Elementborn -> Unity Setup -> Report Required Tags
```

Use these after applying the patch to verify the tag list inside Unity.
