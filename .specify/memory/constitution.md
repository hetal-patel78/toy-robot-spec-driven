<!--
Sync Impact Report
- Version change: 1.0.0 → 1.0.1 (PATCH: wording only; the message for an argument is called the how-to-run message, DEC-04)
- Earlier: template (unversioned) → 1.0.0 (first ratified constitution)
- Principles defined (template placeholders → titles):
  PRINCIPLE_1 → I. Clean Code and SOLID
  PRINCIPLE_2 → II. Three Test Levels, Tests First
  PRINCIPLE_3 → III. Never Crash
  PRINCIPLE_4 → IV. Cross-Platform
  PRINCIPLE_5 → V. No Over-Engineering
  added       → VI. Standard Output for REPORT Output Only
- Added sections: Source of Truth and Technical Baseline; Development Workflow; Governance
- Removed sections: none
- Templates checked: plan-template.md (Constitution Check reads this file at runtime), spec-template.md,
  tasks-template.md: no change needed
- Follow-up TODOs: none
-->

# Toy Robot Simulator Constitution

This constitution holds only the lasting principles listed in [DEC-36][dec-36]. Every detailed rule (commands,
warning texts, exit codes, component responsibilities) lives in
`docs/design-analysis/design-decisions.md`, which this constitution points to and never restates.

## Core Principles

### I. Clean Code and SOLID

Code MUST be clean and follow the SOLID principles ([DEC-34][dec-34]): each class has one job; new behaviour is
added with new classes rather than by changing existing ones, where practical; parts depend on small
interfaces; classes are small and well named. Component names and responsibilities MUST match the
solution design (page 6), so the code can be traced back to a decision.

Rationale: the code is meant to be read by others, so it must be easy to follow and extend.

### II. Three Test Levels, Tests First

Every behaviour MUST be proved at the test levels set by [DEC-31][dec-31]: unit tests, whole-flow tests in
memory (standard output and standard error checked separately and exactly), and end-to-end tests of
the built application run as a separate process. Expected output is compared line by line. The tests
for a piece of behaviour MUST be written before the code that makes them pass, and a phase of work is
finished only when all its tests pass. Tests MUST NOT be weakened, skipped or changed to match the code.

Rationale: tests are the evidence that every decision is implemented, on every system.

### III. Never Crash

No input MUST ever make the application crash ([DEC-32][dec-32]): not an empty input, a very large number, or any
other text. Every bad input has a defined, normal result, and processing continues with the next line.

Rationale: the challenge requires that invalid commands are ignored and valid commands still work; a
crash would stop both.

### IV. Cross-Platform

The application, its tests and its run scripts MUST work on Windows, Linux and macOS ([DEC-33][dec-33]), and
results MUST NOT depend on the machine's regional settings or line endings. This is proved by CI on
all three systems, not by a note.

Rationale: a reviewer may use any of the three systems.

### V. No Over-Engineering

Build only what the decisions describe ([DEC-38][dec-38], [DEC-39][dec-39]). Nothing listed as not included or out of
scope is added; no extra libraries, dependency injection container, logging framework or configuration
are introduced. Room for future change comes from the design, not from building the future now.

Rationale: unrequested features add code, tests and risk without value for the challenge.

### VI. Standard Output for REPORT Output Only

Standard output MUST carry only the REPORT output ([DEC-21][dec-21], [DEC-23][dec-23]). Everything else the application
writes (the prompt, the echo, warnings and the how-to-run message) goes to standard error.

Rationale: the REPORT output stays clean for saving or comparing, with no extra code.

## Source of Truth and Technical Baseline

- Order of precedence ([DEC-36][dec-36]): `docs/design-analysis/design-decisions.md`, then the solution design in
  `docs/solution-design/`, then the Spec Kit files. A later document MUST NOT contradict an earlier one.
- Strings that tests compare (warnings, the how-to-run message, the REPORT format, the prompt and the echo) and
  exit codes are copied character for character from design-decisions.md, never reworded.
- The technical choices are fixed by the solution design (pages 5 and 9): C# on .NET 8 with
  `RollForward` = `LatestMajor`, xUnit, GitHub Actions on Windows, Linux and macOS, and the run scripts
  `run.cmd` and `run.sh` ([DEC-37][dec-37]).

## Development Workflow

- The work follows the chain in [DEC-36][dec-36]: design decisions, solution design, Spec Kit specification
  (constitution, spec, plan, tasks), then implementation with `/speckit-implement`.
- No production code is written outside the Spec Kit tasks (CLAUDE.md).
- Every requirement, scenario and task cites its FR or NFR and DEC IDs so it can be traced.
- Code is reviewed against the component responsibilities on page 6 and against these principles before
  it is merged.

## Governance

This constitution supersedes other practices for the Spec Kit specification and implementation, but it
does not override design-decisions.md. A decision is changed only in design-decisions.md and only after
the developer confirms it (CLAUDE.md); this constitution is then amended if a principle is affected.
Amendments follow semantic versioning: MAJOR for removing or redefining a principle, MINOR for adding a
principle or materially expanding guidance, PATCH for wording. Every plan's Constitution Check and every
code review verifies compliance with these principles.

**Version**: 1.0.1 | **Ratified**: 2026-10-05 | **Last Amended**: 2026-10-05

[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-33]: ../../docs/design-analysis/design-decisions.md#dec-33-cross-platform
[dec-34]: ../../docs/design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-36]: ../../docs/design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-38]: ../../docs/design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
