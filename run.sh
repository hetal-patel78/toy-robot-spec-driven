#!/usr/bin/env bash
# Builds the Toy Robot application and runs it (Linux and macOS; bash 3.2 or later).
#   ./run.sh              commands are typed (or read from redirected console input)
#   ./run.sh commands.txt commands are read from the file
# Messages of this script (progress lines and errors) go to console messages (stderr); console output (stdout) is the application's.
# Up to and including the .NET check only bash built-ins are used, so the check also works when PATH is empty.

# The folder of this script, taken from $0 without running a program.
case "$0" in
  */*) dir=${0%/*} ;;
  *) dir=. ;;
esac

# 1. A .NET SDK, version 8 or later, must be installed.
printf 'Checking for a .NET SDK, version 8 or later...\n' >&2
sdk_ok=0
if command -v dotnet > /dev/null 2>&1; then
  sdks=$(dotnet --list-sdks 2> /dev/null)
  while IFS= read -r line; do
    major=${line%%.*}
    case "$major" in
      '' | *[!0-9]*) ;;
      *) if [ "$major" -ge 8 ]; then sdk_ok=1; fi ;;
    esac
  done <<< "$sdks"
fi
if [ "$sdk_ok" -ne 1 ]; then
  printf 'A .NET SDK, version 8 or later, is required. Download it from https://dotnet.microsoft.com/download\n' >&2
  exit 1
fi

# 2. Build the application quietly; show the build output only when the build fails.
printf 'Building the application (Release)...\n' >&2
out=$(dotnet build "$dir/src/ToyRobot/ToyRobot.csproj" -c Release --nologo 2>&1 < /dev/null)
rc=$?
if [ "$rc" -ne 0 ]; then
  printf '%s\n' "$out" >&2
  exit "$rc"
fi

# 3. Arguments: none, or one existing file.
if [ "$#" -gt 1 ]; then
  printf 'How to run: ./run.sh [commands.txt]\n' >&2
  exit 1
fi
app="$dir/src/ToyRobot/bin/Release/net8.0/ToyRobot"
if [ "$#" -eq 0 ]; then
  printf 'Running the application: type commands, one per line; end with EXIT or Ctrl+D.\n' >&2
  "$app"
  exit $?
fi
if [ ! -f "$1" ]; then
  printf 'File not found: %s\n' "$1" >&2
  exit 1
fi
printf 'Running the application with commands from %s\n' "$1" >&2
"$app" < "$1"
exit $?
