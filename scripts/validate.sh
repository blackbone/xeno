#!/usr/bin/env sh
set -eu

cd "$(dirname "$0")/.."

dotnet test src~/Xeno.sln --no-restore
