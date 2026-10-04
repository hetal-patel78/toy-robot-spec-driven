# Toy Robot design review: index

Review of the handwritten Toy Robot design documents against the "Toy Robot" coding challenge, plus the decisions made while reviewing them.

This is a review only. The handwritten PDFs were not changed.

## Abbreviations
- **p** = page (e.g. p2 = page 2)
- **Brainstorm** = `IntiialBrainStorming_ReadingtheDocument_Understanding_Requirements.pdf`
- **FR** = `ToyRobot_FunctionalRequirements.pdf` (updated version, 13 items)
- **NFR** = `ToyRobot_NonFunctionalRequirements.pdf`
- **HLD** = `ToyRobot_SD_HLD.pdf`
- **Challenge** = the original challenge (now [docs/challenge/README.md](../../challenge/README.md))
- Clarifications = the two PDFs in `Clarifications\`

(The FR, NFR and HLD were earlier in `ToyRobotRequirements_FR.pdf` and `NFR_AND_HLD_ToyRobot.pdf`.)

## Worked examples checked (Brainstorm)
| Page | Trace | Correct? |
|---|---|---|
| p1 | 5x5 grid, (0,0) bottom-left, x across, y up | Yes |
| p2 top | PLACE 0,0,EAST, MOVE, MOVE, LEFT, MOVE gives 2,1,NORTH | Yes |
| p2 bottom | Crossed out on purpose | Ignored |
| p3 | PLACE 4,2,SOUTH, MOVE (4,1), LEFT (EAST), MOVE blocked at x=5 | Yes |
| p4 | PLACE 3,4,WEST, MOVE, MOVE (1,4), LEFT (SOUTH), MOVE (1,3), RIGHT (WEST), MOVE (0,3) | Yes |
| p5 | PLACE 0,0,SOUTH, MOVE blocked (y would be -1) | Yes |

## Files
| File | What it covers | Status |
|---|---|---|
| [01-definite-gaps.md](01-definite-gaps.md) | D1–D8: things that didn't match the challenge | All resolved by decision |
| [02-missing-edge-cases.md](02-missing-edge-cases.md) | E1–E12: edge cases not traced or specified | Most covered by decisions; still to become test data |
| [03-hld-findings.md](03-hld-findings.md) | H1–H7: HLD findings, with your reasoning | H2–H5 moved to sequence diagrams and answered; H1 withdrawn; H7 closed |
| [04-hld-questions-H2-H5.md](04-hld-questions-H2-H5.md) | H2–H5 explained: examples, questions, options | Answered (see the decisions log) |
| [05-inconsistencies-and-wording.md](05-inconsistencies-and-wording.md) | I1–I6 and U1–U4 | U1–U4 resolved |
| [06-judgement-calls.md](06-judgement-calls.md) | J1–J12: open choices with options | All decided (see the decisions log) |
| [07-possible-extras.md](07-possible-extras.md) | X1–X11: not required by the challenge | For your decision |
| [08-decisions-log.md](08-decisions-log.md) | **Every decision, with reasoning, plus what's still unanswered** | Current source of truth |

**Start with [08-decisions-log.md](08-decisions-log.md). It is the source of truth.** Files 01–07 are the review findings; the decisions log records how each was settled.

The handwritten Brainstorm, FR, NFR and HLD documents are the initial analysis that started this review. They are kept as they are and are not updated; where they differ from these files, these files win.
