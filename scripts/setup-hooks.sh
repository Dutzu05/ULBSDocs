#!/usr/bin/env bash
set -euo pipefail

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

chmod +x .githooks/pre-commit

git config core.hooksPath .githooks

echo "Hooks enabled: $(git config --get core.hooksPath)"
echo "Pre-commit will run: dotnet test + coverage gate"
