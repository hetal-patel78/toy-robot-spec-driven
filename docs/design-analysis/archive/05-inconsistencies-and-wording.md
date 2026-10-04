# 05 Inconsistencies and unclear wording (I1–I6, U1–U4)

> Category: places where your documents disagree with each other, and wording that was unclear. Source docs: Brainstorm, FR, NFR, HLD.
> [Back to index](00-index.md) · Current decisions: [08-decisions-log.md](08-decisions-log.md)

## Inconsistencies between documents
| # | Inconsistency | Where | Severity |
|---|---|---|---|
| I1 | The HLD has a Report component, but the FR had no REPORT requirement | FR vs HLD | Resolved by FR item 12 (same root as D1) |
| I2 | FR item 11 ignores *all* commands, but FR items 6 and 7 rely on PLACE working | FR p1 vs p2 | Resolved by the FR item 11 note (D3) |
| I3 | Brainstorm p1 defines the 0..4 grid; FR item 2 says only "5x5" | Brainstorm vs FR | Resolved by decision (D5) |
| I4 | Direction notation varies: FR "(N,S,E,W)", Brainstorm "East/South/W", the challenge NORTH/SOUTH/EAST/WEST | All | Low (D7): decided, full words |
| I5 | Input source is a functional choice but sits under NFR item 2 | NFR | Low |
| I6 | Placeholder or empty items: FR item 4 struck out, NFR items 1 and 2 numbered over each other | FR/NFR | Low (tidiness only, if the docs are shared as artefacts) |

## Unclear or ambiguous wording
- **U1** FR item 5: "could be selectable". **Resolved.** Your answer: you had meant selecting commands from a menu, but decided against it because a menu would multiply the PLACE combinations and the test scope. Users type commands.
- **U2** NFR item 6: "easy to understand & review". **Resolved.** Your answer: follow clean code practice and single responsibility. (A direction, not a measurable target.)
- **U3** NFR item 4: "any O/S". **Resolved.** Your answer: it must be verified to work on Windows, Linux and macOS. This brings X2 into scope.
