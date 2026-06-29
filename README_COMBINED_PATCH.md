# Elementborn Combined Master Patch v2

This is the rolling combined master patch. It includes the earlier master patch plus upgraded/polished map icons and expanded map-marker functionality.

## Included major systems

### Boat + ocean gameplay
- Wind Waker-style boat handling
- Sail up/down
- Wind-based acceleration
- Manual movement with sail down
- Small-boat jump
- Boat ranged combat
- Boat wake and idle rocking
- Knock-off into water / swim entry
- Rare sea creature encounters
- Enemy stone/crumble/death-respawn behavior
- NPC damage immunity
- Kram display-name change and coral island content from earlier patch

### Map marker expansion
- Player
- Boat
- Last-ridden creature with traversal icons
- Active companion
- Camp
- Home base
- Storage chest
- Crafting station
- Stable
- Quest item
- Rare item
- Weapon
- Resource node
- Treasure
- Vendor
- Guide
- Trainer
- Healer
- Quest giver
- Rare enemy sighting
- Boss lair
- Enemy camp
- Danger zone
- Sea monster sighting
- Dock
- Fast travel
- Shrine
- Dungeon
- Cave
- Puzzle
- Locked door
- Fishing spot
- Underwater ruin
- Coral reef
- Wind current
- Custom pin

### New helper scripts
- `PlayerMapMarkerTracker`
- `TrackedMapMarkerRecord`
- `MapDiscoveryTrigger`
- `ImportantItemMapMarker`
- `NpcMapMarkerReporter`
- `CustomPinController`
- `MapMarkerIconLibrary`
- `MapMarkerSeedTester`
- `PlayerMapMarkerRefreshExample`

## Install

1. Close Unity.
2. Extract this ZIP.
3. Copy the included `Elementborn` folder into your repo root so it merges with your existing project folder:
   - `C:\Users\steel\Desktop\Code\elementalClash`
4. Reopen Unity.
5. Select all PNGs in:
   - `Assets/Elementborn/Art/UI/MapIcons`
6. Set import settings:
   - Texture Type: `Sprite (2D and UI)`
   - Sprite Mode: `Single`
   - Alpha Is Transparency: checked
   - Compression: `None` or `Low Quality`
7. Click **Apply**.

## EditMode patch

The file `elementborn_editmode_fixes.patch` is still included. Apply it only if those test fixes have not already been applied.

From:
`C:\Users\steel\Desktop\Code\elementalClash\Elementborn`

Run:
`git apply elementborn_editmode_fixes.patch`

## Notes

This ZIP is still far below the 500 MB split threshold.


## v3 additions

### Save-slot marker persistence
Added:
- `MapMarkerSaveData.cs`
- `MapMarkerSaveBridge.cs`

Use `MapMarkerSaveBridge` to save/load map markers per slot. This is independent from the existing save system so it can be integrated gradually.

### Current objective tracking
Added:
- `QuestObjectiveState.cs`
- `QuestObjectiveTracker.cs`
- `QuestObjectiveTrigger.cs`

Use these to create prototype quest objectives that automatically place a current-objective marker on the map.

### New icon
- `map_icon_current_objective.png`

Assign it to `MapMarkerIconLibrary.Current Objective Icon`.


## v4 additions

### Generic interaction system
Added a reusable interaction layer for:
- doors
- chests
- resource nodes
- shrines
- camps/save points
- custom map pins
- NPC conversations
- boat boarding bridges

### NPC conversation foundation
Added:
- NPC conversation profiles
- rule-based response generation
- backend AI response stub
- typed dialogue panel
- speech-to-text provider interface
- keyboard/debug STT provider
- Windows dictation STT provider

This lets NPCs respond to typed player text immediately, and later respond to voice once an STT provider/backend is connected.


## v5 additions

### Quest, rumor, and dialogue memory
Added:
- `DialogueMemoryTracker`
- `DialogueMemoryFact`
- `NpcRelationshipState`
- `DialogueIntentClassifier`
- `RumorTracker`
- `RumorSourceInteractable`
- `MemoryAwareNpcResponseProvider`
- `NpcMemorySeeder`
- `DialogueMemorySaveBridge`

NPCs can now answer from remembered facts, rumors, current objectives, and relationship tone. This works offline and can later be combined with STT and/or a local/cloud LLM.


## v6 additions

### Journal / codex
Added a runtime tracker for discoveries, locations, creatures, NPC notes, faction lore, tutorials, boat notes, and rumors.

### Notifications
Added a lightweight notification feed and UI view.

### Map filters and waypoint compass
Added map marker filter state/controller and waypoint tracker/compass view.

### Faction reputation
Added faction reputation records, tracker, triggers, and save bridge.

### Default knowledge
Added `ElementbornDefaultKnowledgeSeeder` for early prototype scenes so Kram/NPCs can know about boats, Neritha Reefwood, factions, and last-ridden creature tracking.


## v7 additions

### Inventory, storage, pickups, loot
Added:
- inventory item definitions
- player inventory tracker
- stackable items
- currency wallet
- storage containers
- item pickups
- auto-pickup triggers
- loot tables
- lootable interactables
- inventory save bridge
- debug inventory panel/seeder

This connects to existing map markers, notifications, journal entries, chests, camps, quest items, rare items, and future vendor/crafting systems.


## v8 additions

### Crafting, cooking, and boat repair
Added:
- recipe definitions
- recipe book tracker
- crafting system
- crafting stations
- recipe unlock interactables
- boat repair interactable
- recipe save bridge
- crafting debug panel

### Admin quest authoring
Added:
- runtime custom quest database
- admin quest records/objectives/rewards
- admin quest service
- admin quest creator UI panel
- cheat/admin command bridge
- runtime quest board interactable

Admins can now create, save, activate, complete, and delete quests at runtime. This is intentionally designed to sit beside existing cheat/admin UI tools.


## v9 additions

### Starter content generator
Added an editor menu generator for starter items, recipes, and creatures:
`Elementborn → Generate Starter Content → Items, Recipes, Creatures`

Starter examples include:
- Healing Fruit Tea
- Coral Bandage
- Boat Repair Kit
- Wind-Sail Patch
- Storm Lure
- Creature Treat
- Elemental Arrow Bundle
- Simple Reef Stew
- Stable Feed
- Clarity Tonic
- Skyotter
- Moss Wolf
- Thunderbird
- Teal Serpent
- Earth Mole

### Creature taming, bonding, and stable management
Added:
- creature definitions
- owned creature records
- tame attempts with treats
- bond XP and stages
- favorite treats
- following/ridden/stable/resting states
- stable storage/release
- creature save/load
- creature debug panel


## v10 additions

### Ability unlocks and skill tree foundation
Added:
- ability definitions
- skill tree definitions
- player ability tracker
- unlock requirements
- loadout slots
- cooldown tracker
- default ability executor
- ability input controller
- ability trainer/shrine/item unlock interactables
- ability quest rewards
- admin ability commands
- ability save/load
- starter ability/skill tree generator


## v11 additions

### World events and dynamic encounters
Added:
- world event definitions
- dynamic encounter definitions
- world event tracker
- world event director
- event records/states
- event scheduling/activation/completion/cancellation
- event map markers
- event rumors
- event journal entries
- resource respawn event nodes
- admin world-event commands
- world event save/load
- starter world event generator

Starter examples include:
- Sea Monster at Neritha Reefwood
- Temporary Wind Current
- Faction Patrol at the Border
- Merchant Caravan Arrival
- Coral Resource Bloom
- Kram's Warning
- Boss Lair Awakens


## v12 additions

### Vendor, shop, and economy
Added:
- shop definitions
- shop item listings
- faction discount rules
- shop price calculator
- shop service
- runtime shop inventory
- vendor shop interactable
- merchant caravan event shop
- admin shop commands
- shop save/load
- shop debug panel
- starter shop generator

Starter shops include:
- Kram's Field Supplies
- Neritha Reef Market
- Dockwright's Boatworks
- Stable Keeper
- Traveling Merchant Caravan


## v13 additions

### Resource gathering and harvesting
Added:
- resource node definitions
- harvest requirements
- harvest yield entries
- harvestable resource node interactable
- tool pickup interactable
- gathering tracker
- world-event resource respawner
- admin gathering commands
- gathering save/load
- harvesting debug panel
- starter gathering generator

Starter examples include:
- Coral Shard Outcrop
- Soft Seaweed Patch
- Ember Crystal Deposit
- Wind-Thread Reeds
- Storm Scale Nest
- Healing Fruit Bush
- Water Mint Patch
- Driftwood Pile
- Sailcloth Wreckage
- Sea Salt Flat
- Creature Treat Herbs
- Pickaxe
- Sickle
- Fishing Rod
- Harvest Knife
- Gathering Net


## v14 additions

### Equipment, gear stats, and polished icons
Added:
- equipment item definitions
- equipment slots
- gear stat modifiers
- player equipment tracker
- equipment stat snapshots
- set bonuses
- equipment pickup/station interactables
- admin equipment commands
- equipment save/load
- equipment debug panel
- starter equipment generator
- polished PNG equipment icon set

Starter examples include:
- Basic Sword
- Emberblade
- Coral Shield
- Glider Cloak
- Channeler Focus
- Pickaxe
- Sickle
- Harvest Knife
- Fishing Rod
- Gathering Net
- Boat Sail Patch Upgrade
- Boat Keel Plates
- Creature Saddle
- Storm Tack
- Reef Amulet
- Wind Ring
- Basic Armor
- Travel Boots
- Repair Hammer


## v15 additions

### Wind Waker-style icon refresh
Updated the full generated equipment icon set to better match the project's cel-shaded Wind Waker-like direction and added a new combat icon set.

### Combat damage and effects integration
Added:
- status effects
- attack definitions
- resistances/weaknesses
- simple combat health
- combat damage resolver/utility
- damage numbers
- melee hitboxes
- projectile combat
- loot drop tables
- enemy loot-on-defeat
- boat combat hooks
- creature combat hooks
- admin combat commands
- starter combat/status generator

Starter examples include:
- Burn
- Wet
- Chilled
- Rooted
- Shocked
- Basic Sword Slash
- Emberblade Slash
- Fireball Projectile
- Splash Shot
- Boat Broadside
- Creature Pounce
- Reef Crab Loot
- Ember Wisp Loot
- Stormling Loot


## v16 additions

### Player blocking, dodge, and stamina combat
Added:
- stamina resource
- stamina tuning ScriptableObject
- combat defense state
- blocking
- block stamina drain
- perfect block window
- dodge movement
- dodge i-frames
- dodge cooldown
- combat damage mitigation integration
- stamina pickup interactable
- combat defense debug panel
- admin stamina/defense commands
- stamina save/load
- starter tuning generator

Starter examples include:
- Starter Balanced Stamina
- Heavy Shield Stamina
- Agile Dodge Stamina


## v17 additions

### Enemy AI combat behaviors
Added:
- enemy AI states
- enemy combat profiles
- perception sensor
- patrol route
- movement motor
- melee attack driver
- ranged attack driver
- weakness-aware attack selection
- flee behavior
- enemy combat brain
- boat enemy behavior
- creature pounce behavior
- boss-lite phase controller
- enemy AI debug panel
- admin AI commands
- enemy AI save/load
- starter AI profile generator

Starter examples include:
- Basic Melee Raider
- Ranged Fire Caster
- Coward Scout
- Boat Raider
- Creature Hunter
- Boss-lite Guardian


## v18 additions

### Spell casting and cooldown UI
Added:
- spell cast definitions
- spell targeting modes
- spell resource pool
- spell cooldown tracker
- spell casting controller
- spell loadout controller
- spell cooldown HUD slot
- cast bar UI
- spell resource bar UI
- targeting reticle helper
- spell save/load
- admin spell commands
- starter spell generator
- Wind Waker-like spell icon PNG set

Starter examples include:
- Fireball
- Water Splash
- Earth Root
- Air Gust
- Ice Shard
- Storm Bolt
- Healing Bloom
- Boat Wind Burst


## v19 additions

### Boss framework and boss UI
Added boss definitions, boss phases, phase actions, boss controller, arena controller, arena triggers, arena hazards, reward chest, event hub, health bar UI, phase banner UI, debug panel, admin commands, save/load, starter boss generator, and Wind Waker-like boss icons.

Starter examples include:
- Reef Guardian
- Ember Titan
- Storm Serpent


## v20 additions

### Full quest UI and objective tracker presentation
Added quest UI definitions, runtime tracking, HUD/log/popup/reward views, waypoint binding, start/completion triggers, admin commands, save/load, starter quest examples, and Wind Waker-like quest icon PNGs.

Starter examples include:
- Find Kram at Neritha Reefwood
- Repair the Boat
- Boss Lair Awakens


## v21 additions

### Scene bootstrap and playable test scene scaffolding
Added:
- runtime systems bootstrap
- playable scene bootstrap profile
- player test rig setup helper
- test enemy spawner
- test boss arena builder
- test boat setup helper
- quest/combat UI canvas builder
- starter scene checklist generator
- playable scene example seeder
- one-click editor scene builder
- Wind Waker-like setup icon PNG set

Starter examples include:
- Player Test Rig
- Test Enemy
- Test Boss Arena
- Test Boat
- Quest/Combat UI Canvas
- Playable Scene Checklist


## v22 additions

### Integration hardening pass
Applied targeted compatibility fixes across v1–v21 and added runtime/editor diagnostics.

Concrete fixes include:
- Patched Game/QuestUI/QuestUiTracker.cs: quest objective calls now use QuestObjectiveTracker.Ensure().
- Patched Game/Bosses/BossController.cs: quest objective calls now use QuestObjectiveTracker.Ensure().
- Added WaypointTracker.SetWaypoint(Vector3,string,string) compatibility overload.
- Added BossController.Configure(BossDefinition,BossArenaController) for playable test boss setup.
- Added BossArenaTrigger.Configure(BossController) to avoid reflection-only wiring.
- Updated TestBossArenaSetup to use BossArenaTrigger.Configure instead of reflection for boss trigger wiring.

Added:
- ElementbornIntegrationDiagnostics
- ElementbornIntegrationHardeningMenu
- IntegrationHardeningChecklist_v22 generator


## v23 additions

### Unity setup automation and generator hardening
Added:
- Unity project setup wizard
- tag/layer setup menu
- UI texture sprite-import setup menu
- EventSystem setup menu
- build-settings scene entry helper
- safe starter generator runner
- serialized-field safety validation report
- generated runtime/player/UI prefab builder
- runtime Unity setup validator
- additional setup icons

Starter setup examples include:
- Runtime Systems prefab
- Minimal Player prefab
- Basic UI Canvas prefab
- Unity setup report
- generator field safety report


## v24 additions

### Unity import and runtime error-prevention pass
Added:
- version-safe Unity object find wrappers
- safe component extension helpers
- runtime scene safety net
- runtime null-reference guard
- generated content presence report
- Unity menu to add safety net to the open scene
- Unity menu to write runtime safety report
- replacement pass for obsolete FindObjectOfType / FindObjectsOfType usage

Runtime fallback setup can create:
- EventSystem
- Main Camera
- UI Canvas
- Runtime Bootstrap
- fallback Player capsule with core test components


## v25 additions

### Polished UI prefabs and canvas wiring
Added:
- common Wind Waker-like UI frame sprites
- UI theme ScriptableObject
- UI theme applier
- HUD auto-binder
- quest log/debug panel toggler
- playable HUD prefab builder
- quest log prefab builder
- boss HUD prefab builder
- spell HUD prefab builder
- debug HUD prefab builder
- open-scene UI installer menus
- UI prefab report menu

Generated UI prefabs include:
- Elementborn_PlayableHudCanvas
- Elementborn_QuestLogCanvas
- Elementborn_BossHudCanvas
- Elementborn_SpellHudCanvas
- Elementborn_DebugHudCanvas


## v26 additions

### Animation/VFX placeholders and hit feedback
Added:
- Wind Waker-like impact VFX sprites
- hit feedback definitions
- impact sprite runtime effect
- target hit flash
- hit pause
- camera shake
- character hit reaction animator bridge
- weapon trail controller
- animator combat event bridge
- attack animation bridge
- hit feedback service
- starter hit feedback generator
- combat damage utility feedback integration

Starter examples include:
- normal slash impact
- fire/water/earth/air/ice/lightning impacts
- block spark
- dodge puff
- critical starburst
- healing bloom
- weapon trail swipe


## v27 additions

### Placeholder audio, sound routing, and NPC voice/roster prep
Added:
- actual generated WAV placeholder sounds
- sound event IDs and categories
- sound event ScriptableObjects
- audio bus settings
- audio service
- audio event router for quest/boss events
- sound trigger component
- hit feedback audio integration
- NPC world role enum
- NPC voice line definitions
- NPC world entry definitions
- NPC voice playback controller
- NPC placement marker
- CSV templates for royal family and villain NPC lists
- NPC roster CSV importer menus


## v28 additions

### Earth Capital royal family + richer NPC roster schema
Added:
- richer NPC world entry fields
- richer CSV roster schema
- quoted CSV parsing and flexible header mapping
- Earth Capital royal family CSV with Chrief Gover, Queen Alexis Malachite, and Tatiana "Tibidi" Malachite


## v29 additions

### NPC world placement, quest hooks, and dialogue integration
Added:
- NPC world integration manifest
- NPC world integration manager
- data-driven NPC dialogue interactable
- conversation profile binder
- quest-start NPC interactable
- NPC integration asset generator
- generated dialogue profile workflow
- generated Earth Capital royal family quest workflow
- world placement manager prefab workflow


## v30 additions

### NPC admin UI + Neritha pirate crew
Added:
- Captain Ramón
- First Mate Sarah
- Raucous Tide pirate ship/location notes
- NPC admin filter
- NPC admin registry
- NPC admin panel view
- NPC placement admin tool
- NPC admin command bridge
- NPC admin UI prefab builder
- NPC admin report generator

New runtime commands:
- npc.list
- npc.search text
- npc.region regionName
- npc.element elementName
- npc.id npcId
- npc.place npcId


## v31 additions

### Named ships, crew systems, Raucous Tide, Metal Capital royals, and Howlj/Larissa
Added:
- named ship definition system
- ship crew roles
- ship reputation tracker
- raid celebration controller
- ship quest hook interactable
- ship admin command bridge
- named ship asset generator
- Raucous Tide crew roster
- Metal Capital royal family roster
- Howlj and Larissa villain roster
- Raucous Tide Afterparty starter quest


## v32 additions

### Metal Capital world hooks
Added:
- black market contact definitions
- black market listings
- thieves guild reputation tracker
- Metal Capital intrigue hooks
- hidden channeler secret tracker
- Metal Capital registry
- vendor/intrigue interactables
- Metal Capital admin commands
- Metal Capital asset generator
- Bubba's Black-Market Bolt quest
- The Metal in Larissa's Silence quest


## v33 additions

### Wind Capital, Sarah hidden-past hooks, and Redbeard's theocracy
Added:
- Wind Capital theocracy roster
- High Priest Redbeard
- Priestess Lizkota
- infant Ruth
- Wind Capital districts
- religious fervor tracker
- Wind Capital secret tracker
- Wind Capital intrigue hooks
- Sarah past quest bridge
- Wind Capital admin commands
- The Wind Sarah Won't Name quest
- The Chaotic Aerie quest


## v34 additions

### Capital political pressure world state
Added:
- shared capital IDs
- capital control statuses
- political pressure types
- pressure severity calculation
- capital world-state definitions
- runtime state tracker
- political pressure events
- regional sync from Wind/Metal systems
- world-state admin commands
- generated capital state assets
- generated pressure event assets


## v35 additions

### Political world event director
Added:
- political world event states
- political event categories
- pressure-based event conditions
- event consequences
- event runtime records
- event hub
- event director
- event trigger interactable
- event admin commands
- generated starter political events


## v36 additions

### Quest chain director
Added:
- quest-chain definitions
- quest-chain runtime records
- multi-stage progression
- branching choices
- choice consequences
- political event trigger linkage
- chain event hub
- chain director
- chain choice interactable
- chain admin commands
- generated starter chain assets


## v37 additions

### Narrative runtime save/load
Added:
- capital world-state save data and bridge
- political world-event save data and bridge
- quest-chain save data and bridge
- combined narrative runtime save bridge
- admin command bridge for narrative save/load
- Unity setup menu for narrative save bridges
- generated save prefab/report


## v38 additions

### Story debug dashboard and new villain/friendly encounters
Added:
- story systems debug dashboard
- dashboard input shortcuts
- new villains/friendly NPC roster
- story encounter definitions
- story encounter registry
- Donowl sleeper/distracted monster controller
- The Judge thunder-voice territory guardian
- timed dual-leader wolf-pack respawn controller
- creature orphanage healing service
- story encounter admin commands
- generated story encounter assets/reports


## v39 additions

### Story encounter progression, quests, and save support
Added:
- story encounter runtime state
- story encounter progress tracker
- story encounter save/load bridge
- encounter progress admin commands
- generated encounter quests
- dashboard story encounter progress section


## v40 additions

### Encounter prefab automation and creature orphanage recovery hub
Added:
- placeholder prefab creation for Donowl, The Judge, Romilus/Madrangea pack, Michelle, and Crab-Sign Creature Orphanage
- open-scene placeholder encounter installation menu
- tamed creature orphanage return hook
- orphanage resident recovery records
- buy-back / lure-back / rehome flow
- orphanage recovery save/load bridge
- narrative runtime save integration
- dashboard orphanage recovery summary and save path


## v41 additions

### Social circle and neighborhood NPC roster expansion
Added:
- a new NPC roster for Rekr Ap, Manon, Marie Conflag, Amy Whine, Kelly, and Johna Rold
- a Social NPC roster generator/report menu
- Wind Capital and Fire Capital world-state flavor updates and new pressure events
- NPC admin report coverage for the new roster


## v42 additions

### Social NPC quest/dialogue/prefab setup
Added:
- social NPC dialogue profile assets and registry
- runtime dialogue hook interactable
- social NPC behavior controllers for Rekr, Manon, Marie, Kelly, and Johna
- generated social NPC quests and quest chains
- placeholder prefabs and open-scene installer
- Fire Capital royal family draft template outside the auto-import roster folder
- story debug dashboard social NPC section


## v43 additions

### Social group / neighborhood life system
Added:
- social group ScriptableObject definitions
- social group event definitions
- relationship/member records
- group runtime records
- event activation flow
- schedule director
- admin command bridge
- social group save bridge
- generated Wind Lower Terrace and Fire Basalt Row group assets
- dashboard social group summary


## v44 additions

### Polish and debug pass
Fixed:
- generated interactables using an obsolete `GetPrompt` return type
- missing `JournalEntryType.Character` compatibility alias
- missing `MapMarkerType.QuestObjective` and `MapMarkerType.Vendor` compatibility aliases
- missing `Elementborn.Core` imports where generated files use map marker types
- duplicate local variable in `NamedShipAssetGenerator`

Added:
- `Elementborn → Diagnostics → Write v44 Polish Debug Report`


## v45 additions

- capital landmark prefab generator and scene installer
- Fire Capital royal family roster, names, and relationships
- rounded playable scene builder and generated-system installer
- UI action buttons added to the story debug dashboard
- EditMode tests for newest systems
- runtime bootstrap extended to include major story/system singletons
