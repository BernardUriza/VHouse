#!/usr/bin/env bash
set -euo pipefail
SOLUTION="${1:-VHouse.sln}"
PROJECT="${2:-VHouse/VHouse.csproj}"

ROOT="$(pwd)"
LOGS="$ROOT/_logs"
mkdir -p "$LOGS"
STAMP="$(date +%Y%m%d-%H%M%S)"
LOG="$LOGS/diagnose-$STAMP.log"

exec > >(tee -a "$LOG") 2>&1

echo "===== DIAG START $(date) ====="
dotnet --info

echo -e "\n== restore =="
dotnet restore "$SOLUTION"

echo -e "\n== build (Release) =="
dotnet build "$SOLUTION" -c Release -warnaserror

echo -e "\n== vulnerable packages (transitive) =="
dotnet list "$PROJECT" package --vulnerable --include-transitive || true

echo -e "\n== tests =="
mapfile -t TESTS < <(find . -name "*Tests.csproj")
if (( ${#TESTS[@]} )); then
  for t in "${TESTS[@]}"; do
    echo "running tests: $t"
    dotnet test "$t" -c Release --collect:"XPlat Code Coverage" --no-build || true
  done
else
  echo "no test projects found."
fi

echo "===== DIAG END $(date) ====="
echo "Log written to $LOG"