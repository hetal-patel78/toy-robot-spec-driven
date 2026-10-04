# 03 HLD findings (H1–H7)

> Category: findings on the high-level design diagram, with your reasoning. Source doc: HLD.
> [Back to index](00-index.md) · Current decisions: [08-decisions-log.md](08-decisions-log.md)

Context from you: the HLD is intentionally high-level. Detailed flows will go into sequence diagrams, and test flow will go into a separate test diagram.

| # | Finding | Your reasoning | Status | Severity now |
|---|---|---|---|---|
| H1 | Output | The arrow from Report goes back to the Runner. Once the Runner gets the response from the Report command, it sends it to the console, which prints it. | **Withdrawn.** The flow Report → Runner → console is shown on the diagram. | — |
| H2 | Who enforces the table edges | The HLD intentionally leaves out the detailed flow. Detailed flow goes into sequence diagrams. | Moved to sequence diagrams; answered (see the decisions log) | Medium |
| H3 | How "not placed yet" is represented | Same as H2: detailed flow goes into sequence diagrams. | Moved to sequence diagrams; answered | Medium |
| H4 | What the parser does with bad input | Same as H2: detailed flow goes into sequence diagrams. | Moved to sequence diagrams; answered | Medium |
| H5 | How "Command" and "Robot Commands" (RobotOperator) relate | Same as H2: detailed flow goes into sequence diagrams. | Moved to sequence diagrams; answered | Low–medium |
| H5a | REPORT appears to be missing from the Command box | REPORT is listed in the Command box. There wasn't enough space, so it isn't clearly visible. | Not a design issue (the circled Report box is the output, which is correct). Legibility only. | Low |
| H7 | Tests not shown | Test flow was left out of the hand-drawn HLD on purpose. Tests are definitely planned, with a separate diagram for the test flow. | **Closed.** When reviewing the test diagram, check that (1) the tests drive the Runner (or RobotOperator) with in-memory input and outputs, never through Program or the real console (NFR item 5, SC5), and (2) the test-data files run through the full flow (D6). | — |

**Caveat for H2–H5:** the sequence diagrams close these only if they include the unhappy paths: commands before PLACE, an invalid PLACE, a blocked MOVE and a malformed line.
