#!/bin/sh
# Extract the models that the alias maps reference, from the Meshy AI batch in Assets/generated_assets,
# into Assets/Elementborn/Resources/Models/<Category>/<alias>/<alias>.fbx so the binding bridges find them.
# Run from the project root on your machine (needs `unzip`):  sh tools/import-creature-models.sh
#
# Each line is:  import_one <Category> <alias> <zip-name-prefix>   (prefix because Meshy truncates some names).
# Categories match the ResourceRoot of each map: Creatures, Npcs, Sidekicks, Weapons, Characters, Props.
# Edit/extend to match changes you make to the maps. Unmapped entries keep the primitive fallback.
#
# Meshy zips contain the .fbx plus textures; this copies the .fbx and sibling textures into a per-model folder.
# If your zips nest differently, adjust below.

set -e
SRC="Assets/generated_assets"
ROOT="Assets/Elementborn/Resources/Models"

import_one() {
    category="$1"; alias="$2"; prefix="$3"
    dst="$ROOT/$category/$alias"
    zip=$(ls "$SRC"/${prefix}*.zip 2>/dev/null | head -1 || true)
    if [ -z "$zip" ]; then echo "  SKIP $category/$alias — no zip matching ${prefix}*"; return; fi
    tmp=$(mktemp -d)
    unzip -qo "$zip" -d "$tmp"
    fbx=$(find "$tmp" -iname '*.fbx' | head -1)
    if [ -z "$fbx" ]; then echo "  WARN $category/$alias — no .fbx inside $(basename "$zip")"; rm -rf "$tmp"; return; fi
    mkdir -p "$dst"
    cp "$fbx" "$dst/$alias.fbx"
    find "$tmp" \( -iname '*.png' -o -iname '*.jpg' -o -iname '*.jpeg' \) -exec cp {} "$dst/" \; 2>/dev/null || true
    rm -rf "$tmp"
    echo "  OK   $category/$alias  <-  $(basename "$zip")"
}

# --- Creatures (CreatureModelNames.Aliases) ---
import_one Creatures Azure_Wave_Dragon       Meshy_AI_Azure_Wave_Dragon_
import_one Creatures Fire_Phoenix            Meshy_AI_Fire_Phoenix_
import_one Creatures Thunderbird             Meshy_AI_Thunderbird_
import_one Creatures Giant_Eagle             Meshy_AI_Giant_Eagle_
import_one Creatures Patchwork_Pup           Meshy_AI_Patchwork_Pup_
import_one Creatures Antler_Spider_Creature  Meshy_AI_Antler_Spider_Creatur
import_one Creatures Coral_Crab_Spider       Meshy_AI_Coral_Crab_Spider_
import_one Creatures Teal_Serpent            Meshy_AI_Teal_Serpent_
import_one Creatures Leaf_Cub                Meshy_AI_Leaf_Cub_
import_one Creatures Blue_Dino_Mount         Meshy_AI_Blue_Dino_Mount_
import_one Creatures Blue_Gold_Tuna          Meshy_AI_Blue_Gold_Tuna_
import_one Creatures Teal_Fantasy_Fish       Meshy_AI_Teal_Fantasy_Fish_
import_one Creatures Abyss_Angler            Meshy_AI_Abyss_Angler_
import_one Creatures Purple_Kraken           Meshy_AI_Purple_Kraken_
import_one Creatures Shadow_Wolf             Meshy_AI_Shadow_Wolf_
import_one Creatures Storm_Wyvern            Meshy_AI_Storm_Wyvern_
import_one Creatures Blue_Fantasy_Bird       Meshy_AI_Blue_Fantasy_Bird_
import_one Creatures Fawn_Sprite             Meshy_AI_Fawn_Sprite_

# --- NPCs (NpcModelNames) ---
import_one Npcs Verdant_Dryad                Meshy_AI_Verdant_Dryad_
import_one Npcs Azure_Water_Mage             Meshy_AI_Azure_Water_Mage_
import_one Npcs Steamwright_Adventurer       Meshy_AI_Steamwright_Adventure

# --- Sidekicks (SidekickModelNames) ---
import_one Sidekicks Moss_Wolf               Meshy_AI_Moss_Wolf_
import_one Sidekicks Teal_Hornbill           Meshy_AI_Teal_Hornbill_
import_one Sidekicks Lure_Fish               Meshy_AI_Lure_Fish_
import_one Sidekicks Luminescent_Mushroom    Meshy_AI_Luminescent_Mushroom

# --- Weapons / gear (WeaponModelNames) ---
import_one Weapons Emberblade                Meshy_AI_Emberblade_
import_one Weapons Gilded_Arc_Bow            Meshy_AI_Gilded_Arc_Bow_
import_one Weapons Azure_Aegis               Meshy_AI_Azure_Aegis_
import_one Weapons Stormcleaver_Axe          Meshy_AI_Stormcleaver_Axe_

# --- Items / gear loot (ItemModelNames) ---
import_one Items Emberstone_Gem            Meshy_AI_Emberstone_Gem_
import_one Items Pearl_Oyster              Meshy_AI_Pearl_Oyster_
import_one Items Triskelion_Disc           Meshy_AI_Triskelion_Disc_
import_one Items Prismatic_Helix_Gem       Meshy_AI_Prismatic_Helix_Gem_

# --- Player avatar (PlayerModelNames) ---
import_one Characters Windborne_Traveler     Meshy_AI_Windborne_Traveler_
# Rigged player: place your own skinned humanoid prefab (w/ Animator) at
#   Resources/Models/Characters/PlayerRigged/PlayerRigged.prefab  (no Meshy zip — rig via Mixamo/etc).

echo "Done. Each model is at $ROOT/<Category>/<alias>/<alias>.fbx with its textures alongside,"
echo "which is what the *ModelNames maps point at. Props (PropCatalog) are placed in scenes by hand."
