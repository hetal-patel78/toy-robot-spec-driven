# Toy Robot: design analysis

This folder holds the design decisions for the Toy Robot Simulator (the classic "Toy Robot" coding challenge), a C# .NET 8 console application.

## In one minute
- A 5 x 5 table, positions 0..4, with (0,0) at the SOUTH WEST corner.
- Commands come from standard input, typed or redirected from a file (run scripts: `run commands.txt`, `.\run.cmd commands.txt`, `./run.sh commands.txt`): `PLACE X,Y,F`, `MOVE`, `LEFT`, `RIGHT`, `REPORT`, `EXIT`. Case-insensitive; extra spaces allowed.
- The robot exists from the start but is not on the table until a valid PLACE. A PLACE or MOVE that would put it off the table, a MOVE, LEFT, RIGHT or REPORT before a valid PLACE, and any line that isn't a valid command are ignored with a one-line warning, and processing continues. EXIT works at any point.
- REPORT prints `Output:` and then `X,Y,F` (for example `0,1,NORTH`) on standard output. Prompts, echoed commands and warnings go to standard error.
- Program starts the Runner. For each line, the Runner asks the CommandParser for a command object (one class per command), then passes it to RobotOperator (holds the shared rules and the robot), which asks the Table (checks positions). The whole flow is tested in memory, end to end against the built app, and on Windows, Linux and macOS via GitHub Actions.

## Start here
1. **[design-decisions.md](design-decisions.md): the single source of truth.** Every rule and design decision (DEC-01 to DEC-40), grouped by topic, each with its reason and, where it comes from the challenge, the challenge wording. **A reviewer only needs this file.**
2. **[review-trail.md](review-trail.md): how the decisions were reached.** One line per review finding, pointing to the decision that settled it.
3. **[prompts-design-phase.md](prompts-design-phase.md): the prompts given to the AI assistant** where decisions were made or instructions given, as the challenge asks for prompts used as inputs to code generation.
4. **[archive/](archive/README.md): the original working notes**, kept unchanged as a record of the process.

## How these decisions were produced
1. **Initial analysis (handwritten):** brainstorming with hand-traced examples, functional requirements, non-functional requirements and a high-level design diagram (the PDFs in `docs/design-analysis/handwritten/`).
2. **Review:** the handwritten documents were reviewed against the challenge, producing findings: gaps, missing edge cases, design questions, inconsistencies, judgement calls and possible extras.
3. **Clarifications:** each finding was answered (`docs/design-analysis/clarifications/*.pdf`) and discussed until a decision was reached.
4. **Decisions:** the outcome was consolidated into `design-decisions.md`.
5. **Independent reviews:** three times, a separate AI agent with no prior context reviewed this folder against the challenge; its findings (listed in `review-trail.md`) were fixed in `design-decisions.md`.
6. **Next:** a solution design document (high-level design, logical flows, implementation details) is written from these decisions, then the Spec Kit specification is written from the solution design, then the code (DEC-36).

The handwritten documents are kept as they are, as the starting point. **Where they differ from `design-decisions.md`, `design-decisions.md` wins.**

## Sources
Paths are relative to the repository root.

| Source | Location |
|---|---|
| The challenge | `docs/challenge/README.md` |
| Handwritten analysis | `docs/design-analysis/handwritten/` (Brainstorm, FR, NFR, HLD PDFs) |
| Clarifications | `docs/design-analysis/clarifications/` |
| Project instructions for the AI assistant | `CLAUDE.md` |
