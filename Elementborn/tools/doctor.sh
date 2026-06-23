#!/bin/sh
# Elementborn project doctor: asserts every invariant in one command. Exits non-zero if any check fails,
# so it can gate CI. Runs the existing validate + ip-guard, then the structural/consistency checks.
set -u
cd "$(dirname "$0")/.." || exit 2

FAIL=0
ok()  { echo "  [PASS] $1"; }
bad() { echo "  [FAIL] $1"; FAIL=$((FAIL + 1)); }

echo "== Elementborn doctor =="

# 1. Brace / burn-tick / test-count gates.
if sh tools/validate.sh >/tmp/eb_validate.log 2>&1; then
    ok "validate.sh (braces / burn-tick / test count)"
else
    bad "validate.sh failed:"; sed 's/^/      /' /tmp/eb_validate.log
fi

# 2. Committed franchise-term guard.
if sh tools/ip-guard.sh 2>/dev/null | grep -q "clean"; then ok "ip-guard.sh (franchise terms)"; else bad "ip-guard.sh reported issues"; fi

# 3. Standing IP grep (the lighter, hand-run check).
if [ -z "$(grep -rni 'bending\|bender\|\bbend\b\|\bbends\b\|\bavatar\b\|BendingVR' Assets docs README.md nakama 2>/dev/null | grep -v 'humanoid rig')" ]; then
    ok "standing IP grep"
else
    bad "standing IP grep matched"
fi

# 4. Preprocessor balance (the Nakama files are #if-gated).
IF_=$(grep -rE '^[[:space:]]*#if' Assets 2>/dev/null | wc -l | tr -d ' ')
ND_=$(grep -rE '^[[:space:]]*#endif' Assets 2>/dev/null | wc -l | tr -d ' ')
if [ "$IF_" = "$ND_" ]; then ok "#if/#endif balance ($IF_)"; else bad "#if/#endif unbalanced (#if=$IF_ #endif=$ND_)"; fi

# 5. asmdefs are valid JSON, and the Editor assembly is Editor-only.
ASMOK=1
for f in $(find Assets -name '*.asmdef'); do
    python3 -c "import json;json.load(open('$f'))" 2>/dev/null || { ASMOK=0; echo "      bad JSON: $f"; }
done
[ "$ASMOK" = 1 ] && ok "asmdefs are valid JSON" || bad "an asmdef is invalid JSON"
if python3 -c "import json,sys;d=json.load(open('Assets/Elementborn/Editor/Elementborn.Editor.asmdef'));sys.exit(0 if d.get('includePlatforms')==['Editor'] else 1)" 2>/dev/null; then
    ok "Editor asmdef is Editor-only"
else
    bad "Editor asmdef is not Editor-only"
fi

# 5b. The package manifest is valid JSON and lists the built-in modules XR depends on.
if python3 -c "import json;json.load(open('Packages/manifest.json'))" 2>/dev/null; then
    ok "Packages/manifest.json is valid JSON"
else
    bad "Packages/manifest.json is invalid JSON"
fi
if grep -q '"com.unity.modules.vr"' Packages/manifest.json && grep -q '"com.unity.modules.xr"' Packages/manifest.json; then
    ok "manifest lists the XR built-in modules"
else
    bad "manifest is missing com.unity.modules.vr / .xr (XR packages will fail to resolve)"
fi

# 5c. Every component the bootstrap generator spawns by name resolves to a real class.
gen="Assets/Elementborn/Editor/BootstrapSceneGenerator.cs"
genmiss=0
for t in $(grep -oE 'Add\([a-zA-Z_.]+, "[A-Za-z]+"\)' "$gen" 2>/dev/null | grep -oE '"[A-Za-z]+"' | tr -d '"' | sort -u); do
    grep -rqE "class $t\b" Assets/Elementborn --include=*.cs || { genmiss=$((genmiss + 1)); echo "      unresolved component: $t"; }
done
if [ "$genmiss" -eq 0 ]; then ok "bootstrap generator component types all resolve to classes"; else bad "$genmiss bootstrap component type(s) don't resolve"; fi

# 6. The Element enum stays exactly {Fire, Water, Earth, Air}.
MEMBERS=$(awk '/enum Element/{f=1;next} f&&/}/{f=0} f{ sub(/\/\/.*/,""); print }' Assets/Elementborn/Core/Elements.cs \
    | grep -oE '\b[A-Z][A-Za-z]+\b' | sort -u | tr '\n' ' ')
if [ "$MEMBERS" = "Air Earth Fire Water " ]; then ok "Element enum == {Fire,Water,Earth,Air}"; else bad "Element enum drifted: [$MEMBERS]"; fi

# 7. Retired tokens stay gone.
if [ -z "$(grep -rniE 'republicers|\bpebble\b|wind waker' Assets docs README.md nakama 2>/dev/null)" ]; then
    ok "no retired tokens (Republicers / Pebble / Wind Waker)"
else
    bad "a retired token reappeared"
fi

# 8. The documented EditMode test count matches reality.
DOC_N=$(grep -oE '^- [0-9]+ EditMode test files' docs/WHATS_LEFT.md | grep -oE '[0-9]+' | head -1)
ACTUAL_N=$(ls Assets/Tests/EditMode/*.cs 2>/dev/null | wc -l | tr -d ' ')
if [ "$DOC_N" = "$ACTUAL_N" ]; then ok "doc test count matches actual ($ACTUAL_N)"; else bad "WHATS_LEFT says $DOC_N test files, found $ACTUAL_N"; fi

# 9. Informational: outstanding TODO/FIXME in source.
TODOS=$(grep -rniE '\bTODO\b|\bFIXME\b' Assets/Elementborn --include=*.cs 2>/dev/null | wc -l | tr -d ' ')
echo "  [INFO] TODO/FIXME markers in source: $TODOS"

echo ""
if [ "$FAIL" -eq 0 ]; then
    echo "DOCTOR: all checks passed."
    exit 0
else
    echo "DOCTOR: $FAIL check(s) failed."
    exit 1
fi
