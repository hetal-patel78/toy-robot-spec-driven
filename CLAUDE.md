# Toy Robot Simulator

C# .NET 8 console application for the classic "Toy Robot" coding challenge (`docs/challenge/README.md`).

## Source of truth
- **`docs/design-analysis/design-decisions.md`** defines all behaviour and design. Implement exactly what it says.
- If something is not covered by a decision, or two decisions seem to conflict, **stop and ask the developer**. Do not invent behaviour.
- Each decision has a stable ID (`DEC-nn`). When code or tests implement a decision, reference its ID in a comment or test name where it helps.

## Not sources of truth
- The handwritten PDFs in `docs/design-analysis/handwritten/` and the PDFs in `docs/design-analysis/clarifications/` are the initial analysis. They are kept as they are and are never updated. Where they differ from `design-decisions.md`, `design-decisions.md` wins.
- `docs/design-analysis/archive/` is history. Do not edit it or use it as a reference for behaviour.

## Development approach
The work follows this chain; each step is written from the one before:
1. Design analysis: `design-decisions.md`.
2. Solution design document: high-level design, logical flows and implementation details, in `docs/solution-design/`. To write it, follow `docs/solution-design/solution-design-brief.md` exactly, including its stops for the developer's review.
3. GitHub Spec Kit specification (constitution, spec, plan, tasks).
4. Implementation.

- Order of precedence: `design-decisions.md`, then the solution design document, then the Spec Kit files. A later document must not contradict an earlier one. If it does, point it out and ask; do not resolve it silently.
- The `/speckit-*` commands are run by the developer, or by Claude Code when a prompt the developer approved asks for it. Do not write production code outside the Spec Kit tasks.
- The Spec Kit constitution holds only lasting principles; detailed rules stay in `design-decisions.md`.

## Changing a decision
- Only after the developer confirms the change.
- Record the decision itself only in `design-decisions.md` (not in the solution design or the Spec Kit files), written as if it were the original decision: no "revised from", "renamed" or "previously" notes.
- If a review finding's outcome changes, update its line in `docs/design-analysis/review-trail.md`.

## Prompt log
The challenge asks for the prompts and artefacts used as inputs to code generation.
- Design phase, including the solution design document: `docs/design-analysis/prompts-design-phase.md`.
- Implementation phase: `docs/design-analysis/prompts-implementation-phase.md`.
- Add the prompts that are inputs to the design or the code: where the developer makes a decision or gives an instruction about the application, its design or how it is built, quoted exactly as typed (typos included), with its outcome. Do not add prompts that only ask for an explanation, or editorial and housekeeping requests. Never change a quote.
- If a prompt was drafted and revised before it was used, log only the final version.
- Number new prompts after the last one in the log.

## Scope
- Do not add anything listed as not included or out of scope in `design-decisions.md` without asking.
- Build only what the decisions describe. If a change seems to need something more, ask first.

## Build and test
To be added once the solution exists: `dotnet build`, `dotnet test`, and the run scripts (`run.cmd` for Windows cmd and PowerShell, `run.sh` for Linux and macOS). Note that PowerShell does not support `< file`; use the run scripts or `cmd /c` there.
