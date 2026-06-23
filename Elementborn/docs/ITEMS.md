# Items, inventory & the shop

A small item economy ties combat, the merchant, sidekick feeding, and quests together.

## The pieces

- **`ItemCatalog`** (Core) — the authored items by string id (so mods can extend them): foods (one per Willow
  sidekick), materials, consumables, and sell-only treasure, each with a base value.
- **`Inventory`** (Core) — a pure item bag (count / has / add / remove / total) with change notification. Failed
  removes change nothing.
- **`Shop`** (Core) — pure `Buy` / `Sell` over an `Inventory` + the value-based `Wallet`. Buying spends the cost;
  selling pays back at half value. Unit-tested in `ShopInventoryTests`.
- **`PlayerInventory.Items`** (Game) — the player's bag. `PlayerInventory.AddItem` adds an item and raises
  `QuestEvents.ItemCollected`, so loot and grants advance collect-item quests (buying does not).
- **`InventoryController`** (Game) — a toggled overlay (key **I**) listing your items by category.

## Where items come from and go

- **Loot:** defeating a creature drops a hide (`CreatureController` → `PlayerInventory.AddItem`).
- **Buying / selling:** merchants stock items; the shop (`ShopController`) shows **Buy** rows for the merchant's
  stock and **Sell** rows for what you're carrying. A merchant with no explicit `itemStock` sells the whole
  catalog.
- **Feeding:** feeding one of Willow's sidekicks now **consumes the matching food item**
  (`ItemCatalog.FoodFor`). No food, no feeding — the prompt tells you what you need.
- **Quests:** the `CollectItem` objective tracks item ids — e.g. Willow's "Pelts for the Tanner" wants 3 hides,
  which the wild-creature drops satisfy.

## Adding an item

Add an `ItemDef` to `ItemCatalog` (id, name, description, category, value). It's immediately buyable/sellable at
any "sells everything" merchant and usable as a `CollectItem` quest target. To make something drop it, call
`PlayerInventory.Instance.AddItem(id)` at the relevant moment.

## What's next

The bag is uncapped (no weight/slot limit yet), consumables aren't consumable from the inventory screen yet
(only foods, via feeding), and item counts — like quest state — aren't persisted into saves. Those are the
natural follow-ups.
