#!/usr/bin/env python3
"""
import_generated_assets.py

Run from your Unity Assets directory, for example:

    cd Elementborn/Assets
    python import_generated_assets.py --overwrite

To download the generated asset ZIPs from GitHub first:

    python import_generated_assets.py --download --overwrite

What it does:
- Finds Meshy ZIP files in Assets/generated_assets
- Optionally downloads all .zip files from the GitHub generated_assets folder
- Safely extracts each ZIP to a temporary folder outside Assets
- Finds the primary .fbx in each ZIP
- Maps known asset names into the Resources paths used by Elementborn code
- Copies sidecar textures/material files where possible
- Writes a CSV manifest showing what was imported, mapped, or left unmapped

Expected project layout:

    Elementborn/
      Assets/
        generated_assets/
        Elementborn/
          Resources/
            Models/

No third-party Python packages are required.
"""

from __future__ import annotations

import argparse
import csv
import difflib
import json
import os
import re
import shutil
import sys
import urllib.request
import zipfile
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Optional


# ---------------------------------------------------------------------------
# GitHub source
# ---------------------------------------------------------------------------

DEFAULT_GITHUB_OWNER = "aluminumBath"
DEFAULT_GITHUB_REPO = "elementalClash"
DEFAULT_GITHUB_BRANCH = "master"
DEFAULT_GITHUB_ASSET_PATH = "Elementborn/Assets/generated_assets"


# ---------------------------------------------------------------------------
# Known Elementborn runtime Resources mappings.
#
# These are based on the project's C# ResourcePath values:
#   Assets/Elementborn/Core/CreatureModelNames.cs
#   Assets/Elementborn/Core/ModelBindings.cs
#   Assets/Elementborn/Core/BossCatalog.cs
#
# Values are Resources-relative paths, WITHOUT extension.
# Example:
#   Assets/Elementborn/Resources/Models/Creatures/Fire_Phoenix/Fire_Phoenix.fbx
# is represented as:
#   Models/Creatures/Fire_Phoenix/Fire_Phoenix
# ---------------------------------------------------------------------------

KNOWN_RESOURCE_TARGETS: dict[str, str] = {
    # Creatures
    "Azure_Wave_Dragon": "Models/Creatures/Azure_Wave_Dragon/Azure_Wave_Dragon",
    "Fire_Phoenix": "Models/Creatures/Fire_Phoenix/Fire_Phoenix",
    "Thunderbird": "Models/Creatures/Thunderbird/Thunderbird",
    "Giant_Eagle": "Models/Creatures/Giant_Eagle/Giant_Eagle",
    "Patchwork_Pup": "Models/Creatures/Patchwork_Pup/Patchwork_Pup",
    "Antler_Spider_Creature": "Models/Creatures/Antler_Spider_Creature/Antler_Spider_Creature",
    "Coral_Crab_Spider": "Models/Creatures/Coral_Crab_Spider/Coral_Crab_Spider",
    "Teal_Serpent": "Models/Creatures/Teal_Serpent/Teal_Serpent",
    "Leaf_Cub": "Models/Creatures/Leaf_Cub/Leaf_Cub",
    "Blue_Dino_Mount": "Models/Creatures/Blue_Dino_Mount/Blue_Dino_Mount",
    "Blue_Gold_Tuna": "Models/Creatures/Blue_Gold_Tuna/Blue_Gold_Tuna",
    "Teal_Fantasy_Fish": "Models/Creatures/Teal_Fantasy_Fish/Teal_Fantasy_Fish",
    "Abyss_Angler": "Models/Creatures/Abyss_Angler/Abyss_Angler",
    "Purple_Kraken": "Models/Creatures/Purple_Kraken/Purple_Kraken",
    "Shadow_Wolf": "Models/Creatures/Shadow_Wolf/Shadow_Wolf",
    "Storm_Wyvern": "Models/Creatures/Storm_Wyvern/Storm_Wyvern",
    "Blue_Fantasy_Bird": "Models/Creatures/Blue_Fantasy_Bird/Blue_Fantasy_Bird",
    "Fawn_Sprite": "Models/Creatures/Fawn_Sprite/Fawn_Sprite",
    "Ember_Dragon": "Models/Creatures/Ember_Dragon/Ember_Dragon",
    "Current_Eel": "Models/Creatures/Current_Eel/Current_Eel",
    "Tide_Mermaid": "Models/Creatures/Tide_Mermaid/Tide_Mermaid",
    "EarthMole_Stone_Mole": "Models/Creatures/EarthMole_Stone_Mole/EarthMole_Stone_Mole",
    "AirDragonfly_Gale_Dragonfly": "Models/Creatures/AirDragonfly_Gale_Dragonfly/AirDragonfly_Gale_Dragonfly",
    "AirJellyfish_Sky_Jellyfish": "Models/Creatures/AirJellyfish_Sky_Jellyfish/AirJellyfish_Sky_Jellyfish",
    "WaterCat_Wave_Cat": "Models/Creatures/WaterCat_Wave_Cat/WaterCat_Wave_Cat",
    "IceCat": "Models/Creatures/IceCat/IceCat",
    "Spark_Squirrel": "Models/Creatures/Spark_Squirrel/Spark_Squirrel",
    "Canopy_Monkey": "Models/Creatures/Canopy_Monkey/Canopy_Monkey",
    "Marsh_Crocodile": "Models/Creatures/Marsh_Crocodile/Marsh_Crocodile",
    "Boulder_Rhino": "Models/Creatures/Boulder_Rhino/Boulder_Rhino",
    "Tigris_Prowler": "Models/Creatures/Tigris_Prowler/Tigris_Prowler",
    "Skyotter": "Models/Creatures/Skyotter/Skyotter",

    # Guide NPCs and frog NPCs
    "Verdant_Dryad": "Models/Npcs/Verdant_Dryad/Verdant_Dryad",
    "Azure_Water_Mage": "Models/Npcs/Azure_Water_Mage/Azure_Water_Mage",
    "Steamwright_Adventurer": "Models/Npcs/Steamwright_Adventurer/Steamwright_Adventurer",
    "Hurricane_Frog": "Models/Npcs/Hurricane_Frog/Hurricane_Frog",
    "Steam_Frog": "Models/Npcs/Steam_Frog/Steam_Frog",

    # Willow sidekicks
    "Moss_Wolf": "Models/Sidekicks/Moss_Wolf/Moss_Wolf",
    "Raven_Parrot": "Models/Sidekicks/Raven_Parrot/Raven_Parrot",
    "Lure_Fish": "Models/Sidekicks/Lure_Fish/Lure_Fish",
    "Luminescent_Mushroom": "Models/Sidekicks/Luminescent_Mushroom/Luminescent_Mushroom",
    "Prism_Chameleon": "Models/Sidekicks/Prism_Chameleon/Prism_Chameleon",

    # Weapons and wand
    "Emberblade": "Models/Weapons/Emberblade/Emberblade",
    "Gilded_Arc_Bow": "Models/Weapons/Gilded_Arc_Bow/Gilded_Arc_Bow",
    "Azure_Aegis": "Models/Weapons/Azure_Aegis/Azure_Aegis",
    "Stormcleaver_Axe": "Models/Weapons/Stormcleaver_Axe/Stormcleaver_Axe",
    "Fang_Dagger": "Models/Weapons/Fang_Dagger/Fang_Dagger",
    "Twin_Sai": "Models/Weapons/Twin_Sai/Twin_Sai",
    "Clockwork_Eye_Key": "Models/Weapons/Clockwork_Eye_Key/Clockwork_Eye_Key",

    # Player / character
    "Windborne_Traveler": "Models/Characters/Windborne_Traveler/Windborne_Traveler",
    "PlayerRigged": "Models/Characters/PlayerRigged/PlayerRigged",

    # Items
    "Emberstone_Gem": "Models/Items/Emberstone_Gem/Emberstone_Gem",
    "Pearl_Oyster": "Models/Items/Pearl_Oyster/Pearl_Oyster",
    "Triskelion_Disc": "Models/Items/Triskelion_Disc/Triskelion_Disc",
    "Prismatic_Helix_Gem": "Models/Items/Prismatic_Helix_Gem/Prismatic_Helix_Gem",
    "Healing_Tonic": "Models/Items/Healing_Tonic/Healing_Tonic",
    "Stamina_Draught": "Models/Items/Stamina_Draught/Stamina_Draught",
    "Vigor_Elixir": "Models/Items/Vigor_Elixir/Vigor_Elixir",
    "Ore_Marrow_Bone": "Models/Items/Ore_Marrow_Bone/Ore_Marrow_Bone",
    "Sunflower_Seeds": "Models/Items/Sunflower_Seeds/Sunflower_Seeds",
    "Cured_Leather": "Models/Items/Cured_Leather/Cured_Leather",
    "Poison_Vial": "Models/Items/Poison_Vial/Poison_Vial",
    "Feathered_Arrow": "Models/Items/Feathered_Arrow/Feathered_Arrow",
    "Crescent_Hook": "Models/Items/Crescent_Hook/Crescent_Hook",
    "Ouroboros": "Models/Items/Ouroboros/Ouroboros",

    # Props are currently flat Resources paths in PropCatalog.
    # Example: Resources.Load("Models/Props/Azure_Arc_Portal")
    "Azure_Arc_Portal": "Models/Props/Azure_Arc_Portal",
    "Azure_Crystal_Spire": "Models/Props/Azure_Crystal_Spire",
    "Throne_of_the_Crystal": "Models/Props/Throne_of_the_Crystal",
    "Vine_Gate": "Models/Props/Vine_Gate",
    "Glowcap_Grove": "Models/Props/Glowcap_Grove",
    "Treasure_Chest": "Models/Props/Treasure_Chest",
    "Azure_Ornate_Banner": "Models/Props/Azure_Ornate_Banner",
    "Emerald_Cavern_Pool": "Models/Props/Emerald_Cavern_Pool",
    "Radiant_Purple_Tree": "Models/Props/Radiant_Purple_Tree",
    "Underwater_Coral_Garden": "Models/Props/Underwater_Coral_Garden",

    # Bosses are flat Resources paths in BossCatalog / CreatureModelLibrary.
    "Prismatic_Phoenix": "Models/Bosses/Prismatic_Phoenix",
    "Azure_Arbor_Guardian": "Models/Bosses/Azure_Arbor_Guardian",
    "Ironhorn_Warden": "Models/Bosses/Ironhorn_Warden",

    # --- Explicit aliases: a real generated_assets zip stem -> an existing target path, for targets whose
    #     default name matched no zip. Added after auditing all 225 zips against these targets. Each maps a
    #     real asset into the Resources path the game code already loads, so no C# change is needed.
    "Mossback_Pup_3D": "Models/Creatures/Patchwork_Pup/Patchwork_Pup",        # Dog
    "Plume_Parrot_3D": "Models/Sidekicks/Raven_Parrot/Raven_Parrot",          # Willow's parrot sidekick
    "Mooncap_Glade": "Models/Props/Glowcap_Grove",                            # glowing-mushroom grove prop

    # New purchasable mounts bound to standout creature assets.
    "Bonebound_Behemoth":   "Models/Creatures/Bonebound_Behemoth/Bonebound_Behemoth",
    "Ancient_Stag":         "Models/Creatures/Ancient_Stag/Ancient_Stag",
    "Coral_Whale_Monster":  "Models/Creatures/Coral_Whale_Monster/Coral_Whale_Monster",
    "Embercrest_Kitebeast": "Models/Creatures/Embercrest_Kitebeast/Embercrest_Kitebeast",
    "Azurewing_Knight":     "Models/Creatures/Azurewing_Knight/Azurewing_Knight",
    "Emerald_Dragon":       "Models/Creatures/Emerald_Dragon/Emerald_Dragon",
    "Blue_Dragon":          "Models/Creatures/Blue_Dragon/Blue_Dragon",
    "Storm_Shadow_Wolf":    "Models/Creatures/Storm_Shadow_Wolf/Storm_Shadow_Wolf",
    "Lightning_Dark_Wolf":  "Models/Creatures/Lightning_Dark_Wolf/Lightning_Dark_Wolf",

    # Wardrobe: 12 cosmetic Channeler looks. APPEARANCE ONLY — a look never changes the player's element. Keyed on
    # the element word so both the raw Meshy names and the renamed copies match; one Resources folder per look.
    "Channeler_Hero_3D":     "Models/Characters/Channeler/Default/Default",
    "Channeler_Hero_None":   "Models/Characters/Channeler/Plain/Plain",
    "Channeler_Hero_Air":    "Models/Characters/Channeler/Air/Air",
    "Channeler_Hero_Water":  "Models/Characters/Channeler/Water/Water",
    "Channeler_Hero_Fire":   "Models/Characters/Channeler/Fire/Fire",
    "Channeler_Hero_Earth":  "Models/Characters/Channeler/Earth/Earth",
    "Channeler_Hero_Lava":   "Models/Characters/Channeler/Lava/Lava",
    "Channeler_Hero_Steam":  "Models/Characters/Channeler/Steam/Steam",
    "Channeler_Hero_Metal":  "Models/Characters/Channeler/Metal/Metal",
    "Channeler_Hero_Plant":  "Models/Characters/Channeler/Plant/Plant",
    "Channeler_Hero_Blood":  "Models/Characters/Channeler/Blood/Blood",
    "Channeler_Hero_Paraly": "Models/Characters/Channeler/Paralysis/Paralysis",
}


IGNORED_FILE_PARTS = {"__MACOSX", ".DS_Store"}
IGNORED_SUFFIXES = {".meta"}


@dataclass
class ImportResult:
    zip_file: str
    status: str
    asset_key: str
    matched_key: str
    match_score: str
    resource_path: str
    primary_fbx: str
    destination_fbx: str
    notes: str


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Import Elementborn Meshy ZIP assets into Unity Resources paths."
    )
    parser.add_argument(
        "--download",
        action="store_true",
        help="Download .zip assets from the GitHub generated_assets folder before importing.",
    )
    parser.add_argument(
        "--overwrite",
        action="store_true",
        help="Overwrite existing imported files. Without this, existing destination .fbx files are skipped.",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Report what would happen without extracting or copying files.",
    )
    parser.add_argument(
        "--exact-only",
        action="store_true",
        help="Only import ZIPs whose normalized names exactly match known targets. Others go to Unmapped.",
    )
    parser.add_argument(
        "--min-score",
        type=float,
        default=0.72,
        help="Minimum fuzzy match score for mapping ZIP names to known Resources paths. Default: 0.72.",
    )
    parser.add_argument(
        "--github-owner",
        default=DEFAULT_GITHUB_OWNER,
        help=f"GitHub owner/org. Default: {DEFAULT_GITHUB_OWNER}",
    )
    parser.add_argument(
        "--github-repo",
        default=DEFAULT_GITHUB_REPO,
        help=f"GitHub repo. Default: {DEFAULT_GITHUB_REPO}",
    )
    parser.add_argument(
        "--github-branch",
        default=DEFAULT_GITHUB_BRANCH,
        help=f"GitHub branch. Default: {DEFAULT_GITHUB_BRANCH}",
    )
    parser.add_argument(
        "--github-asset-path",
        default=DEFAULT_GITHUB_ASSET_PATH,
        help=f"Repo path containing generated ZIPs. Default: {DEFAULT_GITHUB_ASSET_PATH}",
    )
    parser.add_argument(
        "--assets-dir",
        default=None,
        help="Unity Assets directory. Defaults to current working directory if it looks like Assets.",
    )
    return parser.parse_args()


def resolve_assets_dir(arg_value: Optional[str]) -> Path:
    if arg_value:
        return Path(arg_value).resolve()

    cwd = Path.cwd().resolve()

    if cwd.name == "Assets":
        return cwd

    if (cwd / "Assets").is_dir():
        return (cwd / "Assets").resolve()

    # If launched from somewhere inside Assets, walk upward.
    for parent in [cwd, *cwd.parents]:
        if parent.name == "Assets":
            return parent.resolve()

    raise SystemExit(
        "Could not determine Unity Assets directory. Run this from Elementborn/Assets "
        "or pass --assets-dir path/to/Elementborn/Assets"
    )


def default_resources_dir(assets_dir: Path) -> Path:
    elementborn_resources = assets_dir / "Elementborn" / "Resources"
    if (assets_dir / "Elementborn").exists():
        return elementborn_resources
    return assets_dir / "Resources"


def generated_assets_dir(assets_dir: Path) -> Path:
    return assets_dir / "generated_assets"


def temp_extract_dir(assets_dir: Path) -> Path:
    # Keep extraction outside Assets so Unity does not import temporary duplicate assets.
    return assets_dir.parent / "Temp" / "generated_asset_extracts"


def ensure_dir(path: Path, dry_run: bool) -> None:
    if dry_run:
        return
    path.mkdir(parents=True, exist_ok=True)


def github_api_request(url: str) -> object:
    headers = {
        "User-Agent": "ElementbornGeneratedAssetImporter/1.0",
        "Accept": "application/vnd.github+json",
    }
    token = os.environ.get("GITHUB_TOKEN", "").strip()
    if token:
        headers["Authorization"] = f"Bearer {token}"

    req = urllib.request.Request(url, headers=headers)
    with urllib.request.urlopen(req) as response:
        raw = response.read().decode("utf-8")
    return json.loads(raw)


def download_generated_zip_assets(
    destination_dir: Path,
    owner: str,
    repo: str,
    branch: str,
    repo_asset_path: str,
    overwrite: bool,
    dry_run: bool,
) -> None:
    api_url = (
        f"https://api.github.com/repos/{owner}/{repo}/contents/"
        f"{repo_asset_path}?ref={branch}"
    )

    print(f"Checking GitHub generated_assets folder:")
    print(f"  {api_url}")

    if dry_run:
        print("DRY RUN: would request GitHub API listing and download .zip files.")
        return

    ensure_dir(destination_dir, dry_run=False)
    items = github_api_request(api_url)

    if not isinstance(items, list):
        raise RuntimeError(f"Unexpected GitHub API response: {items!r}")

    zip_items = [
        item for item in items
        if isinstance(item, dict)
        and item.get("type") == "file"
        and str(item.get("name", "")).lower().endswith(".zip")
        and item.get("download_url")
    ]

    print(f"Found {len(zip_items)} ZIP file(s) on GitHub.")

    for item in zip_items:
        name = item["name"]
        url = item["download_url"]
        dest = destination_dir / name

        if dest.exists() and not overwrite:
            print(f"  skip existing: {name}")
            continue

        print(f"  download: {name}")
        req = urllib.request.Request(
            url,
            headers={"User-Agent": "ElementbornGeneratedAssetImporter/1.0"},
        )
        with urllib.request.urlopen(req) as response, open(dest, "wb") as out:
            shutil.copyfileobj(response, out)


def is_ignored_path(path: Path) -> bool:
    parts = set(path.parts)
    if parts.intersection(IGNORED_FILE_PARTS):
        return True
    if path.name in IGNORED_FILE_PARTS:
        return True
    if path.suffix.lower() in IGNORED_SUFFIXES:
        return True
    return False


def safe_extract_zip(zip_path: Path, destination: Path, dry_run: bool) -> None:
    if dry_run:
        return

    ensure_dir(destination, dry_run=False)

    with zipfile.ZipFile(zip_path, "r") as zf:
        for member in zf.infolist():
            member_name = member.filename.replace("\\", "/")
            member_path = Path(member_name)

            if member_name.endswith("/"):
                continue

            # Prevent path traversal attacks.
            if member_path.is_absolute() or ".." in member_path.parts:
                print(f"  warning: skipped unsafe ZIP entry {member.filename!r}")
                continue

            target = destination / member_path
            target_resolved = target.resolve()
            destination_resolved = destination.resolve()

            if not str(target_resolved).startswith(str(destination_resolved)):
                print(f"  warning: skipped unsafe ZIP entry {member.filename!r}")
                continue

            ensure_dir(target.parent, dry_run=False)
            with zf.open(member, "r") as src, open(target, "wb") as out:
                shutil.copyfileobj(src, out)


def strip_meshy_zip_name(zip_name: str) -> str:
    name = Path(zip_name).stem

    if name.startswith("Meshy_AI_"):
        name = name[len("Meshy_AI_"):]

    # Strip common Meshy timestamp/export suffixes:
    # _0623231738_image-to-3d-texture_fbx
    # _0623183402_texture_fbx
    name = re.sub(
        r"_[0-9]{10}_(image-to-3d-texture_fbx|texture_fbx|image_to_3d_texture_fbx)$",
        "",
        name,
        flags=re.IGNORECASE,
    )

    # Fallback: strip timestamp and everything after it.
    name = re.sub(r"_[0-9]{10}.*$", "", name)

    # Make it filesystem / C# alias friendly.
    name = name.strip(" _-")
    name = re.sub(r"[\s\-]+", "_", name)
    name = re.sub(r"_+", "_", name)

    return name or Path(zip_name).stem


def norm_name(value: str) -> str:
    value = value.lower()
    value = re.sub(r"[^a-z0-9]+", "", value)
    return value


def leaf_from_resource_path(resource_path: str) -> str:
    return resource_path.replace("\\", "/").split("/")[-1]


def find_best_target(asset_key: str, exact_only: bool, min_score: float) -> tuple[Optional[str], Optional[str], float]:
    asset_norm = norm_name(asset_key)

    best_key: Optional[str] = None
    best_path: Optional[str] = None
    best_score = 0.0

    for target_key, resource_path in KNOWN_RESOURCE_TARGETS.items():
        target_norm = norm_name(target_key)

        if asset_norm == target_norm:
            return target_key, resource_path, 1.0

        if exact_only:
            continue

        if asset_norm and target_norm:
            if asset_norm in target_norm or target_norm in asset_norm:
                score = 0.96
            else:
                score = difflib.SequenceMatcher(None, asset_norm, target_norm).ratio()
        else:
            score = 0.0

        if score > best_score:
            best_key = target_key
            best_path = resource_path
            best_score = score

    if best_key and best_path and best_score >= min_score:
        return best_key, best_path, best_score

    return None, None, best_score


def all_files(root: Path) -> list[Path]:
    if not root.exists():
        return []
    return [
        p for p in root.rglob("*")
        if p.is_file() and not is_ignored_path(p.relative_to(root))
    ]


def find_primary_fbx(extract_root: Path, asset_key: str) -> Optional[Path]:
    fbxs = [
        p for p in all_files(extract_root)
        if p.suffix.lower() == ".fbx"
    ]

    if not fbxs:
        return None

    asset_norm = norm_name(asset_key)

    def sort_key(path: Path) -> tuple[float, int]:
        name_norm = norm_name(path.stem)
        score = difflib.SequenceMatcher(None, asset_norm, name_norm).ratio()
        try:
            size = path.stat().st_size
        except OSError:
            size = 0
        return score, size

    fbxs.sort(key=sort_key, reverse=True)
    return fbxs[0]


def copy_sidecar_files(
    source_base: Path,
    destination_dir: Path,
    primary_fbx: Path,
    destination_fbx: Path,
    overwrite: bool,
    dry_run: bool,
) -> int:
    copied = 0
    files = all_files(source_base)

    for src in files:
        if src == primary_fbx:
            continue

        # Copy extra FBX files too, but do not rename them.
        try:
            rel = src.relative_to(source_base)
        except ValueError:
            rel = Path(src.name)

        dest = destination_dir / rel

        # Do not accidentally overwrite the renamed primary FBX.
        if dest.resolve() == destination_fbx.resolve():
            continue

        if dest.exists() and not overwrite:
            continue

        if not dry_run:
            ensure_dir(dest.parent, dry_run=False)
            shutil.copy2(src, dest)
        copied += 1

    return copied


def is_flat_resource_path(resource_path: str) -> bool:
    """
    Most code paths use Models/Category/Folder/FileName.
    Some props/bosses are flat: Models/Props/FileName or Models/Bosses/FileName.
    """
    parts = resource_path.replace("\\", "/").split("/")
    return len(parts) == 3


def import_one_zip(
    zip_path: Path,
    extract_root_base: Path,
    resources_dir: Path,
    overwrite: bool,
    dry_run: bool,
    exact_only: bool,
    min_score: float,
) -> ImportResult:
    asset_key = strip_meshy_zip_name(zip_path.name)
    matched_key, resource_path, score = find_best_target(asset_key, exact_only, min_score)

    if resource_path is None:
        # Keep unmapped assets available but do not pretend the game code references them.
        safe_key = re.sub(r"[^A-Za-z0-9_]+", "_", asset_key).strip("_") or "UnmappedAsset"
        resource_path = f"Models/Unmapped/{safe_key}/{safe_key}"
        matched_key = ""
        status = "unmapped"
        notes = "No confident known ResourcePath match. Imported under Models/Unmapped."
    else:
        status = "mapped"
        notes = ""

    extract_root = extract_root_base / asset_key

    if not dry_run:
        if extract_root.exists():
            shutil.rmtree(extract_root)
        safe_extract_zip(zip_path, extract_root, dry_run=False)

    primary_fbx = None if dry_run else find_primary_fbx(extract_root, asset_key)

    if primary_fbx is None:
        return ImportResult(
            zip_file=zip_path.name,
            status="dry-run" if dry_run else "no_fbx_found",
            asset_key=asset_key,
            matched_key=matched_key or "",
            match_score=f"{score:.3f}",
            resource_path=resource_path,
            primary_fbx="",
            destination_fbx="",
            notes="Would import here." if dry_run else "No .fbx file found inside ZIP.",
        )

    destination_fbx = resources_dir / (resource_path + ".fbx")
    destination_dir = destination_fbx.parent

    if destination_fbx.exists() and not overwrite:
        return ImportResult(
            zip_file=zip_path.name,
            status="skipped_exists",
            asset_key=asset_key,
            matched_key=matched_key or "",
            match_score=f"{score:.3f}",
            resource_path=resource_path,
            primary_fbx=str(primary_fbx),
            destination_fbx=str(destination_fbx),
            notes="Destination exists. Re-run with --overwrite to replace.",
        )

    if not dry_run:
        ensure_dir(destination_dir, dry_run=False)

        # Copy the main FBX to the exact path Resources.Load expects.
        shutil.copy2(primary_fbx, destination_fbx)

        # Copy textures/materials/etc.
        #
        # For folder-form Resources paths, put sidecars next to the renamed FBX.
        # For flat Resources paths, putting every texture in the same category folder can
        # cause collisions, so we keep sidecars in a source folder beside the flat FBX.
        if is_flat_resource_path(resource_path):
            sidecar_dest = destination_dir / f"{leaf_from_resource_path(resource_path)}_source_files"
            sidecars = copy_sidecar_files(
                primary_fbx.parent,
                sidecar_dest,
                primary_fbx,
                destination_fbx,
                overwrite=overwrite,
                dry_run=False,
            )
            if notes:
                notes += " "
            notes += (
                f"Flat Resources path; copied sidecars to {sidecar_dest.name}. "
                "If materials do not bind, create a prefab at the flat path in Unity."
            )
        else:
            sidecars = copy_sidecar_files(
                primary_fbx.parent,
                destination_dir,
                primary_fbx,
                destination_fbx,
                overwrite=overwrite,
                dry_run=False,
            )

        if notes:
            notes += " "
        notes += f"Copied {sidecars} sidecar file(s)."

    return ImportResult(
        zip_file=zip_path.name,
        status=status,
        asset_key=asset_key,
        matched_key=matched_key or "",
        match_score=f"{score:.3f}",
        resource_path=resource_path,
        primary_fbx=str(primary_fbx),
        destination_fbx=str(destination_fbx),
        notes=notes,
    )


def write_manifest(resources_dir: Path, results: Iterable[ImportResult], dry_run: bool) -> Path:
    manifest = resources_dir / "Models" / "_GeneratedAssetImportManifest.csv"
    fieldnames = [
        "zip_file",
        "status",
        "asset_key",
        "matched_key",
        "match_score",
        "resource_path",
        "primary_fbx",
        "destination_fbx",
        "notes",
    ]

    if dry_run:
        print(f"DRY RUN: would write manifest to {manifest}")
        return manifest

    ensure_dir(manifest.parent, dry_run=False)
    with open(manifest, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=fieldnames)
        writer.writeheader()
        for result in results:
            writer.writerow({
                "zip_file": result.zip_file,
                "status": result.status,
                "asset_key": result.asset_key,
                "matched_key": result.matched_key,
                "match_score": result.match_score,
                "resource_path": result.resource_path,
                "primary_fbx": result.primary_fbx,
                "destination_fbx": result.destination_fbx,
                "notes": result.notes,
            })

    return manifest


def print_summary(results: list[ImportResult]) -> None:
    counts: dict[str, int] = {}
    for result in results:
        counts[result.status] = counts.get(result.status, 0) + 1

    print()
    print("Import summary")
    print("--------------")
    for status, count in sorted(counts.items()):
        print(f"{status:16} {count}")

    imported_known = {
        result.matched_key
        for result in results
        if result.matched_key and result.status in {"mapped", "skipped_exists"}
    }
    missing_known = sorted(set(KNOWN_RESOURCE_TARGETS) - imported_known)

    print()
    print(f"Known Elementborn targets matched/imported: {len(imported_known)} / {len(KNOWN_RESOURCE_TARGETS)}")

    if missing_known:
        print()
        print("Known targets not matched by local/downloaded ZIP names:")
        for key in missing_known[:80]:
            print(f"  - {key} -> {KNOWN_RESOURCE_TARGETS[key]}")
        if len(missing_known) > 80:
            print(f"  ... and {len(missing_known) - 80} more")
    else:
        print("All known targets were matched.")


def main() -> int:
    args = parse_args()
    assets_dir = resolve_assets_dir(args.assets_dir)
    zips_dir = generated_assets_dir(assets_dir)
    resources_dir = default_resources_dir(assets_dir)
    extract_dir = temp_extract_dir(assets_dir)

    print("Elementborn generated asset importer")
    print("------------------------------------")
    print(f"Assets dir:          {assets_dir}")
    print(f"Generated ZIPs dir:  {zips_dir}")
    print(f"Resources dir:       {resources_dir}")
    print(f"Temp extract dir:    {extract_dir}")
    print()

    if args.download:
        download_generated_zip_assets(
            destination_dir=zips_dir,
            owner=args.github_owner,
            repo=args.github_repo,
            branch=args.github_branch,
            repo_asset_path=args.github_asset_path,
            overwrite=args.overwrite,
            dry_run=args.dry_run,
        )

    if not zips_dir.exists():
        raise SystemExit(
            f"Missing generated_assets folder: {zips_dir}\n"
            "Create it, pull the repo assets, or re-run with --download."
        )

    zip_files = sorted(
        p for p in zips_dir.iterdir()
        if p.is_file() and p.suffix.lower() == ".zip"
    )

    if not zip_files:
        raise SystemExit(
            f"No .zip files found in {zips_dir}\n"
            "Pull the generated_assets folder or re-run with --download."
        )

    print(f"Found {len(zip_files)} local ZIP file(s).")
    print()

    if not args.dry_run:
        ensure_dir(resources_dir / "Models", dry_run=False)
        ensure_dir(extract_dir, dry_run=False)

    results: list[ImportResult] = []

    for i, zip_path in enumerate(zip_files, start=1):
        print(f"[{i}/{len(zip_files)}] {zip_path.name}")
        try:
            result = import_one_zip(
                zip_path=zip_path,
                extract_root_base=extract_dir,
                resources_dir=resources_dir,
                overwrite=args.overwrite,
                dry_run=args.dry_run,
                exact_only=args.exact_only,
                min_score=args.min_score,
            )
        except zipfile.BadZipFile:
            result = ImportResult(
                zip_file=zip_path.name,
                status="bad_zip",
                asset_key=strip_meshy_zip_name(zip_path.name),
                matched_key="",
                match_score="",
                resource_path="",
                primary_fbx="",
                destination_fbx="",
                notes="ZIP file could not be opened.",
            )
        except Exception as exc:
            result = ImportResult(
                zip_file=zip_path.name,
                status="error",
                asset_key=strip_meshy_zip_name(zip_path.name),
                matched_key="",
                match_score="",
                resource_path="",
                primary_fbx="",
                destination_fbx="",
                notes=f"{type(exc).__name__}: {exc}",
            )

        results.append(result)
        print(f"  {result.status}: {result.asset_key}")
        if result.resource_path:
            print(f"  -> Resources/{result.resource_path}")
        if result.notes:
            print(f"  note: {result.notes}")

    manifest_path = write_manifest(resources_dir, results, dry_run=args.dry_run)
    print()
    print(f"Manifest: {manifest_path}")

    print_summary(results)

    print()
    print("Next steps")
    print("----------")
    print("1. Open the project in Unity and let it import the new FBX/assets.")
    print("2. Run EditMode tests, especially CreatureModelNamesTests and ModelBindingsTests.")
    print("3. Check the manifest for 'unmapped', 'no_fbx_found', or low match_score rows.")
    print("4. If flat prop/boss materials do not bind, create prefabs at the flat Resources paths.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
