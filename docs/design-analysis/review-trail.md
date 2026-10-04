# Toy Robot: review trail

How the design decisions were reached. Finding IDs use one letter per category: **D** gaps against the challenge, **E** edge cases, **H** high-level design, **I** inconsistencies between documents, **U** unclear wording, **J** judgement calls, **X** possible extras. Each finding from the review of the handwritten analysis is listed in one line, with the decision that settled it. The decisions themselves are only in [design-decisions.md](design-decisions.md). The full working notes are in [archive/](archive/README.md).

Back to [README](README.md).

## Process
1. **Handwritten analysis:** Brainstorm (hand-traced examples), FR (functional requirements), NFR (non-functional requirements), HLD (high-level design diagram).
2. **Review against the challenge**, producing the findings below.
3. **Clarifications** answering the findings (`docs/design-analysis/clarifications/*.pdf`), followed by discussion.
4. **Decisions** consolidated into `design-decisions.md`.
5. **Three independent reviews** of this folder by a separate AI agent with no prior context (see below), each followed by fixes.
6. **Next:** the solution design document, then the Spec Kit specification and implementation (DEC-36).

## Hand-traced examples checked (Brainstorm)
| Page | Trace | Correct? |
|---|---|---|
| p1 | 5x5 grid, (0,0) bottom-left, x across, y up | Yes |
| p2 top | PLACE 0,0,EAST, MOVE, MOVE, LEFT, MOVE gives 2,1,NORTH | Yes |
| p2 bottom | Crossed out on purpose (first attempt, corrected on p3) | — |
| p3 | PLACE 4,2,SOUTH, MOVE, LEFT, MOVE blocked at x=5 | Yes |
| p4 | PLACE 3,4,WEST, MOVE, MOVE, LEFT, MOVE, RIGHT, MOVE gives 0,3,WEST | Yes |
| p5 | PLACE 0,0,SOUTH, MOVE blocked at y=-1 | Yes |

## Gaps against the challenge (D)
| # | Finding | Settled by |
|---|---|---|
| D1 | REPORT had no functional requirement | DEC-18, DEC-21 |
| D2 | An off-table PLACE was not covered | DEC-15 |
| D3 | "Any command ignored before placement" also caught PLACE | DEC-15, DEC-19 |
| D4 | Continuing after a blocked move was not stated | DEC-16 |
| D5 | Coordinate range and origin not stated | DEC-01, DEC-02 |
| D6 | Test data, development write-up and AI artefacts missing | DEC-37 |
| D7 | PLACE written as `PLACE X Y F` with N/S/E/W | DEC-06, DEC-08 |
| D8 | "Command line input" instead of standard input | DEC-04 |

## Edge cases (E)
| # | Edge case | Settled by |
|---|---|---|
| E1 | Commands before any PLACE | DEC-19 |
| E2 | An invalid first PLACE | DEC-15 |
| E3 | A second valid PLACE | DEC-13, DEC-15 |
| E4 | An off-table PLACE while already placed | DEC-15 |
| E5–E7 | North and west edges, corners, full LEFT cycle | DEC-16, DEC-17, DEC-31 (tests) |
| E8 | The three challenge examples | DEC-31 (tests) |
| E9 | Hand traces without an expected REPORT | DEC-31 (hand traces listed as required tests) |
| E10 | Malformed input | DEC-20, DEC-22 |
| E11 | A number too large to read | DEC-20 |
| E12 | End of input | DEC-10 |

## High-level design (H)
| # | Finding | Settled by |
|---|---|---|
| H1 | Where output is written | Withdrawn: shown on the diagram (Report → Runner → console) |
| H2 | Who enforces the table edges | DEC-13, DEC-28 |
| H3 | How "not on the table" is represented | DEC-11, DEC-14 |
| H4 | What the parser does with bad input | DEC-20, DEC-28 |
| H5 | How Command and the rule-holding component relate | DEC-26, DEC-27 |
| H7 | Tests not shown on the diagram | DEC-29, DEC-31; test-flow diagram listed in DEC-37 |

Numbering follows the original review: H6 (one input box on the diagram) and two drawing points were withdrawn as not needed. Detailed flows for H2–H5 are drawn as sequence diagrams (DEC-37).

## Inconsistencies and wording (I, U)
| # | Finding | Settled by |
|---|---|---|
| I1 | HLD had Report, FR had no REPORT | DEC-18 |
| I2 | FR item 11 contradicted items 6 and 7 | DEC-15, DEC-19 |
| I3 | Brainstorm grid 0..4 vs FR "5x5" | DEC-01 |
| I4 | Direction notation varied | DEC-06 |
| I5, I6 | Input source under NFR; placeholder items | Handwritten docs kept as the initial analysis |
| U1 | "Could be selectable" | DEC-35 |
| U2 | "Easy to understand & review" | DEC-34 |
| U3 | "Any O/S" | DEC-33 |
| U4 | "(s,6)" note | Withdrawn: it means step 6 |

## Judgement calls (J)
| # | Question | Settled by |
|---|---|---|
| J1 | Coordinate range | DEC-01 |
| J2 | REPORT before placement | DEC-19 |
| J3 | Feedback for ignored commands | DEC-19, DEC-23 |
| J4 | Input source | DEC-04 |
| J5 | Parsing strictness | DEC-06, DEC-07, DEC-08, DEC-09, DEC-10 |
| J6 | Malformed lines | DEC-20 |
| J7 | REPORT format | DEC-21 |
| J8 | Ending a session | DEC-10, DEC-30 |
| J9 | Commands per line | DEC-05 |
| J10 | Where the rules live | DEC-26, DEC-27 |
| J11 | Changing or replacing state | DEC-12, DEC-13 |
| J12 | Representing "not on the table" | DEC-11 |

## Possible extras (X)
| # | Extra | Outcome |
|---|---|---|
| X1 | Both file argument and standard input | Not included (DEC-04) |
| X2 | Proven cross-platform support | Included: GitHub Actions on Windows, Linux and macOS (DEC-33) |
| X3 | Never crash | Included (DEC-32) |
| X4 | Configurable table size | Not included (DEC-03, DEC-39) |
| X5 | EXIT and usage text | Included (DEC-04, DEC-10) |
| X6 | Warnings for ignored commands | Included (DEC-19, DEC-20, DEC-22) |
| X7 | Logging framework or DI container | Not included (DEC-39) |
| X8 | CI pipeline and test tooling | Included: GitHub Actions running all tests (DEC-33) |
| X9 | Multiple robots, obstacles | Not built; design keeps room (DEC-38) |
| X10 | Dockerfile | Not included (DEC-39) |
| X11 | Including the handwritten docs | Included (DEC-37) |
| X12 | Log file | Not included (DEC-39) |

## Independent reviews (F, N, R)
Three reviews by a separate AI agent that had not seen the discussion, reading this folder and the challenge as a reviewer or a new engineer would. **F** = first review, **N** = second review, **R** = third review. All findings were fixed in `design-decisions.md` unless noted.

| # | Finding | Settled by |
|---|---|---|
| F1 | `Output:` label on standard output contradicted "standard output carries only results" | DEC-23 |
| F2 | Nothing said who creates the Table and RobotOperator | DEC-03, DEC-26, DEC-29 |
| F3 | Exact warning wording not specified | DEC-22 |
| F4 | Unclear which lines are echoed in file runs | DEC-25 |
| F5 | Many rules had no listed test; usage hint untestable | DEC-31 |
| F6 | Risk of reviewers feeding `>` not in the assumption | DEC-09, A1 |
| F7 | A1 fallback ignored merged output streams | A1 |
| F8 | Review-trail J5 pointed at the wrong decision | This file |
| F9 | CI tested only in-memory logic | DEC-31, DEC-33 |
| F10 | Edge inputs (extra values, tabs, `+1`, culture, byte-order mark, REPORT case) unspecified | DEC-06, DEC-07, DEC-08, DEC-20 |
| F11 | Exit codes and which arguments are rejected unspecified | DEC-04, DEC-10, DEC-32 |
| F12 | How REPORT and tests read the state undefined | DEC-14, DEC-26 |
| F13 | Example session showed only typed mode | At a glance |
| F14 | Broken formatting in the assumptions | Section 9 |
| F15 | H6 gap unexplained; diagrams had no home | This file, DEC-37 |
| F16 | E9 pointed at a decision that didn't cover the hand traces | DEC-31 |
| F17 | Prompt log missing outcomes and option context | Prompt log |
| F18 | Trimmed or incomplete challenge quotes | DEC-11, DEC-15, DEC-17, DEC-18, DEC-19 |
| F19 | Absolute paths, missing reasons, ambiguous "README" | README, all decisions |
| F20 | EXIT's lenient parsing; volume of process material | Kept as deliberate choices (DEC-10; README "a reviewer only needs design-decisions.md") |
| N1 | Documented `<` commands fail in Windows PowerShell | DEC-04, DEC-23, DEC-37 |
| N2 | How the command word is found contradicted itself | DEC-08, DEC-22 |
| N3 | Byte-order mark and console encoding on Windows | DEC-20, DEC-26 |
| N4 | Line endings and `run.sh` permissions in CI | DEC-31, DEC-33 |
| N5 | Off-table PLACE warning text ambiguous | DEC-22 |
| N6 | Command-name case in warnings; who adds `Output:` | DEC-21, DEC-22, DEC-26 |
| N7 | Run-script behaviour on errors undefined | DEC-37 |
| N8 | Prompt-log gaps; F findings not recorded | Prompt log, this section |
| N9 | A decision ID with a letter suffix | Decisions renumbered |
| N10 | README wording, "file" vs redirected input, path separators | README, DEC-04, DEC-25 |
| N11 | Two weak cross-references | This file, DEC-24 |
| N12 | Emphasis added to a quote without saying so | DEC-11 |
| N13 | Some tests not listed (exit code 0, several REPORTs, scripts) | DEC-31, DEC-33 |
| R1 | Setting the console encoding can fail on Windows without a console window | DEC-26 |
| R2 | .NET 8 support ends November 2026; SDK check and roll-forward not set | DEC-37 |
| R3 | Removing the byte-order mark "before reading" could delay the first prompt | DEC-20, DEC-26 |
| R4 | `ToyRobot < commands.txt` does not run as written in bash | DEC-04 |
| R5 | Stream for script messages and the CI script check unspecified | DEC-33, DEC-37 |
| R6 | README chain implied the parser calls RobotOperator | README |
| R7 | `CLAUDE.md` "record only in design-decisions.md" read as contradicting the review-trail rule | `CLAUDE.md` |
| R8 | Ctrl+C exit behaviour unspecified | DEC-10 |
