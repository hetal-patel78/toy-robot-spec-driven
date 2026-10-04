# 02 Missing edge cases (E1–E12)

> Category: edge cases that the challenge states or implies but that are not traced in Brainstorm or specified in the FR. Source docs: Brainstorm, FR, NFR vs the challenge.
> [Back to index](00-index.md) · Current decisions: [08-decisions-log.md](08-decisions-log.md)

Most of these now have a decided behaviour (see the decisions log). They are still worth turning into test data.

| # | Edge case | Severity | Notes |
|---|---|---|---|
| E1 | MOVE, LEFT, RIGHT or REPORT **before** any PLACE | High | The challenge says discard. Decided: ignored with a warning on standard error (SC4). |
| E2 | **Invalid** first PLACE followed by commands, which must also be discarded | High | Decided: the robot stays off the table, warning (SC1, D2). |
| E3 | A **second PLACE** that moves an already-placed robot (new position *and* direction) | Medium | Decided: the existing Robot's state is replaced if valid (H3, J11). |
| E4 | An off-table second PLACE while placed: the robot keeps its old state | Medium | Decided (D2 part 1). |
| E5 | Blocked moves at the **north** (y=4, NORTH) and **west** (x=0, WEST) edges | Low | Brainstorm covers only the east (p3) and south (p5) edges. |
| E6 | Corners, for example 4,4 facing NORTH or EAST | Low | Not traced. |
| E7 | A full LEFT cycle (N→W→S→E→N) | Low | Brainstorm p4 writes only the RIGHT cycle (N→E→S→W). |
| E8 | The **three challenge examples themselves** with their expected output | Medium | None of them is traced. Brainstorm p2 top looks like Example 3 but is a different scenario. |
| E9 | The Brainstorm traces on p3, p4 and p5 end without a REPORT or expected output | Low | Expected end states would be 4,1,EAST / 0,3,WEST / 0,0,SOUTH. As written, they can't be turned directly into test data. |
| E10 | Malformed input: unknown words, PLACE with missing or extra values, non-numeric X/Y, an invalid direction, blank lines | Medium | Decided: skip, warn, continue (H4, J6). Add `PLACE0,0,NORTH` (no space) and `PLACE 0 0 NORTH` (no commas) to the test data. |
| E11 | An integer overflow value for X/Y, for example 99999999999 | Low | NFR item 1 says "big random number". It's not clear whether you mean "within int range but off the table" or "too big to parse". |
| E12 | Input file not found, or stdin closing (EOF) | Medium | With standard input only, there is no "file not found" in the app (the terminal handles `<`). EOF ends the session (J8). |

Newly decided behaviours worth testing too: EXIT at any point, including before PLACE and mid-file (J8); a pasted `> MOVE` is a bad command (J5); a file-name argument shows a usage hint and exits (J4).
