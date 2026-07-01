# Inventory migration

Elementborn currently has **two** item stores. This doc tracks merging them safely, one
non-destructive step at a time.

## The two systems

1. **Legacy bag** — `PlayerInventory.Items` (the pure `Core.Inventory`, id→count). Used by crafting
   (`CraftingViewer`) and equipment (`EquipmentViewer`). Item set is authored in `Core/Items.cs`
   (`ItemCatalog`), 22 ids with names/descriptions/prices.
2. **Tracker bag** — `Game/Inventory/PlayerInventoryTracker` (stacks). Used by loot, pickups, storage,
   and `Game/Crafting/CraftingSystem`. Items are `InventoryItemStack`s, optionally backed by an
   `InventoryItemDefinition` ScriptableObject.

Both index items by the **same snake_case string ids** ("iron_helm", "fire_arrow", …), which is what
makes a merge tractable.

## Step 1 — unified deposit view (done, non-destructive)

The Home storage screen (key **H** → Storage) now shows **both** pools with a live read-only count line
("Bag: N items     Legacy bag: M items") and a **"Your bag (legacy)"** section that lets you deposit
legacy items into the shared chest. From the chest they withdraw into the Tracker bag — a safe,
player-driven way to move items across. Every move is guarded by the amount the chest actually accepts,
so nothing is ever lost or duplicated. Nothing auto-migrates.

## Step 2 — definition gap audit (done, read-only)

Run **Unity menu → Elementborn → Inventory → Audit Migration Gaps** (or `InventoryMigrationAudit.RunAndLog()`
at runtime). It scans every `InventoryItemDefinition` asset, compares against `ItemCatalog`, and logs which
legacy ids have no definition. Re-run it as you author definitions; the list shrinks to empty when done.

### Current result

There are **no `InventoryItemDefinition` assets in the project yet**, so **all 22 legacy ids lack one**.
Tracker stacks created by id show the **raw id** as their name (e.g. "iron_helm"), because
`InventoryItemStack.DisplayName` falls back to the id when there is no definition.

| Category | ids |
|---|---|
| Food | `ore_marrow_bone`, `sunflower_seeds`, `deep_jelly`, `compost_truffle`, `iridescent_beetle` |
| Material | `hide`, `ember_shard`, `river_pearl`, `tough_leather`, `iron_helm`, `warding_cloak`, `sturdy_boots`, `arrow`, `fire_arrow`, `water_arrow`, `earth_arrow`, `air_arrow` |
| Consumable | `healing_tonic`, `stamina_draught`, `elixir_of_vigor` |
| Treasure | `old_relic`, `elemental_charm` |

## Step 3 — close the name gap (done)

Chose option B: `InventoryItemStack.DisplayName` now falls back to `ItemCatalog.Get(id)?.Name` when a stack
has no definition, so every migrated legacy id reads as "Iron Helm" instead of "iron_helm" with **zero** asset
authoring — the name stays sourced from the single authored list in `Core/Items.cs`. Ids with no catalog entry
(Tracker-only loot) keep their raw id, so nothing else changes. Definitions can still be added later for items
that want a custom icon or stack size.

(The alternative — hand-authoring 22 `InventoryItemDefinition` assets — was rejected as redundant given the
shared id space.)

## Step 4 — unify the item bag on the Tracker (done)

Every active reader and writer of the legacy `PlayerInventory.Items` bag now goes through
`PlayerInventoryTracker`, so the game runs on a single item pool:

- `PlayerInventory.AddItem` delegates to `PlayerInventoryTracker.AddItemId` (keeping the quest-collection event
  and pickup sound), which moves *all* add callers at once — crafting output, creature drops, world pickups,
  and trade rewards.
- New `PlayerInventoryTracker.EntriesById()` sums stacks by id so the older id-based UIs can enumerate the
  Tracker as `(id, count)` pairs.
- Repointed reads/removes: `CraftingViewer` (affordability + input consumption), `EquipmentViewer` (bag list),
  `EquipmentController` (wear-requires-possession), `PlayerCombatController` (bow ammo), `InventoryController`
  (use + list), `SidekickFeedingController` (feeding), and `TradeController` (offer clamp, stake list, give).

`ShopController` is left alone — superseded by the Tracker-based `ShopService` and not wired into the scene. The
`HomeMenuViewer` "deposit legacy items" panel stays: it's now purely a manual drain for items an *old save*
loaded into the legacy bag. One behavioural note — an open Inventory panel no longer live-updates from external
changes (the Tracker's change signal is a serialized UnityEvent, not a C# event); it rebuilds on open and after
each use.

Not touched here: **save/load**. New items persist through the Tracker's own save bridge; the legacy
`itemIds` / `itemCounts` fields are now written empty. An *old* save still loads its legacy items into the (now
unread) legacy bag, so those stay invisible until step 5.

## Step 5 — save migration + retire the legacy bag (next)

Fold any legacy `itemIds` / `itemCounts` from a loaded save into the Tracker (watch ordering against the
Tracker's own save-bridge restore so nothing double-counts or gets wiped), then delete the vestigial
`Inventory Items` field, its save fields, and the `HomeMenuViewer` legacy-drain panel. Wants an in-Unity pass
with a pre-step-4 save before shipping.
