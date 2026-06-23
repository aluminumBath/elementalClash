#!/bin/sh
# check-imports.sh — guard against invalid `using` directives (the kind only a Unity compile would otherwise catch).
#
# Every `using Elementborn.X;` must reference a namespace that actually exists — i.e. X is a declared namespace or
# a prefix of one. Every other `using` must start with a known external root (System / UnityEngine / etc.). This
# catches typo'd or removed namespaces. `using static ...` and aliases (`using A = B;`) are skipped, since their
# target can be a type rather than a namespace. Exits non-zero (and lists the offenders) if anything fails to
# resolve, so it can gate CI alongside validate / ip-guard.
set -u
cd "$(dirname "$0")/.." || exit 2

EXTERNAL='System UnityEngine UnityEditor Unity TMPro NUnit Nakama'

ns_closure=$(mktemp)
# Declared namespaces, expanded to every prefix (so `using Elementborn.Game.Social` resolves even when only a
# deeper namespace is declared).
grep -rhoE '^[[:space:]]*namespace[[:space:]]+[A-Za-z0-9_.]+' Assets --include=*.cs \
  | sed -E 's/^[[:space:]]*namespace[[:space:]]+//' \
  | awk -F. '{s="";for(i=1;i<=NF;i++){s=(i==1?$i:s"."$i); print s}}' \
  | sort -u > "$ns_closure"

usings=$(mktemp)
grep -rnE '^[[:space:]]*using[[:space:]]+[A-Za-z0-9_.]+[[:space:]]*;' Assets --include=*.cs \
  | grep -vE 'using[[:space:]]+static[[:space:]]' \
  | grep -v '=' > "$usings"

bad=0
while IFS= read -r rec; do
    [ -n "$rec" ] || continue
    ns=$(printf '%s\n' "$rec" | sed -E 's/.*using[[:space:]]+([A-Za-z0-9_.]+)[[:space:]]*;.*/\1/')
    root=$(printf '%s\n' "$ns" | cut -d. -f1)
    case "$root" in
        Elementborn)
            grep -Fxq "$ns" "$ns_closure" || { echo "  invalid: $rec  (namespace '$ns' not declared)"; bad=$((bad + 1)); }
            ;;
        *)
            hit=0
            for e in $EXTERNAL; do [ "$root" = "$e" ] && hit=1 && break; done
            [ "$hit" -eq 1 ] || { echo "  invalid: $rec  (unknown external root '$root')"; bad=$((bad + 1)); }
            ;;
    esac
done < "$usings"

rm -f "$ns_closure" "$usings"
if [ "$bad" -eq 0 ]; then
    echo "Imports OK (all using directives resolve)."
    exit 0
fi
echo "Imports: $bad invalid using directive(s)."
exit 1
