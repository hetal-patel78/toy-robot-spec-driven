# 01 Definite gaps or errors (D1–D8)

> Category: things that clearly didn't match the challenge. Source docs: FR, NFR, HLD, Brainstorm vs the challenge.
> [Back to index](00-index.md) · Current decisions: [08-decisions-log.md](08-decisions-log.md)

## What changed in the updated docs
- FR item 11 now has a note: "Place command is not ignored".
- FR item 12 is new: REPORT prints the robot's current location and direction.
- FR item 13 is new: a bad command must not crash the app or stop the next step. It is ignored, and a warning is logged to the console and a file so the user knows the reason.
- The NFR content is the same as before (items 1–6). The empty item 7 has been removed.
- The HLD (now its own file) is unchanged.

## Status
| # | Gap | Status | Severity now |
|---|---|---|---|
| D1 | REPORT has no requirement | **Resolved.** FR item 12, plus your decision to add REPORT to FR item 7. | — |
| D2 | An off-table PLACE is not covered | **Resolved by decision.** | — |
| D3 | FR item 11 ignores "any command", including PLACE | **Resolved.** The FR item 11 note and the D2 decision cover it. | — |
| D4 | Keep processing after a blocked move | **Resolved by decision** (H2 answer). | — |
| D5 | Coordinate range and origin not stated | **Resolved by decision** (0..4, origin SOUTH WEST). | — |
| D6 | Deliverables missing | **Resolved:** confirmed in scope. | — |
| D7 | PLACE syntax differs from the challenge | **Resolved:** `PLACE X,Y,F` with full direction words. | — |
| D8 | "Command line input" vs "standard input" | **Resolved by decision:** standard input only. | — |

## Details

**D1. REPORT has no functional requirement.**
- Original issue: FR items 1–11 did not cover REPORT.
- Now: FR item 12 says that when the user types REPORT, or a file contains REPORT, the system prints the robot's current location and direction. Together with FR item 11, this also covers REPORT before placement (it is ignored).
- Leftover (low): FR item 7 still lists "MOVE, LEFT, RIGHT & including PLACE" and leaves out REPORT. Item 12 makes the intent clear, so this is a consistency point only.
- Not a gap, but still open as a judgement call at the time: item 12 doesn't give the output format. That is J7.

**D2. No requirement that an off-table PLACE is ignored.**
- Issue: the challenge's constraints say the no-fall rule "also includes the initial placement". FR item 5 describes PLACE but never says X,Y must be on the table, or what happens when they are not.
- Why it matters: this is a stated constraint, and reviewers commonly probe it (for example PLACE 5,5,NORTH or PLACE -1,0,EAST).
- Point raised: FR item 11's note says "Place command is not ignored". Read literally, that says *every* PLACE is accepted, including PLACE 9,9,NORTH.
- Your reasoning in discussion: FR item 1 ("robot must not fall in any case") already covers the intent. Remaining gap at the time: the outcome (what happens instead) wasn't stated.
- Assumption: "ignored" means the robot's state stays as it was. If the robot was unplaced, it stays unplaced. If it was placed, it keeps its old position and direction.

**D3. FR item 11 says "any command" is ignored when the robot is not placed. That includes PLACE.**
- Original issue: read literally, the robot could never be placed.
- Now: the note "Place command is not ignored" fixes the main contradiction.
- Leftover: the challenge says to discard commands "until a **valid** PLACE has been executed". The note doesn't say "valid", so an invalid first PLACE isn't covered. Same root as D2.

**D4. "Keep processing after a blocked move" is not captured.** FR p1 item 3.
- Issue: the challenge says further valid movement commands "must still be allowed". FR item 3 only says to prevent the fall. It does not say that the robot stays usable, that the session continues, or that the blocked command does not count as an error.
- Why it matters: a design that stops or throws on a blocked move would still satisfy FR item 3 as written, but would fail the challenge.
- Note on the update: FR item 13 says exactly this, but only for a *bad command*. `MOVE` at the edge is a *valid* command that can't be carried out, so item 13 doesn't clearly cover it.

**D5. The table coordinate range and origin are not stated in the FR.** FR p1 item 2.
- Issue: FR item 2 says only "table dimension 5x5". It does not state that (0,0) is the SOUTH WEST corner, and it does not give the valid range of X and Y. Brainstorm p1 shows 0..4 in a drawing, but the FR never writes it down.
- Why it matters: boundary checks are the core logic, and an off-by-one error (allowing 5) is the classic bug in this challenge.

**D6. Required deliverables are missing from every document.**
- Issue: NFR item 3 covers the README only. The challenge also requires:
  - **test data** to exercise the app
  - a **development process description**: tools, how they were used, resources, decisions and trade-offs
  - **AI prompts and artefacts**, if AI was used
- Why it matters: these are explicit deliverables. You plan to use AI and Spec Kit, so the AI-artefacts item applies to you.

**D7. The PLACE syntax is written differently from the challenge.** FR p1 item 5.
- Issue: FR writes "PLACE X Y F" (no commas) and gives directions as "(N,S,E,W)". The challenge uses `PLACE X,Y,F` with commas and the full words NORTH/SOUTH/EAST/WEST.
- Why it matters: if the FR is the source for the parser, it would accept or expect the wrong format.

**D8. In the HLD, the input box "Command line input" doesn't match the challenge's "standard input".** HLD diagram, NFR item 2 and FR item 13.
- Issue: the challenge says the input can come "from a file, or from standard input". "Command line input" could mean command-line arguments, interactive stdin, or piped stdin. These are three different mechanisms.
- Why it matters: it affects the README instructions, the shape of the test data, and how the app knows the input has ended.
