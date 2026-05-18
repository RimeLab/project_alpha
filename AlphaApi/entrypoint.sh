#!/bin/sh
set -e

dotnet restore

exec dotnet watch run --non-interactive --no-launch-profile
