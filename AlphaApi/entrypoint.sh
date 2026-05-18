#!/bin/sh
set -e

echo "Restoring packages..."
dotnet restore

echo "Applying migrations..."
until dotnet ef database update --no-build 2>&1; do
  echo "Database not ready, retrying in 3s..."
  sleep 3
done

exec dotnet watch run --non-interactive --no-launch-profile
