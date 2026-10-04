# 06 Judgement calls (J1–J12)

> Category: choices the challenge leaves open, with options and trade-offs. Source: the challenge.
> [Back to index](00-index.md) · Your decisions: [08-decisions-log.md](08-decisions-log.md)

All of these are now decided. The options are kept here as the record of what was weighed.

| # | Decided |
|---|---|
| J1 | 0..4 |
| J2 | Message, on standard error (via J3 and SC4) |
| J3 | Warning on standard error |
| J4 | Standard input only |
| J5 | Case-insensitive; extra spaces tolerated; full direction words; pasted `>` not stripped |
| J6 | Skip, warn, continue |
| J7 | `Output:` label, then `X,Y,F` |
| J8 | Both: end of input and EXIT |
| J9 | One command per line |
| J10 | Table plus RobotOperator |
| J11 | Replace state, don't change it in place |
| J12 | Robot exists from the start; its placement is nothing until a valid PLACE (option c) |

**J1. Coordinate range.**
- Option (a): positions 0..4, i.e. 5 cells. This is the common reading, and it matches Brainstorm p1.
- Option (b): 0..5, i.e. 6 grid points on a 5-unit board.
- Trade-off: (a) matches all the challenge examples and community convention. (b) is a literal reading of "units".

**J2. REPORT when the robot is not placed.**
- Option (a): silent (no output).
- Option (b): print a message such as "Robot not placed".
- Option (c): write the message to stderr.
- Trade-off: the challenge says "can choose to ignore". The challenge does not require silence; "ignore" means no state change.

**J3. Invalid or blocked commands: give feedback or not.**
- Option (a): ignore silently.
- Option (b): warning to stderr.
- Option (c): warning to stdout.
- Option (d): a verbose/debug flag.
- Trade-off: warnings help users but can pollute stdout-based tests unless sent to stderr.

**J4. Input source.**
- Options: (a) stdin only; (b) file only; (c) both, with a file argument or stdin fallback; (d) command-line arguments.
- Trade-off: the challenge needs only one ("as the developer chooses"). Stdin also covers `app < file` with no extra code.

**J5. Parsing strictness.**
- Case sensitivity: options are `place` accepted or rejected.
- Whitespace: options are tolerating `PLACE 1, 2, NORTH` or extra spaces, or rejecting them.
- Direction abbreviations: options are accepting N/S/E/W or not.
- The `> ` prefix shown in the challenge examples: options are treating it as part of the input or as a prompt.
- Trade-off: strict parsing is simpler and matches the spec exactly. Lenient parsing is friendlier but widens the input grammar and the test surface.

**J6. Malformed lines** (unknown command, bad PLACE arguments).
- Options: (a) skip the line and continue; (b) skip with a warning; (c) stop processing.
- Trade-off: (a) and (b) follow the spirit of "further valid commands must still be allowed". (c) is stricter than the challenge implies.

**J7. REPORT output format.**
- Option (a): exactly `X,Y,F` as in the challenge examples.
- Option (b): a richer format, for example "Output:" then `0,1,NORTH`.
- Trade-off: the challenge says "any form", but an automated check may compare against `0,1,NORTH` (see A1 in the decisions log).

**J8. Ending interactive input.**
- Options: (a) EOF only (Ctrl+Z then Enter on Windows, Ctrl+D elsewhere); (b) an EXIT/QUIT command; (c) both.
- Trade-off: (a) needs no extra grammar but isn't obvious to users. (b) adds a command that the challenge doesn't define.

**J9. Multiple commands on one line** (for example `PLACE 0,0,NORTH MOVE REPORT`).
- Option (a): one command per line.
- Option (b): split each line into tokens.
- Trade-off: the challenge examples are one per line. (b) complicates parsing because PLACE carries its own arguments.

**J10. Where the rules live.**
- Option (a): Robot validates its own moves against Table.
- Option (b): Table answers "is this position valid?" and RobotOperator decides.
- Option (c): each Command object executes itself (command pattern).
- Trade-off: testability versus the number of classes.

**J11. Robot state: changed in place or replaced.**
- Option (a): a mutable Robot.
- Option (b): an immutable position/direction value, replaced on each command.
- Trade-off: (b) makes "ignore the move" trivial (keep the old value) and is easier to test. (a) is more familiar and shorter.

**J12. Representing the unplaced state.**
- Options: (a) no Robot / nullable; (b) an IsPlaced flag; (c) a separate state type, or a null-object robot.
- Trade-off: simplicity versus making invalid states unrepresentable.
