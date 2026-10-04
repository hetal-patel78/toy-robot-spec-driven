# Research: Console session

Phase 0 of the plan. The Technical Context has no open clarification item: the language, framework, test framework, layout and CI are fixed by solution design pages 5 and 9 (Spec Kit brief rule 7). The items below record how the plan meets the sources where they leave a detail open. Items marked **Decided** are decisions under brief rule 10 and are also listed in the spec's Assumptions.

## R-01 Runner method and Program entry point (Decided)

- **Decision**: `Runner.Run()` returns nothing and returns at EXIT or at the end of the input; Program is `static int Main(string[] args)` in a class named `Program` and returns the exit code.
- **Rationale**: page 6 says Program "creates ... and starts the Runner" and "gives back" exit code 1 or 0; returning the code from `Main` is the standard .NET form and keeps Program testable end-to-end without `Environment.Exit`. A class named `Program` matches page 6's component name.
- **Alternatives considered**: top-level statements (no visible `Program` component to review against page 6); `Environment.Exit` (abrupt, harder to follow); `Run()` returning a value (page 6: the Runner gives back only text on the two writers).

## R-02 The UTF-8 reader for redirected input (DEC-20, DEC-26)

- **Decision**: `new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: false)`.
- **Rationale**: [DEC-26][dec-26] asks for Program's own UTF-8 reader that removes a leading byte-order mark. `StreamReader` skips the UTF-8 preamble when its encoding has one (`Encoding.UTF8` does), so the mark is removed without reading ahead past the first line. Not detecting other byte-order marks keeps the reader exactly UTF-8, as [DEC-26][dec-26] says. The Runner removes a U+FEFF left at the start of the first line as well ([DEC-20][dec-20]), which is what the whole-flow tests exercise.
- **Alternatives considered**: `Console.In` for redirected input (its encoding depends on the system's console code page, so a byte-order mark or non-ASCII whitespace such as a non-breaking space could be read differently on Windows; [DEC-26][dec-26] rules it out); setting `Console.InputEncoding` ([DEC-26][dec-26]: Program does not change the console's encoding settings, because that can fail on Windows without a console window).

## R-03 Writing lines and the prompt (spec Assumptions, "line breaks")

- **Decision**: the Runner writes each echo, warning, `Output:` label and position text with `WriteLine` on the writer it was given; the prompt `> ` with `Write` (no line break). Program writes the how-to-run message with `Console.Error.WriteLine`.
- **Rationale**: page 2 shows every output as whole lines and the [FR-13][fr-13] prompt with no line break. `Console.Out` and `Console.Error` flush automatically, so the prompt appears before the read.
- **Alternatives considered**: writing `"\n"` explicitly (would differ from the system's line break on Windows; the comparison is line by line anyway, [DEC-31][dec-31]).

## R-04 How the end-to-end tests start the built application (Decided)

- **Decision**: the end-to-end test project keeps the `ProjectReference` to `src/ToyRobot/ToyRobot.csproj` that spec 001 T005 adds, which builds the application first. Each test starts the executable at `src/ToyRobot/bin/<configuration>/net8.0/ToyRobot.exe` (Windows) or `src/ToyRobot/bin/<configuration>/net8.0/ToyRobot` (Linux, macOS), found from the repository root (the folder holding `ToyRobot.sln`, found by walking up from the test's output folder). `<configuration>` is read from the test assembly's `AssemblyConfigurationAttribute`, which the .NET SDK sets to the build configuration.
- **Rationale**: page 5 gives the path `src/ToyRobot/bin/Release/net8.0/` for the run scripts' Release build; the same folder pattern with the tests' own configuration means `dotnet test` from the repository root (Debug by default) and a Release test run both start the build they just made, with no earlier step. Starting the application's own executable tests what a reviewer runs ([DEC-31][dec-31] level 3: "the built app as a separate process"), including `RollForward` = `LatestMajor`.
- **Alternatives considered**: always the Release path (needs a separate Release build before `dotnet test`); `dotnet run` (slow, mixes build output into console output); `dotnet ToyRobot.dll` (does not start the executable the README and scripts use).

## R-05 E2E-01 with no test data yet (Decided)

- **Decision**: [E2E-01][e2e-01] is one xUnit `[Fact]` that enumerates every `testdata/<name>.txt` that has a `testdata/<name>.expected.txt` (excluding files ending `.expected.txt` from the inputs), runs each, collects every mismatch with the pair's name, and fails if any mismatch was found. If `testdata/` is missing or holds no pair, it passes.
- **Rationale**: spec 002 is implemented before spec 003 delivers `testdata/` (brief: order 001, 002, 003). An xUnit `[Theory]` whose data source is empty is reported as a failure, which would stop spec 002's phases from ending green. Spec 003's [TD-02][td-02] makes sure the challenge example pairs are present.
- **Alternatives considered**: `[Theory]` with `MemberData` (fails with no data); spec 002 creates the challenge example files (moves ownership from spec 003, brief table).

## R-06 Exact comparison in whole-flow tests (NFR-04, NFR-05)

- **Decision**: the harness sets `NewLine = "\n"` on both `StringWriter`s and compares each channel's full text exactly with the expected lines joined by `"\n"` (with a final `"\n"` after each written line; the prompt has none).
- **Rationale**: [DEC-31][dec-31] asks for standard output and standard error "checked separately and exactly"; a fixed line break makes the same expected text pass on every system.
- **Alternatives considered**: `Environment.NewLine` in expected texts (works, but every expected text then needs building in code); line-by-line comparison only (would not catch a missing or extra line break after the prompt).

## R-07 A last safeguard in Program, no catch-all in the Runner (Decided, DEC-32)

- **Decision**: the Runner catches no exceptions. Program's `Run` does its argument check, creates the parts and runs the Runner inside one `try` that catches any exception, writes exactly one line, `Error: unexpected problem - <exception message>` (the exception's `Message`), to console messages (stderr), and returns exit code 2; if writing that line throws, it still returns 2 ([PG-02][pg-02]).
- **Rationale**: [DEC-32][dec-32] is met for input by design: the CommandParser returns failures instead of throwing and uses `int.TryParse` (spec 001, [UT-04][ut-04]), and RobotOperator checks before it replaces ([DEC-13][dec-13]). [DEC-32][dec-32] adds the safeguard for faults outside the application's control (such as the console or a pipe closing while it writes) and defects, so the user gets one clear line and a defined exit code instead of a .NET stack trace. The tests check output and exit codes exactly, so a defect still fails them. The safeguard is in `Run`, not `Main`, so the tests can drive it in memory with a reader or writer that fails ([SG-01][sg-01]).
- **Alternatives considered**: a catch-all in the Runner that writes a warning and continues (a message no decision defines; it could hide a defect); the `try` in `Main` (testable only end-to-end, and no input can make the built application fail).

## R-08 Test file names and test names (DEC-31, NFR-06)

- **Decision**: whole-flow tests in `tests/ToyRobot.Tests/WholeFlow/`, end-to-end tests in `tests/ToyRobot.EndToEndTests/`, one file per area (plan, Project Structure). Test methods use the challenge wording where possible, with underscores, for example `Robot_not_on_table_ignores_MOVE`, `EXIT_mid_file_stops_later_lines`, `How_to_run_message_and_exit_code_1_when_started_with_help`.
- **Rationale**: [DEC-31][dec-31] and page 9 ask for challenge wording; grouping by requirement makes the comparison with [DEC-31][dec-31]'s list ([SC-006][sc-006]) direct.
- **Alternatives considered**: one large test file per level (harder to trace to FR IDs).

## R-09 Visibility of Runner and Program

- **Decision**: the Runner has the same visibility as spec 001's types (it takes a `RobotOperator` in its constructor, so it cannot be more visible than RobotOperator); the whole-flow tests reach it the same way spec 001's unit tests reach spec 001's types. Program's `Main` is tested only end-to-end (page 10); its internal `Run` is also called in memory by the [SG-01][sg-01] tests, through `<InternalsVisibleTo Include="ToyRobot.Tests" />` in `src/ToyRobot/ToyRobot.csproj`.
- **Rationale**: C# requires a constructor parameter type to be at least as accessible as the constructor. Following spec 001's choice keeps one rule for the whole application.
- **Decided**: Program is `internal static class Program`; only Runner is added to spec 001's public type list (spec 001 DesignRulesTests, T054), by T020a.
- **Alternatives considered**: a different visibility for the Runner (fails to compile, or exposes more than spec 001 does).

[dec-13]: ../../docs/design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-26]: ../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[e2e-01]: spec.md#e2e-01
[fr-13]: spec.md#fr-13
[pg-02]: spec.md#pg-02
[sc-006]: spec.md#sc-006
[sg-01]: spec.md#sg-01
[td-02]: ../003-delivery/spec.md#td-02
[ut-04]: ../001-robot-rules/spec.md#ut-04
