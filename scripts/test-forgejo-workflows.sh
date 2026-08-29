#!/usr/bin/env bash
set -euo pipefail

unsupported=$(grep -R -nE '^[[:space:]]*(permissions|continue-on-error):' .forgejo/workflows || true)
if [ -n "$unsupported" ]; then
    echo "bruh Forgejo ignores these workflow keys, unsupported cosplay stays out 💀🔥"
    echo "$unsupported"
    exit 1
fi

shorthand=$(grep -R -nE '^[[:space:]]*uses:' .forgejo/workflows | grep -vE 'uses:[[:space:]]+(https://|\./)' || true)
if [ -n "$shorthand" ]; then
    echo "bruh remote actions need fully qualified URLs so DEFAULT_ACTIONS_URL cannot reroute the vibes 💀🔥"
    echo "$shorthand"
    exit 1
fi

echo "Forgejo workflows use supported keys and explicit action URLs, zero GitHub cosplay 🔒🔥"
