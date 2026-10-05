# Spec Kit brief

The developer's instructions for writing the GitHub Spec Kit specification (step 3 of the development approach, DEC-36). Follow them exactly. The session runs autonomously: the developer does not answer questions during the run, so the session decides under rule 10 and logs every decision for review.

---

## Goal

When the session finishes, the repository holds the whole application, built from a reviewed and checked Spec Kit specification and tested at all three levels on Windows, Linux and macOS:
- one constitution;
- three feature specs (001, 002, 003), each with its spec, plan and tasks;
- the application, its tests, test data, run scripts, CI workflow and a first README, implemented with `/speckit-implement` from those specs, with every test passing locally and in CI;
- a run report that says what was done, what was checked, and every decision taken in the run, for the developer to confirm.

The work has two phases: first the specification (steps 1 to 8), then the implementation (steps 9 to 12). No code is written until the specification has passed its checks. The developer reviews everything when the run finishes; the application must be ready and tested by 15:00.

## Sources

Use only these:
1. `docs/design-analysis/design-decisions.md`: the source of truth for every rule (DEC-01 to DEC-39, A1).
2. `docs/solution-design/`: the solution design, written from those decisions. This is the main input for the specs. Start from its `README.md`.
3. `CLAUDE.md`: the project rules.

Order of precedence: design-decisions.md, then the solution design, then the Spec Kit files. Do NOT use `docs/design-analysis/archive/`. The handwritten PDFs and clarifications are background only.

## Rules

1. **No new behaviour.** The specs describe exactly what the decisions and the solution design say. Do not add features, options, messages or edge-case behaviour. Nothing listed as not included in DEC-39 or as out of scope on page 4.
2. **Copy, don't reword, anything a test will compare:** every warning text (DEC-22), the usage hint (DEC-04), the REPORT format `Output:` then `X,Y,F` (DEC-21), the prompt `> ` (DEC-24), the echo `> ` plus the trimmed line (DEC-25), and every exit code. Copy them character for character.
3. **Keep the output channel of every line.** Use the terms from the solution design with their tags: console output (stdout) is only the REPORT result; console messages (stderr) are the prompt, the echo, warnings and the usage hint; console input (stdin) is where commands come from. Every acceptance scenario states stdout and stderr separately, as page 2 does.
4. **Cite IDs.** Every requirement, scenario and task cites its FR or NFR and DEC IDs, so the traceability page (page 10) can be checked against the specs.
5. **Acceptance scenarios** come from page 2's Given / When / Then, copied with their exact input and output. Add more scenarios only from the cases DEC-31 says must be covered.
6. **Component names and members exactly as on page 6:** Program, Runner, CommandParser (parse result: a command object, EXIT, or a failure with its reason), `IRobotCommand` (`Name`, `NeedsRobotOnTable`, `Execute(IRobotOperator robot)`), `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand`, `ReportCommand`, `IRobotOperator` (`CurrentState`, `TryMoveTo(next)`), RobotOperator (outcome: the position text, a warning, or done with nothing to show), Robot, RobotState, Direction, Table. All three specs use the same names.
7. **Technical choices are already made** (pages 5 and 9): C# on .NET 8 with `RollForward` = `LatestMajor`; `ToyRobot.sln` at the root; `src/ToyRobot/`, `tests/ToyRobot.Tests/` (unit tests and Runner whole-flow tests), `tests/ToyRobot.ConsoleEndToEndTests/`, `testdata/`; xUnit; GitHub Actions on `windows-latest`, `ubuntu-latest` and `macos-latest`; `run.cmd` and `run.sh`; `.gitattributes`. The plans use these and add nothing else (no other libraries, no dependency injection container, no logging framework).
8. **Tests first.** In every tasks file, the tests for a piece of behaviour come before the code that makes them pass, and each phase ends with all its tests passing.
9. **The constitution holds only lasting principles**, the list in DEC-36: clean code and SOLID, the three test levels, never crash, cross-platform, no over-engineering, standard output for REPORT output only. Detailed rules stay in design-decisions.md; the constitution points to it.
10. **Decide, so nothing is blocked.** The developer delegates decisions in this run to the session, within these rules, so that the whole application can be implemented and run without an error when the run finishes. When something is not covered or is ambiguous, choose the option that fits design-decisions.md and the solution design best, is the simplest, and keeps every path testable; write it into the spec, and list it in the run report under "Decided in the run" with the reason and the other options. Limits:
    - Prefer deciding within the existing documents: a gap is filled in the Spec Kit files, and design-decisions.md and the solution design stay as they are.
    - **When a document blocks a full, working, error-free system**, for example a decision that cannot work on one of the three systems, two sources that contradict each other, or a design point that makes a requirement impossible to meet or to test, change the document. The developer has confirmed in advance, for this run, any change needed for that purpose; this is the confirmation that CLAUDE.md's "Changing a decision" asks for. Make the change in the right place and keep the chain consistent: a decision in `design-decisions.md` (written as the decision, as CLAUDE.md requires), then the solution design pages that describe it, then the Spec Kit files. Each change is its own small commit whose message starts `Run change:` and names the DEC, page or file, so the developer can review or revert it alone.
    - Never pass by weakening: do not delete, skip or loosen a test, change an expected output to match the code, remove a requirement, or drop anything the challenge asks for. Fix the code or the design instead.
    - Never add anything rule 1 forbids, and do not edit `CLAUDE.md` or this brief.
    - No `[NEEDS CLARIFICATION]` marker may be left in any spec.
    - List every decision under "Decided in the run" and every document change under "Changed in the run" in the run report: what changed, why it blocked a working system, and the commit.
11. **When Spec Kit asks a clarifying question**, answer it from the sources, citing the page or DEC ID. If the sources do not answer it, decide under rule 10.
12. **Never delete anything.** The developer has asked that nothing is deleted:
    - No file or folder is deleted, moved away or emptied: not in `docs/`, not the PDFs, not the prompt logs, not any test, test-data file or source file. Spec Kit's placeholder `constitution.md` is filled in, not removed.
    - No content is removed from a document to make room or to resolve a conflict. Where rule 10 allows a document change, the text is changed in place and the change is listed under "Changed in the run"; nothing else around it is cut.
    - No prompt-log entry is changed or removed; new entries are only added.
    - Git history is kept: no `git reset --hard`, `git clean`, `git rebase`, `git commit --amend` on pushed commits, force push, branch deletion, `git stash drop` or `git worktree remove`. The worktrees and branches made in this run are left in place and listed in the run report.
    - If something seems to need deleting, leave it, and list it in the run report for the developer to decide.

## The three specs

Each functional and non-functional requirement is owned by exactly one spec. A spec may refer to another spec's components, but does not restate their rules.

| Spec | Owns | Components | Tests it plans | Solution design pages |
|---|---|---|---|---|
| **001-robot-rules** | FR-01 to FR-07, FR-09, FR-10, NFR-07. Also the solution and project skeleton (`ToyRobot.sln`, the three projects, xUnit, `RollForward`), because the other specs build on it. | Table, Direction, RobotState, Robot, `IRobotCommand` and the five command classes, `IRobotOperator`, RobotOperator, CommandParser | Unit tests | 2, 3, 5, 6, 7.1–7.5, 10 |
| **002-console-session** | FR-08, FR-11 to FR-17, NFR-01, NFR-04, NFR-05, NFR-06 | Runner, Program | Runner whole-flow tests in memory, covering every FR as console text; console end-to-end tests of the built application | 2, 3, 5, 6, 7 (all), 9, 10 |
| **003-delivery** | NFR-02, NFR-03, NFR-08, NFR-09 | Run scripts, test-data files (including the three challenge examples), the CI workflow, `.gitattributes`, the README | CI runs on all three systems; the run scripts run with a challenge example | 3, 5, 9, 10 |

Dependencies for implementation: 002 builds on 001's types; 003 needs the built application and the console end-to-end tests from 002. The tasks say so, and they are implemented in the order 001, 002, 003.

## How to work

### Setup
- Spec Kit is installed in the repository before this session starts (`.specify/` exists). If it is not, stop and write the run report saying so; do not install it.
- The installed version is Spec Kit 1.1.1.dev0, with PowerShell scripts. Its commands are skills in `.claude/skills/`: `/speckit-constitution`, `/speckit-specify`, `/speckit-clarify`, `/speckit-plan`, `/speckit-tasks`, `/speckit-analyze`, `/speckit-checklist` and `/speckit-implement`.
- Work in git worktrees under `C:\Dev\` named `codefoundry-<slug>`, never inside the repository.
- How this version tracks features: `/speckit-specify` numbers a new spec from the folders already in `specs/`, and the current feature is held per checkout in `.specify/feature.json` (not committed) or the `SPECIFY_FEATURE` / `SPECIFY_FEATURE_DIRECTORY` environment variables. So:
  - create the three specs **one after another in one checkout**, so they are numbered 001, 002 and 003 with the names in the table above, and commit;
  - then give each spec its own worktree for the parallel work, and point Spec Kit at that spec's folder in each one (`SPECIFY_FEATURE_DIRECTORY`, or that worktree's `feature.json`), so no line of work ever acts on another spec.
- Commit after each step with a small, clear commit. Push each spec's own branch to GitHub as soon as that spec is finished (step 4.5), and the combined `specs` branch at the end (step 7). Push `main` to GitHub only after a separate review agent has approved it (the "Pushing main" section below).

### Models
To save tokens, give each agent in the workflow the model that fits its job (the `model` option: `opus` is Opus 5.5, `sonnet` is Sonnet 5.5, `haiku` is Haiku 4.5). The session itself runs on Opus 5.5. Record the models used in the run report. Use the strongest model only where judgement matters.

| Work | Model | Why |
|---|---|---|
| Writing the constitution, specs, plans and tasks (`/speckit-constitution`, `-specify`, `-clarify`, `-plan`, `-tasks`) | `opus` | Everything after depends on them; mistakes here are the most expensive |
| Decisions under rule 10, and any change to design-decisions.md or the solution design | `opus` | Judgement calls the developer will rely on |
| Spec review, re-review and the cross-check of the three specs (steps 4–5) | `opus` | Finding what is wrong or missing needs the strongest reasoning |
| The separate review agent before every push of `main` | `opus` | The last check before work is published |
| Fixing review findings in the specs | `sonnet` | The finding already says what to fix |
| Implementation (`/speckit-implement`) and fixing failing tests | `sonnet` | Strong at code, much cheaper |
| Code review and re-review of each implemented spec (step 9.4) | `sonnet` | Checks against a clear list (page 6, SOLID, exact strings, channels) |
| Fixing CI failures | `sonnet` | |
| Scripted checks (coverage, exact strings, names, no code), running `dotnet build` / `dotnet test`, the real runs, watching CI | `haiku` | Mechanical: run, compare, report |
| Updating the run report after each step | `haiku` | Writing down results; the final report at the finish uses `sonnet` |

**Escalate when stuck:** if a `haiku` or `sonnet` agent fails at the same task twice (for example a fix that does not make the tests pass, or a CI failure it cannot explain), give that task to `opus`.

### Steps
1. **Read** CLAUDE.md, this brief, design-decisions.md and every page of the solution design.
2. **Constitution** (first, on its own): run `/speckit-constitution` with the principles in rule 9. Commit.
3. **Create the three specs, one after another:** run `/speckit-specify` for 001, 002 and 003 in turn, each with its scope from the table above and the rules in this brief. Commit. The parallel work starts from this commit.
4. **For each spec, in parallel** (a workflow, one line of work per spec, each in its own worktree):
   1. **Complete:** `/speckit-clarify` (rule 11), then `/speckit-plan`, then `/speckit-tasks`.
   2. **Review** against the checklist below and run `/speckit-analyze`.
   3. **Fix** every finding: from the sources where they settle it, otherwise by deciding under rule 10.
   4. **Re-review** with a fresh reviewer that did not write the spec. Repeat fix and re-review until the checklist passes and analyze reports no CRITICAL or HIGH issue, at most three rounds; anything still open goes to the run report.
   5. **Test and push:** run the checks on the specification (below) that apply to one spec: coverage of the requirements it owns, exact strings, exit codes and channels, names, analyze, and no code. Once its checklist and these checks pass, push that spec's branch to GitHub, so each finished spec is saved on its own. A spec that still fails after three rounds is pushed too, and the run report says it is not finished.
5. **Cross-check the three specs together:** the same component names, members, outcomes, parse results and exact strings everywhere; every FR and NFR owned by exactly one spec; dependencies between the tasks files in the right order; no contradiction between specs. Fix every finding in the spec it belongs to (rule 10 where the sources are silent), then cross-check again until nothing is found.
6. **Test the specification** with the checks below, by script where possible. Fix every failure, then run all the checks again until every one passes, and record the results.
7. **Bring it together:** merge the three spec branches into one branch, `specs`, in `C:\Dev\codefoundry-specs`; then fast-forward the local `main` in the main checkout to it so the developer can review it in VS Code. Push the `specs` branch to GitHub, then push `main` as described in "Pushing main".
8. **Write the specification part of the run report** (below). Commit it on `specs` and push the `specs` branch again.

#### Implementation (only after step 8, and only if every spec passed its checks; a spec that did not pass is fixed first)
9. **Implement 001, then 002, then 003**, one at a time, in one worktree `C:\Dev\codefoundry-impl` on a branch `impl` made from `specs`. For each spec:
   1. Point Spec Kit at that spec's folder and run `/speckit-implement`. Follow its tasks in order: tests first, then the code that makes them pass. Write only what the tasks say (CLAUDE.md: no production code outside the Spec Kit tasks).
   2. After each phase of tasks, run `dotnet build` and `dotnet test` from the repository root. Every test must pass before the next phase starts. Commit in small steps, each naming its task IDs.
   3. If a task turns out to be wrong or conflicts with a source, the sources win: fix the spec, plan or tasks first, commit that fix, then the code. Decide under rule 10 where the sources are silent, and add the decision to the run report.
   4. **Review** the spec's code with a fresh reviewer that did not write it: against page 6 (names, responsibilities, what each component does NOT do), SOLID (DEC-34), the exact strings (rule 2) and the stdout/stderr split (rule 3). Fix what it finds and run all tests again; then hand the fixed code to a new fresh reviewer. Repeat until a reviewer finds nothing (at most three rounds; anything still open goes to the run report).
   5. **Push** the `impl` branch once that spec's tests all pass.
10. **Run the application for real** on this machine, using `run.cmd` from cmd and from PowerShell:
    - each test-data file, including the three challenge examples: console output (stdout) matches the expected file, line by line;
    - with an argument: the usage hint on console messages (stderr) and exit code 1;
    - empty input, a file starting with a byte-order mark, and a file with EXIT in the middle: exit code 0 and the expected output.
    Fix any failure through the tasks as in 9.3.
11. **CI:** after pushing, watch the GitHub Actions run on `impl` (for example `gh run watch`). It must pass on Windows, Linux and macOS. Fix failures and push again, at most five attempts; if it still fails, record the failing job and the error in the report.
12. **Finish:** fast-forward the local `main` in the main checkout to `impl` so the developer can review it in VS Code, and push `main` as described in "Pushing main". Complete the run report and the prompt log entry, commit them on `impl`, fast-forward `main` again and push `impl`.

**No deadline; always a green point and a current report.** Keep working until everything above is finished. The developer may start at any time, even while the session is still working, so:
- Write the run report early (after step 1) and update it after every step, so it always says what is done, what is in progress and what is left.
- After every step that ends with everything passing (a spec finished and checked; an implementation phase with all tests passing; a CI fix that passes), commit, push the branch being worked on, and fast-forward the local `main` to it. If `main` cannot be fast-forwarded (for example, the developer has local changes there), leave it and say so in the report.
- Never push work that is not passing.

**Pushing main.** The developer wants `main` on GitHub kept up to date during the run, but never with unreviewed work. Before every push of `main`:
1. All tests pass (`dotnet build` and `dotnet test` from the repository root), and once CI exists, the CI run for that commit has passed on all three systems.
2. Hand the changes since the last pushed `main` to a **separate review agent**: one that did not write or fix any of them. It reviews them against design-decisions.md, the solution design, the specs, this brief's rules (exact strings, stdout/stderr, names, SOLID, tests first, nothing deleted, nothing out of scope), and any "Run change" commits, and answers approve or reject with its findings.
3. If it rejects, fix the findings (through the tasks, as in step 9.3), run the tests again, and hand the new changes to a new separate review agent. Repeat until it approves.
4. When it approves, fast-forward `main` and push it. Never force push. Record each review (what it found, what was fixed, the commit pushed) in the run report.
Push `main` at least after the specification phase (step 7), after each spec is implemented, and at the finish (step 12).

### Review checklist (each spec)
- [ ] Every FR and NFR it owns is present, with its DEC IDs; none is missing or belongs to another spec.
- [ ] Each acceptance scenario matches page 2 exactly, with console output (stdout) and console messages (stderr) stated separately.
- [ ] Every compared string and exit code matches design-decisions.md character for character (rule 2).
- [ ] Component names and members match page 6 (rule 6).
- [ ] The plan uses only the choices in rule 7.
- [ ] Tasks put tests first, cite their IDs, and end each phase with its tests passing.
- [ ] Nothing out of scope (DEC-39, page 4) and no new behaviour.
- [ ] No `[NEEDS CLARIFICATION]` left; every decision taken in this run is listed for the run report.
- [ ] Every behaviour in the spec, including every error and bad-input path, has a planned test, so the application can be shown to run without an error.
- [ ] Nothing contradicts design-decisions.md or the solution design.

### Checks on the whole specification ("testing" it)
1. **Coverage:** every FR-01 to FR-17 and NFR-01 to NFR-09 from page 10 appears in exactly one spec, and every requirement in the specs traces back to one of them.
2. **Exact strings:** extract every warning, the usage hint, the `Output:` format, the prompt and the echo format from the specs and compare them with design-decisions.md by script. Every one matches.
3. **Exit codes and channels:** every scenario's exit code and stdout/stderr split matches page 2 and DEC-23.
4. **Names:** every component and member name in the three plans is in page 6's list, and no other component is introduced.
5. **DEC-31 coverage:** every case in DEC-31's "Must be covered" list has a planned test in some tasks file.
6. **Analyze:** Spec Kit's analyze command reports no CRITICAL or HIGH issue for any spec.
7. **No code yet:** at the end of the specification phase, `git diff` against the starting commit shows no files under `src/` or `tests/` (the projects are created by the tasks in step 9).

## Run report

Write `docs/spec-kit/implementation-run-report.md`, short and plain:
1. **Status:** for each spec and the constitution, done or not, with the paths of its files.
2. **Check results:** each check above, pass or fail, with details for any failure.
3. **Decided in the run:** each decision taken under rule 10, with what the sources say, the option chosen and why, and the other options. The developer confirms or changes each one; a confirmed decision that changes behaviour is then recorded in design-decisions.md (CLAUDE.md).
   **Changed in the run:** each change made to design-decisions.md, the solution design or another document under rule 10: the DEC, page or file, what it said before and after, why it blocked a working system, and the commit, so the developer can confirm or revert it.
4. **Fixed during review:** a short list of what the reviews found and how it was fixed, by spec.
5. **Review first:** the three things the developer should look at first.
6. **Implementation:** for each spec, done or not; the number of tests at each level and whether all pass; what the code review found and fixed.
7. **Real runs and CI:** the results of step 10, and the CI result on each system with a link to the run.
8. **Left for the developer:** what still needs doing, at least: confirm the "Decided in the run" list, review the code, check the README, and follow it word for word on a fresh clone in cmd, PowerShell and bash (DEC-37).

## Prompt log

Add the developer's prompt that started this session, quoted exactly as typed, to `docs/design-analysis/prompts-implementation-phase.md` (create it with a one-line heading if it does not exist), numbered after the last prompt in either prompt log. Its outcome lists the Spec Kit commands run under it, for each spec, with the input given to each, including each `/speckit-implement` run (DEC-36).
