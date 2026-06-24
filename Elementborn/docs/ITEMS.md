# Items, inventory & the shop

A small item economy ties combat, the merchant, sidekick feeding, and quests together.

## The pieces

- **`ItemCatalog`** (Core) — the authored items by string id (so mods can extend them): foods (one per Willow
  sidekick), materials, consumables, and sell-only treasure, each with a base value.
- **`Inventory`** (Core) — a pure item bag (count / has / add / remove / total) with change notification. Failed
  removes change nothing.
- **`Shop`** (Core) — pure `Buy` / `Sell` over an `Inventory` + the value-based `Wallet`. Buying spends the cost;
  selling pays back a **category-aware** fraction of value (`SellFractionFor`): sell-only **treasure** fetches
  ~90%, **tools** and **consumables** ~40%, **materials** and **food** the base 50%. Every fraction is below
  full price, so buying then re-selling is always a loss. Unit-tested in `ShopInventoryTests`.
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

## Loot drops

Defeated creatures drop items through a **weighted loot table** rather than a fixed hand-out. `LootTable` (Core)
does `Rolls` weighted picks — each selects one `LootEntry` by weight and grants a quantity in its range, with an
empty entry acting as a "nothing" slot so a kill can come up dry; identical items stack. It's seeded through
`IRandomSource`, so it's deterministic and unit-tested (a scripted source asserts exact picks). `LootTables.For(kind)`
returns a table themed by the creature's element/habitat over a common beast baseline (fire → `ember_shard`, water →
`river_pearl`/`deep_jelly`, earth → `ore_marrow_bone`, air → `iridescent_beetle`, apex → a real shot at `old_relic`),
and a test asserts every entry references a real `ItemCatalog` id. `CreatureController.OnDefeated` rolls the table,
grants the drops via `PlayerInventory.AddItem`, and toasts what was looted. To retune drops, edit the tables in
`Loot.cs`; to add a creature's table, extend the `For` switch.

## Crafting

Loot is also raw material. `RecipeBook` (Core) holds the recipes; `Recipe` lists `CraftIngredient` inputs and a
single output (id + count). `Crafting.CanCraft(recipe, have)` and `Crafting.Missing(recipe, have)` are pure checks
against a snapshot of held counts (unit-tested) — the runtime does the consume/grant. The `CraftingViewer` (key
**B**, and on the VR hub) lists every recipe with its inputs and how many you hold; clicking removes the inputs
from your inventory (`Items.Remove`) and grants the output via `PlayerInventory.AddItem` (so it also counts toward
the Collector achievement), or toasts what's missing. Recipes upgrade raw drops into the existing consumables and
into three craft-only items — **Tough Leather**, **Elixir of Vigor**, and the four-element **Elemental Charm** —
plus a Reforged Relic. To add or retune one, edit `RecipeBook` in `Crafting.cs` (and add any new output to
`ItemCatalog`).

## Using consumables

Consumables can now be **used from the inventory screen** (key **I**): each consumable shows a **Use** button, and
using one applies its effect and removes one from the bag. The effects live in `Consumables` (Core, pure,
unit-tested) — `Consumables.TryGet(id)` returns a `ConsumableEffect` (a heal amount + whether it refills stamina) —
and the inventory applies them to the player's `Health.Heal` and `StaminaController.Refill`: a Healing Tonic heals,
a Stamina Draught refills stamina, and an Elixir of Vigor does both. To make a new item usable, add it to
`Consumables` (and it should be an `ItemCategory.Consumable` in the catalog — a test enforces that).

## What's next

The bag is uncapped (no weight/slot limit yet). Item counts persist through the save system (alongside quest
progress). A weight/slot cap is the natural follow-up, and equippable gear effects from crafted items (the
Elemental Charm, Tough Leather) would build on crafting + consumables.
