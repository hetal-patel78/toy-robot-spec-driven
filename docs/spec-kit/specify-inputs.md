# Spec Kit: inputs given to each command

The text passed to each Spec Kit command in the implementation run ([DEC-36][dec-36], spec-kit-brief.md). Every command
also follows the rules in `docs/spec-kit/spec-kit-brief.md`, which the runs were told to read first.

## /speckit-constitution

The DEC-36 principles: clean code and SOLID (DEC-34); the three test levels, tests first (DEC-31); never
crash (DEC-32); cross-platform (DEC-33); no over-engineering (DEC-38, DEC-39); standard output for REPORT
output only (DEC-21, DEC-23). Detailed rules stay in design-decisions.md; the constitution points to it.

## /speckit-specify 001-robot-rules (short name `robot-rules`)

Robot rules for the Toy Robot Simulator. Owns FR-01 to FR-07, FR-09, FR-10 and NFR-07 from
docs/solution-design/10-traceability.md, and the solution and project skeleton (ToyRobot.sln at the root,
src/ToyRobot, tests/ToyRobot.Tests, tests/ToyRobot.ConsoleEndToEndTests, xUnit, .NET 8 with RollForward =
LatestMajor) because specs 002 and 003 build on it. Components: Table, Direction, RobotState, Robot,
IRobotCommand with PlaceCommand, MoveCommand, LeftCommand, RightCommand and ReportCommand, IRobotOperator,
RobotOperator and CommandParser, exactly as on solution design page 6. Tests it plans: unit tests. Sources:
design-decisions.md and solution design pages 2, 3, 5, 6, 7.1 to 7.5 and 10. Acceptance scenarios are
copied from page 2 with exact input, console output (stdout) and console messages (stderr) stated
separately, plus the DEC-31 cases this spec's components own. Every warning text is copied character for
character from DEC-22. Every requirement cites its FR or NFR and DEC IDs. No behaviour beyond the sources.

## /speckit-specify 002-console-session (short name `console-session`)

The console session of the Toy Robot Simulator. Owns FR-08, FR-11 to FR-17, NFR-01, NFR-04, NFR-05 and
NFR-06 from docs/solution-design/10-traceability.md. Components: Runner and Program, exactly as on solution
design page 6; it uses spec 001's types and does not restate their rules. Tests it plans: Runner whole-flow tests
in memory covering every FR as console text, and console end-to-end tests of the built application (exit codes, the
usage hint, test-data files, the byte-order mark). Sources: design-decisions.md and solution design pages 2,
3, 5, 6, 7 (all), 9 and 10. Acceptance scenarios are copied from page 2 with exact input, console output
(stdout), console messages (stderr) and exit code stated separately, plus the DEC-31 cases this spec owns.
The usage hint (DEC-04), the `Output:` format (DEC-21), the prompt `> ` (DEC-24) and the echo (DEC-25) are
copied character for character. Every requirement cites its FR or NFR and DEC IDs.

## /speckit-specify 003-delivery (short name `delivery`)

Delivery of the Toy Robot Simulator. Owns NFR-02, NFR-03, NFR-08 and NFR-09 from
docs/solution-design/10-traceability.md. Covers the run scripts run.cmd and run.sh (DEC-37), the test-data
files in testdata/ including the three challenge examples with matching expected-output files, the GitHub Actions
CI workflow on windows-latest, ubuntu-latest and macos-latest (DEC-33), .gitattributes, and the
README (DEC-37). It needs the built application and spec 002's console end-to-end tests. Tests it plans: CI runs on
all three systems; each run script runs with a challenge example and its console output (stdout) is compared with
the expected file line by line; the script error cases are checked by exit code. Sources:
design-decisions.md and solution design pages 3, 5, 9 and 10. Every requirement cites its NFR and DEC IDs.

[dec-36]: ../design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
