#!/usr/bin/env sh
set -eu

cd "$(dirname "$0")/.."

find . \
  \( -path './src~/.bin' -o -path './src~/.obj' -o -path './src~/*/src~/.bin' -o -path './src~/*/src~/.obj' \) \
  -prune -exec rm -rf {} +

dotnet restore src~/Xeno.sln
dotnet build src~/Xeno.sln --no-restore
dotnet test src~/Xeno.sln --no-restore --no-build
