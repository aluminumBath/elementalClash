# Elementborn Compile Issue Preflight Report

This is a static preflight report from inside Unity. It does not replace the compiler.

- **Duplicate type name** `order`:
  - `Assets/Elementborn\Core\Equipment.cs`
  - `Assets/Elementborn\Core\Equipment.cs`
  - `Assets/Elementborn\Game\SaveSystem.cs`
  - `Assets/Elementborn\Game\SaveSystem.cs`
- **Duplicate type name** `LootEntry`:
  - `Assets/Elementborn\Core\Loot.cs`
  - `Assets/Elementborn\Game\Inventory\LootTableDefinition.cs`
- **Duplicate type name** `Mode`:
  - `Assets/Elementborn\Game\CharacterCreationUI.cs`
  - `Assets/Elementborn\Game\GameBootstrap.cs`
- **Duplicate type name** `for`:
  - `Assets/Elementborn\Core\Social\UserDirectory.cs`
  - `Assets/Elementborn\Game\Interaction\IInteractable.cs`

## Summary

```text
C# files scanned: 963
Brace issues: 0
Old GetPrompt signatures: 0
Bad ElementbornFindUtility references: 0
Duplicate type names: 4
```
