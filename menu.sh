#!/usr/bin/env bash
# Toy Robot menu (Linux and macOS; DEC-41). Lists what can be run and runs the choice by number:
# a test-data file (its console output compared with the expected file), all of them, a typed session, or the automated tests.
# Each run goes through run.sh, so the .NET check and the build are the same as when run.sh is used directly.

cd "$(dirname "$0")" || exit 1
out=$(mktemp) || exit 1
trap 'rm -f "$out"' EXIT
files=(challenge-example-1 challenge-example-2 challenge-example-3 bom-challenge-example-1 exit-mid-file empty long-session)
failed=0

# Colours: PASS green, FAIL red, the app's console output bright cyan; only when the menu writes to a terminal
# and NO_COLOR is not set, so saved or piped output stays plain.
green='' red='' bright='' bold='' reset=''
if [ -t 1 ] && [ -z "${NO_COLOR:-}" ]; then
  green=$'\e[92m' red=$'\e[91m' bright=$'\e[1;96m' bold=$'\e[1m' reset=$'\e[0m'
fi

show_menu() {
  printf '\n'
  printf '%sToy Robot - choose what to run%s\n' "$bold" "$reset"
  printf '\n'
  printf '  Run a test-data file (shows what the app prints, then PASS or FAIL against the expected output)\n'
  local i
  for i in "${!files[@]}"; do
    printf '   %d  %s\n' "$((i + 1))" "${files[$i]}"
  done
  printf '   8  all seven files, one after another\n'
  printf '\n'
  printf '  Type commands yourself\n'
  printf '   9  start a typed session (end with EXIT, or Ctrl+D; you come back to this menu)\n'
  printf '\n'
  printf '  Automated tests\n'
  printf '  10  run all tests (dotnet test)\n'
  printf '\n'
  printf '   0  quit\n'
  printf '\n'
}

# $1 is a test-data name: run testdata/$1.txt, show its console output (stdout) and compare it with testdata/$1.expected.txt.
run_file() {
  printf '\n%s=== %s ===%s\n' "$bold" "$1" "$reset"
  ./run.sh "testdata/$1.txt" > "$out"
  local rc=$?
  printf -- '--- console output (stdout):\n'
  printf '%s' "$bright"
  cat "$out"
  printf '%s' "$reset"
  if ! diff <(tr -d '\r' < "$out") <(tr -d '\r' < "testdata/$1.expected.txt") > /dev/null; then
    printf '%sFAIL: %s - console output differs from testdata/%s.expected.txt%s\n' "$red" "$1" "$1" "$reset"
    failed=$((failed + 1))
  elif [ "$rc" -ne 0 ]; then
    printf '%sFAIL: %s - exit code %d, expected 0%s\n' "$red" "$1" "$rc" "$reset"
    failed=$((failed + 1))
  else
    printf '%sPASS: %s%s\n' "$green" "$1" "$reset"
  fi
}

while true; do
  show_menu
  printf 'Choose: '
  # The end of the input (Ctrl+D, or the end of redirected input) ends the menu.
  if ! IFS= read -r choice; then
    printf '\n'
    break
  fi
  case "$choice" in
    0) break ;;
    [1-7]) run_file "${files[$((choice - 1))]}" ;;
    8)
      failed=0
      for f in "${files[@]}"; do run_file "$f"; done
      printf '\n'
      if [ "$failed" -eq 0 ]; then printf '%sAll 7 files PASS%s\n' "$green" "$reset"; else printf '%s%d of 7 files FAIL%s\n' "$red" "$failed" "$reset"; fi
      ;;
    9)
      printf '\n'
      ./run.sh
      printf '\nTyped session ended.\n'
      ;;
    10)
      printf '\n'
      dotnet test ToyRobot.sln --nologo
      ;;
    '') ;;
    *) printf 'Not a choice: %s\n' "$choice" ;;
  esac
done
exit 0
