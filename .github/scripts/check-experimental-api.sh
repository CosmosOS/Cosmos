#!/usr/bin/env bash
# Checks that the driver kit seam is declared experimental in every
# src/**/PublicAPI.*.txt, and that nothing else is.
#
# The seam is everything in Cosmos.Kernel.HAL.Drivers(.Pci/.Usb) and
# Cosmos.Kernel.System.Drivers, plus the Kernel.RegisterDrivers hook. The
# PublicAPI analyzer prefixes a symbol's line with [COSMOS0003] only when the
# symbol carries [Experimental("COSMOS0003")], so a public seam type that lost
# its attribute would otherwise be recorded, and later shipped, as stable API
# without anyone noticing. Two rules:
#   1. A line that names the seam anywhere (as the declared symbol, or in a
#      signature) carries the [COSMOS0003] prefix: a stable member cannot
#      expose a seam type either.
#   2. A [COSMOS0003] line declares a seam symbol, so the ID does not leak
#      onto another API.
#
# Usage: check-experimental-api.sh [root]
#   root defaults to the repository root; the files checked are
#   <root>/src/**/PublicAPI.Shipped.txt and PublicAPI.Unshipped.txt.
# Exits 0 when every file passes, 1 on a violation (each one printed as
# file:line), 2 when no PublicAPI file is found under the root.

set -euo pipefail

root="${1:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
diag_id='COSMOS0003'

# Rule 1: anywhere in the line. The character class after each name keeps
# a namespace such as Cosmos.Kernel.HAL.DriversExtra out of the seam.
names_seam='Cosmos\.Kernel\.(HAL|System)\.Drivers([^A-Za-z0-9_]|$)|Cosmos\.Kernel\.System\.Kernel\.RegisterDrivers([^A-Za-z0-9_]|$)'

# Rule 2: the declared symbol, which is the first token once the removal
# marker, the ID and the modifiers are stripped.
declares_seam='^(Cosmos\.Kernel\.(HAL|System)\.Drivers\.|Cosmos\.Kernel\.System\.Kernel\.RegisterDrivers\()'

mapfile -t files < <(find "$root/src" -name 'PublicAPI.*.txt' -not -path '*/obj/*' -not -path '*/bin/*' | sort)
if [[ ${#files[@]} -eq 0 ]]; then
    echo "check-experimental-api: no PublicAPI.*.txt under $root/src" >&2
    exit 2
fi

violations=0
for file in "${files[@]}"; do
    line_number=0
    while IFS= read -r line || [[ -n "$line" ]]; do
        line_number=$((line_number + 1))
        line="${line%$'\r'}"
        if [[ -z "$line" || "$line" == \#* ]]; then
            continue
        fi

        # A removed symbol keeps its original line behind a *REMOVED*
        # marker, which is accepted on either side of the ID.
        body="${line#\*REMOVED\*}"
        has_id=false
        if [[ "$body" == "[$diag_id]"* ]]; then
            has_id=true
        fi

        if [[ "$has_id" == false && "$body" =~ $names_seam ]]; then
            echo "$file:$line_number: names the driver kit seam without the [$diag_id] prefix: $line"
            violations=$((violations + 1))
            continue
        fi

        if [[ "$has_id" == true ]]; then
            symbol="${body#"[$diag_id]"}"
            symbol="${symbol#\*REMOVED\*}"
            # Modifiers the analyzer writes before the symbol.
            while [[ "$symbol" =~ ^(abstract|const|override|readonly|sealed|static|virtual)\  ]]; do
                symbol="${symbol#* }"
            done

            if [[ ! "$symbol" =~ $declares_seam ]]; then
                echo "$file:$line_number: [$diag_id] on a symbol outside the driver kit seam: $line"
                violations=$((violations + 1))
            fi
        fi
    done < "$file"
done

if [[ $violations -gt 0 ]]; then
    echo "check-experimental-api: $violations violation(s) in ${#files[@]} file(s)." >&2
    echo "Every driver kit seam symbol carries [Experimental(Experimentals.DriverKitDiagId)], and no other symbol uses $diag_id; run 'make api' after fixing the attributes." >&2
    exit 1
fi

echo "check-experimental-api: ${#files[@]} file(s) checked, every $diag_id line is on the seam and every seam line has $diag_id."
