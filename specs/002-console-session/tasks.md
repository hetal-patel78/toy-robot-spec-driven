---

description: "Task list for spec 002-console-session (Runner, Program, Runner whole-flow and console end-to-end tests)"
---

# Tasks: Console session

**Input**: Design documents from `specs/002-console-session/` (spec.md, plan.md, research.md, data-model.md, contracts/, quickstart.md)

**Prerequisites**: spec 001-robot-rules is fully implemented and all its tests pass (see Phase 1). Spec 003-delivery is implemented after this spec and depends on it (Dependencies section).

**Tests**: Required. The spec plans Runner whole-flow tests in memory ([WF-01][wf-01] to [WF-03][wf-03]) and console end-to-end tests ([E2E-01][e2e-01] to [E2E-09][e2e-09]), and the safeguard tests of Program's `Run` in memory ([SG-01][sg-01]), and the constitution (principle II) and the Spec Kit brief (rule 8) require tests first. In every phase the test tasks come before the code that makes them pass; until that code exists the new tests fail or do not compile (red), and the phase ends only when `dotnet build` and `dotnet test` from the repository root both pass (green).

**Exact strings**: every expected text below, and every expected text a task takes from `spec.md` or `specs/001-robot-rules/spec.md`, is copied character for character (brief rule 2). Runner whole-flow tests compare console output (stdout) and console messages (stderr) separately and exactly, using `SessionHarness` (writers with `NewLine = "\n"`, research [R-06][r-06]); console end-to-end tests compare line by line using `LineComparer` ([DEC-31][dec-31]).

**Invisible characters**: in this file `<U+FEFF>` stands for the byte-order mark character (C# `(char)0xFEFF`) and `<U+00A0>` for the non-breaking space (C# `(char)0xA0`); inside a quoted test string the test writes that one character, not the notation.

**Organization**: one phase per user story of spec.md, in priority order. Where a scenario needs a Runner step that a later phase builds, its test is placed in that later phase, and the task says so.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: the spec.md user story (US1 to US5)
- Every task cites its FR / NFR and DEC IDs.

## Path Conventions

Single solution `ToyRobot.sln` at the repository root (page 5): `src/ToyRobot/`, `tests/ToyRobot.Tests/`, `tests/ToyRobot.ConsoleEndToEndTests/`, `testdata/` (spec 003).

Namespaces follow the folders, as in spec 001: `Runner` is in `ToyRobot.Session` (folder `src/ToyRobot/Session/`, moved there by spec 001 T060) and `Program` in `ToyRobot`; the Runner whole-flow tests and `SessionHarness` are in `ToyRobot.Tests.RunnerWholeFlow`; the console end-to-end tests, `AppProcess` and `LineComparer` are in `ToyRobot.ConsoleEndToEndTests`.

---

## Phase 1: Setup (shared infrastructure)

**Purpose**: confirm the base from spec 001 and prepare the console end-to-end test project.

- [x] T001 Confirm spec 001-robot-rules is complete ([NFR-06][nfr-06]; [DEC-26][dec-26], [DEC-31][dec-31]): from the repository root `dotnet build` and `dotnet test` pass; `ToyRobot.sln`, `src/ToyRobot/ToyRobot.csproj`, `tests/ToyRobot.Tests/ToyRobot.Tests.csproj` and `tests/ToyRobot.ConsoleEndToEndTests/ToyRobot.ConsoleEndToEndTests.csproj` exist (spec 001 [SK-01][sk-01]). Confirm that the names fixed in `specs/001-robot-rules/contracts/robot-rules-api.md` exist, public, in namespace `ToyRobot`: `CommandParser.Parse(string line)` returning `ParseResult` (`ParseResult.CommandObject(IRobotCommand Command)`, `ParseResult.Exit`, `ParseResult.Failure(string Reason)`); `RobotOperator.Run(IRobotCommand command)` returning `Outcome` (`Outcome.PositionText(string Text)`, `Outcome.Warning(string Text)`, `Outcome.Done`). Do not change spec 001's code, except the DesignRulesTests list update in T020a and the replacement of the placeholder `src/ToyRobot/Program.cs` in T022. (Depends on: spec 001 implemented.)
- [x] T002 Confirm that `tests/ToyRobot.ConsoleEndToEndTests/ToyRobot.ConsoleEndToEndTests.csproj` has the project reference to `..\..\src\ToyRobot\ToyRobot.csproj` added by spec 001 T005, so building the console end-to-end tests builds the application first (research [R-04][r-04]; [NFR-06][nfr-06]; [DEC-31][dec-31] level 3). Add no second reference and no package (brief rule 7).
- [x] T003 Run `dotnet build` and `dotnet test` from the repository root: both pass. ([NFR-06][nfr-06]; [DEC-31][dec-31])

---

## Phase 2: Foundational (blocking prerequisites)

**Purpose**: console end-to-end test helpers that every story's console end-to-end tests use. They depend only on the .NET base class library, so they build before the Runner exists.

- [x] T004 [P] Create `LineComparer` in `tests/ToyRobot.ConsoleEndToEndTests/LineComparer.cs`: `static IReadOnlyList<string> Lines(string text)` splits on `"\r\n"` or `"\n"` and drops the single empty element left by a final line break; an empty text gives no lines ([DEC-31][dec-31] "compared line by line", [NFR-04][nfr-04]; plan "Console end-to-end test harness").
- [x] T005 [P] Create `AppProcess` in `tests/ToyRobot.ConsoleEndToEndTests/AppProcess.cs` (plan "Console end-to-end test harness", research [R-04][r-04]; [NFR-06][nfr-06]; [DEC-04][dec-04], [DEC-31][dec-31] level 3): `static AppResult Run(byte[] stdin, params string[] args)` returning stdout text, stderr text and exit code. It finds the repository root by walking up from `AppContext.BaseDirectory` to the folder holding `ToyRobot.sln`; the executable is `src/ToyRobot/bin/<configuration>/net8.0/ToyRobot.exe` on Windows and `src/ToyRobot/bin/<configuration>/net8.0/ToyRobot` on Linux and macOS, with `<configuration>` from the test assembly's `AssemblyConfigurationAttribute`; it fails the test with a clear message if the file is missing. It starts the process with `UseShellExecute = false`, console input, console output and console messages redirected, `StandardOutputEncoding` and `StandardErrorEncoding` UTF-8, and arguments through `ArgumentList`; starts reading both outputs asynchronously before writing; writes the bytes to `StandardInput.BaseStream` and closes it, ignoring an `IOException` (the application exits without reading when given an argument, [E2E-08][e2e-08]); waits up to 30 seconds, and on timeout kills the process and fails the test. Also add `static byte[] Utf8(params string[] lines)` that joins lines with `"\n"` and a final `"\n"` as UTF-8 bytes without a byte-order mark.
- [x] T006 Run `dotnet build` and `dotnet test` from the repository root: both pass. ([NFR-06][nfr-06]; [DEC-31][dec-31])

**Checkpoint**: the base is ready; user story phases follow in order.

---

## Phase 3: User Story 1 - Run a file of commands and keep the REPORT results apart (Priority: P1) MVP

**Goal**: with redirected input, every line is echoed to console messages (stderr) with its warning under it, only REPORT results reach console output (stdout), a byte-order mark is ignored, and the session ends at the end of the input with exit code 0 ([FR-08][fr-08], [FR-11][fr-11], [FR-12][fr-12], [FR-14][fr-14], [FR-16][fr-16]; [NFR-04][nfr-04]).

**Independent Test**: the Runner whole-flow tests below through the Runner with `interactive` = false, and the console end-to-end tests below with redirected input.

### Tests for User Story 1 (write first; they fail or do not compile until T021 and T022, except T015, which passes while testdata/ has no pairs, and these tests, which spec 001's placeholder Program already meets because they check only empty stdout and exit code 0: T019's empty-input test, T017's `Line_with_two_commands_gives_warning_and_exit_code_0` and T018's `Redirected_REPORT_before_PLACE_writes_nothing_to_console_output`; all of them guard T022)

- [x] T007 [US1] Create `SessionHarness` in `tests/ToyRobot.Tests/RunnerWholeFlow/SessionHarness.cs` (plan "Runner whole-flow test harness", contracts/runner.md; [NFR-05][nfr-05], [DEC-29][dec-29]): `static (string Stdout, string Stderr) Run(string input, bool interactive = false, int width = 5, int height = 5)` builds `new Table(width, height)`, `new RobotOperator(table)`, `new StringReader(input)` and two `StringWriter`s with `NewLine = "\n"`, calls `new Runner(reader, outWriter, errWriter, robotOperator, interactive).Run()` (which must return normally, without an exception), and returns both texts. Also `static string Lines(params string[] lines)` returning each line followed by `"\n"` (empty string for no lines).
- [x] T008 [P] [US1] In `tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs`, test `Line_with_two_commands_is_one_bad_line` (spec US1 scenario 1; [FR-08][fr-08]; [DEC-04][dec-04], [DEC-05][dec-05]): input `PLACE 0,0,NORTH MOVE` and a line break; stdout `""`; stderr exactly `> PLACE 0,0,NORTH MOVE` and `Warning: 'PLACE 0,0,NORTH MOVE' ignored - F must be NORTH, SOUTH, EAST or WEST`, each followed by `"\n"`.
- [x] T009 [P] [US1] In `tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs`, test `Only_REPORT_result_goes_to_console_output` (spec US1 scenario 2; [FR-12][fr-12]; [DEC-21][dec-21], [DEC-23][dec-23]): input `MOVE`, `PLACE 0,0,NORTH`, `REPORT`; stdout exactly `Output:` and `0,0,NORTH`; stderr exactly `> MOVE`, `Warning: MOVE ignored - robot is not on the table`, `> PLACE 0,0,NORTH`, `> REPORT`.
- [x] T010 [P] [US1] In `tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs`, test `Byte_order_mark_at_start_of_input_is_ignored` (spec US1 scenario 4; [FR-11][fr-11]; [DEC-20][dec-20], [DEC-26][dec-26]): input `"<U+FEFF>"` + `PLACE 0,0,NORTH`, `MOVE`, `REPORT`; stdout exactly `Output:`, `0,1,NORTH`; stderr exactly `> PLACE 0,0,NORTH`, `> MOVE`, `> REPORT` (no U+FEFF anywhere, no warning).
- [x] T011 [P] [US1] In `tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs`, test `Byte_order_mark_is_removed_only_from_the_first_line` (spec Edge Cases, [FR-11][fr-11]; [DEC-20][dec-20], [DEC-08][dec-08], [DEC-22][dec-22]): input `PLACE 0,0,NORTH`, then `"<U+FEFF>"` + `REPORT`; stdout `""`; stderr exactly `> PLACE 0,0,NORTH`, `> ` + U+FEFF + `REPORT`, `Warning: '` + U+FEFF + `REPORT' ignored - unknown command` (U+FEFF is not whitespace, so it stays part of the command word; page 9 "Known limitations").
- [x] T012 [P] [US1] In `tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs`, test `Echo_removes_whitespace_of_any_kind_from_both_ends_only` ([WF-02][wf-02]; [FR-14][fr-14]; [DEC-08][dec-08], [DEC-25][dec-25], [DEC-31][dec-31] "echo content with redirected input"): input lines `"  \tPLACE 0 , 0 , NORTH<U+00A0>"`, `"<U+00A0>HELLO there\t"`, `" \t<U+00A0>"`, `"REPORT"`; stdout exactly `Output:`, `0,0,NORTH`; stderr exactly `> PLACE 0 , 0 , NORTH`, `> HELLO there`, `Warning: 'HELLO there' ignored - unknown command`, `> ` (`>` and a space), `Warning: blank line ignored`, `> REPORT`.
- [x] T013 [P] [US1] In `tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs`, test `CRLF_and_LF_input_give_identical_output` ([WF-03][wf-03]; [NFR-04][nfr-04]; [DEC-31][dec-31], [DEC-33][dec-33]): the lines `MOVE`, `PLACE 0,0,NORTH`, `HELLO`, `REPORT` joined once with `"\r\n"` and once with `"\n"`; both runs give the same stdout (`Output:`, `0,0,NORTH`) and the same stderr (`> MOVE`, `Warning: MOVE ignored - robot is not on the table`, `> PLACE 0,0,NORTH`, `> HELLO`, `Warning: 'HELLO' ignored - unknown command`, `> REPORT`), each compared exactly.
- [x] T014 [P] [US1] In `tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs`, test `Empty_input_writes_nothing_and_session_ends` (spec US2 scenario 2 in memory; [FR-16][fr-16]; [DEC-10][dec-10], [DEC-32][dec-32], [DEC-31][dec-31] "an empty input"): input `""`; `Run()` returns; stdout `""`; stderr `""`. (Placed here because this phase builds the Runner's end-of-input step.)
- [x] T015 [P] [US1] In `tests/ToyRobot.ConsoleEndToEndTests/TestDataTests.cs`, test `Every_testdata_input_gives_its_expected_console_output` ([E2E-01][e2e-01]; [FR-08][fr-08], [FR-12][fr-12], [NFR-06][nfr-06]; [DEC-23][dec-23], [DEC-31][dec-31] "the three challenge examples"; research [R-05][r-05]): one `[Fact]` that finds every `testdata/<name>.txt` under the repository root having a `testdata/<name>.expected.txt` (files ending `.expected.txt` are never inputs), runs each through `AppProcess.Run(File.ReadAllBytes(input))`, and records a mismatch when the exit code is not 0 or `LineComparer.Lines(stdout)` differs from `LineComparer.Lines(expected file text)`; it fails listing every mismatch by name. With no `testdata/` folder or no pair it passes (spec 003 [TD-02][td-02] checks the challenge example pairs).
- [x] T016 [P] [US1] In `tests/ToyRobot.ConsoleEndToEndTests/ByteOrderMarkTests.cs`, test `Input_starting_with_byte_order_mark_runs_challenge_example_1` ([E2E-05][e2e-05]; [FR-11][fr-11]; [DEC-20][dec-20], [DEC-26][dec-26], [DEC-31][dec-31] "input starting with a byte-order mark (console end-to-end, on all three systems)"): stdin bytes `EF BB BF` followed by `AppProcess.Utf8("PLACE 0,0,NORTH", "MOVE", "REPORT")`; stdout lines exactly `Output:`, `0,1,NORTH`; exit code 0.
- [x] T017 [P] [US1] In `tests/ToyRobot.ConsoleEndToEndTests/OutputChannelTests.cs`, tests ([E2E-06][e2e-06]; [FR-12][fr-12], [FR-08][fr-08]; [DEC-04][dec-04], [DEC-05][dec-05], [DEC-23][dec-23]): `Only_REPORT_result_on_console_output` with the [FR-07][fr-07] input `MOVE`, `PLACE 0,0,NORTH`, `REPORT`: stdout lines exactly `Output:`, `0,0,NORTH`; exit code 0. `Line_with_two_commands_gives_warning_and_exit_code_0` with input `PLACE 0,0,NORTH MOVE`: stdout no lines; exit code 0.
- [x] T018 [P] [US1] In `tests/ToyRobot.ConsoleEndToEndTests/StartUpTests.cs`, tests ([E2E-09][e2e-09]; [PG-01][pg-01]; [FR-08][fr-08], [FR-01][fr-01] in spec 001 "Program creates 5 x 5"; [DEC-03][dec-03], [DEC-26][dec-26]): `Redirected_REPORT_before_PLACE_writes_nothing_to_console_output`: input `REPORT`; stdout no lines; exit code 0. `Application_table_is_5_by_5`: input `PLACE 4,4,NORTH`, `REPORT`, `PLACE 5,0,NORTH`, `REPORT`, `PLACE 0,5,NORTH`, `REPORT`; stdout lines exactly `Output:`, `4,4,NORTH` three times; exit code 0 (the off-table PLACEs left the robot at 4,4).
- [x] T019 [P] [US1] In `tests/ToyRobot.ConsoleEndToEndTests/SessionEndTests.cs`, tests ([E2E-03][e2e-03] end of input, [E2E-04][e2e-04]; [FR-16][fr-16], [NFR-01][nfr-01]; [DEC-10][dec-10], [DEC-32][dec-32], [DEC-31][dec-31] "an empty input", "exit codes (console end-to-end)"): `Empty_input_gives_no_output_and_exit_code_0`: stdin no bytes; stdout `""`; exit code 0 (stderr empty stays proved by Runner whole-flow T014). `Exit_code_0_at_end_of_input`: input `PLACE 0,0,NORTH`, `MOVE`, `REPORT` with no EXIT; stdout lines exactly `Output:`, `0,1,NORTH`; exit code 0.
- [x] T020 [P] [US1] In `tests/ToyRobot.ConsoleEndToEndTests/LineEndingTests.cs`, test `CRLF_input_gives_same_console_output_as_LF` ([E2E-07][e2e-07]; [NFR-04][nfr-04]; [DEC-31][dec-31], [DEC-33][dec-33]): Challenge Example 1 as bytes `PLACE 0,0,NORTH\r\nMOVE\r\nREPORT\r\n` and as `AppProcess.Utf8(...)` with LF; both give stdout lines exactly `Output:`, `0,1,NORTH`, and exit code 0.

### Implementation for User Story 1

- [x] T020a [US1] In `tests/ToyRobot.Tests/Unit/DesignRulesTests.cs` (spec 001 T054), add `Runner` to the expected list of public top-level types, as spec 001 T054 asks ([NFR-07][nfr-07] in spec 001; [DEC-26][dec-26], [DEC-34][dec-34]). Fails until T021.
- [x] T021 [US1] Create the Runner in `src/ToyRobot/Session/Runner.cs` ([RN-01][rn-01]; page 6; contracts/runner.md; [DEC-20][dec-20], [DEC-21][dec-21], [DEC-23][dec-23], [DEC-25][dec-25], [DEC-26][dec-26], [DEC-27][dec-27], [DEC-29][dec-29]): constructor `Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)`, with the same visibility as spec 001's types (research [R-09][r-09]); it creates its own CommandParser. `Run()` (returns `void`) loops: read a line with `input.ReadLine()` and return when it is `null` ([FR-16][fr-16]); on the first line only, remove one leading `'<U+FEFF>'` ([FR-11][fr-11]); when not `interactive`, `error.WriteLine("> " + line.Trim())` ([FR-14][fr-14]); pass the untrimmed line to `parser.Parse(line)`; for `ParseResult.Failure f`, `error.WriteLine("Warning: " + f.Reason)`; for `ParseResult.CommandObject c`, `robotOperator.Run(c.Command)`, and for its `Outcome`: `Outcome.PositionText p` → `output.WriteLine("Output:")` then `output.WriteLine(p.Text)` ([FR-12][fr-12]); `Outcome.Warning w` → `error.WriteLine("Warning: " + w.Text)`; `Outcome.Done` → nothing. The Runner never uses `Console`, holds no robot rule or table size, never passes a failure to RobotOperator, and has no catch-all (research [R-07][r-07]). The EXIT branch is added in T030 and the prompt in T036, each after its tests.
- [x] T022 [US1] Replace the skeleton entry point in `src/ToyRobot/Program.cs` with `internal static class Program` (not public, so the public type list in spec 001's DesignRulesTests stays exact; research [R-09][r-09]) and `static int Main(string[] args)` for the no-argument path ([PG-01][pg-01]; page 6; page 7.7; [DEC-03][dec-03], [DEC-24][dec-24], [DEC-26][dec-26], [DEC-29][dec-29]; research [R-01][r-01], [R-02][r-02]): `var table = new Table(5, 5)`; `var robotOperator = new RobotOperator(table)`; `bool interactive = !Console.IsInputRedirected`; reader = `interactive ? Console.In : new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: false)`; `new Runner(reader, Console.Out, Console.Error, robotOperator, interactive).Run()`; `return 0`. Program does not read or parse commands, does not set `Console.OutputEncoding` or `Console.InputEncoding`, and does not handle Ctrl+C. The argument check is added in T040 after its tests.
- [x] T023 [US1] Run `dotnet build` and `dotnet test` from the repository root: all tests pass (spec 001's unit tests, T008 to T020). ([NFR-06][nfr-06]; [DEC-31][dec-31])

**Checkpoint**: a file of commands runs end to end with the right output on each channel; MVP.

---

## Phase 4: User Story 2 - End the session (Priority: P1)

**Goal**: EXIT, in any case and with anything after it, ends the session at once with exit code 0, even before PLACE; later lines are neither read, echoed nor run; `EXITNOW` is an unknown command ([FR-15][fr-15], [FR-14][fr-14] for the EXIT line; [FR-16][fr-16]'s end-of-input path was built and tested in Phase 3).

**Independent Test**: the Runner whole-flow tests below with `interactive` = false, and the console end-to-end exit-code tests.

### Tests for User Story 2 (write first; T024 to T026, T028 and T029 fail until T030; T027 passes already, because `EXITNOW` and `EXITING` are parse failures handled since T021, and it guards that T030 stops only on EXIT)

- [x] T024 [P] [US2] In `tests/ToyRobot.Tests/RunnerWholeFlow/EndOfSessionTests.cs`, test `EXIT_with_text_after_it_ends_session` (spec US2 scenario 1; [FR-15][fr-15]; [DEC-10][dec-10], [DEC-30][dec-30], [DEC-31][dec-31] "`EXIT now` exits"): input `exit now`, `REPORT`; stdout `""`; stderr exactly `> exit now`; `Run()` returns.
- [x] T025 [P] [US2] In `tests/ToyRobot.Tests/RunnerWholeFlow/EndOfSessionTests.cs`, test `EXIT_mid_file_stops_later_lines` (spec US2 scenario 3; [FR-15][fr-15]; [DEC-10][dec-10], [DEC-25][dec-25], [DEC-31][dec-31] "mid-file (later lines neither echoed nor run)"): input `PLACE 0,0,NORTH`, `REPORT`, `EXIT`, `MOVE`, `REPORT`; stdout exactly `Output:`, `0,0,NORTH`; stderr exactly `> PLACE 0,0,NORTH`, `> REPORT`, `> EXIT`.
- [x] T026 [P] [US2] In `tests/ToyRobot.Tests/RunnerWholeFlow/EndOfSessionTests.cs`, theory `EXIT_before_PLACE_ends_session` (spec US2 scenario 4; [FR-15][fr-15]; [DEC-10][dec-10], [DEC-30][dec-30], [DEC-31][dec-31] "EXIT before PLACE", "`EXIT now` exits") with rows `EXIT`, `EXIT now`, `Exit`, `exit`: input the row's line, then `PLACE 0,0,NORTH`, `REPORT`; stdout `""`; stderr exactly `> ` + the row's line (no warning, so RobotOperator never received EXIT).
- [x] T027 [P] [US2] In `tests/ToyRobot.Tests/RunnerWholeFlow/EndOfSessionTests.cs`, theory `EXITNOW_and_EXITING_are_unknown_commands_and_session_continues` (spec US2 scenario 5; [FR-15][fr-15], [FR-10][fr-10] in spec 001; [DEC-10][dec-10], [DEC-22][dec-22], [DEC-31][dec-31] "`EXITNOW` warns") with rows `EXITNOW` and `EXITING`: input the row, then `REPORT`; stdout `""`; stderr exactly `> <row>`, `Warning: '<row>' ignored - unknown command`, `> REPORT`, `Warning: REPORT ignored - robot is not on the table`.
- [x] T028 [P] [US2] In `tests/ToyRobot.Tests/RunnerWholeFlow/EndOfSessionTests.cs`, test `Echo_includes_trimmed_line_blank_line_and_EXIT_but_not_lines_after_EXIT` (spec US1 scenario 3, placed here because its input contains EXIT; [FR-14][fr-14]; [DEC-25][dec-25]): input lines `"  MOVE  "`, `""`, `"EXIT"`, `"REPORT"`; stdout `""`; stderr exactly `> MOVE`, `Warning: MOVE ignored - robot is not on the table`, `> ` (`>` and a space), `Warning: blank line ignored`, `> EXIT`.
- [x] T029 [P] [US2] In `tests/ToyRobot.ConsoleEndToEndTests/SessionEndTests.cs`, tests ([E2E-03][e2e-03]; [FR-15][fr-15]; [DEC-10][dec-10], [DEC-31][dec-31] "exit codes (console end-to-end)", "mid-file"): `Exit_code_0_after_EXIT`: input `EXIT`, `PLACE 0,0,NORTH`, `REPORT`; stdout no lines; exit code 0. `EXIT_mid_file_later_lines_not_run`: input `PLACE 0,0,NORTH`, `REPORT`, `EXIT`, `MOVE`, `REPORT`; stdout lines exactly `Output:`, `0,0,NORTH`; exit code 0 (stdout and exit code show that the later lines are not run).

### Implementation for User Story 2

- [x] T030 [US2] In `src/ToyRobot/Session/Runner.cs`, add the EXIT branch to `Run()` ([RN-01][rn-01] step 5; [FR-15][fr-15]; [DEC-10][dec-10], [DEC-30][dec-30]): when `Parse` returns `ParseResult.Exit`, return at once, writing nothing and passing nothing to RobotOperator; no further line is read.
- [x] T031 [US2] Run `dotnet build` and `dotnet test` from the repository root: all tests pass. ([NFR-06][nfr-06]; [DEC-31][dec-31])

**Checkpoint**: every way a redirected session ends is proved in memory and end to end.

---

## Phase 5: User Story 3 - Type commands at the prompt (Priority: P2)

**Goal**: when a person is typing (`interactive` = true), `> ` is written to console messages (stderr) before each read, including the read that finds the end of the input, and typed lines are not echoed; with redirected input there is no prompt ([FR-13][fr-13]; [DEC-24][dec-24], [DEC-10][dec-10]).

**Independent Test**: Runner whole-flow tests with `interactive` = true (a console end-to-end test cannot make console input look typed, page 10).

### Tests for User Story 3 (write first; T032 to T034 fail until T036; T035 passes already and guards T036)

- [x] T032 [P] [US3] In `tests/ToyRobot.Tests/RunnerWholeFlow/PromptTests.cs`, test `Prompt_before_each_read_and_typed_lines_not_echoed` (spec US3 scenario 1; [FR-13][fr-13]; [DEC-24][dec-24]): `SessionHarness.Run("MOVE\nEXIT\n", interactive: true)`; stdout `""`; stderr exactly `"> Warning: MOVE ignored - robot is not on the table\n> "`.
- [x] T033 [P] [US3] In `tests/ToyRobot.Tests/RunnerWholeFlow/PromptTests.cs`, test `Prompt_also_shown_before_the_read_that_finds_end_of_input` (spec US3 scenario 2; [FR-13][fr-13], [FR-16][fr-16]; [DEC-10][dec-10], [DEC-24][dec-24], [DEC-31][dec-31] "prompt shown when interactive"): `SessionHarness.Run("PLACE 0,0,NORTH\nREPORT\n", interactive: true)`; stdout exactly `"Output:\n0,0,NORTH\n"`; stderr exactly `"> > > "`.
- [x] T034 [P] [US3] In `tests/ToyRobot.Tests/RunnerWholeFlow/PromptTests.cs`, test `Empty_typed_input_shows_one_prompt` (contracts/runner.md "Guarantees"; [FR-13][fr-13], [FR-16][fr-16]; [DEC-10][dec-10], [DEC-24][dec-24]): `SessionHarness.Run("", interactive: true)`; stdout `""`; stderr exactly `"> "`.
- [x] T035 [P] [US3] In `tests/ToyRobot.Tests/RunnerWholeFlow/PromptTests.cs`, test `No_prompt_when_input_is_redirected` (spec US3 scenario 3; [FR-13][fr-13], [FR-14][fr-14]; [DEC-24][dec-24], [DEC-25][dec-25], [DEC-31][dec-31] "and not otherwise"): `SessionHarness.Run("MOVE\nEXIT\n", interactive: false)`; stdout `""`; stderr exactly `"> MOVE\nWarning: MOVE ignored - robot is not on the table\n> EXIT\n"` (every `> ` is followed by the line read). This test passes before T036 and guards that T036 adds the prompt only when interactive.

### Implementation for User Story 3

- [x] T036 [US3] In `src/ToyRobot/Session/Runner.cs`, add step 1 to `Run()` ([RN-01][rn-01]; [FR-13][fr-13]; [DEC-24][dec-24]): when `interactive`, `error.Write("> ")` (no line break) before each `input.ReadLine()`, including the read that returns `null`; typed lines are not echoed (the echo stays only for `interactive` = false).
- [x] T037 [US3] Run `dotnet build` and `dotnet test` from the repository root: all tests pass. ([NFR-06][nfr-06]; [DEC-31][dec-31])

**Checkpoint**: both typed and redirected sessions behave as [DEC-24][dec-24] and [DEC-25][dec-25] say.

---

## Phase 6: User Story 4 - Started the wrong way (Priority: P2)

**Goal**: with any argument, Program writes the how-to-run message as one line to console messages (stderr), nothing to console output (stdout), reads no input, and returns exit code 1 ([FR-17][fr-17]; [DEC-04][dec-04]).

**Independent Test**: console end-to-end tests (only Program sees the arguments and sets the exit code, page 10).

### Tests for User Story 4 (write first; they fail until T040)

- [x] T038 [P] [US4] In `tests/ToyRobot.ConsoleEndToEndTests/HowToRunMessageTests.cs`, theory `How_to_run_message_and_exit_code_1_when_started_with_an_argument` ([E2E-02][e2e-02]; [FR-17][fr-17]; [DEC-04][dec-04], [DEC-31][dec-31] "the how-to-run message and exit codes (console end-to-end)") with rows `--help`, `commands.txt`, and the two arguments `a`, `b`: stdin no bytes; stdout `""`; stderr lines exactly one line, `How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)` (three spaces before the bracket); exit code 1.
- [x] T039 [P] [US4] In `tests/ToyRobot.ConsoleEndToEndTests/HowToRunMessageTests.cs`, test `Input_is_not_read_when_started_with_an_argument` ([E2E-08][e2e-08]; [FR-17][fr-17]; [DEC-04][dec-04]): argument `--help`, stdin `AppProcess.Utf8("PLACE 0,0,NORTH", "REPORT")`; stdout `""`; stderr lines exactly the how-to-run message line (no echo); exit code 1.

### Implementation for User Story 4

- [x] T040 [US4] In `src/ToyRobot/Program.cs`, add the argument check at the start of `Main` ([PG-01][pg-01]; [FR-17][fr-17]; [DEC-04][dec-04], [DEC-26][dec-26]): `if (args.Length > 0) { Console.Error.WriteLine("How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)"); return 1; }`, before anything touches console input or creates the Table, RobotOperator or Runner.
- [x] T041 [US4] Run `dotnet build` and `dotnet test` from the repository root: all tests pass. ([NFR-06][nfr-06]; [DEC-31][dec-31])

**Checkpoint**: Program is complete; every exit code in page 9 that the application sets is proved end to end.

---

## Phase 7: User Story 5 - Bad input never stops the session (Priority: P2)

**Goal**: prove that no input stops the session or crashes the application, that the whole flow works on any table size, and that every spec 001 scenario holds as console text ([NFR-01][nfr-01], [NFR-05][nfr-05], [NFR-06][nfr-06], [WF-01][wf-01]; [DEC-03][dec-03], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-29][dec-29], [DEC-31][dec-31], [DEC-32][dec-32]).

**Independent Test**: the Runner whole-flow and console end-to-end tests below. They need no new production code: they test the Runner, Program and spec 001's components together. If one fails, the cause is fixed in the code (or in a task, spec or plan first if that is where it is wrong; brief step 9.3), never by changing the expected text.

### Tests for User Story 5

- [x] T042 [P] [US5] In `tests/ToyRobot.Tests/RunnerWholeFlow/NeverCrashTests.cs`, test `Bad_lines_never_stop_the_session` (spec US5 scenario 1; [NFR-01][nfr-01]; [DEC-20][dec-20], [DEC-22][dec-22], [DEC-32][dec-32]): input `PLACE 99999999999,0,NORTH`, `HELLO`, `""`, `PLACE 0,0,NORTH`, `REPORT`; stdout exactly `Output:`, `0,0,NORTH`; stderr exactly `> PLACE 99999999999,0,NORTH`, `Warning: 'PLACE 99999999999,0,NORTH' ignored - X is too large`, `> HELLO`, `Warning: 'HELLO' ignored - unknown command`, `> `, `Warning: blank line ignored`, `> PLACE 0,0,NORTH`, `> REPORT`.
- [x] T043 [P] [US5] In `tests/ToyRobot.Tests/RunnerWholeFlow/NeverCrashTests.cs`, test `Every_kind_of_ignored_line_then_valid_commands_still_work` ([NFR-01][nfr-01]; [DEC-19][dec-19], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-32][dec-32], [DEC-31][dec-31] "every warning in [DEC-22][dec-22] with its exact text"): input `HELLO`, `""`, `PLACE 1,2`, `PLACE A,0,NORTH`, `PLACE 0,B,NORTH`, `PLACE 1,2,UP`, `move now`, `PLACE 99999999999,0,NORTH`, `> MOVE`, `MOVE`, `PLACE 9,9,NORTH`, `PLACE 0,0,SOUTH`, `MOVE`, `REPORT`; stdout exactly `Output:`, `0,0,SOUTH`; stderr exactly: `> HELLO`, `Warning: 'HELLO' ignored - unknown command`, `> `, `Warning: blank line ignored`, `> PLACE 1,2`, `Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F`, `> PLACE A,0,NORTH`, `Warning: 'PLACE A,0,NORTH' ignored - X must be a whole number`, `> PLACE 0,B,NORTH`, `Warning: 'PLACE 0,B,NORTH' ignored - Y must be a whole number`, `> PLACE 1,2,UP`, `Warning: 'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST`, `> move now`, `Warning: 'move now' ignored - MOVE takes no values`, `> PLACE 99999999999,0,NORTH`, `Warning: 'PLACE 99999999999,0,NORTH' ignored - X is too large`, `> > MOVE`, `Warning: '> MOVE' ignored - unknown command`, `> MOVE`, `Warning: MOVE ignored - robot is not on the table`, `> PLACE 9,9,NORTH`, `Warning: PLACE 9,9,NORTH ignored - position is off the table`, `> PLACE 0,0,SOUTH`, `> MOVE`, `Warning: MOVE ignored - next position is off the table so robot stays at 0,0,SOUTH`, `> REPORT`.
- [x] T044 [P] [US5] In `tests/ToyRobot.Tests/RunnerWholeFlow/SmallTableTests.cs`, test `Whole_flow_on_a_2_by_2_table` (spec US5 scenario 2; [NFR-05][nfr-05]; [DEC-03][dec-03], [DEC-29][dec-29], [DEC-31][dec-31] "a small table such as 2 x 2"): `SessionHarness.Run(..., width: 2, height: 2)` with input `PLACE 1,1,NORTH`, `MOVE`, `PLACE 2,2,NORTH`, `REPORT`; stdout exactly `Output:`, `1,1,NORTH`; stderr exactly `> PLACE 1,1,NORTH`, `> MOVE`, `Warning: MOVE ignored - next position is off the table so robot stays at 1,1,NORTH`, `> PLACE 2,2,NORTH`, `Warning: PLACE 2,2,NORTH ignored - position is off the table`, `> REPORT`.
- [x] T045 [P] [US5] In `tests/ToyRobot.Tests/RunnerWholeFlow/RobotRulesAsConsoleTextTests.cs`, add one Runner whole-flow test (or one theory row) per acceptance scenario of `specs/001-robot-rules/spec.md`, with its exact input, console output (stdout) and console messages (stderr), on a 5 x 5 table with `interactive` = false ([WF-01][wf-01], spec US5 scenario 3; [NFR-05][nfr-05], [NFR-06][nfr-06]; [FR-01][fr-01] to [FR-07][fr-07], [FR-09][fr-09], [FR-10][fr-10]; [DEC-31][dec-31] "Runner whole-flow tests covering every FR as console text"): User Story 1 scenarios 1 to 8; User Story 2 scenarios 1 to 11 (scenario 8: one row per edge and corner row); User Story 3 scenarios 1 to 3; User Story 4 scenarios 1 to 7 (scenarios 3, 4 and 7: one row per table row). Spec 001 User Story 1 scenario 9 (2 x 2 Table) is covered by T044 and User Story 4 scenario 8 (EXIT parsed) by T024 to T028. Test names use the challenge wording where possible, for example `Robot_not_on_table_ignores_MOVE` ([DEC-31][dec-31]).
- [x] T046 [P] [US5] In `tests/ToyRobot.ConsoleEndToEndTests/OutputChannelTests.cs`, test `Bad_lines_never_stop_the_application` ([E2E-06][e2e-06] [NFR-01][nfr-01] part; [NFR-01][nfr-01]; [DEC-32][dec-32]): the input of T042 through `AppProcess`; stdout lines exactly `Output:`, `0,0,NORTH`; exit code 0. Also test `Every_kind_of_ignored_line_then_valid_commands_end_to_end` with the input of T043: stdout lines exactly `Output:`, `0,0,SOUTH`; exit code 0.
- [x] T047 [US5] Run `dotnet build` and `dotnet test` from the repository root: all tests pass. Any failure is fixed in the production code or, where a task or the spec is wrong, there first (brief step 9.3); no expected text is changed to match the code (brief rule 10). ([NFR-01][nfr-01], [NFR-06][nfr-06]; [DEC-31][dec-31], [DEC-32][dec-32])

**Checkpoint**: all five user stories are proved; the application never stops on bad input.

---

## Phase 8: Polish and cross-cutting checks

**Purpose**: check the result against [DEC-31][dec-31]'s list, page 6 and the exact strings; no new behaviour.

- [x] T048 Compare the test list with [DEC-31][dec-31]'s "Must be covered" list using the coverage table below ([SC-006][sc-006], [NFR-06][nfr-06]; [DEC-31][dec-31]); every row must point to at least one existing, passing test. Add any missing test (tests first) before continuing.
- [x] T049 [P] Review `src/ToyRobot/Session/Runner.cs` and `src/ToyRobot/Program.cs` against page 6 and [DEC-26][dec-26] ([NFR-07][nfr-07] in spec 001; [DEC-34][dec-34]): names and members exactly as page 6 and [DEC-29][dec-29]; the Runner never uses `Console`, knows no robot rule or table size, never passes EXIT or a failure to RobotOperator; Program does not read or parse commands, apply rules, change the console's encoding settings or handle Ctrl+C; no library, dependency injection container or logging framework added ([DEC-39][dec-39]).
- [x] T050 [P] Check by search that the how-to-run message, `Output:`, the prompt `> `, the echo prefix `> ` and the `Warning: ` prefix in `src/ToyRobot/Session/Runner.cs`, `src/ToyRobot/Program.cs` and in the tests match [DEC-04][dec-04], [DEC-21][dec-21], [DEC-22][dec-22], [DEC-24][dec-24] and [DEC-25][dec-25] character for character (brief rule 2; [SC-003][sc-003]), and that no code in this spec depends on the machine's regional settings ([NFR-04][nfr-04]; [DEC-08][dec-08]).
- [x] T051 Run `dotnet build`, `dotnet test` and `dotnet test -c Release` from the repository root: all pass in both configurations ([NFR-04][nfr-04], [NFR-06][nfr-06]; [DEC-31][dec-31]; research [R-04][r-04]).
- [x] T052 Follow `specs/002-console-session/quickstart.md` "Try it by hand", including the typed session ([FR-13][fr-13]; [DEC-10][dec-10], [DEC-24][dec-24]) and Ctrl+Z / Ctrl+D ([FR-16][fr-16]), which no automated test can type; every expected result matches. Done by the developer on Windows, 5 October 2026: typed sessions in cmd and PowerShell, ended with `EXIT` and with Ctrl+Z then Enter; Ctrl+D on Linux or macOS was not tried by hand.

### DEC-31 coverage for this spec's levels (T048)

| [DEC-31][dec-31] "Must be covered" item | Tests (this spec) | Also in spec 001 (unit) / spec 003 |
|---|---|---|
| the three challenge examples | T045 (spec 001 US2 s1, s3, s4), T016, T020, T015 over `testdata/` | unit (spec 001); test-data pairs and CI script runs (spec 003) |
| hand-traced Brainstorm examples | T045 (spec 001 US2 s2, s5, s6, s7) | unit (spec 001) |
| every edge and corner; full LEFT and RIGHT cycles | T045 (spec 001 US2 s8 to s11) | unit (spec 001) |
| a small table such as 2 x 2 | T044 | unit (spec 001) |
| MOVE, LEFT, RIGHT, REPORT each before PLACE; invalid first PLACE; invalid later PLACE; second valid PLACE; negative position; several REPORTs | T045 (spec 001 US1 s2, s4 to s8; US3 s1 to s3), T043 | unit (spec 001) |
| case-insensitivity, REPORT upper case after lower-case input | T045 (spec 001 US1 s3, US3 s3, US4 s1, s6) | unit (spec 001) |
| every row of the [DEC-08][dec-08] table; tabs and non-breaking spaces | T045 (spec 001 US4 s2 to s5), T012 | unit (spec 001) |
| every warning in [DEC-22][dec-22] with exact text (incl. `> MOVE`, `MOVE now`, `PLACEHOLDER`, `PLACE 99999999999,0,NORTH`, `PLACE 0,0,N`, blank lines, off-table with parsed values) | T045 (spec 001 US1 s7, US4 s3, s7), T042, T043, T008, T027 | unit (spec 001) |
| EXIT before PLACE, mid-file, `EXIT now` exits, `EXITNOW` warns | T024 to T029 | parser unit tests (spec 001) |
| prompt shown when interactive and not otherwise; echo content | T032 to T035, T012, T028 | — |
| an empty input | T014, T019 | spec 003 script run |
| input starting with a byte-order mark (console end-to-end, all three systems) | T016 (console end-to-end, stdout), T010, T011 (Runner whole-flow, both channels) | CI on three systems (spec 003) |
| the how-to-run message and exit codes (console end-to-end) | T038, T039, T019, T029 | script exit codes (spec 003) |
| the unexpected-problem line and exit code 2 | T053 (in memory, through Program's `Run`) | — |

---

## Phase 9: Safeguard for unexpected problems (DEC-32)

**Goal**: Program's `Run` turns any exception that escapes its work into exactly one line, `Error: unexpected problem - <exception message>`, on console messages (stderr) and exit code 2, even when that line cannot be written; `Main` stays the thin wiring of the real console ([PG-01][pg-01], [PG-02][pg-02]; [NFR-01][nfr-01], [FR-17][fr-17]; [DEC-23][dec-23], [DEC-26][dec-26], [DEC-32][dec-32]).

**Independent Test**: the [SG-01][sg-01] tests below call Program's internal `Run` in memory with a reader or writer that fails on purpose (`IOException` with the message `boom`), and with the normal inputs.

- [x] T053 [US5] Write the safeguard tests in `tests/ToyRobot.Tests/RunnerWholeFlow/ProgramSafeguardTests.cs` (namespace `ToyRobot.Tests.RunnerWholeFlow`; [SG-01][sg-01], [PG-02][pg-02]; [NFR-01][nfr-01]; [DEC-23][dec-23], [DEC-26][dec-26], [DEC-32][dec-32]; spec.md User Story 5 scenarios 4 to 6), calling `Program.Run(args, input, output, error, interactive)` with no arguments and `interactive: false`. In the same file add the test helper `FailingReader` (a `TextReader` whose `ReadLine` throws `IOException("boom")`) and the test helper `FailingWriter` (a `TextWriter` whose every write throws `IOException("boom")`); the other writers are `StringWriter`s with `NewLine = "\n"`. Tests:
  (a) `Unexpected_problem_while_reading_gives_one_error_line_and_exit_code_2`: input `FailingReader` → stdout `""`; stderr exactly `Error: unexpected problem - boom` followed by `"\n"` (one line, nothing written after it); exit code 2.
  (b) `Unexpected_problem_while_writing_console_output_gives_one_error_line_and_exit_code_2`: input `PLACE 0,0,NORTH`, `REPORT`; output `FailingWriter` → stderr exactly the lines `> PLACE 0,0,NORTH`, `> REPORT`, `Error: unexpected problem - boom`; exit code 2.
  (c) `Unexpected_problem_when_the_error_line_cannot_be_written_still_gives_exit_code_2`: input `PLACE 0,0,NORTH`, `REPORT`; output and error both `FailingWriter` → `Run` returns exit code 2 and does not throw.
  They do not compile until T055 adds `Run` (red).
- [x] T054 [US4] [US5] In the same file, write the tests of the existing behaviour through `Program.Run` ([SG-01][sg-01], [PG-01][pg-01]; [FR-17][fr-17], [FR-16][fr-16], [NFR-01][nfr-01]; [DEC-04][dec-04], [DEC-10][dec-10], [DEC-26][dec-26]):
  (a) `Run_with_an_argument_gives_the_how_to_run_message_and_exit_code_1`: args `--help`, input a `StringReader` holding `PLACE 0,0,NORTH` → stdout `""`; stderr exactly `How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)` followed by `"\n"`; exit code 1; the input was not read (its next `ReadLine` still gives `PLACE 0,0,NORTH`).
  (b) `Run_with_Challenge_Example_1_gives_its_output_and_exit_code_0`: input `PLACE 0,0,NORTH`, `MOVE`, `REPORT`, `interactive: false` → stdout exactly `Output:`, `0,1,NORTH`; stderr exactly `> PLACE 0,0,NORTH`, `> MOVE`, `> REPORT`; exit code 0.
  They do not compile until T055 (red).
- [x] T055 Implement the safeguard in `src/ToyRobot/Program.cs` and `src/ToyRobot/ToyRobot.csproj` ([PG-01][pg-01], [PG-02][pg-02]; [NFR-01][nfr-01], [FR-17][fr-17]; [DEC-23][dec-23], [DEC-26][dec-26], [DEC-32][dec-32]; plan "Program"): add an `ItemGroup` with `<InternalsVisibleTo Include="ToyRobot.Tests" />` to the project file (an MSBuild item, no library or package; Program stays `internal static`, so spec 001's DesignRulesTests list is unchanged); add `internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)` that does the argument check (how-to-run message on `error`, return 1), creates `Table(5, 5)`, `RobotOperator(table)` and the Runner over `input`, `output` and `error`, runs it and returns 0, all inside one `try`; its `catch (Exception exception)` writes `Error: unexpected problem - ` and the exception's `Message` as one line to `error`, ignoring an exception from that write, and returns 2. `Main` keeps only the console wiring: it decides `interactive`, chooses the reader as before and returns `Run(args, reader, Console.Out, Console.Error, interactive)`. The Runner is not changed. T053 and T054 pass.
- [x] T056 Run `dotnet build ToyRobot.sln` (0 warnings) and `dotnet test ToyRobot.sln` from the repository root: all pass, including every earlier Runner whole-flow and console end-to-end test; run the built application by hand from cmd with `src\ToyRobot\bin\Debug\net8.0\ToyRobot.exe < testdata\challenge-example-1.txt`: the same output as before and exit code 0 ([NFR-01][nfr-01], [NFR-06][nfr-06]; [DEC-31][dec-31], [DEC-32][dec-32]).
- [x] T057 Review `src/ToyRobot/Program.cs` against page 6 and [PG-01][pg-01], [PG-02][pg-02] ([NFR-07][nfr-07] in spec 001; [DEC-26][dec-26], [DEC-32][dec-32], [DEC-34][dec-34]): the only `catch` in the application is in Program's `Run`; `Main` holds only the console wiring; the error line matches [DEC-23][dec-23] and [DEC-32][dec-32] character for character; Ctrl+C is still not handled ([DEC-10][dec-10]).

---

## Phase 10: Ctrl+Z line ends the input (DEC-10)

**Goal**: a line that is exactly the single character Ctrl+Z (U+001A), as the Windows console gives Ctrl+Z then Enter, ends the input like the end of a file ([FR-16][fr-16]; [DEC-10][dec-10]).

**Independent Test**: Runner whole-flow tests in memory, typed and redirected.

- [x] T058 [US2] Write the tests first in `tests/ToyRobot.Tests/RunnerWholeFlow/EndOfSessionTests.cs` (spec.md User Story 2 scenario 6 and User Story 3 scenario 4; [FR-16][fr-16]; [DEC-10][dec-10]), feeding `PLACE 0,0,NORTH`, `REPORT`, `"\u001A"`, `MOVE`, `REPORT`: with `interactive: false` stdout is `Output:` / `0,0,NORTH` and stderr only the two echo lines; with `interactive: true` stdout is the same and stderr is `> > > `. Confirm they fail (red), then add the check to `Runner.Run` in `src/ToyRobot/Session/Runner.cs`, next to the end-of-input check, so that a line equal to `"\u001A"` returns before the echo and before parsing; run `dotnet build` and `dotnet test` from the repository root: all pass.

---

## Phase 11: Names of the two test levels (DEC-31)

**Goal**: the two test levels have self-explanatory names: "Runner whole-flow tests" (in memory, from the Runner inward) and "console end-to-end tests" (the real built application as a separate process) ([DEC-31][dec-31]).

**Independent Test**: `dotnet build` (0 warnings) and `dotnet test` from the repository root: all tests pass, with the same totals as before the change.

- [x] T059 Rename the test folder and project ([NFR-06][nfr-06]; [DEC-31][dec-31]): with `git mv`, move `tests/ToyRobot.Tests/WholeFlow/` to `tests/ToyRobot.Tests/RunnerWholeFlow/` (namespace `ToyRobot.Tests.RunnerWholeFlow`) and `tests/ToyRobot.EndToEndTests/` to `tests/ToyRobot.ConsoleEndToEndTests/` (project file `ToyRobot.ConsoleEndToEndTests.csproj`, namespace `ToyRobot.ConsoleEndToEndTests`); update `ToyRobot.sln`, every `using`, the CI workflow step names and test command, and the paths and names in the docs and the Spec Kit files. No test is changed otherwise. Run `dotnet build` (0 warnings), `dotnet build -c Release` and `dotnet test` from the repository root: all pass with the same totals.

---

## Dependencies & Execution Order

### Dependencies on other specs

- **Spec 001-robot-rules** must be implemented first, with all its tests passing: the solution skeleton, the three projects, and the types the Runner and Program use (T001).
- **Spec 003-delivery** is implemented after this spec. It relies on: the console end-to-end test project and `AppProcess` (T005), [E2E-01][e2e-01] (T015), which runs the test-data pairs spec 003 adds, and the complete application (T021, T022, T030, T036, T040).

### Phase dependencies

- Phase 1 (Setup) → Phase 2 (Foundational) → Phase 3 (US1) → Phase 4 (US2) → Phase 5 (US3) → Phase 6 (US4) → Phase 7 (US5) → Phase 8 (Polish) → Phase 9 (Safeguard, [DEC-32][dec-32]).
- The stories build one class step by step (the Runner's steps 2 to 5 in US1, EXIT in US2, the prompt in US3; Program's no-argument path in US1 and its argument check in US4), so the phases run in this order; each ends with every test passing.

### Within each phase

- Test tasks first, then the implementation task, then the build-and-test task.
- `SessionHarness` (T007) comes before every Runner whole-flow test; `LineComparer` and `AppProcess` (T004, T005) before every console end-to-end test.

### Parallel opportunities

- T004 and T005.
- In Phase 3: T008 to T014 (one file, written together as a set) in parallel with T015 to T020 (separate files).
- In Phase 4: T024 to T028 with T029. In Phase 5: T032 to T035. In Phase 6: T038 with T039 (same file; written together). In Phase 7: T042 to T046.
- In Phase 8: T049 with T050.
- In Phase 9: T053 and T054 are written together (one file), before T055.

## Parallel Example: User Story 1

```text
Task: "T008-T014 Runner whole-flow tests in tests/ToyRobot.Tests/RunnerWholeFlow/RedirectedInputTests.cs"
Task: "T015 E2E-01 in tests/ToyRobot.ConsoleEndToEndTests/TestDataTests.cs"
Task: "T016 E2E-05 in tests/ToyRobot.ConsoleEndToEndTests/ByteOrderMarkTests.cs"
Task: "T017 E2E-06 in tests/ToyRobot.ConsoleEndToEndTests/OutputChannelTests.cs"
Task: "T018 start-up checks in tests/ToyRobot.ConsoleEndToEndTests/StartUpTests.cs"
Task: "T019 E2E-03/E2E-04 in tests/ToyRobot.ConsoleEndToEndTests/SessionEndTests.cs"
Task: "T020 E2E-07 in tests/ToyRobot.ConsoleEndToEndTests/LineEndingTests.cs"
```

## Implementation Strategy

### MVP first (User Story 1)

1. Phases 1 and 2.
2. Phase 3: a redirected file runs end to end with the right channels and exit code 0.
3. Stop and check: `dotnet test` passes; `ToyRobot < commands.txt` behaves as quickstart.md says.

### Incremental delivery

Each later phase adds one Runner or Program step after its tests (EXIT, prompt, arguments) or adds only tests (US5, Polish), and ends with all tests passing, so every phase is a safe stopping point.

## Notes

- [P] tasks are in different files or are tests written together with no dependency on an unfinished task.
- Commit after each phase (or smaller), naming the task IDs (brief step 9.2).
- Never weaken a test or change an expected text to match the code (brief rule 10; constitution principle II).

[dec-03]: ../../docs/design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-04]: ../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-05]: ../../docs/design-analysis/design-decisions.md#dec-05-one-command-per-line
[dec-08]: ../../docs/design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-10]: ../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-19]: ../../docs/design-analysis/design-decisions.md#dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-24]: ../../docs/design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../../docs/design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-26]: ../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../../docs/design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-29]: ../../docs/design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-30]: ../../docs/design-analysis/design-decisions.md#dec-30-exit-is-handled-by-the-runner
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../../docs/design-analysis/design-decisions.md#dec-33-cross-platform
[dec-34]: ../../docs/design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[e2e-01]: spec.md#e2e-01
[e2e-02]: spec.md#e2e-02
[e2e-03]: spec.md#e2e-03
[e2e-04]: spec.md#e2e-04
[e2e-05]: spec.md#e2e-05
[e2e-06]: spec.md#e2e-06
[e2e-07]: spec.md#e2e-07
[e2e-08]: spec.md#e2e-08
[e2e-09]: spec.md#e2e-09
[fr-01]: ../../docs/solution-design/02-functional-requirements.md#fr-01
[fr-07]: ../../docs/solution-design/02-functional-requirements.md#fr-07
[fr-08]: spec.md#fr-08
[fr-09]: ../../docs/solution-design/02-functional-requirements.md#fr-09
[fr-10]: ../../docs/solution-design/02-functional-requirements.md#fr-10
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
[nfr-07]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-07
[pg-01]: spec.md#pg-01
[pg-02]: spec.md#pg-02
[r-01]: research.md#r-01-runner-method-and-program-entry-point-decided
[r-02]: research.md#r-02-the-utf-8-reader-for-redirected-input-dec-20-dec-26
[r-04]: research.md#r-04-how-the-console-end-to-end-tests-start-the-built-application-decided
[r-05]: research.md#r-05-e2e-01-with-no-test-data-yet-decided
[r-06]: research.md#r-06-exact-comparison-in-runner-whole-flow-tests-nfr-04-nfr-05
[r-07]: research.md#r-07-a-last-safeguard-in-program-no-catch-all-in-the-runner-decided-dec-32
[r-09]: research.md#r-09-visibility-of-runner-and-program
[rn-01]: spec.md#rn-01
[sc-003]: spec.md#sc-003
[sc-006]: spec.md#sc-006
[sg-01]: spec.md#sg-01
[sk-01]: ../001-robot-rules/spec.md#sk-01
[td-02]: ../003-delivery/spec.md#td-02
[wf-01]: spec.md#wf-01
[wf-02]: spec.md#wf-02
[wf-03]: spec.md#wf-03
