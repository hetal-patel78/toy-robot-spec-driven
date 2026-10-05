# Implementation Plan: Delivery

**Branch**: `003-delivery` | **Date**: 2026-10-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-delivery/spec.md`

## Summary

This spec delivers what lets a reviewer run, check and trust the application on any system: the run scripts `run.cmd` and `run.sh` ([NFR-08][nfr-08]), the seven test-data pairs in `testdata/` including the three challenge examples ([TD-01][td-01]), the GitHub Actions workflow that builds and tests on `windows-latest`, `ubuntu-latest` and `macos-latest` and checks the scripts there ([NFR-02][nfr-02], [CI-01][ci-01] to [CI-09][ci-09]), `.gitattributes` ([NFR-02a][nfr-02a]), the README ([NFR-09][nfr-09]), and two tests in the console end-to-end test project: [TD-02][td-02] (the test data is all there) and [BS-01][bs-01] (the project file targets .NET 8 and rolls forward, [NFR-03][nfr-03]). It writes no application code: Program, Runner and every other component on page 6 come from specs 001 and 002 and are only started, through the built application, by the scripts and CI.

Technical approach (research.md): each script captures the output of its .NET check and of a `Release` build of `src/ToyRobot/ToyRobot.csproj`, shows it only if the build fails, then starts the built application at the page 5 path with the file (if any) redirected to its console input (stdin). CI runs every script path, including the error paths, with exit-code and line-by-line console output (stdout) checks, on all three systems.

## Technical Context

**Language/Version**: C# on .NET 8 (`net8.0`, `RollForward` = `LatestMajor`, from spec 001 [SK-02][sk-02]); Windows batch (`run.cmd`, cmd.exe); bash (`run.sh`, compatible with bash 3.2, the version macOS ships)

**Primary Dependencies**: none added. The .NET SDK 8 or later on the reviewer's machine; in CI, GitHub Actions' standard `actions/checkout` and `actions/setup-dotnet` steps (spec Assumptions; brief rule 7)

**Storage**: files only: `testdata/*.txt` pairs

**Testing**: xUnit in `tests/ToyRobot.ConsoleEndToEndTests/` ([TD-02][td-02], [BS-01][bs-01], plus spec 002's [E2E-01][e2e-01] that runs every pair); GitHub Actions workflow steps for the scripts ([CI-01][ci-01] to [CI-06][ci-06]); the README check (quickstart.md, "README check")

**Target Platform**: Windows (cmd and PowerShell), Linux and macOS (bash or zsh); CI on `windows-latest`, `ubuntu-latest`, `macos-latest`

**Project Type**: console application (delivery artefacts around it)

**Performance Goals**: none set by the sources

**Constraints**: console output (stdout) carries only the application's REPORT result ([DEC-23][dec-23]): the scripts write their own messages (progress lines, errors) and any build output (only when the build fails) to console messages (stderr) and nothing of their own to console output (stdout), except the menu's lines ([MN-01][mn-01], added later); exit codes exactly as [NFR-08][nfr-08]; no PowerShell script, no other tool or library ([DEC-37][dec-37], [DEC-39][dec-39]); nothing is deleted (brief rule 12)

**Scale/Scope**: 2 scripts, 12 test-data files, 1 workflow, 1 `.gitattributes`, 1 `.gitignore` checked (created by spec 001, research [R-11][r-11]), 1 README changed in place, 1 test file in the console end-to-end project

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | How this plan meets it | Result |
|---|---|---|
| I. Clean Code and SOLID | No new component; scripts each do the four [NFR-08][nfr-08] steps in order, nothing else; page 6 names used only to refer to Program and the built application | Pass |
| II. Three Test Levels, Tests First | The three levels run in CI on all three systems; [TD-02][td-02] is written before the test data, the CI script checks before the scripts, the README check before the README; each phase ends with its tests passing | Pass |
| III. Never Crash | The only argument the scripts pass to the application is `--help` ([DEC-04][dec-04]); every script error path has a defined message on stderr and exit code, and a CI check ([CI-02][ci-02], [CI-04][ci-04], [CI-05][ci-05]) | Pass |
| IV. Cross-Platform | CI on `windows-latest`, `ubuntu-latest`, `macos-latest`; `.gitattributes`; `run.sh` executable; line-by-line comparison | Pass |
| V. No Over-Engineering | Only standard checkout and SDK steps; no demo mode in the run scripts at first (the menu of [DEC-41][dec-41] was added later at the developer's request), no PowerShell script, no extra tool; scripts hold only what [DEC-37][dec-37] lists | Pass |
| VI. Standard Output for REPORT Output Only | Scripts write nothing of their own to stdout, except the menu's lines (MN-01, added later); CI checks stdout is empty in every error case and equals the expected file otherwise | Pass |

Post-design re-check (after research.md, data-model.md, contracts/, quickstart.md): all six still pass. The `.gitignore` ([R-11][r-11], created by spec 001 and checked here) adds no behaviour; it keeps build output out of the repository.

## Project Structure

### Documentation (this feature)

```text
specs/003-delivery/
├── plan.md              # This file
├── research.md          # Phase 0: decisions R-01 to R-12
├── data-model.md        # Phase 1: test-data pairs, script steps, CI job, line-ending rules
├── quickstart.md        # Phase 1: how to validate this spec, including the README check
├── contracts/
│   ├── run-scripts.md   # the run scripts' arguments, channels and exit codes
│   ├── ci-workflow.md   # the CI workflow's trigger, systems and steps
│   └── testdata.md      # the test-data pair format and the seven pairs
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 (/speckit-tasks)
```

### Source Code (repository root)

```text
toy-robot-spec-driven/
├── src/ToyRobot/                         spec 001 and 002 (not changed here)
├── tests/
│   ├── ToyRobot.Tests/                   spec 001 and 002 (not changed here)
│   └── ToyRobot.ConsoleEndToEndTests/           spec 002; this spec adds DeliveryTests.cs (TD-02, BS-01)
├── testdata/                             this spec: seven pairs (TD-01)
│   ├── challenge-example-1.txt / .expected.txt
│   ├── challenge-example-2.txt / .expected.txt
│   ├── challenge-example-3.txt / .expected.txt
│   ├── bom-challenge-example-1.txt / .expected.txt
│   ├── exit-mid-file.txt / .expected.txt
│   └── empty.txt / .expected.txt
├── .github/workflows/ci.yml              this spec (NFR-02, CI-01 to CI-06)
├── ToyRobot.sln                          spec 001
├── run.cmd                               this spec (NFR-08; the menu: MN-01, developer request 2026-10-05)
├── run.sh                                this spec (NFR-08; MN-01), executable
├── .gitattributes                        this spec (NFR-02a)
├── .gitignore                            created by spec 001 (SK skeleton); checked here (R-11)
└── README.md                             this spec: the README (NFR-09), changed in place
```

**Structure Decision**: the layout on page 5, unchanged. The only file this spec adds to a project of another spec is `tests/ToyRobot.ConsoleEndToEndTests/DeliveryTests.cs`, because [TD-02][td-02] and [BS-01][bs-01] are console end-to-end-project tests (spec [TD-02][td-02], [BS-01][bs-01]) and the brief gives this spec the test data and [NFR-03][nfr-03]. `DeliveryTests` is in namespace `ToyRobot.ConsoleEndToEndTests`, as the project's other files.

### Dependencies on the other specs

- **Spec 001-robot-rules**: `ToyRobot.sln`, the three projects, xUnit, `net8.0` and `RollForward` = `LatestMajor` ([SK-01][sk-01], [SK-02][sk-02]). [BS-01][bs-01] reads what [SK-02][sk-02] sets.
- **Spec 002-console-session**: the built application's behaviour (Program and Runner: echo, exit codes, how-to-run message) and the console end-to-end tests, in particular [E2E-01][e2e-01], which runs every pair this spec adds. The scripts and CI rely on Program reading console input (stdin) and returning exit code 0 at the end of the input or EXIT.
- This spec is implemented after both; its first task checks that `dotnet build` and `dotnet test` pass at the repository root.

## Complexity Tracking

No constitution violations. Nothing to justify.

[bs-01]: spec.md#bs-01
[ci-01]: spec.md#ci-01
[ci-02]: spec.md#ci-02
[ci-04]: spec.md#ci-04
[ci-05]: spec.md#ci-05
[ci-06]: spec.md#ci-06
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-41]: ../../docs/design-analysis/design-decisions.md#dec-41-menu-in-the-run-scripts
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[e2e-01]: ../002-console-session/spec.md#e2e-01
[nfr-02]: spec.md#nfr-02
[nfr-02a]: spec.md#nfr-02a
[nfr-03]: spec.md#nfr-03
[nfr-08]: spec.md#nfr-08
[nfr-09]: spec.md#nfr-09
[r-11]: research.md#r-11-gitignore
[sk-01]: ../001-robot-rules/spec.md#sk-01
[sk-02]: ../001-robot-rules/spec.md#sk-02
[td-01]: spec.md#td-01
[td-02]: spec.md#td-02
[dec-04]: ../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[ci-09]: spec.md#ci-09
[mn-01]: spec.md#mn-01
