# Data model: Console session

This spec has no stored data. Its "entities" are the two components it builds, the session state the Runner keeps while it runs, and the test helpers. Spec 001's types (Table, Direction, RobotState, Robot, `IRobotCommand` and the five command classes, `IRobotOperator`, RobotOperator, CommandParser, the parse result and the outcome) are used exactly as spec 001's plan defines them and are not restated here.

## Runner (page 6; DEC-26, DEC-29, DEC-30)

| Member | Type | Notes |
|---|---|---|
| constructor | `Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)` | `output` is console output (stdout), `error` is console messages (stderr) ([DEC-29][dec-29]) |
| CommandParser | created by the Runner | the parser has no settings or state, so it is not passed in ([DEC-26][dec-26]) |
| first-line flag | `bool` | true until the first line has been read; used only to remove a byte-order mark ([FR-11][fr-11]) |
| `Run()` | `void` | runs the session; returns at EXIT or at the end of the input ([FR-15][fr-15], [FR-16][fr-16]) |

**What the Runner writes** (exact strings, [DEC-21][dec-21], [DEC-22][dec-22], [DEC-24][dec-24], [DEC-25][dec-25]):

| When | Channel | Text |
|---|---|---|
| Before each read, `interactive` = true | `error` | `> ` (no line break) |
| After each read, `interactive` = false | `error` | `> ` + the line with whitespace removed from both ends, then a line break |
| Parse failure (`ParseResult.Failure`, `Reason`) | `error` | `Warning: ` + the reason, then a line break |
| Outcome: position text (`Outcome.PositionText`) | `output` | `Output:`, a line break, the position text, a line break |
| Outcome: warning (`Outcome.Warning`) | `error` | `Warning: ` + the warning text, then a line break |
| Outcome: done with nothing to show (`Outcome.Done`) | none | nothing |
| EXIT (`ParseResult.Exit`) | none | nothing; `Run()` returns |
| End of input | none | nothing; `Run()` returns |

**Session states** (one session, one direction only):

```text
Reading ──(line read)──> Processing ──(failure / command outcome)──> Reading
Reading ──(end of input)──> Ended
Processing ──(EXIT)──> Ended
```

The robot's own state (on the table or not, its position) is held by RobotOperator (spec 001), not by the Runner.

## Program (page 6, page 7.7; DEC-03, DEC-04, DEC-24, DEC-26, DEC-32)

| Member | Type | Notes |
|---|---|---|
| `Main(string[] args)` | `static int` | decides `interactive`, chooses the reader, returns what `Run` returns for that reader, `Console.Out` and `Console.Error` |
| `Run(string[] args, TextReader input, TextWriter output, TextWriter error, bool interactive)` | `internal static int` | the argument check, the parts and the Runner, inside the safeguard; returns the exit code ([PG-01][pg-01], [PG-02][pg-02]) |

| Input | What Program does | Exit code |
|---|---|---|
| any argument | writes `How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)` and a line break to `error` (`Console.Error`); reads nothing | 1 |
| no argument, console input redirected | `Table(5, 5)`, `RobotOperator(table)`, `interactive` = false, its own UTF-8 reader over console input, `Runner(reader, Console.Out, Console.Error, robotOperator, false).Run()` | 0 |
| no argument, a person typing | the same, with `interactive` = true and `Console.In` | 0 |
| any exception escapes the work above (not caused by any input) | writes `Error: unexpected problem - <exception message>` (the exception's `Message`) and a line break to `error`, and nothing more; if that write throws, it is ignored | 2 |

## Test helpers (test projects only)

| Helper | Project | Holds | Gives back |
|---|---|---|---|
| `SessionHarness` | `ToyRobot.Tests` | the input text, `interactive`, the table width and height (default 5 x 5) | the text written to `output` (stdout) and to `error` (stderr), with `"\n"` line breaks |
| `FailingReader`, `FailingWriter` | `ToyRobot.Tests` | nothing; every read (`ReadLine`) or write throws `IOException` with the message `boom` | used by the [SG-01][sg-01] tests to stand for a fault outside the application |
| `AppProcess` | `ToyRobot.ConsoleEndToEndTests` | the bytes for console input and the arguments | console output (stdout) text, console messages (stderr) text, exit code |
| `LineComparer` | `ToyRobot.ConsoleEndToEndTests` | a text | its lines, split on CRLF or LF, without the empty element after a final line break |
| Test-data pair | `testdata/` (spec 003) | `<name>.txt` (input) and `<name>.expected.txt` (expected console output, stdout) | used by [E2E-01][e2e-01] |

[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-24]: ../../docs/design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../../docs/design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-26]: ../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-29]: ../../docs/design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[e2e-01]: spec.md#e2e-01
[fr-11]: spec.md#fr-11
[fr-15]: spec.md#fr-15
[fr-16]: spec.md#fr-16
[pg-01]: spec.md#pg-01
[pg-02]: spec.md#pg-02
[sg-01]: spec.md#sg-01
