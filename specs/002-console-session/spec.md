# Feature Specification: Console session

**Feature Branch**: `002-console-session`

**Created**: 2026-10-05

**Status**: Draft

**Input**: User description: "The console session of the Toy Robot Simulator. Owns FR-08, FR-11 to FR-17, NFR-01, NFR-04, NFR-05 and NFR-06 from docs/solution-design/10-traceability.md. Components: Runner and Program, exactly as on solution design page 6; it uses spec 001's types and does not restate their rules. Tests it plans: whole-flow tests in memory covering every FR as console text, and end-to-end tests of the built application (exit codes, the usage hint, test-data files, the byte-order mark). Sources: design-decisions.md and solution design pages 2, 3, 5, 6, 7 (all), 9 and 10. Acceptance scenarios are copied from page 2 with exact input, console output (stdout), console messages (stderr) and exit code stated separately, plus the DEC-31 cases this spec owns. The usage hint (DEC-04), the `Output:` format (DEC-21), the prompt `> ` (DEC-24) and the echo (DEC-25) are copied character for character. Every requirement cites its FR or NFR and DEC IDs."

## Sources and ownership

- **Source of truth**: `docs/design-analysis/design-decisions.md` (DEC-nn), then the solution design in `docs/solution-design/` (pages 2, 3, 5, 6, 7 with 7.1 to 7.7, 9 and 10). Where this spec and those documents differ, those documents win ([DEC-36][dec-36]).
- **Owned by this spec**: [FR-08][fr-08], [FR-11][fr-11], [FR-12][fr-12], [FR-13][fr-13], [FR-14][fr-14], [FR-15][fr-15], [FR-16][fr-16], [FR-17][fr-17], [NFR-01][nfr-01], [NFR-04][nfr-04], [NFR-05][nfr-05] and [NFR-06][nfr-06].
- **Components specified here** (names exactly as on page 6): Runner and Program.
- **Used from spec 001-robot-rules, not restated here**: Table, Direction, RobotState, Robot, `IRobotCommand` with `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand`, `IRobotOperator`, RobotOperator and CommandParser; the parse result (a command object, EXIT, or a failure with its reason) and RobotOperator's outcome (the position text, a warning, or done with nothing to show); the rules and warning texts of [FR-01][fr-01] to [FR-07][fr-07], [FR-09][fr-09] and [FR-10][fr-10]; the solution skeleton.
- **Referred to, owned by spec 003-delivery**: [NFR-02][nfr-02], [NFR-03][nfr-03], [NFR-08][nfr-08], [NFR-09][nfr-09], the test-data files in `testdata/`, the run scripts and the CI workflow.

## Clarifications

### Session 2026-10-05

Questions raised by `/speckit-clarify` were answered from the sources (Spec Kit brief rule 11); where the sources are silent the answer is a decision under brief rule 10, marked "Decided" and listed in Assumptions.

- Q: How does a scenario that is checked only in memory state its exit code, when an in-memory run has no process and no exit code? → A: Exit codes are returned by Program, so the end-to-end tests check them (page 10, notes); exit code 2 for an unexpected problem cannot be caused by any input and is checked in memory through Program's internal `Run` ([PG-02][pg-02], [SG-01][sg-01]). A whole-flow test checks that the Runner's `Run()` returns normally, without an exception, at EXIT or at the end of the input; the exit code stated in such a scenario is the one Program returns when `Run()` returns (0, [PG-01][pg-01], [DEC-10][dec-10]). (Decided: the wording of this check; the rule itself is page 10.)
- Q: What are the names of the Runner's method that runs the session and of Program's entry point? → A: Decided: `Runner.Run()`, which returns nothing and returns to its caller at EXIT or at the end of the input, and Program's `static int Main(string[] args)`, which returns the exit code that Program's `internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)` gives back (1 after the how-to-run message, 0 when the Runner's `Run()` returns, 2 after an unexpected problem, [PG-01][pg-01], [PG-02][pg-02]). Page 6 says only "run it" and "gives back exit code 1 ... otherwise exit code 0" ([DEC-26][dec-26], [DEC-29][dec-29]).
- Q: How do the end-to-end tests find and start the built application? → A: Decided: they start the application's own executable (`ToyRobot.exe` on Windows, `ToyRobot` on Linux and macOS) from the page 5 output folder `src/ToyRobot/bin/<configuration>/net8.0/`, where `<configuration>` is the configuration the tests themselves were built with (Debug by default, Release when built so); the end-to-end test project references the application project, so `dotnet test` from the repository root builds the application first (page 5, [DEC-31][dec-31] level 3).
- Q: How does the end-to-end test over the test data ([E2E-01][e2e-01]) pair an input file with its expected-output file, and what happens while `testdata/` has no pairs yet? → A: A pair is `testdata/<name>.txt` and `testdata/<name>.expected.txt` (spec 003, [TD-01][td-01]); a file whose name ends in `.expected.txt` is never an input. Decided: [E2E-01][e2e-01] is one test that runs every pair found and reports every mismatch; with no pairs it passes, because spec 002 is implemented before spec 003 delivers the files, and spec 003's [TD-02][td-02] then fails if the three challenge example pairs are missing.
- Q: Are Windows (CRLF) and Linux/macOS (LF) line endings in the input tested, and at which level? → A: Yes. The edge case and [NFR-04][nfr-04] say both give the same result ([DEC-31][dec-31], [DEC-33][dec-33]). A whole-flow test feeds the same lines once with CRLF and once with LF and gets identical console output (stdout) and console messages (stderr); an end-to-end test feeds CRLF input bytes and gets the same console output (stdout) as for LF.

## User Scenarios & Testing *(mandatory)*

### How to read the scenarios

- **Given**, unless a scenario says otherwise: a new session, the robot not on the table, and input **redirected** from a file (not typed) (page 2). `ToyRobot` stands for the built application.
- **Console output (stdout)** carries only the REPORT result: `Output:` and the position. **Console messages (stderr)** carry everything else: the `> ` prompt, the echo of each line, warnings, the how-to-run message and the unexpected-problem error line ([PG-02][pg-02]). **Console input (stdin)** is where the commands come from. "(empty)" means nothing is written to that channel ([DEC-23][dec-23], page 2).
- With redirected input every line read is echoed to console messages (stderr) as `> ` followed by the line without whitespace at either end ([FR-14][fr-14]).
- Each scenario states its **exit code**. Exit codes are returned by Program, so the end-to-end tests check them (page 10); the unexpected-problem scenarios (User Story 5, scenarios 4 to 6) check exit code 2 in memory, as returned by Program's internal `Run` ([PG-02][pg-02]). Where a scenario is checked only in memory (whole-flow), the test checks that `Run()` returns normally, without an exception, and the stated exit code is the one Program returns when `Run()` returns.

### User Story 1 - Run a file of commands and keep the REPORT results apart (Priority: P1)

A reviewer feeds a file of commands to the application through console input (stdin). Each line is echoed with its warning, if any, directly under it on console messages (stderr), and only the REPORT results appear on console output (stdout), so they can be saved or compared on their own.

**Why this priority**: This is how the challenge's examples and the test data are run, and how results are checked.

**Independent Test**: Whole-flow tests in memory (Runner with a reader holding the lines, two writers, RobotOperator and `interactive` = false) and end-to-end tests of the built application with redirected input.

**Acceptance Scenarios**:

1. **[FR-08; DEC-04, DEC-05]** **Given** a file redirected to console input (`ToyRobot < commands.txt`). **When** the file holds:
   ```text
   PLACE 0,0,NORTH MOVE
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH MOVE
   Warning: 'PLACE 0,0,NORTH MOVE' ignored - F must be NORTH, SOUTH, EAST or WEST
   ```
   **Exit code:** 0

2. **[FR-12; DEC-23]** **Given** the [FR-07][fr-07] input in `commands.txt`:
   ```text
   MOVE
   PLACE 0,0,NORTH
   REPORT
   ```
   **When** run in cmd, bash or zsh as `ToyRobot < commands.txt > results.txt`. **Then** `results.txt` holds exactly:
   ```text
   Output:
   0,0,NORTH
   ```
   and the screen shows only the console messages (stderr) of [FR-07][fr-07]:
   ```text
   > MOVE
   Warning: MOVE ignored - robot is not on the table
   > PLACE 0,0,NORTH
   > REPORT
   ```
   **Exit code:** 0

3. **[FR-14; DEC-25]** **When** the lines are: `MOVE` with spaces before and after it, an empty line, `EXIT`, `REPORT`:
   ```text
     MOVE  

   EXIT
   REPORT
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > MOVE
   Warning: MOVE ignored - robot is not on the table
   > 
   Warning: blank line ignored
   > EXIT
   ```
   (the third line is `>` and a space). `REPORT` is after EXIT, so it is neither read, echoed nor run.
   **Exit code:** 0

4. **[FR-11; DEC-20, DEC-26]** **Given** a file that starts with a UTF-8 byte-order mark. **When** the file holds Challenge Example 1:
   ```text
   PLACE 0,0,NORTH
   MOVE
   REPORT
   ```
   **Then** console output (stdout) and console messages (stderr) are exactly as for [FR-03][fr-03], with no marker in the first echo line and no warning. **Console output (stdout):**
   ```text
   Output:
   0,1,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH
   > MOVE
   > REPORT
   ```
   **Exit code:** 0. End-to-end on Windows, Linux and macOS ([DEC-31][dec-31]), console output (stdout) matches Challenge Example 1's expected output with exit code 0; in a whole-flow test where the reader's text starts with U+FEFF, both channels match exactly as above.

5. **[FR-08, FR-12; DEC-23, DEC-31 "the three challenge examples"]** **Given** each test-data input file in `testdata/` (spec 003), including the three challenge examples, redirected to console input. **Then** console output (stdout) matches that file's expected-output file line by line, and the exit code is 0. **Console messages (stderr):** the echo `> ` followed by each line read, without whitespace at either end, with any warning directly under its line ([FR-14][fr-14]). They are not compared with a file, because a test-data pair holds only the expected console output (stdout) (page 9; spec 003 [TD-01][td-01]). For example, Challenge Example 3 (`PLACE 1,2,EAST`, `MOVE`, `MOVE`, `LEFT`, `MOVE`, `REPORT`) gives console output (stdout):
   ```text
   Output:
   3,3,NORTH
   ```
   and console messages (stderr):
   ```text
   > PLACE 1,2,EAST
   > MOVE
   > MOVE
   > LEFT
   > MOVE
   > REPORT
   ```

---

### User Story 2 - End the session (Priority: P1)

A person or a file ends the session with EXIT, or simply by running out of input. Either way the application stops at once with exit code 0, even before the robot is placed, and EXIT itself prints nothing.

**Why this priority**: Every run must end cleanly with the right exit code; scripts and tests rely on it.

**Independent Test**: Whole-flow tests for what is written and what is not read after EXIT; end-to-end tests for the exit code 0 after EXIT and after end of input.

**Acceptance Scenarios**:

1. **[FR-15; DEC-10, DEC-30]** **When** the input lines are:
   ```text
   exit now
   REPORT
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > exit now
   ```
   **Exit code:** 0

2. **[FR-16; DEC-10, DEC-32, DEC-31 "an empty input"]** **Given** an empty file. **Then console output (stdout):** (empty). **Console messages (stderr):** (empty). **Exit code:** 0

3. **[FR-15; DEC-10, DEC-25, DEC-31 "EXIT mid-file (later lines neither echoed nor run)"]** **When** the input lines are:
   ```text
   PLACE 0,0,NORTH
   REPORT
   EXIT
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH
   > REPORT
   > EXIT
   ```
   **Exit code:** 0

4. **[FR-15; DEC-10, DEC-31 "EXIT before PLACE", "`EXIT now` exits"]** For each row, **When** the input lines are the row's first line followed by `PLACE 0,0,NORTH` and `REPORT`, **Then console output (stdout):** (empty), **console messages (stderr)** are only the echo shown, and the **exit code** is 0:

   | First line | Console messages (stderr) |
   |---|---|
   | `EXIT` | `> EXIT` |
   | `EXIT now` | `> EXIT now` |
   | `Exit` | `> Exit` |
   | `exit` | `> exit` |

5. **[FR-15, FR-10 (spec 001); DEC-10, DEC-22, DEC-31 "`EXITNOW` warns"]** **When** the input lines are:
   ```text
   EXITNOW
   REPORT
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > EXITNOW
   Warning: 'EXITNOW' ignored - unknown command
   > REPORT
   Warning: REPORT ignored - robot is not on the table
   ```
   **Exit code:** 0 (the session continues after `EXITNOW` and ends at the end of the input). The same holds with `EXITING` in place of `EXITNOW`, in the echo and in the warning.

6. **[FR-16; DEC-10]** **Given** input redirected from a file (`interactive` = false). **When** the input lines are `PLACE 0,0,NORTH`, `REPORT`, then a line holding only the single character Ctrl+Z (U+001A, as the Windows console gives Ctrl+Z then Enter), then `MOVE`, `REPORT`. **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH
   > REPORT
   ```
   (the Ctrl+Z line is not echoed and gives no warning; the lines after it are not read). **Exit code:** 0

---

### User Story 3 - Type commands at the prompt (Priority: P2)

A person starts the application without redirecting input and types commands. Before each line is read the application shows the prompt `> ` on console messages (stderr); what the person types is not echoed again, because the terminal already shows it.

**Why this priority**: The typed session is the second way to use the application ([DEC-04][dec-04]), but files are how it is checked.

**Independent Test**: Whole-flow tests with `interactive` = true (the prompt is checked exactly there; an end-to-end test cannot make console input look typed, page 10).

**Acceptance Scenarios**:

1. **[FR-13; DEC-24]** **Given** a person is typing. **When** they type `MOVE`, then `EXIT`, pressing Enter after each. **Then console output (stdout):** (empty). **Console messages (stderr)** (`⏎` marks a line break):
   `> Warning: MOVE ignored - robot is not on the table⏎> `
   **Exit code:** 0

2. **[FR-13, FR-16; DEC-10, DEC-24, DEC-31 "prompt shown when interactive and not otherwise"]** **Given** a person is typing. **When** they type `PLACE 0,0,NORTH`, then `REPORT`, pressing Enter after each, and then end the input (Ctrl+Z then Enter on Windows, Ctrl+D on Linux and macOS). **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):** `> > > ` (one prompt before each of the three reads, with no line break; the typed lines are not echoed, and the last prompt stays on screen when the input ends, [DEC-10][dec-10]).
   **Exit code:** 0

3. **[FR-13, FR-14; DEC-24, DEC-25, DEC-31 "prompt shown when interactive and not otherwise"]** **Given** input redirected from a file (`interactive` = false). **When** the input lines are `MOVE`, `EXIT`. **Then console output (stdout):** (empty). **Console messages (stderr):**

   ```
   > MOVE
   Warning: MOVE ignored - robot is not on the table
   > EXIT
   ```
   (no prompt before any read; each `> ` is the echo of the line read). **Exit code:** 0

4. **[FR-13, FR-16; DEC-10, DEC-24]** **Given** a person is typing on Windows. **When** the input lines are `PLACE 0,0,NORTH`, `REPORT`, then a line holding only the single character Ctrl+Z (U+001A), then `MOVE`, `REPORT`. **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):** `> > > ` (one prompt before each of the three reads; no warning for the Ctrl+Z line; the lines after it are not read). **Exit code:** 0

---

### User Story 4 - Started the wrong way (Priority: P2)

Someone starts the application with a file name or `--help`. The application takes no arguments, so instead of seeming to hang it prints the how-to-run message and stops with exit code 1, without reading any input.

**Why this priority**: Prevents the most likely mistake (passing a file name) from looking like a hang.

**Independent Test**: End-to-end tests of the built application (only Program sees the arguments and sets the exit code, page 10), and Program's `Run` called in memory with an argument ([SG-01][sg-01]).

**Acceptance Scenarios**:

1. **[FR-17; DEC-04]** **When** started as `ToyRobot --help` (or with a file name, for example `ToyRobot commands.txt`). **Then console output (stdout):** (empty). **Console messages (stderr):**
   ```text
   How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)
   ```
   (three spaces before the bracket). **Exit code:** 1

---

### User Story 5 - Bad input never stops the session (Priority: P2)

Whatever the input holds, the application keeps going: every bad line gets its warning and the next line is processed. It ends only at EXIT, at the end of the input, when started with an argument, when the user presses Ctrl+C, or with an unexpected problem that no input causes (a fault outside the application's control, such as the console or a pipe closing while it writes, or a defect): then a last safeguard in Program writes one error line instead of a .NET stack trace and stops with exit code 2 ([PG-02][pg-02]).

**Why this priority**: the challenge requires that invalid commands are ignored and valid commands still work; a crash would stop both ([DEC-32][dec-32]).

**Independent Test**: Whole-flow and end-to-end tests with empty input, numbers too large to read and every kind of bad line, followed by valid commands; for the safeguard, Program's `Run` called in memory with a reader or writer that fails on purpose ([SG-01][sg-01]).

**Acceptance Scenarios**:

1. **[NFR-01; DEC-20, DEC-22, DEC-32]** **When** the input lines are:
   ```text
   PLACE 99999999999,0,NORTH
   HELLO

   PLACE 0,0,NORTH
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 99999999999,0,NORTH
   Warning: 'PLACE 99999999999,0,NORTH' ignored - X must be a whole number
   > HELLO
   Warning: 'HELLO' ignored - unknown command
   > 
   Warning: blank line ignored
   > PLACE 0,0,NORTH
   > REPORT
   ```
   (the fifth line is `>` and a space). **Exit code:** 0

2. **[NFR-05; DEC-03, DEC-29, DEC-31 "a small table such as 2 x 2"]** **Given** a whole-flow test whose RobotOperator is created with a Table of width 2 and height 2 (redirected input, `interactive` = false). **When** the input lines are:
   ```text
   PLACE 1,1,NORTH
   MOVE
   PLACE 2,2,NORTH
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   1,1,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 1,1,NORTH
   > MOVE
   Warning: MOVE ignored - next position is off the table so robot stays at 1,1,NORTH
   > PLACE 2,2,NORTH
   Warning: PLACE 2,2,NORTH ignored - position is off the table
   > REPORT
   ```
   **Exit code:** not applicable (in memory only; `Run()` returns normally at the end of the input).

3. **[NFR-05, NFR-06; DEC-31 "whole-flow tests covering every FR as console text"]** Every acceptance scenario of spec 001-robot-rules ([FR-01][fr-01] to [FR-07][fr-07], [FR-09][fr-09], [FR-10][fr-10], each with its exact input, console output (stdout) and console messages (stderr)) also passes as a whole-flow test in memory through the Runner on a 5 x 5 table with `interactive` = false. Two of spec 001's scenarios are stated at component level and have their whole-flow counterpart in this spec instead: spec 001 User Story 1 scenario 9 (a 2 x 2 Table) is covered by scenario 2 above, and spec 001 User Story 4 scenario 8 (EXIT recognised by the parser) is covered by User Story 2.

4. **[NFR-01; DEC-23, DEC-26, DEC-32]** **Given** Program's `Run` is called in memory with no argument, `interactive` = false, and an input reader whose `ReadLine` throws `IOException` with the message `boom` (standing for a fault outside the application, for example a pipe that closes: an unexpected problem). **Then console output (stdout):** (empty). **Console messages (stderr):**
   ```text
   Error: unexpected problem - boom
   ```
   (exactly one line, and nothing after it). **Exit code:** 2

5. **[NFR-01; DEC-23, DEC-25, DEC-32]** **Given** Program's `Run` is called in memory with no argument, `interactive` = false, and a console output (stdout) writer that throws `IOException` with the message `boom` on every write (an unexpected problem). **When** the input lines are:
   ```text
   PLACE 0,0,NORTH
   REPORT
   ```
   **Then console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH
   > REPORT
   Error: unexpected problem - boom
   ```
   **Exit code:** 2

6. **[NFR-01; DEC-32]** **Given** the input of scenario 5, and both the console output (stdout) writer and the console messages (stderr) writer throw `IOException` with the message `boom` on every write (an unexpected problem). **Then** nothing can be written, Program's `Run` does not throw, and the **exit code** is 2.

---

### Edge Cases

- The byte-order mark is removed only from the start of the first line read, and never by reading ahead, so the first prompt is never delayed ([DEC-20][dec-20], page 6).
- Windows (CRLF) and Linux/macOS (LF) line endings in the input give the same result, and expected output is compared line by line, so both pass ([DEC-31][dec-31], [NFR-04][nfr-04]). Checked by a whole-flow test (the same lines with CRLF and with LF give identical console output (stdout) and console messages (stderr)) and an end-to-end test (CRLF input bytes give the same console output (stdout) as LF).
- A blank line is echoed as `> ` (`>` and a space) before its warning ([DEC-25][dec-25]).
- EXIT is echoed with redirected input like every other line; lines after it are not read ([DEC-10][dec-10], [DEC-25][dec-25]).
- When a typed session ends with Ctrl+Z / Ctrl+D, the last `> ` prompt stays on screen; this is expected ([DEC-10][dec-10]).
- A line that is exactly the single character Ctrl+Z (U+001A) ends the input like the end of a file, typed or redirected: it is not echoed, gives no warning, and nothing after it is read ([DEC-10][dec-10]). The Windows console gives Ctrl+Z then Enter as this line.
- Ctrl+C is not handled by the application: the standard behaviour applies (it stops at once with the non-zero exit code set by the system) ([DEC-10][dec-10]). There is no test for it.

## Requirements *(mandatory)*

### Functional Requirements (owned)

- **FR-08**<a id="fr-08"></a> ([DEC-04][dec-04], [DEC-05][dec-05], [DEC-24][dec-24], [DEC-26][dec-26]): Commands MUST be read from console input (stdin), typed or redirected from a file or a pipe, one command per line. Program MUST decide `interactive` once: true when console input is not redirected (`Console.IsInputRedirected` is false). For redirected input Program MUST read console input through its own UTF-8 reader; when a person is typing it MUST use the console's normal input (`Console.In`). The Runner MUST read one line at a time and never read ahead.
- **FR-11**<a id="fr-11"></a> ([DEC-20][dec-20], [DEC-26][dec-26]): A UTF-8 byte-order mark at the start of the input MUST be ignored. Program's UTF-8 reader removes it for redirected input, and the Runner MUST also remove a byte-order mark (U+FEFF) from the start of the first line it reads, so in-memory tests behave the same. Nothing is read ahead.
- **FR-12**<a id="fr-12"></a> ([DEC-21][dec-21], [DEC-23][dec-23]): Console output (stdout) MUST carry only the REPORT result: for each position-text outcome from RobotOperator, the Runner writes the line `Output:` and then the position text as its own line, for example:
  ```text
  Output:
  0,1,NORTH
  ```
  Warnings, the prompt, the echo, the how-to-run message and the unexpected-problem error line ([PG-02][pg-02]) MUST go to console messages (stderr). With redirected input, `ToyRobot < commands.txt > results.txt` saves only the REPORT results.
- **FR-13**<a id="fr-13"></a> ([DEC-10][dec-10], [DEC-24][dec-24]): When a person is typing (`interactive` = true), the Runner MUST write the prompt `> ` (`>` and a space, no line break) to console messages (stderr) before each line is read, including the read that finds the end of the input, and MUST NOT echo typed lines. With redirected input (`interactive` = false) there is no prompt.
- **FR-14**<a id="fr-14"></a> ([DEC-08][dec-08], [DEC-25][dec-25]): With redirected input (`interactive` = false), the Runner MUST echo every line read to console messages (stderr), before it is processed, as `> ` followed by the line with whitespace of any kind removed from both ends (after the byte-order mark is removed from the first line). This includes bad lines, blank lines (echoed as `> `) and the EXIT line; lines after EXIT are not read, so they are not echoed.
- **FR-15**<a id="fr-15"></a> ([DEC-10][dec-10], [DEC-30][dec-30]): When the CommandParser returns EXIT (the command word is EXIT in any case; spec 001, [FR-10a][fr-10a]), the Runner MUST stop at once: the session ends with exit code 0, even before PLACE; anything after EXIT on its line is ignored, later lines are not read, and EXIT gives no output of its own. RobotOperator never receives EXIT. `EXITNOW` is an unknown command (spec 001, [FR-10][fr-10]).
- **FR-16**<a id="fr-16"></a> ([DEC-10][dec-10], [DEC-32][dec-32]): The session MUST end at the end of the input with exit code 0. A typing user ends input with Ctrl+Z then Enter (Windows) or Ctrl+D (Linux, macOS). A line that is exactly the single character Ctrl+Z (U+001A), which is how the Windows console gives Ctrl+Z then Enter, MUST end the input in the same way, whether the input is typed or redirected: the Runner returns, the line is not echoed and gives no warning, and nothing after it is read or run. Empty input MUST NOT cause an error: with redirected empty input both console output (stdout) and console messages (stderr) are empty.
- **FR-17**<a id="fr-17"></a> ([DEC-04][dec-04], [DEC-26][dec-26]): If the application is started with any argument (a file name, `--help` or anything else), Program MUST NOT read input; it MUST write this how-to-run message as one line to console messages (stderr), write nothing to console output (stdout), and stop with exit code 1:
  ```text
  How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)
  ```
  (three spaces before the bracket).

### Components (owned; part of NFR-07's page 6 structure, which spec 001 owns)

- **RN-01 Runner**<a id="rn-01"></a> ([DEC-26][dec-26], [DEC-27][dec-27], [DEC-29][dec-29], [DEC-30][dec-30]; supports [FR-08][fr-08], [FR-11][fr-11] to [FR-16][fr-16]): The Runner MUST be created as `Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)`, where `output` is console output (stdout) and `error` is console messages (stderr), and MUST create its own CommandParser. Its method `Run()` runs the session and returns nothing; it returns to its caller at EXIT or at the end of the input. `Run()` repeats, for each line, in this order:
  1. If `interactive`, write `> ` to `error` ([FR-13][fr-13]).
  2. Read a line. If there is none (end of input), or the line is exactly the single character Ctrl+Z (U+001A), return ([FR-16][fr-16]).
  3. If this is the first line, remove a byte-order mark from its start ([FR-11][fr-11]).
  4. If not `interactive`, write `> ` and the line without whitespace at either end to `error` ([FR-14][fr-14]).
  5. Pass the line to the CommandParser:
     - a failure (`ParseResult.Failure`, `Reason`): write `Warning: ` and its reason to `error`;
     - EXIT (`ParseResult.Exit`): return ([FR-15][fr-15]);
     - a command object (`ParseResult.CommandObject`): pass it to RobotOperator (`RobotOperator.Run`) and write its outcome: the position text (`Outcome.PositionText`) as `Output:` then the text to `output` ([FR-12][fr-12]); a warning (`Outcome.Warning`) as `Warning: ` then the text to `error`; done with nothing to show (`Outcome.Done`): write nothing.
  The Runner MUST NOT use `Console` directly, know any robot rule or the table size, or pass EXIT or a failure to RobotOperator.
- **PG-01 Program**<a id="pg-01"></a> ([DEC-03][dec-03], [DEC-04][dec-04], [DEC-24][dec-24], [DEC-26][dec-26], [DEC-29][dec-29], [DEC-32][dec-32]; supports [FR-01][fr-01] in spec 001, [FR-08][fr-08], [FR-11][fr-11], [FR-15][fr-15] to [FR-17][fr-17], [NFR-01][nfr-01]): Program MUST be the thin entry point, with two methods. `static int Main(string[] args)` is the thin wiring of the real console: it MUST decide `interactive` ([FR-08][fr-08]), choose the input reader ([FR-08][fr-08], [FR-11][fr-11]; choosing it reads nothing), and return what `Run(args, reader, Console.Out, Console.Error, interactive)` returns. `internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)` does the rest, inside the safeguard of [PG-02][pg-02]: if any argument is given it does [FR-17][fr-17] (the how-to-run message on `error`) and nothing else; otherwise it MUST create `Table(5, 5)` and `RobotOperator(table)`, create the Runner with `input`, `output` (console output, stdout), `error` (console messages, stderr), RobotOperator and `interactive`, call the Runner's `Run()`, and return exit code 0 when it returns. `Run` is internal so the tests can call it with in-memory readers and writers ([SG-01][sg-01]). Program MUST NOT read or parse commands, apply rules, change the console's encoding settings (that can fail on Windows when there is no console window, for example in CI; the output is plain ASCII), or handle Ctrl+C.
- **PG-02 Safeguard for unexpected problems**<a id="pg-02"></a> ([NFR-01][nfr-01]; [DEC-10][dec-10], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-23][dec-23], [DEC-26][dec-26], [DEC-32][dec-32]): Program's `Run` MUST do its argument check, create the parts and run the Runner inside one `try` that catches any exception. When one is caught it MUST write exactly one line, `Error: unexpected problem - <exception message>` (where `<exception message>` is the exception's `Message`), to `error` (console messages, stderr), write nothing more, and return exit code 2. If writing that line throws too, `Run` MUST still return 2 and MUST NOT throw. No input causes this: every bad input has a defined result ([DEC-20][dec-20], [DEC-22][dec-22]) and no component throws for input (spec 001, [UT-04][ut-04]). It covers faults outside the application's control (for example the console or a pipe closing while it writes) and defects. The Runner catches nothing. Ctrl+C is still not handled ([DEC-10][dec-10]). Exit codes: 0 normal end ([FR-15][fr-15], [FR-16][fr-16]), 1 any argument ([FR-17][fr-17]), 2 unexpected problem.

### Non-Functional Requirements (owned)

- **NFR-01 Never crash**<a id="nfr-01"></a> ([DEC-20][dec-20], [DEC-32][dec-32]): No input may make the application crash: not an empty input, a very large number, or any other text. It ends only at EXIT, at the end of the input, when started with an argument ([FR-17][fr-17]), when the user presses Ctrl+C, or with an unexpected problem with exit code 2: a fault outside the application's control (for example the console or a pipe closing while it writes) or a defect, which Program's last safeguard turns into one line, `Error: unexpected problem - <exception message>`, on console messages (stderr) instead of a .NET stack trace ([PG-02][pg-02]). Checked by tests that feed empty input, numbers too large to read and every kind of bad line, and check that processing continues and the exit code is 0 (whole-flow and end-to-end; the unit-level part is in spec 001, [UT-04][ut-04]), and by tests that give Program's `Run` a reader or writer that fails and check the exact error line and exit code 2, including when writing the error line fails too ([SG-01][sg-01]).
- **NFR-04 Same results on every machine**<a id="nfr-04"></a> ([DEC-08][dec-08], [DEC-31][dec-31], [DEC-33][dec-33]): Results MUST NOT depend on the machine's regional settings (numbers are read with fixed rules, spec 001 [FR-09][fr-09]) or on the line endings its system uses. Checked by a review that numbers are read with fixed rules, by comparing expected output line by line in every whole-flow and end-to-end test (so Windows and Linux line endings both pass), and by running the same tests and test files on all three systems in CI ([NFR-02][nfr-02], spec 003).
- **NFR-05 Testable without the console**<a id="nfr-05"></a> ([DEC-03][dec-03], [DEC-29][dec-29], [DEC-31][dec-31]): The whole flow, from reading a line to writing the result, MUST be testable in memory, with any table size, without starting the application: the Runner is given an in-memory reader as input and two in-memory writers as its outputs, and console output (stdout) and console messages (stderr) are checked separately and exactly, including on a 2 x 2 table.
- **NFR-06 Tested at three levels**<a id="nfr-06"></a> ([DEC-31][dec-31]): There MUST be unit tests (planned in spec 001), whole-flow tests in memory and end-to-end tests of the built application (planned here), together covering every case in [DEC-31][dec-31]'s "Must be covered" list, with test names in the challenge wording where possible (for example `Robot_not_on_table_ignores_MOVE`). Expected output is compared line by line. CI runs all three levels on all three systems (spec 003). The test list is compared with [DEC-31][dec-31]'s list.

### Testing (planned by this spec)

- **WF-01**<a id="wf-01"></a> ([DEC-31][dec-31] level 2; [NFR-05][nfr-05], [NFR-06][nfr-06]): Whole-flow tests (Runner → CommandParser → RobotOperator → Table, in memory) MUST check, as console text with console output (stdout) and console messages (stderr) compared separately and exactly: every acceptance scenario of spec 001 ([FR-01][fr-01] to [FR-07][fr-07], [FR-09][fr-09], [FR-10][fr-10]; its two component-level scenarios through their counterparts named in User Story 5, scenario 3) and every acceptance scenario of this spec except those that need the built application ([FR-12][fr-12]'s file redirection, [FR-17][fr-17] and exit codes). [FR-13][fr-13] is checked with `interactive` = true; all others with `interactive` = false.
- **WF-02**<a id="wf-02"></a> ([DEC-25][dec-25], [DEC-31][dec-31] "echo content with redirected input"; [FR-14][fr-14]): Whole-flow tests MUST check that the echo removes whitespace of any kind from both ends of the line (spaces, tabs and non-breaking spaces) and nothing inside it, for a valid line, a bad line and a whitespace-only line.
- **WF-03**<a id="wf-03"></a> ([DEC-31][dec-31], [NFR-04][nfr-04]; Edge Cases): A whole-flow test MUST feed the same lines once with CRLF and once with LF line endings and check that console output (stdout) and console messages (stderr) are identical.
- **E2E-01**<a id="e2e-01"></a> ([DEC-31][dec-31] level 3; [FR-08][fr-08], [FR-12][fr-12]; [NFR-06][nfr-06]): An end-to-end test MUST start the built application as a separate process for each test-data pair in `testdata/` (`<name>.txt` and `<name>.expected.txt`, spec 003 [TD-01][td-01]; a file ending `.expected.txt` is never an input), redirect the input file to console input, compare console output (stdout) with the expected file line by line, and check exit code 0. It is one test that runs every pair found and reports every mismatch; with no pairs it passes (spec 003, [TD-02][td-02], checks that the challenge example pairs are present).
- **E2E-02**<a id="e2e-02"></a> ([FR-17][fr-17]): Started with `--help`, and with a file name, the built application writes exactly the how-to-run message line to console messages (stderr), nothing to console output (stdout), and exits with code 1.
- **E2E-03**<a id="e2e-03"></a> ([FR-15][fr-15], [FR-16][fr-16]): Exit code 0 after the end of the input, and after EXIT; with EXIT in the middle of the input, the lines after it are not run (User Story 2, scenario 3).
- **E2E-04**<a id="e2e-04"></a> ([FR-16][fr-16], [NFR-01][nfr-01]): Empty input gives empty console output (stdout) and exit code 0 (empty console messages (stderr) are proved by the whole-flow test T014).
- **E2E-05**<a id="e2e-05"></a> ([FR-11][fr-11]): Input that starts with a UTF-8 byte-order mark followed by Challenge Example 1 gives the console output (stdout) of Challenge Example 1, line by line, and exit code 0; runs on all three systems in CI (spec 003).
- **E2E-06**<a id="e2e-06"></a> ([FR-12][fr-12], [FR-08][fr-08], [NFR-01][nfr-01]): The [FR-07][fr-07] input gives exactly `Output:` and `0,0,NORTH` on console output (stdout), read separately from console messages (stderr); the [FR-08][fr-08] input and the [NFR-01][nfr-01] input of User Story 5 give their stated console output (stdout) and exit code 0.
- **E2E-07**<a id="e2e-07"></a> ([NFR-04][nfr-04]; [DEC-31][dec-31], [DEC-33][dec-33]): Challenge Example 1 fed as CRLF input bytes gives the same console output (stdout) as with LF (`Output:`, `0,1,NORTH`) and exit code 0.
- **E2E-08**<a id="e2e-08"></a> ([FR-17][fr-17]; [DEC-04][dec-04]): Started with an argument while console input holds a command line, the application does not read it: console messages (stderr) hold only the how-to-run message line (no echo), and the exit code is 1.
- **E2E-09**<a id="e2e-09"></a> ([PG-01][pg-01]; [FR-08][fr-08], [FR-01][fr-01] in spec 001; [DEC-03][dec-03], [DEC-26][dec-26]): Test `Redirected_REPORT_before_PLACE_writes_nothing_to_console_output`: input `REPORT` gives console output (stdout) with no lines and exit code 0. The application's table is 5 x 5, test `Application_table_is_5_by_5`: input `PLACE 4,4,NORTH`, `REPORT`, `PLACE 5,0,NORTH`, `REPORT`, `PLACE 0,5,NORTH`, `REPORT` gives console output (stdout) lines `Output:`, `4,4,NORTH` three times, and exit code 0 (the off-table PLACEs left the robot at 4,4).
- **SG-01**<a id="sg-01"></a> ([NFR-01][nfr-01], [FR-17][fr-17]; [DEC-04][dec-04], [DEC-10][dec-10], [DEC-23][dec-23], [DEC-26][dec-26], [DEC-32][dec-32]; [PG-01][pg-01], [PG-02][pg-02]): Tests in `ToyRobot.Tests` call Program's internal `Run` in memory (reached through `<InternalsVisibleTo Include="ToyRobot.Tests" />` in `src/ToyRobot/ToyRobot.csproj`) and check: an input reader whose `ReadLine` throws `IOException` with the message `boom` gives console messages (stderr) of exactly the one line `Error: unexpected problem - boom`, nothing after it, and exit code 2 (User Story 5, scenario 4); a console output (stdout) writer that throws on write, with input `PLACE 0,0,NORTH` and `REPORT`, gives the two echo lines, then the error line, and exit code 2 (scenario 5); when the console messages (stderr) writer throws too, `Run` returns 2 and does not throw (scenario 6); an argument gives the how-to-run message line, nothing on console output (stdout), exit code 1 and an unread input; Challenge Example 1 gives its normal console output (stdout) and console messages (stderr) and exit code 0.
- **Start-up for end-to-end tests**: each end-to-end test starts the application's own executable (`ToyRobot.exe` on Windows, `ToyRobot` on Linux and macOS) from `src/ToyRobot/bin/<configuration>/net8.0/` (page 5), where `<configuration>` is the one the tests were built with; console input, console output (stdout) and console messages (stderr) are each redirected and read separately.

### Key Entities

- **Runner**: runs one session over a reader and two writers.
- **Program**: the entry point; checks arguments, builds the parts, picks the input reader, returns the exit code, and turns an unexpected exception into one error line and exit code 2.
- **Console input (stdin)**, **console output (stdout)**, **console messages (stderr)**: the three channels ([DEC-23][dec-23]).
- **Interactive**: whether a person is typing (prompt) or input is redirected (echo).
- **Test-data pair**: an input file and its expected-output file in `testdata/` (delivered by spec 003).
- Spec 001's types (RobotOperator, CommandParser, parse result, outcome, Table) are used as defined there.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**<a id="sc-001"></a> ([FR-08][fr-08], [FR-11][fr-11] to [FR-17][fr-17]): 100% of the acceptance scenarios in this spec pass at the level that checks them: console output (stdout) and console messages (stderr) exactly in the whole-flow tests; console output (stdout) line by line, the how-to-run message on console messages (stderr) and the exit code in the end-to-end tests (page 10); for User Story 5 scenarios 4 to 6, the error line and the exit code that Program's `Run` returns in memory ([SG-01][sg-01]). User Story 1 scenario 5's console messages (stderr) are stated for reference and are not compared per test-data file (spec 003 [TD-01][td-01] holds only expected stdout).
- **SC-002**<a id="sc-002"></a> ([WF-01][wf-01]): 100% of spec 001's acceptance scenarios also pass as whole-flow console text; spec 001 User Story 1 scenario 9 and User Story 4 scenario 8, which are stated at component level, pass through their whole-flow counterparts named in User Story 5 scenario 3 (T044; T024 to T028).
- **SC-003**<a id="sc-003"></a> ([FR-17][fr-17]): The how-to-run message matches [DEC-04][dec-04] character for character (including the three spaces before the bracket), with exit code 1, for both `--help` and a file name.
- **SC-004**<a id="sc-004"></a> ([FR-12][fr-12], [DEC-23][dec-23]): In 0 tests does anything other than `Output:` and position lines appear on console output (stdout).
- **SC-005**<a id="sc-005"></a> ([NFR-01][nfr-01]): 0 test runs end with a crash or a non-zero exit code for any input when no argument is given. Exit code 2 appears only in the [SG-01][sg-01] tests, where a reader or writer fails on purpose, and always with exactly one `Error: unexpected problem - <exception message>` line.
- **SC-006**<a id="sc-006"></a> ([NFR-06][nfr-06]): Every case in [DEC-31][dec-31]'s "Must be covered" list maps to at least one named test at one of the three levels.
- **SC-007**<a id="sc-007"></a> ([FR-11][fr-11], [NFR-04][nfr-04]): The byte-order-mark, EXIT and end-of-input end-to-end tests pass on Windows, Linux and macOS (in CI, spec 003).

## Assumptions

- <a id="decided-e2e-compare-stdout"></a>Decided: end-to-end tests compare console output (stdout) line by line with the expected output and check the exit code. They check console messages (stderr) only for the how-to-run message ([FR-17][fr-17]: [E2E-02][e2e-02] and [E2E-08][e2e-08], which page 10 marks end-to-end only and [DEC-31][dec-31] lists as end-to-end). The byte-order-mark case ([FR-11][fr-11]) is proved end-to-end by stdout matching Challenge Example 1's expected output: a mark that was not removed would make the first line an unknown command, so no REPORT position would be printed. The echo without a mark, the prompt and the echo content are proved by the whole-flow tests (T010, T011, T012, T032 to T035), as page 10 says. Other options: compare stderr end-to-end too (contradicts page 10 and page 9).
- <a id="decided-e2e-09-startup"></a>**Decided: [E2E-09][e2e-09] covers the start-up checks.** The redirected-REPORT and 5 x 5 table checks (T018) get their own planned test ID, [E2E-09][e2e-09], in `StartUpTests.cs`. Other options: fold them into [E2E-06][e2e-06] (mixes unrelated checks).
- <a id="decided-e2e-09-stdout-only"></a>Decided: [E2E-09][e2e-09] checks the 5 x 5 table through console output (stdout) only. Other options: also check console messages (stderr) (contradicts page 10).
- <a id="decided-namespaces"></a>Decided: namespaces follow the folders, as in spec 001. Runner and Program are in namespace `ToyRobot`; the whole-flow tests and `SessionHarness` are in `ToyRobot.Tests.WholeFlow`; the end-to-end tests, `AppProcess` and `LineComparer` are in `ToyRobot.EndToEndTests`. Other options: one namespace for all tests (breaks spec 001's folder-equals-namespace rule).
- <a id="decided-us1-scenario-5"></a>**Decided: User Story 1 scenario 5 states console messages (stderr).** They are the echo and warnings and are not compared with a file, because a test-data pair holds only the expected console output (stdout). Other options: add expected-stderr files (outside page 9 and spec 003).
- <a id="decided-program-internal"></a>**Decided: Program is internal static; only Runner is added to spec 001's public type list.** Spec 001's DesignRulesTests (T054) asserts the exact list of public top-level types and expects Runner to be added; T020a updates it. The tests reach Program's internal `Run` ([PG-01][pg-01]) through `<InternalsVisibleTo Include="ToyRobot.Tests" />` in `src/ToyRobot/ToyRobot.csproj`, an MSBuild item, not a library, so Program stays internal and the list is unchanged. Other options: a public Program (breaks that test).
- <a id="decided-phase3-passing-tests"></a>**Decided: Phase 3 tests that pass before T022.** T015 ([E2E-01][e2e-01] passes while `testdata/` has no pairs), T019's empty-input test, T017's `Line_with_two_commands_gives_warning_and_exit_code_0` and T018's `Redirected_REPORT_before_PLACE_writes_nothing_to_console_output` (spec 001's placeholder Program already reads nothing, writes nothing and returns 0, and these three check only empty stdout and exit code 0) pass before T022 and guard it. Other options: expect them to fail first (they would not).
- <a id="decided-t027-guard"></a>**Decided: T027 is a passing guard in Phase 4.** `EXITNOW` and `EXITING` are parse failures handled since T021, so T027 passes before T030 and guards that T030 stops only on EXIT. Other options: expect it to fail first (it would not).
- The test-data files, the run scripts and the CI workflow are delivered by spec 003; this spec's end-to-end tests are what CI runs on each system.
- The tests use xUnit and live in `tests/ToyRobot.Tests/` (whole-flow) and `tests/ToyRobot.EndToEndTests/` (end-to-end), created by spec 001 (pages 5 and 9).
- <a id="decided-testdata-and-e2e"></a>**Decided in the run: test-data files and the end-to-end tests.** The brief gives the test-data files to spec 003 but the end-to-end test that runs them to this spec, and spec 003 is implemented after this one. Chosen: [E2E-01][e2e-01] runs every input/expected pair found in `testdata/` (none are required for it to pass until spec 003 adds them; spec 003 then checks that the three challenge example pairs are present), and this spec's own end-to-end cases ([E2E-02][e2e-02] to [E2E-09][e2e-09]) create their input inside the test (for example a temporary file, with the byte-order mark written as bytes) so that spec 002 is complete and fully tested before spec 003. Other options: spec 002 creates the challenge example files (moves ownership away from spec 003); move [E2E-01][e2e-01] into spec 003 (contradicts the brief's table, which gives the test-data runs to spec 002's end-to-end tests).
- <a id="decided-line-breaks"></a>**Decided in the run: line breaks.** The sources show the prompt without a line break ([FR-13][fr-13]'s scenario) and every other output as lines. Chosen: the Runner writes each echo, warning, `Output:` label and position text as a full line ending with the writer's line break; the prompt `> ` has no line break; Program writes the how-to-run message as one full line. Other options: no line break after the how-to-run message (an odd shell prompt position, no source asks for it).
- <a id="decided-which-build"></a>**Decided in the run: which build the end-to-end tests start.** Page 5 gives the Release path for the run scripts; the end-to-end tests run inside `dotnet test`. Chosen: the end-to-end test project references the application project, so the same `dotnet test` builds it, and the tests start the application's own executable from the page 5 folder `src/ToyRobot/bin/<configuration>/net8.0/`, with `<configuration>` being the one the tests were built with (Debug by default, Release when the tests are built in Release). So `dotnet test` from the repository root works with no earlier step. Other options: always start the Release build (needs a separate Release build before `dotnet test`); use `dotnet run` (slower; mixes build output into the process output).
- <a id="decided-method-names"></a>**Decided in the run: method names of Runner and Program.** Page 6 says the Runner is created and "run", and Program "gives back" an exit code, without naming members. Chosen: `Runner.Run()`, returning nothing, and `static int Main(string[] args)`, returning the exit code that Program's `internal static int Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)` gives back; `Run` holds the work and the safeguard so the tests can call it in memory ([PG-01][pg-01], [PG-02][pg-02]). Other options: `Main` that calls `Environment.Exit` (harder to read; an exit code returned from `Main` is the standard .NET form); `Run()` returning a value (the Runner has no result to give back, page 6).
- <a id="decided-exit-codes-in-memory"></a>**Decided in the run: exit codes in in-memory scenarios.** Page 10 says the end-to-end tests check exit codes. Chosen: a whole-flow test checks that `Run()` returns normally at EXIT or at the end of the input; the scenario's stated exit code is what Program then returns. The unexpected-problem scenarios (User Story 5, scenarios 4 to 6) are checked through Program's `Run` itself, so their exit code 2 is checked in memory (page 10, [SG-01][sg-01]). Other options: leave the exit code out of in-memory scenarios (brief rule 3 asks each scenario to state it).
- <a id="decided-e2e-01-no-testdata"></a>**Decided in the run: [E2E-01][e2e-01] with no test data.** Spec 002 is implemented before spec 003 adds `testdata/`. Chosen: [E2E-01][e2e-01] is one test over every pair found, passing with none, so spec 002's phases end with all tests passing; spec 003's [TD-02][td-02] guards the challenge examples. Other options: one test case per pair (xUnit reports a test with no cases as a failure, so spec 002 could not end green); spec 002 adds the files (moves ownership away from spec 003).
- <a id="decided-line-endings-tests"></a>**Decided in the run: tests for input line endings and echo whitespace.** The edge case "CRLF and LF give the same result" and [DEC-31][dec-31]'s "echo content with redirected input" had no named test. Chosen: [WF-02][wf-02], [WF-03][wf-03] and [E2E-07][e2e-07]. Other options: rely on the test-data files only (they are stored with LF by `.gitattributes`, so CRLF input would never be run).
- <a id="decided-program-safeguard"></a>**Decided: a last safeguard in Program, no catch-all in the Runner ([DEC-32][dec-32]).** [NFR-01][nfr-01] and [DEC-32][dec-32] say no input may crash the application, and page 9 says each bad input has a defined, normal result: no component throws for any input (spec 001, [UT-04][ut-04]), and the Runner catches no exceptions. [DEC-32][dec-32] adds one last safeguard in Program's `Run`: an exception that escapes (a fault outside the application's control, such as the console or a pipe closing while it writes, or a defect) gives exactly one line, `Error: unexpected problem - <exception message>`, on console messages (stderr) and exit code 2, even if writing that line fails ([PG-02][pg-02], [DEC-23][dec-23], [DEC-26][dec-26]). [NFR-01][nfr-01] is still proved by the tests, which check output and exit codes exactly, so a defect still fails them. Other options: a catch-all in the Runner that writes a message and continues (a message no decision defines, and it could hide a defect from the tests); no safeguard (a .NET stack trace and an exit code set by the system).
- <a id="decided-args-stop-reading"></a>**Decided in the run: a test that arguments stop all input reading.** [FR-17][fr-17] says Program "MUST NOT read input" with an argument. Chosen: [E2E-08][e2e-08] gives console input a line together with the argument and checks there is no echo. Other options: no check (the rule would be untested).
- <a id="decided-us3-scenario-3"></a>**Decided: US3 scenario 3 uses the input of T035.** T035 already tests exactly this input, so no task changes. Other options: a new input (would need a new test).
- <a id="decided-no-second-reference"></a>**Decided: spec 002 adds no second project reference.** The end-to-end test project keeps the ProjectReference to `src/ToyRobot/ToyRobot.csproj` that spec 001 T005 adds, which builds the application first (a second reference would give NuGet warning NU1504). Other options: a reference with `ReferenceOutputAssembly="false"` (duplicate).
- <a id="decided-tasks-cite-ids"></a>**Decided: every task cites its FR or NFR and DEC IDs.** Tasks that only run the build and tests cite [NFR-06][nfr-06] and [DEC-31][dec-31]; T001 also cites [DEC-26][dec-26]; T047 cites [NFR-01][nfr-01] and [DEC-32][dec-32]. Other options: leave them without citations (brief rule 4).
- <a id="decided-sc-001-levels"></a>**Decided: [SC-001][sc-001] is measured at the level that checks each scenario.** Whole-flow tests compare stdout and stderr; end-to-end tests compare stdout line by line, stderr only for the how-to-run message, and the exit code (page 10); in-memory scenarios have no exit code. Other options: require all three for every scenario (cannot be met; contradicts page 10 and User Story 5 scenario 2).
- <a id="decided-no-read-ahead"></a>**Decided: the Runner reads no line ahead.** The Runner contract says it reads the next line only after the previous line has been processed ([DEC-20][dec-20], [FR-08][fr-08], [FR-11][fr-11]). Other options: allow reading one line ahead (contradicts [DEC-20][dec-20]).
- <a id="decided-runner-code-names"></a>**Decided: the Runner uses the code names fixed in spec 001** (`CommandParser.Parse(line)`; `ParseResult.CommandObject`, `ParseResult.Exit`, `ParseResult.Failure`; `RobotOperator.Run(command)`; `Outcome.PositionText`, `Outcome.Warning`, `Outcome.Done`). Reason: brief rule 6, and spec 001's Assumptions say the Runner uses them. Other options: describe the results only in words (leaves the names open and able to differ from spec 001).

[dec-03]: ../../docs/design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-04]: ../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-05]: ../../docs/design-analysis/design-decisions.md#dec-05-one-command-per-line
[dec-08]: ../../docs/design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-10]: ../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
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
[dec-36]: ../../docs/design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
[e2e-01]: #e2e-01
[e2e-02]: #e2e-02
[e2e-06]: #e2e-06
[e2e-07]: #e2e-07
[e2e-08]: #e2e-08
[e2e-09]: #e2e-09
[fr-01]: ../../docs/solution-design/02-functional-requirements.md#fr-01
[fr-03]: ../../docs/solution-design/02-functional-requirements.md#fr-03
[fr-07]: ../../docs/solution-design/02-functional-requirements.md#fr-07
[fr-08]: #fr-08
[fr-09]: ../../docs/solution-design/02-functional-requirements.md#fr-09
[fr-10]: ../../docs/solution-design/02-functional-requirements.md#fr-10
[fr-10a]: ../001-robot-rules/spec.md#fr-10a
[fr-11]: #fr-11
[fr-12]: #fr-12
[fr-13]: #fr-13
[fr-14]: #fr-14
[fr-15]: #fr-15
[fr-16]: #fr-16
[fr-17]: #fr-17
[nfr-01]: #nfr-01
[nfr-02]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-02
[nfr-03]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-03
[nfr-04]: #nfr-04
[nfr-05]: #nfr-05
[nfr-06]: #nfr-06
[nfr-08]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-08
[nfr-09]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-09
[pg-01]: #pg-01
[pg-02]: #pg-02
[sc-001]: #sc-001
[sg-01]: #sg-01
[td-01]: ../003-delivery/spec.md#td-01
[td-02]: ../003-delivery/spec.md#td-02
[ut-04]: ../001-robot-rules/spec.md#ut-04
[wf-01]: #wf-01
[wf-02]: #wf-02
[wf-03]: #wf-03
