#!/usr/bin/env sh
# Bump the project version (semver) and roll the changelog.
#   Usage: sh tools/bump-version.sh [major|minor|patch]   (default: patch)
# Updates the VERSION file and the in-game AppVersion constant (kept in sync), and converts the changelog's
# [Unreleased] section into a dated version with a fresh [Unreleased] above it.
set -eu
cd "$(dirname "$0")/.."

PART="${1:-patch}"
[ -f VERSION ] || { echo "VERSION file missing"; exit 1; }
CUR=$(tr -d ' \n' < VERSION)

MAJOR=$(echo "$CUR" | cut -d. -f1)
MINOR=$(echo "$CUR" | cut -d. -f2)
PATCH=$(echo "$CUR" | cut -d. -f3)

case "$PART" in
  major) MAJOR=$((MAJOR + 1)); MINOR=0; PATCH=0 ;;
  minor) MINOR=$((MINOR + 1)); PATCH=0 ;;
  patch) PATCH=$((PATCH + 1)) ;;
  *) echo "Unknown part '$PART' (use major|minor|patch)"; exit 1 ;;
esac
NEW="$MAJOR.$MINOR.$PATCH"

# 1) VERSION file (single source of truth)
printf '%s\n' "$NEW" > VERSION

# 2) in-game constant, kept in sync
APPV="Assets/Elementborn/Core/AppVersion.cs"
[ -f "$APPV" ] && sed -i "s/Version = \"[0-9][0-9.]*\"/Version = \"$NEW\"/" "$APPV"

# 3) changelog: [Unreleased] -> [NEW] - date, with a fresh empty [Unreleased] on top
if grep -q "## \[Unreleased\]" CHANGELOG.md; then
  sed -i "s/## \[Unreleased\]/## [Unreleased]\n\n## [$NEW] - $(date +%Y-%m-%d)/" CHANGELOG.md
fi

echo "Bumped $CUR -> $NEW"
echo "Next:  git commit -am \"Release $NEW\"  &&  git tag v$NEW  &&  git push --follow-tags"
