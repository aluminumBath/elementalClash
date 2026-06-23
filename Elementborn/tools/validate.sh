#!/usr/bin/env sh
# Structural validation gate: brace/paren/bracket balance, the burn-tick guard, and a test-count report.
# Two files carry documented, intentional bracket mismatches and are whitelisted:
#   * Core/RandomSource.cs            — a "[0,1)" interval in a doc comment
#   * Tests/EditMode/ControlGlyphsTests.cs — a StartsWith("[") string literal
# Run locally or in CI:  sh tools/validate.sh   (exit 0 = ok, 1 = a check failed)
set -eu
cd "$(dirname "$0")/.."

fail=0

echo "== Brace / paren / bracket balance =="
for f in $(find Assets -name '*.cs'); do
  case "$(basename "$f")" in RandomSource.cs|ControlGlyphsTests.cs) continue ;; esac
  ob=$(tr -cd '{' < "$f" | wc -c); cb=$(tr -cd '}' < "$f" | wc -c)
  op=$(tr -cd '(' < "$f" | wc -c); cp=$(tr -cd ')' < "$f" | wc -c)
  os=$(tr -cd '[' < "$f" | wc -c); cs=$(tr -cd ']' < "$f" | wc -c)
  if [ "$ob" != "$cb" ] || [ "$op" != "$cp" ] || [ "$os" != "$cs" ]; then
    echo "  UNBALANCED: $f  {}=$ob/$cb ()=$op/$cp []=$os/$cs"; fail=1
  fi
done
[ "$fail" = 0 ] && echo "  balanced (2 files whitelisted)"

echo "== Burn-tick guard =="
n=$(grep -c "Status.BurnDamagePerSecond \* Time.deltaTime" Assets/Elementborn/Game/Combat/Damageable.cs || true)
if [ "$n" = "1" ]; then echo "  ok (exactly 1)"; else echo "  FAIL: expected 1, found $n"; fail=1; fi

echo "== Test-count report =="
echo "  EditMode test files: $(ls Assets/Tests/EditMode/*.cs | wc -l)"

if [ "$fail" != 0 ]; then echo "VALIDATION FAILED"; exit 1; fi
echo "Validation OK."
