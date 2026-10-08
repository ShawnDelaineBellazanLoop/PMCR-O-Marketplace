#!/usr/bin/env bash
# PMCR-O item 85 gate. Run before a 02-make.jsonl is finalized.
# Exit 0 = no secrets found. Non-zero = a secret was found (or gitleaks failed).
# The findings are redacted; the secret value must never be copied into a trail.
set -uo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
bin="${GITLEAKS:-$(command -v gitleaks || echo "$HOME/.local/bin/gitleaks")}"
if [ ! -x "$bin" ]; then
  echo "secret-scan: gitleaks not found; cannot finish the check" >&2
  exit 3
fi
"$bin" dir "$root" --config "$root/.gitleaks.toml" --redact --no-banner --exit-code 1
