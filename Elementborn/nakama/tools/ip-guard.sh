#!/usr/bin/env sh
# IP / trademark regression guard for Elementborn.
# Exits 1 if any trademarked term from the source franchises appears in shipped code or docs.
# Run locally or in CI:  sh tools/ip-guard.sh
#
# Deliberately NOT flagged (allowed — factual/nominative, or simply unrelated words):
#   * Real controller hardware names in ControlGlyphs.cs (nintendo, switch, joy-con, playstation,
#     dualsense, xbox, ...). These identify actual devices so the UI shows the right button glyphs —
#     nominative use, not branding.
#   * "Blender" (the 3D tool) and "blend"/"blending" (terrain biome blending) — note these do NOT
#     contain the substring "bender"/"bending" (the 'l' breaks it), so no exclusion is even needed.
#   * Unity's Mecanim "Avatar" humanoid-rig type — a technical API term. Lines mentioning
#     "humanoid rig" or "mecanim" are excluded so that legitimate engine usage stays allowed.
#
# This is best-effort hygiene, not legal advice.
set -eu
cd "$(dirname "$0")/.."

# Terms that must NEVER appear in the project.
PATTERN="bending|bender|\bavatar\b|bendingvr|korra|\baang\b|katara|sokka|\btoph\b|zuko|\biroh\b|azula|tenzin|\bamon\b|sozin|equalist|probend|pro-bend|fire nation|water tribe|earth kingdom|air nomad|republic city|white lotus|chi.?block|avatar state|lion turtle|pandora|na'vi|ikran|thanator|leonopteryx|tulkun|\beywa\b|unobtanium|toruk|hometree|direhorse|wind ?waker|hyrule|ganondorf|pikachu|pokeball|pokemon"

HITS=$(grep -rniE "$PATTERN" Assets docs README.md 2>/dev/null | grep -viE "humanoid rig|mecanim" || true)

if [ -n "$HITS" ]; then
  echo "IP guard: FAIL — trademarked term(s) found:"
  echo "$HITS"
  exit 1
fi
echo "IP guard: clean."
