# Run report

Report of the Spec Kit run (`docs/spec-kit/spec-kit-brief.md`). Updated after every step.

**Status:** finished. Steps 1 to 12 are done: the specification, all three specs implemented (455 tests passing), real runs, CI green on Windows, Linux and macOS, and `main` pushed after a separate review approved. **Left:** the developer's checks in section 8.

## 1. Status

| Item | Status | Files |
|---|---|---|
| Constitution | Done (v1.0.0) | `.specify/memory/constitution.md` |
| 001-robot-rules | Done: specified, clarified, planned, tasks (58), reviewed, checked | `specs/001-robot-rules/` (spec, plan, research, data-model, contracts, quickstart, tasks, checklists) |
| 002-console-session | Done: specified, clarified, planned, tasks, reviewed, checked | `specs/002-console-session/` (same files) |
| 003-delivery | Done: specified, clarified, planned, tasks, reviewed, checked | `specs/003-delivery/` (same files) |
| Specification checks script | Done | `docs/spec-kit/checks/Test-Specification.ps1` |

### Worktrees and branches

All worktrees and branches made in this run are left in place.

| Worktree | Branch | Purpose |
|---|---|---|
| `.claude\worktrees\permission-mode-file-commands-4c4204` | `claude/permission-mode-file-commands-4c4204` | Working checkout: constitution, creating the specs, merging the three spec branches, cross-check and checks |
| `.claude\worktrees\specs` | (detached) | First home of the `specs` branch; detached so the `specs` branch could be moved forward without touching its files |
| `.claude\worktrees\wf_*` | `worktree-wf_*` | Isolated worktrees made by the workflow agents for the parallel spec work |
| — | `001-robot-rules`, `002-console-session`, `003-delivery` | Each spec's parallel work |
| — | `specs` | The three specs merged, cross-checked and checked |

Notes on the worktrees:
- The brief names `C:\Dev\codefoundry-<slug>`. At the developer's request the worktrees are inside the
  repository folder, in `.claude\worktrees\` (listed in `.git/info/exclude`, so never
  committed). The `specs` worktree was first made at `C:\Dev\codefoundry-specs` and moved with
  `git worktree move`.
- The Claude desktop app does not let a session edit files in another worktree, so the session's own
  worktree is the working checkout and each parallel line of work ran in its own isolated worktree.
- An uncommitted test file `docs/spec-kit/.probe` and an untracked copy of `docs/spec-kit/specify-inputs.md`
  are in the `specs` worktree. Nothing is deleted in this run, so they are left for the developer.
- The first parallel workflow was stopped after a few minutes and restarted with a corrected git merge
  step; its three empty worktrees (`wf_6748e8ca-cca-1` to `-3`) are left in place.

### Spec Kit commands

Spec Kit 1.1.1.dev0 provides its commands as skills in `.claude/skills/`; each run read that skill's
`SKILL.md` and followed it. Inputs: [specify-inputs.md](specify-inputs.md).

| Step | Command | Run for |
|---|---|---|
| 2 | `/speckit-constitution` | the project |
| 3 | `/speckit-specify` | 001, 002, 003, one after another in the working checkout |
| 4.1 | `/speckit-clarify`, `/speckit-plan`, `/speckit-tasks` | each spec, in parallel, each in its own worktree |
| 4.2, 6 | `/speckit-analyze` | each spec after every review round, and once more on the merged specs |

## 2. Check results

Final run on the merged specs (`docs/spec-kit/checks/Test-Specification.ps1`, then `/speckit-analyze`):

| # | Check | Result | Details |
|---|---|---|---|
| 1 | Coverage | Pass | [FR-01][fr-01] to [FR-17][fr-17] and [NFR-01][nfr-01] to [NFR-09][nfr-09] each owned by exactly one spec, as in the brief; 68 spec-local requirement IDs each cite an FR or NFR |
| 2 | Exact strings | Pass | 239 warning texts match the [DEC-22][dec-22] patterns (the reason for each quoted line is also worked out from [DEC-08][dec-08]/[DEC-22][dec-22] and compared); all 10 [DEC-22][dec-22] texts appear verbatim; how-to-run message, `Output:`, prompt and echo match |
| 3 | Exit codes and channels | Pass | 65 scenarios parsed; stdout holds only `Output:` and a position; every warning sits under its echo line; page 2's text found for all 17 FRs |
| 4 | Names | Pass | 1361 identifiers in the plans, data models and contracts are page 6 names, decided code names, .NET/xUnit names or test helpers; decided names agree across the three specs |
| 5 | [DEC-31][dec-31] coverage | Pass | 72 of 72 cases have a planned test task |
| 6 | Analyze | Pass | No CRITICAL or HIGH: 001 (2 medium, 3 low), 002 (1 medium, 4 low), 003 (3 medium, 7 low) |
| 7 | No code yet | Pass | No file under `src/` or `tests/` changed in the specification phase |

The first version of the checks script only tested that key words appeared. It was rewritten to compare
the texts exactly, and shown to catch 12 deliberately introduced mistakes before being trusted.

## 3. Decided in the run

While the specs were written, about 70 questions came up that the sources (the design decisions, the
solution design and the brief) do not answer, or answer in two ways. Each was decided in the run and is
listed below. Each is also written out in full in that spec's **Assumptions** section (what the sources
say, what was chosen and why, and the other options); every item below links to its entry there.

**Status:** every item below is confirmed by the developer: items 1 to 7 one at a time ([P153][p153] to [P155][p155]); the Table size, a command run with no placed robot, `Robot.Replace` refusing a missing state, the one position text and the placeholder entry point after a walk-through ([P158][p158] to [P160][p160]); the rest as they stand ([P160][p160]). None of them changes behaviour, so design-decisions.md is unchanged.

Words used in this section:
- **Spec 001, 002, 003** are the three specifications: 001 the robot's rules, 002 the console session
  (reading input, printing), 003 the delivery (test data, run scripts, CI, README). **Page N** means page N
  of the solution design (`docs/solution-design/`).
- **Console output (stdout)** is the results the app prints: the `Output:` line and the position.
- **Console messages (stderr)** is everything else the app prints: the `> ` prompt, the echo of each input
  line when input is redirected from a file, the warnings, the how-to-run message and the one-line error
  message.
- **Exit code** is the number the app hands back to the shell when it ends: 0 for a normal end, 1 when it
  printed the how-to-run message, 2 for an unexpected problem.
- **End-to-end test** starts the real, built application as a separate process and checks what it prints.
  **Whole-flow test** runs a whole session in memory, inside the test program, and checks everything the
  session wrote.
- **CI** is the GitHub Actions runs that build and test the project on Windows, Linux and macOS after every
  push.
- **Runner** is the class that reads each input line and writes the console text; **Program** is the
  entry point that starts it and returns the exit code.

**Most worth checking** (they shape behaviour or tests). Each item ends with its sources.
1. **End-to-end tests check the printed results and the exit code, not the other console messages.**
   Confirmed by the developer ([P153][p153]). The end-to-end tests start the real app and compare the
   results it prints (`Output:` and the position, line by line) and its exit code; they look at the other
   messages only for the how-to-run message. The echo, the prompt and the warnings are already checked
   exactly by the whole-flow tests, and a file starting with a byte-order mark (an invisible marker some
   editors add, which the app must ignore) is still proved end-to-end, because if the mark were not removed
   the first line would be an unknown command and no position would be printed.
   *Sources:* [spec 002, Assumptions][decided-002-e2e-compare-stdout]; page 9; page 10.
2. **CI compares the printed results and the exit codes of the script runs, and the scripts' progress
   lines in a normal run.** Confirmed by the developer ([P153][p153]). CI runs `run.cmd` and `run.sh` on each
   test-data file and compares the saved printed results with the expected file, and the exit code. Other
   messages are checked only in the script's error cases (missing file, no .NET SDK, failed build), where
   the error text is what [DEC-37][dec-37] says is shown. A normal run's progress lines are checked too
   ([CI-07][ci-07], added with [P141][p141]).
   *Sources:* [spec 003, Assumptions: how CI compares][decided-003-ci-comparison],
   [spec 003, Assumptions: CI-01 and CI-03][decided-003-ci01-ci03-stdout-only],
   [spec 003, Assumptions: User Story 1 scenarios][decided-003-us1-stderr-reference]; [DEC-33][dec-33];
   [CI-07][ci-07] in spec 003.
3. **Matching letter case and writing numbers do not depend on the machine's regional settings.** Confirmed by the developer ([P155][p155]). Command
   words are matched ignoring case with fixed rules, and numbers are written with fixed rules, so the result
   is the same on every machine ([DEC-08][dec-08]). Two tests run under Turkish and Swedish settings, where
   the usual rules differ (Turkish capital I, Swedish minus sign).
   *Sources:* [spec 001, Assumptions][decided-001-culture-independent]; [NFR-04][nfr-04].
4. **Code names for the things the solution design describes but does not name.** Confirmed by the developer ([P154][p154]). Specs 001 and 002 must use
   the same names, so they are taken from page 6's own wording: `ParseResult` (`CommandObject`, `Exit`,
   `Failure`), `Outcome` (`PositionText`, `Warning`, `Done`), `Parse`, `Run`, `Placement`, `Replace`,
   `IsOnTable`, `Width`, `Height`, `RobotState(X, Y, Facing)`, `Direction` with `North`/`East`/`South`/`West`,
   `All`, `Name`, `TurnLeft`, `TurnRight`. They become the public shape of the code.
   *Sources:* [spec 001, Assumptions][decided-001-code-names]; research R4 in spec 001.
5. **Direction is a class with four fixed instances, not an enum.** Confirmed by the developer ([P154][p154]). It has `Name`, `TurnLeft()`,
   `TurnRight()` and `All`; a direction given as a number (`PLACE 1,2,1`) is an invalid direction. The
   one-step offsets stay in `MoveCommand`, because page 6 says Direction does not know positions.
   *Sources:* [spec 001, Assumptions][decided-001-direction-class]; research R7 in spec 001.
6. **The test-data set.** Confirmed by the developer ([P154][p154]). Six pairs of files, each `<name>.txt` (input) with `<name>.expected.txt` (expected
   printed results): `challenge-example-1/2/3`, `bom-challenge-example-1`, `exit-mid-file` and `empty`. They cover the
   three challenge examples, an input with a byte-order mark, an EXIT in the middle of a file and an empty input.
   *Sources:* [spec 003, Assumptions][decided-003-testdata-set]; [DEC-37][dec-37]; [DEC-31][dec-31].
7. **The README is the existing root `README.md`, changed in place.** Confirmed by the developer ([P154][p154]). It describes the design stage
   today. It becomes the README, keeping its "Where to look" links and updating its "Status"
   text; nothing is moved or deleted. A second README at the root would confuse a reviewer.
   *Sources:* [spec 003, Assumptions][decided-003-readme-root].

**All items, by spec.** Items 1 to 7 above are repeated here in one line each, pointing back to them.

**001-robot-rules**
- **Spec 001's scenarios show console text, but only spec 002 tests it.** The sources want each scenario
  copied from page 2 with the printed results and the other messages, but the classes that print them (Runner
  and Program) belong to spec 002. So spec 001's unit tests check the parse result and the outcome texts, and
  spec 002's whole-flow tests check the console text. *Sources:* [spec 001][decided-001-console-text]; brief
  rules 3 and 5; [DEC-31][dec-31].
- **Spec 001 covers recognising EXIT; spec 002 covers what the session does with it.** The parser that
  recognises EXIT is a spec 001 part, but "EXIT ends the session" ([FR-15][fr-15]) is spec 002's, so spec 001
  specifies only the parse result EXIT and its unit tests. *Sources:* [spec 001][decided-001-exit-in-parser];
  [DEC-08][dec-08]; [DEC-10][dec-10].
- **The inputs of the hand-traced Brainstorm examples come from `review-trail.md`.** [DEC-31][dec-31] names
  those examples only by their result (for example 2,1,NORTH), so their input lines are taken from the
  "Hand-traced examples checked" table in `docs/design-analysis/review-trail.md`, each ended with REPORT so
  its end state can be checked. *Sources:* [spec 001][decided-001-brainstorm-inputs]; [DEC-31][dec-31].
- **Edge and corner positions: the middle of each edge, and both outward directions at each corner.**
  [DEC-31][dec-31] asks for "every edge and corner" without naming positions; testing every position on every
  edge would add tests but cover no extra rule. The edge cases are 2,4,NORTH; 2,0,SOUTH; 4,2,EAST; 0,2,WEST.
  *Sources:* [spec 001][decided-001-edge-corner-positions]; [DEC-31][dec-31].
- **Spec 001 creates the empty end-to-end test project.** The brief gives spec 001 "the three projects", but
  the end-to-end tests are planned by spec 002, so spec 001 adds `tests/ToyRobot.EndToEndTests/` as a project
  that builds and has no tests yet. *Sources:* [spec 001][decided-001-e2e-project-skeleton].
- **The two test projects also set `RollForward`.** [DEC-37][dec-37] makes the application run on a newer
  .NET when 8 is not installed (`RollForward` = `LatestMajor`); the test projects get the same setting so
  `dotnet test` from the README works on such a machine. The application's behaviour does not change.
  *Sources:* [spec 001][decided-001-rollforward-tests]; [DEC-37][dec-37]; [NFR-09][nfr-09].
- **Code names** (item 4 above). *Sources:* [spec 001][decided-001-code-names].
- **Culture-independent comparison and number writing** (item 3 above).
  *Sources:* [spec 001][decided-001-culture-independent].
- **A placeholder entry point until spec 002.** A console app needs an entry point to build, but Program is
  spec 002's, so spec 001 adds `src/ToyRobot/Program.cs` that reads nothing, writes nothing and returns exit
  code 0, and spec 002 replaces it. The template's "Hello, World!" is not used, because it prints to console
  output, against [DEC-23][dec-23]. *Sources:* [spec 001][decided-001-placeholder-entry-point].
- **`Robot.Replace` refuses a missing state, at compile time and at run time.** [DEC-14][dec-14] says it
  accepts only a real state, never `null`. The parameter is declared non-null and a `null` is also refused
  at run time with an argument error, with a unit test; no input line can reach this.
  *Sources:* [spec 001][decided-001-replace-refuses-null]; [DEC-14][dec-14].
- **The Table does not check its own size.** Page 6 gives the Table only a width, a height and the on-table
  rule, so a size of 0 or less is not refused (no source asks for that); Program always passes 5 and 5.
  *Sources:* [spec 001][decided-001-table-size].
- **One text form for a position, `X,Y,F`, written by `RobotState`.** [DEC-22][dec-22] requires the blocked
  MOVE warning to use the same form as REPORT (and the off-table PLACE warning too), so `RobotState.ToString()`
  gives it and the three commands do not each format it. *Sources:* [spec 001][decided-001-position-text];
  [DEC-22][dec-22].
- **Direction as a class** (item 5 above). *Sources:* [spec 001][decided-001-direction-class].
- **A command run directly with no placed robot throws, but no input can cause that.** If `MoveCommand`,
  `LeftCommand`, `RightCommand` or `ReportCommand` is called with no robot state, it raises an
  invalid-operation error instead of a warning. RobotOperator checks first ([FR-07][fr-07], [DEC-27][dec-27])
  and tests prove it never runs them unplaced, and input lines never cause an exception.
  *Sources:* [spec 001][decided-001-command-without-robot].
- **A `.gitignore` for build output.** The repository had none, so the skeleton adds one listing `bin/`,
  `obj/`, `TestResults/` and `.vs/`. *Sources:* [spec 001][decided-001-gitignore].
- **User Story 4 scenario 8 shows console text for reference.** It states the console text of page 2's
  [FR-15][fr-15] example so every scenario shows both the printed results and the other messages; spec 002
  owns and tests it. *Sources:* [spec 001][decided-001-us4-scenario-8].
- **Every SK and UT requirement names the NFR it supports.** This adds citations only, so each requirement
  traces to an FR or NFR; no behaviour changes. *Sources:* [spec 001][decided-001-requirements-cite-nfr];
  [NFR-06][nfr-06]; [NFR-01][nfr-01].
- **Every task names its FR or NFR ID and its DEC IDs.** The 19 tasks that lacked one or both now cite them,
  as brief rule 4 and the constitution require. *Sources:* [spec 001][decided-001-tasks-cite-ids].
- **How the public-type test is handed to spec 002.** A spec 001 test lists exactly which types are public.
  Spec 002 adds `Runner` to that list in its task T020a, just before it creates the Runner, and makes Program
  internal, so the test stays green. This is written in tasks.md. *Sources:* [spec 001][decided-001-public-type-handoff].

**002-console-session**
- **End-to-end tests check the printed results and the exit code** (item 1 above, confirmed by the developer,
  [P153][p153]). *Sources:* [spec 002][decided-002-e2e-compare-stdout].
- **The start-up checks get their own test ID, [E2E-09][e2e-09].** The redirected-REPORT check and the 5 x 5
  table check (T018) live in `StartUpTests.cs` under that ID, instead of being mixed into
  [E2E-06][e2e-06]. *Sources:* [spec 002][decided-002-e2e-09-startup].
- **[E2E-09][e2e-09] checks the 5 x 5 table through the printed results only.** Checking the other messages
  as well would contradict page 10. *Sources:* [spec 002][decided-002-e2e-09-stdout-only].
- **Namespaces follow the folders, as in spec 001.** Runner and Program are in `ToyRobot`; the whole-flow tests
  and `SessionHarness` in `ToyRobot.Tests.WholeFlow`; the end-to-end tests, `AppProcess` and `LineComparer` in
  `ToyRobot.EndToEndTests`. *Sources:* [spec 002][decided-002-namespaces].
- **User Story 1 scenario 5 lists the other messages for reference only.** They (the echo and warnings) are
  not compared with a file, because a test-data pair holds only the expected printed results; expected-message
  files would go beyond page 9 and spec 003. *Sources:* [spec 002][decided-002-us1-scenario-5].
- **Program is `internal static`; only Runner is added to spec 001's public-type list.** The tests reach
  Program's internal `Run` through `InternalsVisibleTo` in the project file, so Program stays internal and
  spec 001's test needs only Runner added (task T020a). *Sources:* [spec 002][decided-002-program-internal].
- **Four tests pass before Program is written.** T015 passes because `testdata/` has no file pairs yet; T017's two-commands test, T018's
  redirected-REPORT test and T019's empty-input test only check empty printed results and exit code 0,
  which spec 001's placeholder Program already gives. So all four guard Program (T022) instead of failing first.
  *Sources:* [spec 002][decided-002-phase3-passing-tests].
- **Test T027 passes from the start and guards T030.** `EXITNOW` and `EXITING` are parse failures handled
  since T021, so T027 passes before T030 and checks that T030 stops only on exactly EXIT.
  *Sources:* [spec 002][decided-002-t027-guard].
- **Spec 002 is finished and tested before spec 003 adds the test-data files.** [E2E-01][e2e-01] runs every
  input/expected pair found in `testdata/` and passes when there are none; the other end-to-end tests
  ([E2E-02][e2e-02] to [E2E-09][e2e-09]) create their input inside the test, for example a temporary file
  with the byte-order mark written as bytes. *Sources:* [spec 002][decided-002-testdata-and-e2e].
- **Line breaks: the prompt has none; every other line is a full line.** The Runner writes each echo, warning,
  `Output:` label and position as a full line, and the prompt `> ` without a line break; Program writes the
  how-to-run message as one full line. *Sources:* [spec 002][decided-002-line-breaks]; [FR-13][fr-13].
- **The end-to-end tests start the build made by the same `dotnet test`.** The end-to-end project references
  the application project, so `dotnet test` builds it first, and the tests start its executable from
  `src/ToyRobot/bin/<configuration>/net8.0/` for the configuration under test (Debug unless built in
  Release). So `dotnet test` from the repository root needs no earlier step.
  *Sources:* [spec 002][decided-002-which-build]; page 5.
- **Names of the Runner and Program methods.** Page 6 does not name them, so: `Runner.Run()` returns nothing;
  `static int Main(string[] args)` returns the exit code given by Program's internal `Run(args, input, output,
  error, interactive)`, which holds the work so tests can call it in memory.
  *Sources:* [spec 002][decided-002-method-names].
- **Exit codes in in-memory scenarios.** Page 10 says the end-to-end tests check exit codes, so a whole-flow test checks that
  `Run()` returns normally at EXIT or at the end of input, and the scenario's exit code is what Program then
  returns; the unexpected-problem scenarios (User Story 5, scenarios 4 to 6) call Program's `Run`, so their
  exit code 2 is checked in memory. *Sources:* [spec 002][decided-002-exit-codes-in-memory].
- **[E2E-01][e2e-01] is one test over all pairs and passes when there are none.** Spec 002 is built before
  spec 003 adds `testdata/`, and a test with no cases counts as a failure in xUnit, so one case per pair
  would stop spec 002 ending green; spec 003's [TD-02][td-02] guards the challenge examples.
  *Sources:* [spec 002][decided-002-e2e-01-no-testdata].
- **Tests for CRLF versus LF input and for echo whitespace.** The edge case "CRLF and LF give the same
  result" and [DEC-31][dec-31]'s "echo content with redirected input" had no named test, so
  [WF-02][wf-02], [WF-03][wf-03] and [E2E-07][e2e-07] are added. The test-data files are stored with LF, so
  CRLF input would otherwise never run. *Sources:* [spec 002][decided-002-line-endings-tests].
- **No catch-all in the Runner (a safeguard in Program was added later).** No component throws for any input
  and the Runner catches nothing, so a defect fails the tests instead of being hidden. The spec entry also
  records the later decision ([DEC-32][dec-32]): one last safeguard in Program's `Run` turns an escaped
  exception into the line `Error: unexpected problem - <message>` and exit code 2 (see "Changes after the run").
  *Sources:* [spec 002][decided-002-program-safeguard]; [NFR-01][nfr-01]; [DEC-32][dec-32].
- **A test that an argument stops all input reading.** [FR-17][fr-17] says Program must not read input when
  given an argument, so [E2E-08][e2e-08] gives the console a line together with the argument and checks there
  is no echo. *Sources:* [spec 002][decided-002-args-stop-reading].
- **User Story 3 scenario 3 reuses T035's input.** T035 already tests exactly this input, so no task changes.
  *Sources:* [spec 002][decided-002-us3-scenario-3].
- **No second project reference.** The end-to-end project keeps the one reference to the application that
  spec 001 T005 adds; a second would give NuGet warning NU1504. *Sources:* [spec 002][decided-002-no-second-reference].
- **Every task names its FR or NFR ID and its DEC IDs.** Tasks that only build and run tests cite
  [NFR-06][nfr-06] and [DEC-31][dec-31]. *Sources:* [spec 002][decided-002-tasks-cite-ids].
- **[SC-001][sc-001] is measured at the level that checks each scenario.** Whole-flow tests compare the
  printed results and the other messages; end-to-end tests compare the printed results line by line, the
  other messages only for the how-to-run message, and the exit code; in-memory scenarios have no exit code.
  Requiring all three everywhere cannot be met. *Sources:* [spec 002][decided-002-sc-001-levels]; page 10.
- **The Runner reads the next line only after finishing the previous one.** The Runner contract says so
  ([DEC-20][dec-20], [FR-08][fr-08], [FR-11][fr-11]); reading one line ahead would contradict [DEC-20][dec-20].
  *Sources:* [spec 002][decided-002-no-read-ahead].
- **The Runner uses spec 001's code names** (item 4 above), so the two specs cannot differ (brief rule 6).
  *Sources:* [spec 002][decided-002-runner-code-names].

**003-delivery**
- **The test-data set** (item 6 above). *Sources:* [spec 003][decided-003-testdata-set].
- **CI runs all three challenge examples through the scripts.** [DEC-33][dec-33] asks for "a challenge example file";
  CI runs all three on each system because the README gives ready-to-copy commands for all three, so each is
  proved everywhere. *Sources:* [spec 003][decided-003-challenge-examples-in-ci]; [DEC-33][dec-33].
- **On Windows, CI runs `run.cmd` from cmd, not PowerShell.** Windows PowerShell 5.1 saves `>` output as
  UTF-16 ([DEC-23][dec-23]), so the saved results would differ byte for byte; the PowerShell form is checked
  by hand in the real runs and the README check. *Sources:* [spec 003][decided-003-windows-shell];
  [DEC-37][dec-37].
- **The script error cases are checked in CI without depending on where the SDK is installed.** Page 3 says
  they are checked by exit code but not where. [CI-04][ci-04] runs a script with a `PATH` that has no `dotnet`
  (and with a stand-in that lists only a version 7 SDK); [CI-05][ci-05] breaks the build in a temporary copy
  of the repository, never in the checkout; [CI-03][ci-03] feeds a test-data file to the script's console
  input with no argument, because CI cannot type. The same cases are also run by hand on Windows.
  *Sources:* [spec 003][decided-003-error-paths-checks].
- **CI runs every test-data file through each script.** [CI-01][ci-01] runs all six inputs through each
  script on each system (18 comparisons), so the empty input and the byte-order mark are proved through the
  scripts as well as end-to-end. *Sources:* [spec 003][decided-003-every-testdata-file]; [DEC-33][dec-33].
- **The scripts build only the application, in Release, and stay quiet unless the build fails.** They build
  `src/ToyRobot/ToyRobot.csproj`, hide the .NET check and build output unless the build fails, and give the
  build no console input. On success they show only their progress lines (a later decision, see "Changes
  after the run"). Building the whole solution would also build the tests, which is slower, and a test error
  would stop a reviewer running the app. *Sources:* [spec 003][decided-003-script-build].
- **How a script decides that a .NET SDK is installed.** `dotnet --list-sdks` must list major version 8 or
  more, and the message names `https://dotnet.microsoft.com/download`; `dotnet --version` was rejected because
  it reports the SDK for the current folder, which can be lower than an installed one.
  *Sources:* [spec 003][decided-003-sdk-check].
- **A folder given as the file argument counts as "file not found".** It gives that message and exit code 1
  ([CI-02][ci-02]); otherwise the message and exit code would differ by shell.
  *Sources:* [spec 003][decided-003-folder-not-file].
- **How CI compares printed results.** The saved results and the expected file are compared with carriage
  returns removed, in a bash step on every system (Git Bash on Windows), so Windows and Linux line endings
  both pass ([DEC-31][dec-31]). *Sources:* [spec 003][decided-003-ci-comparison].
- **Spec 003 only checks the `.gitignore`, which spec 001 creates.** It checks that the four lines are there
  and `git status` shows no build output, adding only a missing line; no behaviour changes.
  *Sources:* [spec 003][decided-003-gitignore].
- **[CI-01][ci-01] and [CI-03][ci-03] compare the printed results and the exit code only.** The other messages
  are checked only in the script error cases, where [DEC-37][dec-37] says what is shown: [CI-02][ci-02] not
  empty, [CI-04][ci-04] contains the download address, [CI-05][ci-05] contains `CS1029`. Since then
  [CI-07][ci-07] also checks the progress lines of a normal run.
  *Sources:* [spec 003][decided-003-ci01-ci03-stdout-only]; [DEC-33][dec-33]; item 2 above.
- **One run of a script from another folder.** [CI-01][ci-01] runs one script from inside `testdata` and
  compares the result with `challenge-example-1.expected.txt` (exit code 0), proving the script finds the
  application relative to its own folder. *Sources:* [spec 003][decided-003-other-folder].
- **The local check of [CI-06][ci-06] (line endings) runs in a fresh clone.** The working copy was checked out
  with `core.autocrlf=true` before `.gitattributes` existed, so its text files stay CRLF on disk although the
  repository is right, and the usual fixes (a hard reset, deleting files) are not allowed (brief rule 12).
  The check runs in a new clone under `%TEMP%`, never in a reused folder.
  *Sources:* [spec 003][decided-003-local-ci06-check].
- **The shape of the CI workflow.** One workflow `CI`, run on every push, with one job over `windows-latest`,
  `ubuntu-latest` and `macos-latest`: one system failing does not cancel the others, and a hung step fails
  after 30 minutes instead of 6 hours. It uses `actions/checkout@v5` and `actions/setup-dotnet@v5` (.NET
  8.0.x), and runs the unit and whole-flow tests and the end-to-end tests as two steps, to show page 9's two
  levels. *Sources:* [spec 003][decided-003-workflow-shape]; research [R-05][r-05].
- **The `.gitattributes` content.** Three lines: `* text=auto eol=lf`, `*.cmd text eol=crlf` and
  `*.pdf binary`; the last makes the handling of the PDFs explicit. *Sources:* [spec 003][decided-003-gitattributes];
  research [R-10][r-10].
- **Where the test-data and build-settings tests live.** [TD-02][td-02] and [BS-01][bs-01] are in
  `tests/ToyRobot.EndToEndTests/DeliveryTests.cs`; the repository root is found by walking up to the folder
  with `ToyRobot.sln`, so spec 002 needs no shared helper. *Sources:* [spec 003][decided-003-td02-bs01-location];
  research [R-12][r-12].
- **The `DeliveryTests` namespace is `ToyRobot.EndToEndTests`,** following the folder-equals-namespace rule.
  *Sources:* [spec 003][decided-003-deliverytests-namespace].
- **Two repeatable checks, [BS-01][bs-01] and [CI-06][ci-06].** Page 3 checks [NFR-03][nfr-03] by review and
  states the line endings of [NFR-02][nfr-02] ([DEC-33][dec-33]) without a check. The review stays, and
  [BS-01][bs-01] (reads the project file) and [CI-06][ci-06] (git's view of line endings and `run.sh`'s
  executable mode) repeat both on every push. *Sources:* [spec 003][decided-003-bs01-ci06].
- **Where the scripts look for the solution and the file.** Each script finds the solution relative to its own
  folder, so it also works from another folder, and takes the file argument as given, relative to the current
  folder; the README always runs it from the repository root. *Sources:* [spec 003][decided-003-script-lookup].
- **The README** (item 7 above). *Sources:* [spec 003][decided-003-readme-root].
- **README stages and the Release build.** Spec 003's tasks do all the README stages in [DEC-37][dec-37]'s
  order (skeleton, commands as each works, badge and test-data list once CI passes). The implementation
  followed the README word for word on a fresh clone on Windows (cmd, PowerShell, Git Bash), and the developer
  repeats that before finishing. The build command builds Release, so the direct run commands use page 5's
  path. *Sources:* [spec 003][decided-003-readme-stages]; [DEC-37][dec-37].
- **How a cmd step saves an exit code.** The redirection is written first (`>"<file>" echo %ERRORLEVEL%`),
  because `echo %ERRORLEVEL%> file` reads a one-digit code before `>` as a handle number and saves nothing;
  saved codes are cleaned with `tr -d '\r\n '` before they are compared. *Sources:* [spec 003][decided-003-cmd-exit-code].
- **The [CI-06][ci-06] test is shown failing locally, not on GitHub.** T013a is committed but not pushed, its
  failure is shown in a fresh clone made with `core.autocrlf=true`, and T013a and T003 are pushed together,
  because the brief forbids pushing work that does not pass. *Sources:* [spec 003][decided-003-ci06-failing-locally].
- **User Story 1 scenarios 1, 4, 5a and 10 show the other messages for reference; CI does not compare them.**
  This matches spec 002 User Story 1 scenario 5 (page 10); comparing them would contradict page 9, page 10 and
  [DEC-33][dec-33]. *Sources:* [spec 003][decided-003-us1-stderr-reference].

**Changed in the run:** none. No change to design-decisions.md, the solution design, CLAUDE.md or the brief
was needed.

## 4. Fixed during review

001-robot-rules (2 review rounds; analyze then 0 CRITICAL/HIGH):
- An invisible non-breaking space in a note replaced by its escape; page 2's `PLACE 1,2,N` example given a
  scenario and a test; 19 tasks given their FR/NFR and DEC IDs; US4 scenario 8 given both channels; a
  helper's task reference corrected; the public-type test hand-off to spec 002 written down.

002-console-session (4 review rounds; round 2 had 1 HIGH, fixed):
- HIGH: spec 001's public-type test would break when the Runner was added; spec 002 now updates its list in
  a task just before the Runner is created, and Program is internal.
- The end-to-end channel rule aligned with page 10 (stdout only, stderr for the how-to-run message); a general
  scenario given concrete input and output; task citations; E2E ID ranges; tests-first statements made
  accurate where a test already passes with the placeholder Program.

003-delivery (4 review rounds; round 1 had 1 CRITICAL, round 2 1 HIGH, both fixed):
- CRITICAL: tests-first broken for the line-ending rule; the check is now written before `.gitattributes`.
- HIGH: `echo %ERRORLEVEL%> file` would be read by cmd as a handle redirection for one-digit codes; the
  exit code is now saved a safe way.
- Exit codes expanded too early inside a cmd loop; a local check run on a copy without the scripts; a
  fixed temporary folder reused by several checks; `.gitattributes` not applying to a first-written file;
  decisions in research.md copied into the Assumptions.

Cross-check of the three specs together (6 rounds, then clean of all but low wording items):
- HIGH: spec 002 and then spec 003 compared stderr end-to-end and in CI, against page 9, page 10 and [DEC-33][dec-33];
  both now compare stdout (stderr only for the how-to-run message and the script error cases).
- Spec 002 now uses spec 001's code names everywhere; namespaces fixed for all test projects; task
  dependency and hand-off wording made consistent; ID ranges aligned.

## 5. Review first

1. The end-to-end and CI channel rule (Decided items 1 and 2 above): it follows page 9, page 10 and [DEC-33][dec-33],
   but it means the echo and warnings are proved only in memory, not through the real console.
2. The code names and Direction-as-a-class (items 4 and 5): they become the public shape of the code.
3. The test-data set and the run-script checks in spec 003, including which script paths are checked by
   hand rather than in CI ([spec 003, Assumptions][decided-003-error-paths-checks]).

## 6. Implementation

Branch `impl`, in the session's worktree (the app allows this session to edit only its own worktree, so
`impl` is there rather than in a separate `impl` worktree).

| Spec | Status | Tests | Code review |
|---|---|---|---|
| 001-robot-rules | Done: T001 to T058 | Unit: 337 passing, 0 failing (`ToyRobot.Tests`); end-to-end project empty, as planned | Round 1 clean |
| 002-console-session | Done: T001 to T051 (with T020a) | Unit and whole-flow: 430 passing (includes 001's 337); end-to-end: 17 passing; 0 failing, in Debug and Release | Round 1 clean |
| 003-delivery | Done: T001 to T037 except T032 and the typed-session part of T035 (left for the developer) | Unit and whole-flow: 430; end-to-end: 25 (adds [TD-02][td-02] test-data presence and [BS-01][bs-01] build settings, and runs [E2E-01][e2e-01] over the six test-data pairs); 0 failing | Round 1 clean |

**Totals at the finish:** 455 tests, all passing: 430 in `ToyRobot.Tests` (unit and whole-flow), 25 in
`ToyRobot.EndToEndTests`. `dotnet build` gives 0 warnings and 0 errors.

Notes from spec 001:
- Each phase's tests were run before its code: Phase 2 did not compile, Phases 3 and 4 failed, Phases 5
  and 6 failed 10 and 66 tests; all pass after the code.
- Small implementation choices: scenario inputs copied from spec.md; a tuple element named `Remainder`
  (`Rest` is reserved in C#); parser rows shared through xUnit `MemberData`.

Notes from spec 002:
- T052 (follow the quickstart by hand, including a typed session ended with Ctrl+Z / Ctrl+D) is left
  unchecked: a typed session needs a real console. The redirected-input, channel and how-to-run-message parts were
  run by hand from bash and were correct. Left for the developer.
- The end-to-end tests start the built executable (`ToyRobot.exe` on Windows, `ToyRobot` elsewhere) for
  the configuration under test; they passed here in Debug and Release.
- After the main review, the byte-order mark in `Runner.cs` and one whole-flow test was changed from an
  invisible literal character to the escape ``\uFEFF``; all 447 tests pass.
- Small choices: `AppProcess.FindRepositoryRoot` is public so the test-data test reuses it; `Program.Main`
  is private inside `internal static class Program`.

Notes from spec 003:
- One task was wrong and fixed through the spec first (brief 9.3): on `windows-latest`, `tar -x -C` rejected
  the Windows form of `RUNNER_TEMP` in the [CI-05][ci-05] broken-copy step; the folder is now resolved with
  `$(cd "$RUNNER_TEMP/broken" && pwd)` (T019, research [R-08][r-08]).
- Small choices: one run step per system family plus one bash check step; `actions/checkout@v5` and
  `actions/setup-dotnet@v5`.
- The README is longer than one screen because it holds every part [DEC-37][dec-37] lists.
- [CI-02][ci-02] (missing file, folder, two arguments) would also pass if a script were missing, because cmd's
  "not recognized" also gives exit code 1 and a message; [CI-01][ci-01], [CI-03][ci-03] and [CI-05][ci-05] fail without the scripts.
- Temporary folders made under `%TEMP%` (`ci06-*`, `rt-*`, `t035`, `tr-review-*`) are left in place.

## 7. Real runs and CI

**Real runs (step 10), on Windows, by the session after spec 003:**

| Run | Result |
|---|---|
| `run.cmd` with each of the six test-data files, from cmd started by PowerShell | stdout matches the expected file for all six; exit code 0 |
| `run testdata\challenge-example-2.txt > file` in cmd | file identical to the expected file; exit code 0 |
| `.\run.cmd testdata\challenge-example-3.txt` in PowerShell | `Output:`, `3,3,NORTH`; exit code 0 |
| The built app with an argument (`--help`, and a file name) | the how-to-run message, character for character, on stderr; exit code 1 |
| Empty input, a file starting with a byte-order mark, EXIT in the middle of a file | expected output; exit code 0 (in the six files above) |
| Script error paths (no .NET SDK, missing file, folder, two arguments, failed build) | exit code 1 (failed build: the build's exit code); checked by the spec 003 implementation (T023, T024, T035) |
| A typed session ended with Ctrl+Z | not run in the run: it needs a person at a console. Run afterwards by the developer on Windows: commands typed at the `> ` prompt, then Ctrl+Z then Enter ended the session, once the fix for [P161][p161] was in ([DEC-10][dec-10]) |

Note: the environment the session runs in sets `NoDefaultCurrentDirectoryInExePath=1`, which stops cmd
finding `run.cmd` in the current folder; it was cleared for these runs. A normal machine and CI do not set it.

**CI (step 11):** the GitHub Actions workflow `CI` builds, tests and runs the scripts on `windows-latest`, `ubuntu-latest` and `macos-latest` on every push; new runs are on the repository's [Actions page](https://github.com/hetal-patel78/toy-robot-spec-driven/actions); the runs listed below ran before this repository was published, so their pages are not public.

| Run | Commit | Result |
|---|---|---|
| 37267711180 | `c756617` (the code as finished) | Pass on `windows-latest`, `ubuntu-latest` and `macos-latest` |

## 8. Left for the developer

- Confirm the "Decided in the run" list: done ([P158][p158] to [P160][p160]).
- Review the code.
- Check the README, and follow it word for word on a fresh clone in cmd, PowerShell and bash ([DEC-37][dec-37]).
- Run a typed session by hand (spec 002 T052): done on Windows by the developer, ended with Ctrl+Z then Enter
  after the fix for [P161][p161]; Ctrl+D on Linux or macOS was not tried by hand.
- Read the README's "how this was built" section and adjust the wording if wanted.
- The implementation-phase prompt log is `docs/design-analysis/prompts-implementation-phase.md` ([P133][p133] to
  [P140][p140]), written as CLAUDE.md asks: every instruction quoted as typed, with its outcome.
- Remove `docs/spec-kit/.probe` and the untracked `specify-inputs.md` copy from the `specs` worktree, and the
  worktrees made by this run if no longer wanted.

## Models used

| Work | Model |
|---|---|
| Session, constitution, `/speckit-specify`, clarify/plan/tasks, reviews, cross-checks, analyze, the checks script rewrite | Opus 5.5 |
| Fixing review and cross-check findings | Sonnet 5.5 |
| Per-spec checks and pushes, first checks script and its runs | Haiku 4.5 |

## Pushes of main

Each push was approved first by a separate review agent (Opus 5.5) that wrote none of the changes.

| After | Review | Non-blocking points |
|---|---|---|
| Specification phase (step 7) | Approved; ran the checks script, compared strings by hand, re-traced the Brainstorm examples | `--format sln` needs SDK 9.0.200 or later (fine here); the README status text is updated in place (003); worktree location; `*.pdf binary` in `.gitattributes` (decided in 003) |
| Spec 001 | Approved; built and ran the tests in a fresh clone: 0 warnings, 337 of 337 pass | Commands throw if run directly with no robot (unreachable through RobotOperator); empty end-to-end project prints "No test is available"; implementation-phase prompt log still to be written; tests-first visible in task order, not in commit history (each phase commit holds its tests and code) |
| Spec 002 | Approved; fresh clone built with warnings as errors (0 warnings), 430 + 17 tests pass; hand runs with a byte-order mark, CRLF, blank line and `EXIT now` gave the exact expected output | Report row was out of date (fixed); T052 left for the developer; literal byte-order mark in `Runner.cs` (fixed); the last `> ` prompt stays on screen at Ctrl+Z / Ctrl+D (expected, [DEC-10][dec-10]) |
| Spec 003 and the finish (step 12) | Approved; fresh clone: 0 warnings, 455 of 455 tests pass; `run.cmd` with Challenge Example 1 matches; CI green on all three systems; every prompt-log quote found word for word (before names, folder paths and links were edited for publication) | T032 (README fresh-clone check) open; T035 was ticked though the typed session was not run (now left open with a note); README typing note lacked Ctrl+Z / Ctrl+D (fixed); README status said the commands were run as written before the final check (fixed); README now links this report; `run.cmd` quotes the missing file name and `run.sh` does not (cosmetic); an empty second argument `run a ""` counts as none (trivial) |

## Changes after the run

| Change | Why | Steps, in order | Checks |
|---|---|---|---|
| The run scripts show progress lines on console messages (stderr): checking .NET, building, then running (with the file name, or a typing hint) | Developer's request ([P141][p141]). Spec 003 had decided that a successful run shows nothing from the script; [DEC-37][dec-37] only requires the build to be quiet and the script's messages to go to stderr, so this stays within [DEC-37][dec-37] and [DEC-23][dec-23] | spec 003, [CI-07][ci-07] check first; then scripts, README | 455 tests pass; hand runs in cmd and Git Bash: stdout unchanged, progress lines then the echo on stderr, no "Running" line for a missing file or two arguments |
| `run.cmd` rejects a file argument containing `*` or `?` (exit 1, message on stderr) and turns delayed expansion off itself | Developer's request ([P142][p142]), after a review found that a wildcard argument printed misleading lines and exited 0, and that a `!` in a file name would be changed if a user's settings turn delayed expansion on | spec 003, [CI-02][ci-02] case first; then script | 455 tests pass; hand runs: wildcards exit 1 with no "Running" line; names with `&`, `^`, `%`, `!` shown as text, also with delayed expansion forced on; `run.sh` already gives File not found |
| `Program` catches any unexpected exception: one line `Error: unexpected problem - <message>` on stderr, exit code 2 | Developer's decision ([P143][p143]), recorded in [DEC-32][dec-32], [DEC-23][dec-23], [DEC-26][dec-26] and [DEC-31][dec-31], then the solution design and spec 002. It replaces the run's decision "no catch-all in the Runner or Program" (spec 002 Assumptions); the Runner still has none | [DEC-32][dec-32]; then solution design; then spec 002; then code and tests | 460 tests pass (5 new: failing reader, failing output writer, failing error writer, how-to-run and normal runs through `Program.Run`); spec checks 1 to 5 pass (the check script now knows the [DEC-32][dec-32] error line and exit code 2) |
| The message for an argument starts `How to run:` instead of `Usage:`, and is called the how-to-run message instead of the usage hint, in the documents, code, tests and run scripts | Developer's decision ([P144][p144] to [P146][p146]): "Usage" is not self-explanatory. Recorded in [DEC-04][dec-04], then the solution design, the three specs and the checks script, then the tests (written first, 5 failed), then `Program`, `run.cmd`, `run.sh` and the README; the constitution wording too (version 1.0.1, after the review). The recorded `/speckit-specify` inputs (`specify-inputs.md`, spec 002 Input line) keep the wording that was run. The end-to-end test file is now `HowToRunMessageTests.cs` (renamed with git, history kept) | [DEC-04][dec-04]; then solution design; then specs, checks; then tests first; then code, scripts, README; then constitution and review notes | 460 tests pass; spec checks 1 to 5 pass; hand runs: `run.cmd a b` and `./run.sh a b` print their `How to run:` line and exit 1; `run.cmd` with Challenge Example 1 unchanged |
| The references in the Markdown documents are clickable: every `DEC-nn`, `FR-nn`, `NFR-nn`, spec-local ID (for example `TD-01`, `CI-02`) and `Pnn` prompt number links to its definition (reference-style links, definitions at the end of each file), with invisible `<a id>` anchors on each requirement of pages 2 and 3, each ID defined in a spec and each prompt in the two logs. No wording changed | Developer's request ([P149][p149] to [P150][p150]): a reader who sees a reference should be able to click it. Left unlinked: the recorded `/speckit-specify` inputs, the scenario tags such as `**[FR-01; DEC-01]**` and two "as for FR-nn" phrases on page 2 that the spec checks read as raw text. Done by a Sonnet 5.5 agent | links; then prompt anchors; then prompt log | 460 tests pass; spec checks 1 to 5 pass (7 fails as before); link checker: 2835 links, every file and anchor resolves; text unchanged once links and anchors are removed |
| A line that is only Ctrl+Z (U+001A) ends the input like the end of a file, typed or redirected, with no echo or warning and exit code 0 | Developer's report from testing on Windows ([P161][p161]) and decision ([P162][p162]), recorded in [DEC-10][dec-10], then the solution design and spec 002 (FR-16, two scenarios, task T058). On Windows the console gives Ctrl+Z then Enter as that one-character line instead of the end of input, so the session did not end. Done by a Sonnet 5.5 agent | [DEC-10][dec-10]; then solution design; then spec 002; then tests first (2 failed); then Runner | 462 tests pass (437 + 25; 2 new, failing before the change); spec checks 1 to 5 pass (7 fails as before) |
| A page [docs/try-it-by-typing.md](../try-it-by-typing.md), linked from the README ("Run it" and "Where to look"): how to start a typed session, the commands, a normal session, the commands that are ignored with their exact warnings, and a session blocked at the edge | Developer's request ([P163][p163] to [P165][p165]), so a reader can try the application by hand without a longer README | the page; then the README links | Every warning and output line on the page was produced by running the same commands through the built application |
| A number written as a whole number but too large to read (outside the range of a 32-bit whole number, in either direction) is still a bad line, but its warning says `X is too large` (or `Y is too large`) instead of `X must be a whole number`; anything not written as a whole number keeps `must be a whole number`; only the first problem on a line is still reported | Developer's report from testing by hand ([P167][p167] to [P169][p169]): `place 9999999999999999999,0,east` said "must be a whole number" although it is one, and a second too-large value on the same line was not mentioned. Recorded in [DEC-20][dec-20] and [DEC-22][dec-22] (the developer's choice of option A in [DEC-20][dec-20] is unchanged), then the solution design, spec 001 (spec, contract, research, task T059) and the other specs and the checks script that list the text. Done by a Sonnet 5.5 agent | [DEC-20][dec-20], [DEC-22][dec-22]; then solution design; then spec 001; then tests first (22 failed); then `CommandParser`; then the try-it page | 495 tests pass (470 unit and whole-flow, 25 end-to-end); 0 build warnings; spec checks 1 to 5 pass; every new warning on the try-it page was produced by running the built application |
| Solution design page 8.3 (event-based alternative): the CommandParser returns a command, which the Runner or a dispatcher turns into an event for the store and reducer; the review log records the issues found while testing by hand | Found while testing by hand ([P170][p170] to [P172][p172]); the diagram had the CommandParser return an event | page 8.3; then the review log | Links checked; no code, test or decision changed |

The first review of the progress-lines change was rejected: `run.cmd` echoed the file name so that a `&` in it ran the text after it as a command, with its output on stdout, and research [R-04][r-04] in spec 003 still said a successful run shows nothing from the script. Both fixed: the name is echoed through a `for` variable (tested by hand with `rvq&echo INJECTED.txt` and `rvq^x (1).txt`: the name shown as text, stdout unchanged), and [R-04][r-04] rewritten.

Check 7 of `Test-Specification.ps1` ("no code yet") now reports the implementation's files; it applies only to the specification phase.

[bs-01]: ../../specs/003-delivery/spec.md#bs-01
[ci-01]: ../../specs/003-delivery/spec.md#ci-01
[ci-02]: ../../specs/003-delivery/spec.md#ci-02
[ci-03]: ../../specs/003-delivery/spec.md#ci-03
[ci-04]: ../../specs/003-delivery/spec.md#ci-04
[ci-05]: ../../specs/003-delivery/spec.md#ci-05
[ci-06]: ../../specs/003-delivery/spec.md#ci-06
[ci-07]: ../../specs/003-delivery/spec.md#ci-07
[dec-04]: ../design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-08]: ../design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-10]: ../design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-14]: ../design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-20]: ../design-analysis/design-decisions.md#dec-20-bad-lines
[dec-22]: ../design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../design-analysis/design-decisions.md#dec-23-output-channels
[dec-26]: ../design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-31]: ../design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../design-analysis/design-decisions.md#dec-33-cross-platform
[dec-37]: ../design-analysis/design-decisions.md#dec-37-deliverables
[decided-001-brainstorm-inputs]: ../../specs/001-robot-rules/spec.md#decided-brainstorm-inputs
[decided-001-code-names]: ../../specs/001-robot-rules/spec.md#decided-code-names
[decided-001-command-without-robot]: ../../specs/001-robot-rules/spec.md#decided-command-without-robot
[decided-001-console-text]: ../../specs/001-robot-rules/spec.md#decided-console-text
[decided-001-culture-independent]: ../../specs/001-robot-rules/spec.md#decided-culture-independent
[decided-001-direction-class]: ../../specs/001-robot-rules/spec.md#decided-direction-class
[decided-001-e2e-project-skeleton]: ../../specs/001-robot-rules/spec.md#decided-e2e-project-skeleton
[decided-001-edge-corner-positions]: ../../specs/001-robot-rules/spec.md#decided-edge-corner-positions
[decided-001-exit-in-parser]: ../../specs/001-robot-rules/spec.md#decided-exit-in-parser
[decided-001-gitignore]: ../../specs/001-robot-rules/spec.md#decided-gitignore
[decided-001-placeholder-entry-point]: ../../specs/001-robot-rules/spec.md#decided-placeholder-entry-point
[decided-001-position-text]: ../../specs/001-robot-rules/spec.md#decided-position-text
[decided-001-public-type-handoff]: ../../specs/001-robot-rules/spec.md#decided-public-type-handoff
[decided-001-replace-refuses-null]: ../../specs/001-robot-rules/spec.md#decided-replace-refuses-null
[decided-001-requirements-cite-nfr]: ../../specs/001-robot-rules/spec.md#decided-requirements-cite-nfr
[decided-001-rollforward-tests]: ../../specs/001-robot-rules/spec.md#decided-rollforward-tests
[decided-001-table-size]: ../../specs/001-robot-rules/spec.md#decided-table-size
[decided-001-tasks-cite-ids]: ../../specs/001-robot-rules/spec.md#decided-tasks-cite-ids
[decided-001-us4-scenario-8]: ../../specs/001-robot-rules/spec.md#decided-us4-scenario-8
[decided-002-args-stop-reading]: ../../specs/002-console-session/spec.md#decided-args-stop-reading
[decided-002-e2e-01-no-testdata]: ../../specs/002-console-session/spec.md#decided-e2e-01-no-testdata
[decided-002-e2e-09-startup]: ../../specs/002-console-session/spec.md#decided-e2e-09-startup
[decided-002-e2e-09-stdout-only]: ../../specs/002-console-session/spec.md#decided-e2e-09-stdout-only
[decided-002-e2e-compare-stdout]: ../../specs/002-console-session/spec.md#decided-e2e-compare-stdout
[decided-002-exit-codes-in-memory]: ../../specs/002-console-session/spec.md#decided-exit-codes-in-memory
[decided-002-line-breaks]: ../../specs/002-console-session/spec.md#decided-line-breaks
[decided-002-line-endings-tests]: ../../specs/002-console-session/spec.md#decided-line-endings-tests
[decided-002-method-names]: ../../specs/002-console-session/spec.md#decided-method-names
[decided-002-namespaces]: ../../specs/002-console-session/spec.md#decided-namespaces
[decided-002-no-read-ahead]: ../../specs/002-console-session/spec.md#decided-no-read-ahead
[decided-002-no-second-reference]: ../../specs/002-console-session/spec.md#decided-no-second-reference
[decided-002-phase3-passing-tests]: ../../specs/002-console-session/spec.md#decided-phase3-passing-tests
[decided-002-program-internal]: ../../specs/002-console-session/spec.md#decided-program-internal
[decided-002-program-safeguard]: ../../specs/002-console-session/spec.md#decided-program-safeguard
[decided-002-runner-code-names]: ../../specs/002-console-session/spec.md#decided-runner-code-names
[decided-002-sc-001-levels]: ../../specs/002-console-session/spec.md#decided-sc-001-levels
[decided-002-t027-guard]: ../../specs/002-console-session/spec.md#decided-t027-guard
[decided-002-tasks-cite-ids]: ../../specs/002-console-session/spec.md#decided-tasks-cite-ids
[decided-002-testdata-and-e2e]: ../../specs/002-console-session/spec.md#decided-testdata-and-e2e
[decided-002-us1-scenario-5]: ../../specs/002-console-session/spec.md#decided-us1-scenario-5
[decided-002-us3-scenario-3]: ../../specs/002-console-session/spec.md#decided-us3-scenario-3
[decided-002-which-build]: ../../specs/002-console-session/spec.md#decided-which-build
[decided-003-bs01-ci06]: ../../specs/003-delivery/spec.md#decided-bs01-ci06
[decided-003-ci-comparison]: ../../specs/003-delivery/spec.md#decided-ci-comparison
[decided-003-ci01-ci03-stdout-only]: ../../specs/003-delivery/spec.md#decided-ci01-ci03-stdout-only
[decided-003-ci06-failing-locally]: ../../specs/003-delivery/spec.md#decided-ci06-failing-locally
[decided-003-cmd-exit-code]: ../../specs/003-delivery/spec.md#decided-cmd-exit-code
[decided-003-deliverytests-namespace]: ../../specs/003-delivery/spec.md#decided-deliverytests-namespace
[decided-003-error-paths-checks]: ../../specs/003-delivery/spec.md#decided-error-paths-checks
[decided-003-every-testdata-file]: ../../specs/003-delivery/spec.md#decided-every-testdata-file
[decided-003-folder-not-file]: ../../specs/003-delivery/spec.md#decided-folder-not-file
[decided-003-gitattributes]: ../../specs/003-delivery/spec.md#decided-gitattributes
[decided-003-gitignore]: ../../specs/003-delivery/spec.md#decided-gitignore
[decided-003-local-ci06-check]: ../../specs/003-delivery/spec.md#decided-local-ci06-check
[decided-003-other-folder]: ../../specs/003-delivery/spec.md#decided-other-folder
[decided-003-readme-root]: ../../specs/003-delivery/spec.md#decided-readme-root
[decided-003-readme-stages]: ../../specs/003-delivery/spec.md#decided-readme-stages
[decided-003-challenge-examples-in-ci]: ../../specs/003-delivery/spec.md#decided-challenge-examples-in-ci
[decided-003-script-build]: ../../specs/003-delivery/spec.md#decided-script-build
[decided-003-script-lookup]: ../../specs/003-delivery/spec.md#decided-script-lookup
[decided-003-sdk-check]: ../../specs/003-delivery/spec.md#decided-sdk-check
[decided-003-td02-bs01-location]: ../../specs/003-delivery/spec.md#decided-td02-bs01-location
[decided-003-testdata-set]: ../../specs/003-delivery/spec.md#decided-testdata-set
[decided-003-us1-stderr-reference]: ../../specs/003-delivery/spec.md#decided-us1-stderr-reference
[decided-003-windows-shell]: ../../specs/003-delivery/spec.md#decided-windows-shell
[decided-003-workflow-shape]: ../../specs/003-delivery/spec.md#decided-workflow-shape
[e2e-01]: ../../specs/002-console-session/spec.md#e2e-01
[e2e-02]: ../../specs/002-console-session/spec.md#e2e-02
[e2e-06]: ../../specs/002-console-session/spec.md#e2e-06
[e2e-07]: ../../specs/002-console-session/spec.md#e2e-07
[e2e-08]: ../../specs/002-console-session/spec.md#e2e-08
[e2e-09]: ../../specs/002-console-session/spec.md#e2e-09
[fr-01]: ../solution-design/02-functional-requirements.md#fr-01
[fr-07]: ../solution-design/02-functional-requirements.md#fr-07
[fr-08]: ../solution-design/02-functional-requirements.md#fr-08
[fr-11]: ../solution-design/02-functional-requirements.md#fr-11
[fr-13]: ../solution-design/02-functional-requirements.md#fr-13
[fr-15]: ../solution-design/02-functional-requirements.md#fr-15
[fr-17]: ../solution-design/02-functional-requirements.md#fr-17
[nfr-01]: ../solution-design/03-non-functional-requirements.md#nfr-01
[nfr-02]: ../solution-design/03-non-functional-requirements.md#nfr-02
[nfr-03]: ../solution-design/03-non-functional-requirements.md#nfr-03
[nfr-04]: ../solution-design/03-non-functional-requirements.md#nfr-04
[nfr-06]: ../solution-design/03-non-functional-requirements.md#nfr-06
[nfr-09]: ../solution-design/03-non-functional-requirements.md#nfr-09
[p133]: ../design-analysis/prompts-implementation-phase.md#p133
[p140]: ../design-analysis/prompts-implementation-phase.md#p140
[p141]: ../design-analysis/prompts-implementation-phase.md#p141
[p142]: ../design-analysis/prompts-implementation-phase.md#p142
[p143]: ../design-analysis/prompts-implementation-phase.md#p143
[p144]: ../design-analysis/prompts-implementation-phase.md#p144
[p146]: ../design-analysis/prompts-implementation-phase.md#p146
[p149]: ../design-analysis/prompts-implementation-phase.md#p149
[p150]: ../design-analysis/prompts-implementation-phase.md#p150
[p153]: ../design-analysis/prompts-implementation-phase.md#p153
[p154]: ../design-analysis/prompts-implementation-phase.md#p154
[p155]: ../design-analysis/prompts-implementation-phase.md#p155
[p158]: ../design-analysis/prompts-implementation-phase.md#p158
[p159]: ../design-analysis/prompts-implementation-phase.md#p159
[p160]: ../design-analysis/prompts-implementation-phase.md#p160
[p161]: ../design-analysis/prompts-implementation-phase.md#p161
[p162]: ../design-analysis/prompts-implementation-phase.md#p162
[p163]: ../design-analysis/prompts-implementation-phase.md#p163
[p164]: ../design-analysis/prompts-implementation-phase.md#p164
[p165]: ../design-analysis/prompts-implementation-phase.md#p165
[p167]: ../design-analysis/prompts-implementation-phase.md#p167
[p168]: ../design-analysis/prompts-implementation-phase.md#p168
[p169]: ../design-analysis/prompts-implementation-phase.md#p169
[p170]: ../design-analysis/prompts-implementation-phase.md#p170
[p171]: ../design-analysis/prompts-implementation-phase.md#p171
[p172]: ../design-analysis/prompts-implementation-phase.md#p172
[r-04]: ../../specs/003-delivery/research.md#r-04-script-channels-on-success
[r-05]: ../../specs/003-delivery/research.md#r-05-ci-workflow-shape-nfr-02
[r-08]: ../../specs/003-delivery/research.md#r-08-ci-05-failed-build-in-a-temporary-copy
[r-10]: ../../specs/003-delivery/research.md#r-10-gitattributes-content-and-the-executable-bit
[r-12]: ../../specs/003-delivery/research.md#r-12-test-data-files-and-the-td-02-and-bs-01-tests
[sc-001]: ../../specs/002-console-session/spec.md#sc-001
[td-02]: ../../specs/003-delivery/spec.md#td-02
[wf-02]: ../../specs/002-console-session/spec.md#wf-02
[wf-03]: ../../specs/002-console-session/spec.md#wf-03
