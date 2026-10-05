#!/usr/bin/env bash
# Builds the Toy Robot application and runs it (Linux and macOS; bash 3.2 or later).
#   ./run.sh              typed at a keyboard: shows the menu (DEC-41); redirected or piped console input: the commands are read from it
#   ./run.sh commands.txt commands are read from the file
#   ./run.sh --help       shows the application's help (DEC-04)
# Messages of this script (progress lines and errors) go to console messages (stderr); console output (stdout) is the application's
# and the menu's own lines.
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
# 3. Arguments: none, --help, or one existing file.
if [ "$#" -gt 1 ]; then
  printf '%s\n' 'How to run: ./run.sh testdata/challenge-example-1.txt to run the commands in a file, or ./run.sh with no file for the menu.' >&2
  exit 1
fi
app="$dir/src/ToyRobot/bin/Release/net8.0/ToyRobot"

# The menu (DEC-41): type commands, run a test-data file (its console output compared with the expected file), run the tests,
# or show the help, one topic at a time.
# Its own lines are console output (stdout).
files=(challenge-example-1 challenge-example-2 challenge-example-3 bom-challenge-example-1 exit-mid-file empty long-session)

# Shows the lines of file $1, indented, in colour $2; "(nothing)" when it has none.
show_lines() {
  if [ ! -s "$1" ]; then
    printf '   (nothing)\n'
    return
  fi
  local line
  while IFS= read -r line || [ -n "$line" ]; do
    printf '   %s%s%s\n' "$2" "${line%$'\r'}" "$reset"
  done < "$1"
}

# $1 is a test-data name: run testdata/$1.txt and show its commands with any warnings (console messages, stderr),
# its output (console output, stdout) and PASS or FAIL against testdata/$1.expected.txt.
run_file() {
  local expected="$dir/testdata/$1.expected.txt" line
  printf '\n%s---------------- %s ----------------%s\n' "$bold" "$1" "$reset"
  "$app" < "$dir/testdata/$1.txt" > "$out" 2> "$err"
  local rc=$?
  printf '%sCommands%s  (testdata/%s.txt)\n' "$bold" "$reset" "$1"
  # The app echoes each line it reads as "> line"; that prefix is dropped. Other lines are warnings, shown in yellow.
  if [ ! -s "$err" ]; then
    printf '   (none)\n'
  fi
  while IFS= read -r line || [ -n "$line" ]; do
    line=${line%$'\r'}
    case "$line" in
      '> '*) printf '   %s\n' "${line#> }" ;;
      *) printf '   %s%s%s\n' "$yellow" "$line" "$reset" ;;
    esac
  done < "$err"
  printf '%sOutput%s\n' "$bold" "$reset"
  show_lines "$out" "$bright"
  if ! diff <(tr -d '\r' < "$out") <(tr -d '\r' < "$expected") > /dev/null; then
    printf '%sExpected%s\n' "$bold" "$reset"
    show_lines "$expected" ''
    printf '%sFAIL: %s - output differs from testdata/%s.expected.txt%s\n' "$red" "$1" "$1" "$reset"
    summary+=("FAIL: $1 - output differs")
    failed=$((failed + 1))
  elif [ "$rc" -ne 0 ]; then
    printf '%sFAIL: %s - exit code %d, expected 0%s\n' "$red" "$1" "$rc" "$reset"
    summary+=("FAIL: $1 - exit code $rc")
    failed=$((failed + 1))
  else
    printf '%sPASS: %s - matches testdata/%s.expected.txt%s\n' "$green" "$1" "$1" "$reset"
    summary+=("PASS: $1")
  fi
}

# Reads one choice into $choice; returns 1 at the end of the input (Ctrl+D, or the end of redirected input), which ends the menu.
choose() {
  printf 'Choose: '
  if ! IFS= read -r choice; then
    printf '\n'
    return 1
  fi
}

# The test-data menu; returns 1 when the menu ends, 0 to go back to the main menu.
files_menu() {
  local i f line
  while true; do
    printf '\n%sToy Robot - choose a test-data file%s (shows what the app prints, then PASS or FAIL against the expected output)\n\n' "$bold" "$reset"
    for i in "${!files[@]}"; do
      printf '   %d  %s\n' "$((i + 1))" "${files[$i]}"
    done
    printf '   8  all seven files, one after another\n'
    printf '   0  back\n\n'
    choose || return 1
    case "$choice" in
      0) return 0 ;;
      [1-7]) run_file "${files[$((choice - 1))]}" ;;
      8)
        failed=0
        summary=()
        for f in "${files[@]}"; do run_file "$f"; done
        printf '\n%sSummary%s\n' "$bold" "$reset"
        for line in "${summary[@]}"; do
          case "$line" in
            PASS*) printf '   %s%s%s\n' "$green" "$line" "$reset" ;;
            *) printf '   %s%s%s\n' "$red" "$line" "$reset" ;;
          esac
        done
        if [ "$failed" -eq 0 ]; then printf '%sAll 7 files PASS%s\n' "$green" "$reset"; else printf '%s%d of 7 files FAIL%s\n' "$red" "$failed" "$reset"; fi
        ;;
      '') ;;
      *) printf 'Not a choice: %s\n' "$choice" ;;
    esac
  done
}

# Writes the section of the application's help text whose heading line starts with $1, up to the line before the next
# blank line; with no $1, all of it. The text is the application's own --help output, so there is one help text (DEC-04).
help_topic() {
  local line inside=0
  printf '\n'
  while IFS= read -r line; do
    if [ -z "$1" ]; then
      printf '%s\n' "$line"
    elif [ "$inside" -eq 1 ]; then
      [ -z "$line" ] && break
      printf '%s\n' "$line"
    else
      case "$line" in
        "$1"*) inside=1; printf '%s\n' "$line" ;;
      esac
    fi
  done < <("$app" --help < /dev/null)
}

# The help menu (DEC-41): one topic of the help at a time, so each fits on a small screen; returns 1 when the menu
# ends, 0 to go back to the main menu. Topics are plain text (no colour).
help_menu() {
  while true; do
    printf '\n%sToy Robot - help: choose a topic%s\n\n' "$bold" "$reset"
    printf '   1  How to start\n'
    printf '   2  The table\n'
    printf '   3  Commands\n'
    printf '   4  Rules\n'
    printf '   5  Ending a session\n'
    printf '   6  All of it\n'
    printf '   0  back\n\n'
    choose || return 1
    case "$choice" in
      0) return 0 ;;
      1) help_topic 'HOW TO START' ;;
      2) help_topic 'THE TABLE' ;;
      3) help_topic 'COMMANDS' ;;
      4) help_topic 'RULES' ;;
      5) help_topic 'ENDING A SESSION' ;;
      6) help_topic '' ;;
      '') ;;
      *) printf 'Not a choice: %s\n' "$choice" ;;
    esac
  done
}

menu() {
  out=$(mktemp) || exit 1
  err=$(mktemp) || exit 1
  trap 'rm -f "$out" "$err"' EXIT
  failed=0
  # Colours: PASS green, FAIL red, warnings yellow, the app's console output bright cyan, headings bold; only when the
  # menu writes to a terminal and NO_COLOR is not set, so saved or piped output stays plain.
  green='' red='' yellow='' bright='' bold='' reset=''
  if [ -t 1 ] && [ -z "${NO_COLOR:-}" ]; then
    green=$'\e[92m' red=$'\e[91m' yellow=$'\e[93m' bright=$'\e[1;96m' bold=$'\e[1m' reset=$'\e[0m'
  fi
  while true; do
    printf '\n%sToy Robot - choose what to run%s\n\n' "$bold" "$reset"
    printf '   1  Type commands yourself\n'
    printf '   2  Run a test-data file\n'
    printf '   3  Run the automated tests (dotnet test)\n'
    printf '   4  Show help (commands, rules, how to end a session)\n'
    printf '   0  Quit\n\n'
    choose || break
    case "$choice" in
      0) break ;;
      1)
        printf '\n'
        printf 'Running the application: type commands, one per line; end with EXIT or Ctrl+D.\n' >&2
        "$app"
        printf '\nTyped session ended.\n'
        ;;
      2) files_menu || break ;;
      3)
        printf '\n'
        dotnet test "$dir/ToyRobot.sln" --nologo < /dev/null
        ;;
      4) help_menu || break ;;
      '') ;;
      *) printf 'Not a choice: %s\n' "$choice" ;;
    esac
  done
  exit 0
}

if [ "$#" -eq 0 ]; then
  # Console input from a keyboard (a terminal) shows the menu; redirected or piped console input is read as commands.
  # TOYROBOT_MENU shows the menu even then (CI-08 uses it).
  if [ -t 0 ] || [ -n "${TOYROBOT_MENU:-}" ]; then
    menu
  fi
  printf 'Running the application: type commands, one per line; end with EXIT or Ctrl+D.\n' >&2
  "$app"
  exit $?
fi
# --help (exactly, as the application matches it) shows the application's help; no commands are read, so there is
# no "Running" line.
if [ "$1" = "--help" ]; then
  "$app" --help < /dev/null
  exit $?
fi
if [ ! -f "$1" ]; then
  printf 'File not found: %s\n' "$1" >&2
  exit 1
fi
printf 'Running the application with commands from %s\n' "$1" >&2
"$app" < "$1"
exit $?
