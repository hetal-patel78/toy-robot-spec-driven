# Implementation Plan: Console session

**Branch**: `002-console-session` | **Date**: 2026-10-05 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-console-session/spec.md`

**Note**: This plan was written by following the `/speckit-plan` command. It uses only the technical choices fixed on solution design pages 5 and 9 (Spec Kit brief rule 7) and the component names on page 6 (brief rule 6).

## Summary

This spec builds the two components that turn spec 001's robot rules into a console application: the **Runner** (runs one session over a reader and two writers) and **Program** (the thin entry point). It also plans the whole-flow tests in memory (level 2 of [DEC-31][dec-31]) and the end-to-end tests of the built application (level 3). It owns [FR-08][fr-08], [FR-11][fr-11] to [FR-17][fr-17], [NFR-01][nfr-01], [NFR-04][nfr-04], [NFR-05][nfr-05] and [NFR-06][nfr-06].

Technical approach (pages 5, 6, 7, 9; [DEC-26][dec-26], [DEC-29][dec-29]):
- `Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)` with one method, `Run()`, which follows page 6's five steps for each line: prompt (interactive), read, remove a byte-order mark from the first line, echo (redirected), parse and dispatch. It creates its own CommandParser and never touches `Console`.
- `Program` with `static int Main(string[] args)`, which picks `Console.In` (typing) or its own UTF-8 reader over console input (redirected) and returns what `internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)` returns for that reader, `Console.Out` and `Console.Error`. `Run`: any argument gives the how-to-run message on `error` and returns 1; otherwise it builds `Table(5, 5)`, `RobotOperator(table)`, runs the Runner, and returns 0; an unexpected exception gives one error line on `error` and returns 2 ([PG-02][pg-02], [DEC-32][dec-32]).
- Whole-flow tests give the Runner a `StringReader` and two `StringWriter`s; end-to-end tests start the built executable as a separate process with redirected console input, console output (stdout) and console messages (stderr).

## Technical Context

**Language/Version**: C# on .NET 8 (`net8.0`), `RollForward` = `LatestMajor` ([DEC-37][dec-37]; set up by spec 001, [SK-02][sk-02])

**Primary Dependencies**: none beyond the .NET base class library (`System.IO`, `System.Text`, `System.Diagnostics.Process` in the end-to-end tests). No other library, no dependency injection container, no logging framework ([DEC-39][dec-39], brief rule 7).

**Storage**: N/A. Input is console input (stdin); output is console output (stdout) and console messages (stderr). Test-data files in `testdata/` are delivered by spec 003.

**Testing**: xUnit (page 5). Whole-flow tests in `tests/ToyRobot.Tests/` (same project as spec 001's unit tests); end-to-end tests in `tests/ToyRobot.EndToEndTests/` (project created empty by spec 001, filled here).

**Target Platform**: Windows, Linux and macOS ([DEC-33][dec-33]); console application; CI on `windows-latest`, `ubuntu-latest`, `macos-latest` (spec 003).

**Project Type**: console application (CLI) with two test projects, in `ToyRobot.sln` at the repository root.

**Performance Goals**: none stated by the sources; one line at a time, no reading ahead ([DEC-20][dec-20]).

**Constraints**: console output (stdout) carries only the REPORT result ([DEC-23][dec-23]); exact strings copied from [DEC-04][dec-04], [DEC-21][dec-21], [DEC-22][dec-22], [DEC-24][dec-24], [DEC-25][dec-25]; never crash ([DEC-32][dec-32]); Program does not change the console's encoding ([DEC-26][dec-26]); no `Console` use outside Program ([DEC-29][dec-29]).

**Scale/Scope**: two production classes (Runner, Program) and their tests.

**Depends on**: spec 001-robot-rules, fully implemented and its tests passing: `ToyRobot.sln`, the three projects, xUnit, and the types Table, Direction, RobotState, Robot, `IRobotCommand` and the five command classes, `IRobotOperator`, RobotOperator and CommandParser, with the parse result (a command object, EXIT, or a failure with its reason) and RobotOperator's outcome (the position text, a warning, or done with nothing to show). The Runner uses those types exactly as spec 001's plan names them (its data model), and does not redefine them. Spec 003 builds on this spec: its test data is run by [E2E-01][e2e-01], and its CI runs these tests.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | How this plan meets it | Status |
|---|---|---|
| I. Clean Code and SOLID | Runner and Program each have the single job page 6 gives them; names and members exactly as page 6 and [DEC-29][dec-29]; the Runner depends on the reader and writers it is given, not on `Console`. | Pass |
| II. Three Test Levels, Tests First | This spec plans levels 2 and 3 (level 1 is spec 001). In `tasks.md` every test task comes before the code that makes it pass; each phase ends with `dotnet build` and `dotnet test` all passing. Expected output is compared line by line (end-to-end) and exactly (whole-flow). | Pass |
| III. Never Crash | The Runner handles every parse result and outcome; nothing is thrown for any input; [NFR-01][nfr-01] tests at both levels (empty input, a number too large, every kind of bad line, BOM). Program's last safeguard turns an unexpected exception into one error line and exit code 2 ([PG-02][pg-02], [DEC-32][dec-32]), tested in memory ([SG-01][sg-01]). | Pass |
| IV. Cross-Platform | No platform-specific code in the application; the end-to-end tests start `ToyRobot.exe` or `ToyRobot` by platform; CRLF and LF input are both tested ([WF-03][wf-03], [E2E-07][e2e-07]); CI runs all levels on three systems (spec 003). | Pass |
| V. No Over-Engineering | Only the BCL and xUnit; no options, no configuration, no extra commands or messages; the table size stays a constructor argument set by Program. | Pass |
| VI. Standard Output for REPORT Output Only | The Runner writes only `Output:` and the position text to `output`; prompt, echo and warnings go to `error`; Program writes the how-to-run message to `Console.Error`. Every test checks the two channels separately. | Pass |

No violations; Complexity Tracking is empty.

**Re-check after Phase 1 design**: the data model, contracts and quickstart add no component, library or behaviour beyond page 6 and the spec. Pass.

## Project Structure

### Documentation (this feature)

```text
specs/002-console-session/
├── spec.md              # Feature spec (with Clarifications)
├── plan.md              # This file
├── research.md          # Phase 0: decisions and their reasons
├── data-model.md        # Phase 1: Runner, Program, session state, test helpers
├── quickstart.md        # Phase 1: how to build, test and check this spec
├── contracts/
│   ├── console-interface.md   # The application's console contract (arguments, channels, exact strings, exit codes)
│   └── runner.md              # The Runner's constructor, Run() and per-line steps
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Written by /speckit-tasks
```

### Source Code (repository root)

```text
src/
└── ToyRobot/
    ├── ToyRobot.csproj           # from spec 001 (net8.0, RollForward = LatestMajor); this spec adds InternalsVisibleTo for ToyRobot.Tests (SG-01)
    ├── Program.cs                # Program (this spec; replaces the skeleton's entry point)
    ├── Runner.cs                 # Runner (this spec)
    └── ...                       # spec 001's types (Table, Direction, RobotState, Robot, commands, RobotOperator, CommandParser)

tests/
├── ToyRobot.Tests/               # from spec 001: unit tests, plus this spec's whole-flow tests
│   └── WholeFlow/
│       ├── SessionHarness.cs                 # runs a Runner over a StringReader and two StringWriters
│       ├── RedirectedInputTests.cs           # FR-08, FR-11, FR-12, FR-14, WF-02, WF-03
│       ├── EndOfSessionTests.cs              # FR-15, FR-16
│       ├── PromptTests.cs                    # FR-13
│       ├── NeverCrashTests.cs                # NFR-01
│       ├── ProgramSafeguardTests.cs          # SG-01: Program's Run in memory (PG-01, PG-02)
│       ├── SmallTableTests.cs                # NFR-05 (2 x 2)
│       └── RobotRulesAsConsoleTextTests.cs   # WF-01: spec 001's scenarios as console text
└── ToyRobot.EndToEndTests/       # from spec 001 (empty); this spec adds:
    ├── ToyRobot.EndToEndTests.csproj         # from spec 001, with its ProjectReference to src/ToyRobot (builds the app first)
    ├── AppProcess.cs                         # starts the built executable, feeds bytes, collects both channels and the exit code
    ├── LineComparer.cs                       # splits text into lines (CRLF or LF) for line-by-line comparison
    ├── TestDataTests.cs                      # E2E-01
    ├── HowToRunMessageTests.cs               # E2E-02, E2E-08
    ├── SessionEndTests.cs                    # E2E-03, E2E-04
    ├── ByteOrderMarkTests.cs                 # E2E-05
    ├── OutputChannelTests.cs                 # E2E-06
    ├── StartUpTests.cs                       # E2E-09
    └── LineEndingTests.cs                    # E2E-07
```

**Structure Decision**: the layout is fixed by page 5 and spec 001 ([SK-01][sk-01]): one application project and two xUnit test projects in `ToyRobot.sln`. This spec adds two files to the application project, a `WholeFlow` folder to `ToyRobot.Tests`, and its files to `ToyRobot.EndToEndTests`. File names inside the test projects are a planning choice (research [R-08][r-08]); the component names are page 6's.

Namespaces follow the folders, as in spec 001: `Runner` and `Program` are in `ToyRobot`; the whole-flow tests and `SessionHarness` are in `ToyRobot.Tests.WholeFlow`; the end-to-end tests, `AppProcess` and `LineComparer` are in `ToyRobot.EndToEndTests`.

## Design Notes

### Runner (page 6, DEC-26, DEC-29, DEC-30)

- Fields: the five constructor arguments, a `CommandParser` it creates, and a flag that says whether the first line has been read.
- `Run()`, for each line (page 6, steps 1 to 5):
  1. if `interactive`, `error.Write("> ")` (no line break, [FR-13][fr-13]);
  2. `var line = input.ReadLine()`; if `null`, return ([FR-16][fr-16]);
  3. on the first line only, if it starts with the byte-order mark character U+FEFF (C# `(char)0xFEFF`), remove that one character ([FR-11][fr-11]);
  4. if not `interactive`, `error.WriteLine("> " + line.Trim())` ([FR-14][fr-14]);
  5. parse the line (not trimmed by the Runner; the parser trims, page 6) and act on the parse result: a failure (`ParseResult.Failure`, `Reason`) → `error.WriteLine("Warning: " + reason)`; EXIT (`ParseResult.Exit`) → return ([FR-15][fr-15]); a command object (`ParseResult.CommandObject`) → pass it to RobotOperator (`RobotOperator.Run`) and act on the outcome: the position text (`Outcome.PositionText`) → `output.WriteLine("Output:")` then `output.WriteLine(text)` ([FR-12][fr-12]); a warning (`Outcome.Warning`) → `error.WriteLine("Warning: " + text)`; done with nothing to show (`Outcome.Done`) → nothing.
- Every line the Runner writes, except the prompt, ends with the writer's own line break (`WriteLine`) (spec Assumptions, "line breaks").
- The Runner does not catch exceptions: no component throws for any input (spec 001 [UT-04][ut-04], [DEC-32][dec-32]), so there is nothing to catch for input, and a catch-all here would hide a defect instead of letting a test find it. The one last safeguard is in Program ([PG-02][pg-02]).

### Program (page 6, page 7.7, DEC-04, DEC-26, DEC-32)

- `internal static class Program` (not public, so the public type list in spec 001's DesignRulesTests, to which only `Runner` is added, stays exact), with two methods.
- `static int Main(string[] args)`: `bool interactive = !Console.IsInputRedirected`; reader = `interactive ? Console.In : new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: false)` (research [R-02][r-02]); return `Run(args, reader, Console.Out, Console.Error, interactive)`. Creating the reader reads nothing, so with an argument console input is still not read.
- `internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)`, all inside one `try`:
  - `if (args.Length > 0)`: `error.WriteLine("How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)")`; return 1. `input` is not read.
  - Otherwise: `var table = new Table(5, 5)`; `var robotOperator = new RobotOperator(table)`; `new Runner(input, output, error, robotOperator, interactive).Run()`; return 0.
  - `catch (Exception exception)`: in an inner `try`, `error.WriteLine("Error: unexpected problem - " + exception.Message)`, with an inner `catch (Exception)` that ignores a failure to write it; return 2 ([PG-02][pg-02], [DEC-23][dec-23], [DEC-32][dec-32]).
- `src/ToyRobot/ToyRobot.csproj` gets an `ItemGroup` with `<InternalsVisibleTo Include="ToyRobot.Tests" />` (an MSBuild item; no library or package), so `ToyRobot.Tests` can call `Run` ([SG-01][sg-01]).
- Program does not set `Console.OutputEncoding` or `Console.InputEncoding`, and does not handle Ctrl+C ([DEC-10][dec-10], [DEC-26][dec-26]).

### Whole-flow test harness

`SessionHarness.Run(string input, bool interactive = false, int width = 5, int height = 5)` builds `Table(width, height)`, `RobotOperator(table)`, a `StringReader(input)`, two `StringWriter`s whose `NewLine` is set to `"\n"`, runs `new Runner(...).Run()`, and returns the two texts. Tests compare each text exactly with the expected lines, each followed by `"\n"` (the prompt with no line break). Setting `NewLine` makes the exact comparison the same on every system ([NFR-04][nfr-04]).

### End-to-end test harness

`AppProcess.Run(byte[] stdin, params string[] args)`:
- finds the repository root by walking up from `AppContext.BaseDirectory` to the folder holding `ToyRobot.sln`;
- finds the executable at `src/ToyRobot/bin/<configuration>/net8.0/ToyRobot.exe` (Windows) or `.../ToyRobot` (Linux, macOS), where `<configuration>` comes from the test assembly's `AssemblyConfigurationAttribute` (research [R-04][r-04]);
- starts it with `UseShellExecute = false`, console input, console output and console messages redirected, and each argument added through `ArgumentList`;
- starts reading console output (stdout) and console messages (stderr) asynchronously before writing, writes the bytes to the input's base stream and closes it (an `IOException` while writing is ignored, because with an argument the application exits without reading, [E2E-08][e2e-08]), waits up to 30 seconds (a timeout kills the process and fails the test), and returns the stdout text, the stderr text and the exit code.

`LineComparer.Lines(string text)` splits on CRLF or LF and drops the one empty element after a final line break, so expected files and output compare line by line whatever line endings they use ([DEC-31][dec-31]).

### Safeguard tests (SG-01)

`ProgramSafeguardTests` in `ToyRobot.Tests` (namespace `ToyRobot.Tests.WholeFlow`) calls `Run` with a `StringReader` or the test helper `FailingReader` (its `ReadLine` throws `IOException` with the message `boom`), and with `StringWriter`s whose `NewLine` is `"\n"` or the test helper `FailingWriter` (every write throws `IOException` with the message `boom`). It checks the exact text on both writers and the returned exit code.

## Complexity Tracking

No Constitution Check violations; nothing to justify.

[dec-04]: ../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-10]: ../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-24]: ../../docs/design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../../docs/design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-26]: ../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-29]: ../../docs/design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../../docs/design-analysis/design-decisions.md#dec-33-cross-platform
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[e2e-01]: spec.md#e2e-01
[e2e-07]: spec.md#e2e-07
[e2e-08]: spec.md#e2e-08
[fr-08]: spec.md#fr-08
[fr-11]: spec.md#fr-11
[fr-12]: spec.md#fr-12
[fr-13]: spec.md#fr-13
[fr-14]: spec.md#fr-14
[fr-15]: spec.md#fr-15
[fr-16]: spec.md#fr-16
[fr-17]: spec.md#fr-17
[nfr-01]: spec.md#nfr-01
[nfr-04]: spec.md#nfr-04
[nfr-05]: spec.md#nfr-05
[nfr-06]: spec.md#nfr-06
[pg-02]: spec.md#pg-02
[r-02]: research.md#r-02-the-utf-8-reader-for-redirected-input-dec-20-dec-26
[r-04]: research.md#r-04-how-the-end-to-end-tests-start-the-built-application-decided
[r-08]: research.md#r-08-test-file-names-and-test-names-dec-31-nfr-06
[sg-01]: spec.md#sg-01
[sk-01]: ../001-robot-rules/spec.md#sk-01
[sk-02]: ../001-robot-rules/spec.md#sk-02
[ut-04]: ../001-robot-rules/spec.md#ut-04
[wf-03]: spec.md#wf-03
