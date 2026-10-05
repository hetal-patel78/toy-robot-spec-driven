# Contract: run scripts (NFR-08; DEC-04, DEC-23, DEC-33, DEC-37)

Files: `run.cmd` (Windows cmd and PowerShell; CRLF) and `run.sh` (Linux and macOS, bash or zsh; LF; executable; bash 3.2 compatible), both at the repository root.

## How they are called

| Shell | No file | With a file |
|---|---|---|
| cmd | `run` | `run commands.txt` |
| PowerShell | `.\run.cmd` | `.\run.cmd commands.txt` |
| bash, zsh | `./run.sh` | `./run.sh commands.txt` |

The file path is taken relative to the current folder. The script finds the application relative to its own folder.

## Results

"App" means the built application at `src\ToyRobot\bin\Release\net8.0\ToyRobot.exe` (Windows) or `src/ToyRobot/bin/Release/net8.0/ToyRobot` (Linux and macOS) (page 5), with its own console output (stdout), console messages (stderr) and exit code (spec 002).

| Case | Console output (stdout) | Console messages (stderr) | Exit code | Checked by |
|---|---|---|---|---|
| One existing file | the app's (only `Output:` and position lines) | the progress lines (SDK, build, "Running the application with commands from <file>"), then the app's (echo and warnings); no build output | the app's (0) | [CI-01][ci-01] (console output (stdout) and exit code), [CI-07][ci-07] (progress lines) |
| No argument, console input from a keyboard | the menu's lines (below); after choice 1, the app's | the progress lines (SDK, build); after choice 1, "Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter." (`run.cmd`) or "... end with EXIT or Ctrl+D." (`run.sh`), then the app's (prompt `> `, warnings) | 0 | the menu: [CI-08][ci-08] (with `TOYROBOT_MENU`); the keyboard case: by hand; prompt itself: spec 002 |
| No argument, console input redirected or piped (and `TOYROBOT_MENU` not set) | the app's; no menu | the first two progress lines and the no-argument "Running" line, then the app's (echo and warnings) | the app's (0) | [CI-03][ci-03] (console output (stdout) and exit code) |
| One argument, `--help` (exactly) | the app's help text | the first two progress lines (SDK, build); no "Running" line | the app's (0) | [CI-09][ci-09] |
| One argument, missing path or a folder | (empty) | file-not-found message (wording not fixed) | 1 | [CI-02][ci-02] |
| File name with `*` or `?` (`run.cmd`) | (empty) | message that the file name must not contain `*` or `?` (contains `must not contain * or ?`); no "Running" line. `run.sh` gives the file-not-found message instead (a quoted `*` or `?` is not a regular file) | 1 | [CI-02][ci-02] (Windows) |
| More than one argument | (empty) | the script's how-to-run message (wording not fixed) | 1 | [CI-02][ci-02] |
| `dotnet` not found | (empty) | where to get the .NET SDK: `https://dotnet.microsoft.com/download` | 1 | [CI-04][ci-04] (a) |
| Only SDKs older than 8 | (empty) | as above | 1 | [CI-04][ci-04] (b) |
| Build fails | (empty) | the build output | the build's | [CI-05][ci-05] |

`run.cmd` turns delayed expansion off itself (research [R-03][r-03]), so `!` in a file name stays text whatever the user's registry says.

Order of checks: SDK, build, arguments (so the argument errors are reported after a successful build). The only argument a script passes to the app is `--help` ([DEC-04][dec-04]); any other argument is taken as a file name, so the app's how-to-run message is never triggered by a script.

## Progress lines (developer request, 2026-10-05; CI-07)

Each script writes these lines to console messages (stderr), in this order, and nothing else of its own on success:

1. `"Checking for a .NET SDK, version 8 or later..."` (before the SDK check)
2. `"Building the application (Release)..."` (before the build)
3. after a successful build and the argument checks, just before the application starts: with a file, `"Running the application with commands from <file>"` (the argument as given, without quotes); with no argument (at a keyboard: when menu choice 1 starts the typed session), `"Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter."` (`run.cmd`) or `"Running the application: type commands, one per line; end with EXIT or Ctrl+D."` (`run.sh`)

Error cases (no SDK, failed build, how-to-run message, file not found) show the lines before the failure but never the "Running" line; nor does `--help`, which reads no commands. Build output is shown only when the build fails. Console output (stdout) is unchanged.

## Menu (developer request, 2026-10-05; MN-01, CI-08)

Shown by `run` (cmd), `.\run.cmd` (PowerShell) or `./run.sh` (bash, zsh) with no argument, when console input is a keyboard or `TOYROBOT_MENU` is set; after the .NET check and the build.

| Main menu choice | Runs | Writes (console output, stdout) |
|---|---|---|
| 1 | the app with the script's console input | the typed session, then "Typed session ended." |
| 2 | the test-data menu | the test-data menu |
| 3 | `dotnet test` on `ToyRobot.sln` with `--nologo`, no console input | the test results |
| 4 (menu line "4  Show help (commands, rules, how to end a session)") | the app with `--help`, no console input | the app's help text (no colour), then the main menu again |
| 0 | — | nothing; exit code 0 |
| anything else | — | "Not a choice: <choice>" |

| Test-data menu choice | Runs | Writes (console output, stdout) |
|---|---|---|
| 1 to 7 | the app with `testdata/<name>.txt` as its console input; its console output (stdout) and console messages (stderr) saved apart | "---------------- <name> ----------------"; "Commands  (testdata/<name>.txt)" and each line of the app's console messages, indented, the echo without "> " (or "(none)"); "Output" and the app's console output, indented (or "(nothing)"); then "PASS: <name> - matches testdata/<name>.expected.txt", or "Expected" with the expected file and "FAIL: <name> - output differs from testdata/<name>.expected.txt", or "FAIL: <name> - exit code <n>, expected 0" (`run.cmd` writes the paths with `\`) |
| 8 | choices 1 to 7 in order | as above for each, then "Summary" with one "PASS: <name>" or "FAIL: <name> - <reason>" line per file, then "All 7 files PASS" or "<n> of 7 files FAIL" |
| 0 | — | the main menu again |
| anything else | — | "Not a choice: <choice>" |

Colours: "PASS" lines green, "FAIL" lines red, warning lines yellow, the app's console output bright cyan, headings bold; `run.sh` only when writing to a terminal; none when `NO_COLOR` is set. The script's own messages stay on console messages (stderr). The end of the input ends `run.sh` (exit code 0); `run.cmd` ends after three empty choices in a row (exit code 0).

[ci-01]: ../spec.md#ci-01
[ci-08]: ../spec.md#ci-08
[ci-02]: ../spec.md#ci-02
[ci-03]: ../spec.md#ci-03
[ci-04]: ../spec.md#ci-04
[ci-05]: ../spec.md#ci-05
[ci-07]: ../spec.md#ci-07
[dec-04]: ../../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[r-03]: ../research.md#r-03-arguments-and-starting-the-application-nfr-08-steps-3-and-4
[ci-09]: ../spec.md#ci-09
