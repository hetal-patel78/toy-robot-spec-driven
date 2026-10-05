# Feature Specification: Robot rules

**Feature Branch**: `001-robot-rules`

**Created**: 2026-10-05

**Status**: Draft

**Input**: User description: "Robot rules for the Toy Robot Simulator. Owns FR-01 to FR-07, FR-09, FR-10 and NFR-07 from docs/solution-design/10-traceability.md, and the solution and project skeleton (ToyRobot.sln at the root, src/ToyRobot, tests/ToyRobot.Tests, tests/ToyRobot.ConsoleEndToEndTests, xUnit, .NET 8 with RollForward = LatestMajor) because specs 002 and 003 build on it. Components: Table, Direction, RobotState, Robot, IRobotCommand with PlaceCommand, MoveCommand, LeftCommand, RightCommand and ReportCommand, IRobotOperator, RobotOperator and CommandParser, exactly as on solution design page 6. Tests it plans: unit tests. Sources: design-decisions.md and solution design pages 2, 3, 5, 6, 7.1 to 7.5 and 10. Acceptance scenarios are copied from page 2 with exact input, console output (stdout) and console messages (stderr) stated separately, plus the DEC-31 cases this spec's components own. Every warning text is copied character for character from DEC-22. Every requirement cites its FR or NFR and DEC IDs. No behaviour beyond the sources."

## Sources and ownership

- **Source of truth**: `docs/design-analysis/design-decisions.md` (DEC-nn), then the solution design in `docs/solution-design/` (pages 2, 3, 5, 6, 7.1 to 7.5 and 10). Where this spec and those documents differ, those documents win ([DEC-36][dec-36]).
- **Owned by this spec**: [FR-01][fr-01], [FR-02][fr-02], [FR-03][fr-03], [FR-04][fr-04], [FR-05][fr-05], [FR-06][fr-06], [FR-07][fr-07], [FR-09][fr-09], [FR-10][fr-10] and [NFR-07][nfr-07]; the solution and project skeleton.
- **Components specified here** (names and members exactly as on page 6): Table, Direction, RobotState, Robot, `IRobotCommand` with `PlaceCommand`, `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand`, `IRobotOperator`, RobotOperator and CommandParser.
- **Referred to, owned elsewhere** (not restated here): Runner and Program, and [FR-08][fr-08], [FR-11][fr-11] to [FR-17][fr-17], [NFR-01][nfr-01], [NFR-04][nfr-04], [NFR-05][nfr-05], [NFR-06][nfr-06] (spec 002-console-session); [NFR-02][nfr-02], [NFR-03][nfr-03], [NFR-08][nfr-08], [NFR-09][nfr-09] (spec 003-delivery).

## Clarifications

### Session 2026-10-05

Clarifications were answered under rule 11 of the Spec Kit brief: each question is answered from the sources where they settle it, otherwise decided under rule 10 (see Assumptions).

- Q: What are the names of the parse result, the outcome, and the members that page 6 describes but does not name (the CommandParser's parse step, RobotOperator's run step, the Robot's placement and its replace method)? → A: Decided (page 6 gives the terms, not code names): the parse result is `ParseResult`, one of `ParseResult.CommandObject` (holding the `IRobotCommand`), `ParseResult.Exit` or `ParseResult.Failure` (holding the reason); the outcome is `Outcome`, one of `Outcome.PositionText` (holding the text), `Outcome.Warning` (holding the text) or `Outcome.Done`; the parse step is `CommandParser.Parse(line)`; the run step is `RobotOperator.Run(command)`; the Robot has `Placement` and the single replace method `Replace(state)`. Each name is page 6's own term (page 6 "Parse result", "Outcome", diagrams 7.1 to 7.5 "pass the line", "run the ... Command", "replace placement"). Applied to [FR-10a][fr-10a], [NFR-07d][nfr-07d], [NFR-07h][nfr-07h], [NFR-07i][nfr-07i] and Key Entities.
- Q: When command words and direction names are compared ignoring letter case, may the comparison depend on the machine's regional settings? → A: No. [DEC-08][dec-08] requires reading that is "the same on every machine" and [DEC-07][dec-07] makes case carry no meaning; the comparison uses fixed, culture-independent rules (a character-by-character comparison that ignores case), so for example `exit`, `right` and `place` are recognised under every regional setting, including Turkish, where the upper case of `i` is not `I`. Applied to [FR-09][fr-09] and Edge Cases.
- Q: When a warning writes numbers back (the off-table PLACE warning with parsed values, the blocked MOVE warning), may the minus sign or digits depend on the machine's regional settings? → A: No. [DEC-22][dec-22] fixes the exact text and [DEC-08][dec-08] reads numbers with the invariant culture; numbers are written with the same fixed rules, so `PLACE -1,0,NORTH` always gives `PLACE -1,0,NORTH ignored - position is off the table` with the ASCII hyphen-minus `-`, under every regional setting. Applied to [FR-02][fr-02], [FR-04][fr-04], [FR-06][fr-06] and Edge Cases.
- Q: Is a direction written as a number, such as `PLACE 1,2,1`, accepted? → A: No. [DEC-06][dec-06] and [FR-09][fr-09] step 5 allow only the four full words NORTH, SOUTH, EAST and WEST; any other text, including a number, is an invalid direction: `'PLACE 1,2,1' ignored - F must be NORTH, SOUTH, EAST or WEST`. Applied to [FR-09][fr-09] and Edge Cases.
- Q: Which characters end the command word: only spaces, or every kind of whitespace? → A: Every kind of whitespace: [DEC-08][dec-08] step 1 defines whitespace as "any whitespace character" (the set `Trim()` removes) and step 2 uses the same term, so a tab or a non-breaking space also ends the command word. `PLACE[TAB]1,2,EAST` is a valid PLACE and `MOVE[NBSP]now` gives `'MOVE[NBSP]now' ignored - MOVE takes no values`. Applied to [FR-09][fr-09] and Edge Cases.

## User Scenarios & Testing *(mandatory)*

### How to read the scenarios

- **Given**, unless a scenario says otherwise: a new session, a 5 x 5 table, the robot not on the table, and input **redirected** from a file (not typed) (page 2).
- **Console output (stdout)** carries only the REPORT result (`Output:` and the position). **Console messages (stderr)** carry everything else: here the echo of each line read (`> ` followed by the line without whitespace at either end) and warnings. "(empty)" means nothing is written to that channel ([DEC-23][dec-23], page 2).
- The **exit code** is 0 in every scenario in this spec (end of input; [FR-16][fr-16], owned by spec 002).
- The console text (the echo, the `Warning: ` prefix and the `Output:` label) is written by the Runner, which spec 002 owns ([DEC-21][dec-21], [DEC-25][dec-25], [DEC-26][dec-26]). The components in this spec produce what the Runner writes: the CommandParser's parse result (a failure's reason is the warning text without `Warning: `) and RobotOperator's outcome (the position text without `Output:`, a warning text without `Warning: `, or done with nothing to show) (page 6). This spec's unit tests check those results exactly; spec 002's Runner whole-flow tests check the same scenarios as console text.
- Invisible characters are written as `[TAB]` (one tab character) and `[NBSP]` (one non-breaking space, U+00A0).

### User Story 1 - Place the robot and read its position (Priority: P1)

A person puts the robot on the table with PLACE and asks where it is with REPORT. A PLACE off the table is refused, so the robot never starts off the table, and a later valid PLACE moves it anywhere on the table.

**Why this priority**: Nothing else works until the robot can be placed and reported; Challenge Example 1 to 3 all start this way.

**Independent Test**: Unit tests of Table, CommandParser, `PlaceCommand`, `ReportCommand` and RobotOperator with PLACE and REPORT inputs only.

**Acceptance Scenarios** (page 2 unless marked [DEC-31][dec-31]):

1. **[FR-01; DEC-01, DEC-02]** **When** the input lines are:
   ```text
   PLACE 4,4,NORTH
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   4,4,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 4,4,NORTH
   > REPORT
   ```

2. **[FR-02; DEC-11, DEC-15]** **When** the input lines are:
   ```text
   PLACE 4,2,NORTH
   PLACE 9,9,NORTH
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   4,2,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 4,2,NORTH
   > PLACE 9,9,NORTH
   Warning: PLACE 9,9,NORTH ignored - position is off the table
   > REPORT
   ```

3. **[FR-06; DEC-07, DEC-18, DEC-21]** **When** the input lines are:
   ```text
   place 1,2,north
   report
   ```
   **Then console output (stdout):**
   ```text
   Output:
   1,2,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > place 1,2,north
   > report
   ```

4. **[FR-02; DEC-15, DEC-19, DEC-31 "an invalid first PLACE"]** **When** the input lines are:
   ```text
   PLACE 9,9,NORTH
   REPORT
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > PLACE 9,9,NORTH
   Warning: PLACE 9,9,NORTH ignored - position is off the table
   > REPORT
   Warning: REPORT ignored - robot is not on the table
   ```

5. **[FR-02; DEC-15, DEC-31 "a second valid PLACE"]** **When** the input lines are:
   ```text
   PLACE 4,2,NORTH
   PLACE 1,1,EAST
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   1,1,EAST
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 4,2,NORTH
   > PLACE 1,1,EAST
   > REPORT
   ```

6. **[FR-02; DEC-15, DEC-20, DEC-22, DEC-31 "a negative position"]** **When** the input line is:
   ```text
   PLACE -1,0,NORTH
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > PLACE -1,0,NORTH
   Warning: PLACE -1,0,NORTH ignored - position is off the table
   ```

7. **[FR-02; DEC-22, DEC-31 "the off-table warning with parsed values"]** **When** the input line is:
   ```text
   place 09 , +9 , north
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > place 09 , +9 , north
   Warning: PLACE 9,9,NORTH ignored - position is off the table
   ```

8. **[FR-06; DEC-21, DEC-31 "several REPORTs in one run"]** **When** the input lines are:
   ```text
   PLACE 0,0,NORTH
   REPORT
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   Output:
   0,1,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH
   > REPORT
   > MOVE
   > REPORT
   ```

9. **[FR-01; DEC-01, DEC-03, DEC-31 "a small table such as 2 x 2"]** **Given** a Table of width 2 and height 2 (built directly in a unit test; the application itself always uses 5 x 5), **Then** positions 0,0, 1,0, 0,1 and 1,1 are on the table, and 2,0, 0,2, -1,0 and 0,-1 are not. **And given** RobotOperator with that table and the robot placed with a `PlaceCommand` at 1,1,NORTH, **When** a `MoveCommand` is run, **Then** RobotOperator returns the warning text `MOVE ignored - next position is off the table so robot stays at 1,1,NORTH` (written by the Runner as `Warning: MOVE ignored - next position is off the table so robot stays at 1,1,NORTH` to console messages (stderr), with nothing on console output (stdout)), and a `ReportCommand` then returns the position text `1,1,NORTH`. A `PlaceCommand` for 2,2,NORTH on that table returns `PLACE 2,2,NORTH ignored - position is off the table`.

---

### User Story 2 - Move and turn without falling off (Priority: P1)

A person moves the robot forward and turns it left and right. A move that would take the robot off the table is refused with a warning, and the robot can still take later commands.

**Why this priority**: The core challenge rule: the robot must never fall off the table, and valid movement must still work after a refused move.

**Independent Test**: Unit tests of Direction, `MoveCommand`, `LeftCommand`, `RightCommand` and RobotOperator with a real Table, each starting from a placed robot.

**Acceptance Scenarios**:

1. **[FR-03; DEC-02, DEC-16] Challenge Example 1.** **When** the input lines are:
   ```text
   PLACE 0,0,NORTH
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,1,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH
   > MOVE
   > REPORT
   ```

2. **[FR-04; DEC-16]** (also the Brainstorm page 5 trace, [DEC-31][dec-31]) **When** the input lines are:
   ```text
   PLACE 0,0,SOUTH
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,SOUTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,SOUTH
   > MOVE
   Warning: MOVE ignored - next position is off the table so robot stays at 0,0,SOUTH
   > REPORT
   ```

3. **[FR-05; DEC-17] Challenge Example 2.** **When** the input lines are:
   ```text
   PLACE 0,0,NORTH
   LEFT
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,WEST
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,NORTH
   > LEFT
   > REPORT
   ```

4. **[FR-03, FR-05; DEC-02, DEC-16, DEC-17, DEC-31 "the three challenge examples"] Challenge Example 3.** **When** the input lines are:
   ```text
   PLACE 1,2,EAST
   MOVE
   MOVE
   LEFT
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   3,3,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 1,2,EAST
   > MOVE
   > MOVE
   > LEFT
   > MOVE
   > REPORT
   ```

5. **[FR-03, FR-05; DEC-31 "the hand-traced Brainstorm examples", 2,1,NORTH]** **When** the input lines are:
   ```text
   PLACE 0,0,EAST
   MOVE
   MOVE
   LEFT
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   2,1,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 0,0,EAST
   > MOVE
   > MOVE
   > LEFT
   > MOVE
   > REPORT
   ```

6. **[FR-03, FR-04, FR-05; DEC-16, DEC-31 Brainstorm page 3, blocked move]** **When** the input lines are:
   ```text
   PLACE 4,2,SOUTH
   MOVE
   LEFT
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   4,1,EAST
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 4,2,SOUTH
   > MOVE
   > LEFT
   > MOVE
   Warning: MOVE ignored - next position is off the table so robot stays at 4,1,EAST
   > REPORT
   ```

7. **[FR-03, FR-05; DEC-31 Brainstorm, 0,3,WEST]** **When** the input lines are:
   ```text
   PLACE 3,4,WEST
   MOVE
   MOVE
   LEFT
   MOVE
   RIGHT
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,3,WEST
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 3,4,WEST
   > MOVE
   > MOVE
   > LEFT
   > MOVE
   > RIGHT
   > MOVE
   > REPORT
   ```

8. **[FR-04; DEC-16, DEC-22, DEC-31 "every edge and corner"]** For each row, **When** the input lines are `PLACE <start>`, `MOVE`, `REPORT`, **Then console output (stdout)** is `Output:` followed by `<start>`, and **console messages (stderr)** are `> PLACE <start>`, `> MOVE`, `Warning: MOVE ignored - next position is off the table so robot stays at <start>`, `> REPORT`:

   | Case | `<start>` |
   |---|---|
   | North edge | `2,4,NORTH` |
   | South edge | `2,0,SOUTH` |
   | East edge (page 7.2) | `4,2,EAST` |
   | West edge | `0,2,WEST` |
   | South west corner | `0,0,SOUTH` and `0,0,WEST` |
   | North west corner | `0,4,NORTH` and `0,4,WEST` |
   | North east corner | `4,4,NORTH` and `4,4,EAST` |
   | South east corner | `4,0,SOUTH` and `4,0,EAST` |

   For example, for the east edge the console messages (stderr) are:
   ```text
   > PLACE 4,2,EAST
   > MOVE
   Warning: MOVE ignored - next position is off the table so robot stays at 4,2,EAST
   > REPORT
   ```

9. **[FR-04; DEC-16]** **When** the input lines are:
   ```text
   PLACE 4,2,EAST
   MOVE
   LEFT
   MOVE
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   4,3,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 4,2,EAST
   > MOVE
   Warning: MOVE ignored - next position is off the table so robot stays at 4,2,EAST
   > LEFT
   > MOVE
   > REPORT
   ```

10. **[FR-05; DEC-17, DEC-31 "full LEFT and RIGHT cycles"]** **When** the input lines are:
    ```text
    PLACE 2,2,NORTH
    LEFT
    REPORT
    LEFT
    REPORT
    LEFT
    REPORT
    LEFT
    REPORT
    ```
    **Then console output (stdout):**
    ```text
    Output:
    2,2,WEST
    Output:
    2,2,SOUTH
    Output:
    2,2,EAST
    Output:
    2,2,NORTH
    ```
    **Console messages (stderr):** the echo of each of the nine lines (`> PLACE 2,2,NORTH`, then `> LEFT` and `> REPORT` four times), and nothing else.

11. **[FR-05; DEC-17, DEC-31 "full LEFT and RIGHT cycles"]** **When** the input lines are:
    ```text
    PLACE 2,2,NORTH
    RIGHT
    REPORT
    RIGHT
    REPORT
    RIGHT
    REPORT
    RIGHT
    REPORT
    ```
    **Then console output (stdout):**
    ```text
    Output:
    2,2,EAST
    Output:
    2,2,SOUTH
    Output:
    2,2,WEST
    Output:
    2,2,NORTH
    ```
    **Console messages (stderr):** the echo of each of the nine lines (`> PLACE 2,2,NORTH`, then `> RIGHT` and `> REPORT` four times), and nothing else.

---

### User Story 3 - Robot commands before the robot is on the table (Priority: P2)

Until a valid PLACE, MOVE, LEFT, RIGHT and REPORT are ignored, each with a warning that names the command, so the person knows why nothing happened.

**Why this priority**: the challenge requires that commands are discarded until a valid PLACE; the warning tells the person why.

**Independent Test**: Unit tests of RobotOperator with a new robot and each of the four command objects.

**Acceptance Scenarios**:

1. **[FR-07; DEC-19]** **When** the input lines are:
   ```text
   MOVE
   PLACE 0,0,NORTH
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > MOVE
   Warning: MOVE ignored - robot is not on the table
   > PLACE 0,0,NORTH
   > REPORT
   ```

2. **[FR-07; DEC-19, DEC-22, DEC-31 "MOVE, LEFT, RIGHT and REPORT each before PLACE"]** **When** the input lines are:
   ```text
   MOVE
   LEFT
   RIGHT
   REPORT
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > MOVE
   Warning: MOVE ignored - robot is not on the table
   > LEFT
   Warning: LEFT ignored - robot is not on the table
   > RIGHT
   Warning: RIGHT ignored - robot is not on the table
   > REPORT
   Warning: REPORT ignored - robot is not on the table
   ```
   At unit level, RobotOperator returns each warning without running the command and without asking the Table (page 7.3).

3. **[FR-07, FR-09; DEC-07, DEC-19, DEC-22]** **When** the input line is:
   ```text
   move
   ```
   **Then console output (stdout):** (empty)
   **Console messages (stderr):**
   ```text
   > move
   Warning: MOVE ignored - robot is not on the table
   ```

---

### User Story 4 - Lines are read the same way however they are typed, and bad lines are refused (Priority: P2)

A person may type commands in any letter case, with extra whitespace of any kind around the line and the PLACE values. A line that is not a valid command is ignored with one warning that quotes the line and says what is wrong, and processing continues with the next line.

**Why this priority**: People do not type exactly; a bad line must never stop the session or crash the application.

**Independent Test**: Unit tests of CommandParser for every row of the [DEC-08][dec-08] table and every bad-line warning in [DEC-22][dec-22], plus the parse result for EXIT.

**Acceptance Scenarios**:

1. **[FR-09; DEC-07, DEC-08]** **When** the input lines are:
   ```text
   place 0 , 0 , north
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > place 0 , 0 , north
   > REPORT
   ```

2. **[FR-10; DEC-08, DEC-20, DEC-22]** **When** the input lines are:
   ```text
   PLACE 1,0,EAST
   PLACE 1,2
   PLACE 3,2,NORTH
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   3,2,NORTH
   ```
   **Console messages (stderr):**
   ```text
   > PLACE 1,0,EAST
   > PLACE 1,2
   Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F
   > PLACE 3,2,NORTH
   > REPORT
   ```

3. **[FR-10; DEC-06, DEC-08, DEC-09, DEC-20, DEC-22, DEC-31 "every warning in DEC-22"] One bad line each.** For each row, **When** the input is that one line, **Then console output (stdout):** (empty), and **console messages (stderr)** are exactly the two lines shown (the echo, then the warning):

   | Input line | Console messages (stderr) |
   |---|---|
   | `HELLO` | `> HELLO`<br>`Warning: 'HELLO' ignored - unknown command` |
   | `> MOVE` | `> > MOVE`<br>`Warning: '> MOVE' ignored - unknown command` |
   | `EXITNOW` | `> EXITNOW`<br>`Warning: 'EXITNOW' ignored - unknown command` |
   | `PLACE0,0,NORTH` | `> PLACE0,0,NORTH`<br>`Warning: 'PLACE0,0,NORTH' ignored - unknown command` |
   | `PLACEHOLDER` | `> PLACEHOLDER`<br>`Warning: 'PLACEHOLDER' ignored - unknown command` |
   | `PLACE,1,2` | `> PLACE,1,2`<br>`Warning: 'PLACE,1,2' ignored - unknown command` |
   | An empty line | `> ` (`>` and a space)<br>`Warning: blank line ignored` |
   | A line of only spaces and tabs | `> ` (`>` and a space)<br>`Warning: blank line ignored` |
   | `PLACE 1,2` | `> PLACE 1,2`<br>`Warning: 'PLACE 1,2' ignored - expected PLACE X,Y,F` |
   | `PLACE 0 0 NORTH` | `> PLACE 0 0 NORTH`<br>`Warning: 'PLACE 0 0 NORTH' ignored - expected PLACE X,Y,F` |
   | `PLACE A,0,NORTH` | `> PLACE A,0,NORTH`<br>`Warning: 'PLACE A,0,NORTH' ignored - X must be a whole number` |
   | `PLACE 9999999999,0,NORTH` | `> PLACE 9999999999,0,NORTH`<br>`Warning: 'PLACE 9999999999,0,NORTH' ignored - X is too large` |
   | `PLACE 99999999999,0,NORTH` | `> PLACE 99999999999,0,NORTH`<br>`Warning: 'PLACE 99999999999,0,NORTH' ignored - X is too large` |
   | `PLACE -99999999999,0,NORTH` | `> PLACE -99999999999,0,NORTH`<br>`Warning: 'PLACE -99999999999,0,NORTH' ignored - X is too large` |
   | `PLACE 1 0,0,NORTH` | `> PLACE 1 0,0,NORTH`<br>`Warning: 'PLACE 1 0,0,NORTH' ignored - X must be a whole number` |
   | `PLACE 0,B,NORTH` | `> PLACE 0,B,NORTH`<br>`Warning: 'PLACE 0,B,NORTH' ignored - Y must be a whole number` |
   | `PLACE 0,9999999999,NORTH` | `> PLACE 0,9999999999,NORTH`<br>`Warning: 'PLACE 0,9999999999,NORTH' ignored - Y is too large` |
   | `PLACE 0,99999999999,NORTH` | `> PLACE 0,99999999999,NORTH`<br>`Warning: 'PLACE 0,99999999999,NORTH' ignored - Y is too large` |
   | `PLACE 1,2,UP` | `> PLACE 1,2,UP`<br>`Warning: 'PLACE 1,2,UP' ignored - F must be NORTH, SOUTH, EAST or WEST` |
   | `PLACE 0,0,N` | `> PLACE 0,0,N`<br>`Warning: 'PLACE 0,0,N' ignored - F must be NORTH, SOUTH, EAST or WEST` |
   | `PLACE 1,2,N` | `> PLACE 1,2,N`<br>`Warning: 'PLACE 1,2,N' ignored - F must be NORTH, SOUTH, EAST or WEST` |
   | `PLACE 1,2,` | `> PLACE 1,2,`<br>`Warning: 'PLACE 1,2,' ignored - F must be NORTH, SOUTH, EAST or WEST` |
   | `PLACE 1,2,NORTH EXTRA` | `> PLACE 1,2,NORTH EXTRA`<br>`Warning: 'PLACE 1,2,NORTH EXTRA' ignored - F must be NORTH, SOUTH, EAST or WEST` |
   | `move now` | `> move now`<br>`Warning: 'move now' ignored - MOVE takes no values` |
   | `MOVE now` | `> MOVE now`<br>`Warning: 'MOVE now' ignored - MOVE takes no values` |
   | `LEFT now` | `> LEFT now`<br>`Warning: 'LEFT now' ignored - LEFT takes no values` |
   | `RIGHT now` | `> RIGHT now`<br>`Warning: 'RIGHT now' ignored - RIGHT takes no values` |
   | `REPORT now` | `> REPORT now`<br>`Warning: 'REPORT now' ignored - REPORT takes no values` |

   At unit level, CommandParser returns a failure whose reason is the warning text without `Warning: ` (page 6), for example `'PLACE 1,2' ignored - expected PLACE X,Y,F` and `blank line ignored`.

4. **[FR-09; DEC-08, DEC-31 "every row of the DEC-08 table"] Valid rows.** For each row, **When** the input lines are the row's lines, **Then** console output (stdout) and console messages (stderr) are as shown:

   | Input lines | Console output (stdout) | Console messages (stderr) |
   |---|---|---|
   | `PLACE 0,0,NORTH`<br>`   MOVE   ` (three spaces before and after)<br>`REPORT` | `Output:`<br>`0,1,NORTH` | `> PLACE 0,0,NORTH`<br>`> MOVE`<br>`> REPORT` |
   | `PLACE     0,0,NORTH` (five spaces after PLACE)<br>`REPORT` | `Output:`<br>`0,0,NORTH` | `> PLACE     0,0,NORTH`<br>`> REPORT` |
   | `place 0 , 0 , north`<br>`REPORT` | `Output:`<br>`0,0,NORTH` | `> place 0 , 0 , north`<br>`> REPORT` |
   | `PLACE 01,+1,EAST`<br>`REPORT` | `Output:`<br>`1,1,EAST` | `> PLACE 01,+1,EAST`<br>`> REPORT` |

   The bad rows of the [DEC-08][dec-08] table (`PLACE 0 0 NORTH`, `PLACE0,0,NORTH`, `PLACEHOLDER`, `PLACE,1,2`, `PLACE 1 0,0,NORTH`, `PLACE 1,2,`, `PLACE 1,2,NORTH EXTRA`) are in scenario 3.

5. **[FR-09; DEC-08, DEC-31 "tabs and non-breaking spaces as whitespace"]** **When** the input lines are:
   ```text
   [TAB]place[NBSP]0,[NBSP]0[TAB],north[NBSP]
   REPORT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   0,0,NORTH
   ```
   **Console messages (stderr):** (whitespace removed from both ends only; the whitespace inside the line is kept)
   ```text
   > place[NBSP]0,[NBSP]0[TAB],north
   > REPORT
   ```

6. **[FR-09; DEC-07, DEC-21, DEC-31 "case-insensitivity, lower and mixed case, REPORT in upper case after lower-case input"]** **When** the input lines are:
   ```text
   Place 1,1,East
   mOvE
   rEpOrT
   ```
   **Then console output (stdout):**
   ```text
   Output:
   2,1,EAST
   ```
   **Console messages (stderr):**
   ```text
   > Place 1,1,East
   > mOvE
   > rEpOrT
   ```

7. **[FR-10; DEC-22] Only the first problem is reported** (order: command word, number of values, X, Y, F). For each row, **When** the input is that one line, **Then console output (stdout):** (empty), and **console messages (stderr)** are the echo followed by the warning shown:

   | Input line | Warning in console messages (stderr) |
   |---|---|
   | `PLACE A,B` | `Warning: 'PLACE A,B' ignored - expected PLACE X,Y,F` |
   | `PLACE 1,2,3,4` | `Warning: 'PLACE 1,2,3,4' ignored - expected PLACE X,Y,F` |
   | `PLACE` | `Warning: 'PLACE' ignored - expected PLACE X,Y,F` |
   | `PLACE A,B,UP` | `Warning: 'PLACE A,B,UP' ignored - X must be a whole number` |
   | `PLACE 0,B,UP` | `Warning: 'PLACE 0,B,UP' ignored - Y must be a whole number` |
   | `PLACE 9999999999999999999,111111111111111111111111111,east` | `Warning: 'PLACE 9999999999999999999,111111111111111111111111111,east' ignored - X is too large` |
   | `PLACE 1,111111111111111111111111111,east` | `Warning: 'PLACE 1,111111111111111111111111111,east' ignored - Y is too large` |

8. **[FR-10; DEC-08, DEC-10, page 6 "Parse result"] EXIT is recognised by the parser.** **When** the input lines are `exit now`, `REPORT`, **Then console output (stdout):** (empty); **console messages (stderr):** `> exit now` (page 2 [FR-15][fr-15] example; the session ending, the unread REPORT line and exit code 0 are [FR-15][fr-15], owned and tested as console text by spec 002). At unit level, CommandParser returns EXIT for `EXIT`, `exit`, `Exit`, `EXIT now` and `exit now` (anything after the command word is ignored), and a failure with reason `'EXITING' ignored - unknown command` for `EXITING`. What the session does with EXIT (stop reading, exit code 0) is [FR-15][fr-15], owned by spec 002.

---

### Edge Cases

All of these are covered by the scenarios above or follow directly from the rules below; each has a planned unit test.

- A PLACE at the largest number that can be read (`PLACE 2147483647,0,NORTH`) is read normally and refused by the Table: `Warning: PLACE 2147483647,0,NORTH ignored - position is off the table`. One more (`PLACE 2147483648,0,NORTH`) is a whole number but too large to read: `Warning: 'PLACE 2147483648,0,NORTH' ignored - X is too large` ([DEC-20][dec-20], [DEC-22][dec-22]).
- A Y value too large to read (`PLACE 0,99999999999,NORTH`): `Warning: 'PLACE 0,99999999999,NORTH' ignored - Y is too large` ([DEC-20][dec-20], [DEC-22][dec-22]).
- A hugely negative X written as a whole number (`PLACE -99999999999,0,NORTH`) is too large to read in the other direction and gets the same reason, `X is too large`; a value not written as a whole number (`PLACE 1.5,0,NORTH`) keeps `X must be a whole number` ([DEC-20][dec-20], [DEC-22][dec-22]).
- Three empty PLACE values (`PLACE ,,`): `Warning: 'PLACE ,,' ignored - X must be a whole number` ([DEC-08][dec-08] step 3, [DEC-22][dec-22] order).
- A bad line is refused by the CommandParser before the robot is considered, so `REPORT now` before PLACE gives the "takes no values" warning, not the "not on the table" warning ([DEC-27][dec-27], [DEC-22][dec-22]).
- A refused PLACE or MOVE never leaves the robot changed: the next position is checked with the Table before the robot's state is replaced ([DEC-13][dec-13]); a later valid command still works ([DEC-16][dec-16]).
- LEFT and RIGHT always succeed once the robot is on the table, because turning never changes the position ([DEC-13][dec-13], [DEC-17][dec-17]).
- An off-table PLACE before any valid PLACE leaves the robot off the table; after a valid PLACE it leaves the robot where it was ([DEC-15][dec-15]).
- Letter case is compared with culture-independent rules: `exit`, `right` and `place 1,2,east` are read the same under every regional setting, including Turkish ([DEC-07][dec-07], [DEC-08][dec-08]; Clarifications).
- Numbers in warnings are written with culture-independent rules: under any regional setting, `PLACE -1,0,NORTH` gives `Warning: PLACE -1,0,NORTH ignored - position is off the table` with the ASCII hyphen-minus ([DEC-08][dec-08], [DEC-22][dec-22]; Clarifications).
- A direction written as a number is invalid: `PLACE 1,2,1` gives `Warning: 'PLACE 1,2,1' ignored - F must be NORTH, SOUTH, EAST or WEST` ([DEC-06][dec-06]; Clarifications).
- Any whitespace character ends the command word: `PLACE[TAB]1,2,EAST` places the robot at 1,2,EAST, and `MOVE[NBSP]now` gives `Warning: 'MOVE[NBSP]now' ignored - MOVE takes no values` ([DEC-08][dec-08] steps 1 and 2; Clarifications).

## Requirements *(mandatory)*

### Functional Requirements (owned)

- **FR-01**<a id="fr-01"></a> ([DEC-01][dec-01], [DEC-02][dec-02], [DEC-03][dec-03]): The table MUST be 5 x 5 with valid positions X = 0..4 and Y = 0..4; (0,0) is the SOUTH WEST corner, X increases towards EAST and Y towards NORTH. The Table MUST receive its width and height when it is created and answer whether a position X,Y is on the table: on the table when 0 ≤ X < width and 0 ≤ Y < height. The application's 5 x 5 table is created by Program (spec 002); tests may create any size, for example 2 x 2.
- **FR-02**<a id="fr-02"></a> ([DEC-11][dec-11], [DEC-13][dec-13], [DEC-15][dec-15], [DEC-22][dec-22], [DEC-28][dec-28]): A valid PLACE MUST put the robot at X,Y facing F, whether it is the first PLACE or a later one. A PLACE whose position is off the table MUST be ignored with the warning text `PLACE X,Y,F ignored - position is off the table`, where X, Y and F are the values as read (parsed numbers, written with culture-independent rules so a negative number always starts with the ASCII hyphen-minus `-`; direction in upper case), for example `PLACE 9,9,NORTH ignored - position is off the table`; the robot MUST stay as it was (still not on the table, or at its previous position). There is no default position: the robot exists from the start but is not on the table until a valid PLACE.
- **FR-03**<a id="fr-03"></a> ([DEC-02][dec-02], [DEC-13][dec-13], [DEC-16][dec-16]): MOVE MUST move the robot one unit forward in the direction it faces: NORTH Y + 1, SOUTH Y − 1, EAST X + 1, WEST X − 1.
- **FR-04**<a id="fr-04"></a> ([DEC-13][dec-13], [DEC-16][dec-16], [DEC-22][dec-22]): A MOVE that would take the robot off the table MUST be ignored with the warning text `MOVE ignored - next position is off the table so robot stays at X,Y,F`, using the robot's current position in the same form as REPORT ([FR-06][fr-06]), for example `MOVE ignored - next position is off the table so robot stays at 4,2,EAST`. The robot MUST stay where it is, and later commands MUST still work.
- **FR-05**<a id="fr-05"></a> ([DEC-13][dec-13], [DEC-17][dec-17]): LEFT and RIGHT MUST turn the robot 90 degrees without moving it. LEFT: NORTH → WEST → SOUTH → EAST → NORTH. RIGHT: NORTH → EAST → SOUTH → WEST → NORTH.
- **FR-06**<a id="fr-06"></a> ([DEC-07][dec-07], [DEC-18][dec-18], [DEC-21][dec-21]): REPORT MUST produce the position as `X,Y,F` with the direction in upper case whatever case the input used, for example `0,1,NORTH`; the numbers are written with culture-independent rules ([DEC-08][dec-08]; Clarifications). RobotOperator returns only this position text; the Runner (spec 002) writes `Output:` and then the position text, both to console output (stdout), so each REPORT prints its own two lines:
  ```text
  Output:
  0,1,NORTH
  ```
  How a reviewer will check this format is not stated (assumption A1); the format above is the default.
- **FR-07**<a id="fr-07"></a> ([DEC-19][dec-19], [DEC-27][dec-27]): Until a valid PLACE, MOVE, LEFT, RIGHT and REPORT MUST each be ignored with the warning text `<NAME> ignored - robot is not on the table`, where `<NAME>` is the command name in upper case: `MOVE ignored - robot is not on the table`, `LEFT ignored - robot is not on the table`, `RIGHT ignored - robot is not on the table`, `REPORT ignored - robot is not on the table`. RobotOperator MUST make this check once, before running the command; the command is not run and the Table is not asked. PLACE is never refused by this check.
- **FR-09**<a id="fr-09"></a> ([DEC-07][dec-07], [DEC-08][dec-08]): Commands and directions MUST be accepted in any letter case. Letter case MUST be ignored with culture-independent rules, so the result never depends on the machine's regional settings (for example `exit` and `right` are recognised under a Turkish setting). A line MUST be read as follows:
  1. Whitespace of any kind (spaces, tabs, non-breaking spaces and every other character that the platform's standard trim removes) at the start and end of the line is removed. Whitespace inside the line is never removed; it only separates the parts.
  2. The command word is the text up to the first whitespace character (any character of the same kind as in step 1, so a tab or a non-breaking space also ends it), or the whole line if there is none.
  3. For PLACE, the rest of the line is split on commas into exactly three parts (X, Y, F), and whitespace of any kind around each part is removed. At least one whitespace character is required between `PLACE` and its values.
  4. X and Y are whole numbers read with fixed rules that do not depend on the machine's regional settings: an optional leading sign (`-` or `+`) followed by digits, leading zeros allowed (`-1`, `+1` and `01` are numbers; `1 0` and `1.5` are not), within the range of a 32-bit signed whole number (−2147483648 to 2147483647). A value written this way but outside that range, in either direction, is too large to read (see FR-10).
  5. F is one of the full words NORTH, SOUTH, EAST, WEST in any case; abbreviations (N, S, E, W), numbers (such as `1`) and any other text are not accepted ([DEC-06][dec-06]).
- **FR-10**<a id="fr-10"></a> ([DEC-06][dec-06], [DEC-08][dec-08], [DEC-09][dec-09], [DEC-20][dec-20], [DEC-22][dec-22], [DEC-28][dec-28]): A line that is not a valid command MUST be ignored with exactly one warning, and processing continues with the next line; typed and redirected input behave the same. The CommandParser MUST return a failure, never throw an exception, and MUST check the format only (never whether a position is on the table). The failure reasons, which are the warning texts without the `Warning: ` prefix, MUST be exactly:

  | Failure | Reason (`<line>` is the line as typed with whitespace removed from both ends; `<COMMAND>` is the command name in upper case) |
  |---|---|
  | Unknown command word (including `> MOVE`, `EXITNOW`, `PLACE0,0,NORTH`, `PLACEHOLDER`, `PLACE,1,2`) | `'<line>' ignored - unknown command` |
  | Empty or whitespace-only line | `blank line ignored` |
  | PLACE without exactly three comma-separated values | `'<line>' ignored - expected PLACE X,Y,F` |
  | X is not a whole number | `'<line>' ignored - X must be a whole number` |
  | X is written as a whole number (as in FR-09 step 4) but is outside the 32-bit range, in either direction | `'<line>' ignored - X is too large` |
  | Y is not a whole number | `'<line>' ignored - Y must be a whole number` |
  | Y is written as a whole number (as in FR-09 step 4) but is outside the 32-bit range, in either direction | `'<line>' ignored - Y is too large` |
  | Invalid direction (including abbreviations and an empty F) | `'<line>' ignored - F must be NORTH, SOUTH, EAST or WEST` |
  | Values after MOVE, LEFT, RIGHT or REPORT | `'<line>' ignored - <COMMAND> takes no values` |

  When a line has several problems, only the first MUST be reported, checked in this order: command word, number of values, X, Y, F. A leading `>` is not part of a command: `> MOVE` is an unknown command ([DEC-09][dec-09]; assumption A1).
- **FR-10a**<a id="fr-10a"></a> ([DEC-08][dec-08], [DEC-10][dec-10], [DEC-26][dec-26], [DEC-30][dec-30]; page 6 "Parse result"): For each line, `CommandParser.Parse(line)` MUST return exactly one parse result (`ParseResult`): a command object (`ParseResult.CommandObject`, holding a new `PlaceCommand` with X, Y and F, or a new `MoveCommand`, `LeftCommand`, `RightCommand` or `ReportCommand`), EXIT (`ParseResult.Exit`), or a failure with its reason (`ParseResult.Failure`). EXIT MUST be returned when the command word is EXIT in any case, ignoring anything after it on the line (`EXIT now` is EXIT); `EXITNOW` and `EXITING` are unknown commands. The session behaviour of EXIT belongs to [FR-15][fr-15] (spec 002); RobotOperator never receives EXIT or a failure.

### Non-Functional Requirement (owned)

- **NFR-07**<a id="nfr-07"></a> ([DEC-34][dec-34], [DEC-12][dec-12], [DEC-13][dec-13], [DEC-14][dec-14], [DEC-26][dec-26], [DEC-27][dec-27], [DEC-28][dec-28], [DEC-38][dec-38]): The code MUST be clean and follow the SOLID principles: each class has one job, new behaviour is added with new classes where practical, parts depend on small interfaces, and classes are small and well named. The components in this spec MUST have exactly the names, members, responsibilities and limits on page 6:
  - **NFR-07a — Table**<a id="nfr-07a"></a> knows its size and answers "is this position on the table?"; it does not hold the robot, know about commands or read any configuration ([DEC-03][dec-03], [DEC-28][dec-28]).
  - **NFR-07b — Direction**<a id="nfr-07b"></a> is the fixed set NORTH, SOUTH, EAST, WEST, gives the direction after a left or right turn, and its name in upper case ([DEC-07][dec-07], [DEC-17][dec-17]).
  - **NFR-07c — RobotState**<a id="nfr-07c"></a> is an immutable value holding X, Y and the facing Direction; it never changes after it is created and does not check the table ([DEC-12][dec-12]).
  - **NFR-07d — Robot**<a id="nfr-07d"></a> holds its placement (`Placement`), which is either nothing (not on the table) or a RobotState, and has a single method that replaces it (`Replace(state)`); that method accepts only a real RobotState, never nothing, so the robot can never be taken off the table once placed ([DEC-11][dec-11], [DEC-14][dec-14]).
  - **NFR-07e — `IRobotCommand`**<a id="nfr-07e"></a> has exactly the members `Name` (the command name in upper case, used in warnings), `NeedsRobotOnTable` (true for MOVE, LEFT, RIGHT and REPORT; false for PLACE) and `Execute(IRobotOperator robot)`, which carries out the command and returns an outcome ([DEC-27][dec-27]).
  - **NFR-07f — Command classes**<a id="nfr-07f"></a> `PlaceCommand` (holds X, Y and F), `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand` each hold only their own logic: where the robot would go next (PLACE, MOVE, LEFT, RIGHT) or the position to report (REPORT), and their own warning text (`PlaceCommand`: the off-table PLACE warning; `MoveCommand`: the blocked MOVE warning; LEFT, RIGHT and REPORT: none). They reach the robot only through `IRobotOperator`; they do not check the table, change the robot directly, check whether the robot is on the table, parse text or write output ([DEC-13][dec-13], [DEC-14][dec-14], [DEC-27][dec-27]).
  - **NFR-07g — `IRobotOperator`**<a id="nfr-07g"></a> has exactly the members `CurrentState` (the robot's current state, read-only: a RobotState, or nothing when the robot is not on the table) and `TryMoveTo(next)` (asks the Table whether `next` is on the table; if it is, replaces the Robot's state with it and answers yes; if not, leaves the robot unchanged and answers no) ([DEC-13][dec-13], [DEC-14][dec-14]).
  - **NFR-07h — RobotOperator**<a id="nfr-07h"></a> is created with a Table, creates and holds the only Robot and never hands it out, implements `IRobotOperator`, applies the rules shared by all commands once (the robot must be on the table, except for PLACE; a new state is checked with the Table before it replaces the old one), and, through `Run(command)`, returns exactly one outcome (`Outcome`) per command: the position text (`Outcome.PositionText`; REPORT with the robot on the table), a warning text (`Outcome.Warning`), or done with nothing to show (`Outcome.Done`; PLACE, MOVE, LEFT or RIGHT carried out). It does not parse text, write output, add the `Output:` label or the `Warning: ` prefix, handle EXIT, or hold any one command's own logic ([DEC-14][dec-14], [DEC-21][dec-21], [DEC-26][dec-26], [DEC-27][dec-27], [DEC-30][dec-30]).
  - **NFR-07i — CommandParser**<a id="nfr-07i"></a> has no settings or state, turns one line into a parse result with `Parse(line)` ([FR-10a][fr-10a]) and creates the command object for the command it reads; it does not check positions against the table, know where the robot is, write anything or throw for bad input ([DEC-20][dec-20], [DEC-26][dec-26], [DEC-28][dec-28]).
  - **NFR-07j — Room to grow, not built**<a id="nfr-07j"></a>: there is no static or global Robot, the Table does not hold the robot, and the Robot never reads commands or the console; adding a robot command needs one new class implementing `IRobotCommand` and the CommandParser recognising its word, with no change to the existing command classes ([DEC-27][dec-27], [DEC-38][dec-38]).

### Solution skeleton (owned; supports NFR-03 in spec 003 and NFR-06 in spec 002)

- **SK-01**<a id="sk-01"></a> ([DEC-37][dec-37], [DEC-31][dec-31]; pages 5 and 9; supports [NFR-06][nfr-06]): The repository MUST hold `ToyRobot.sln` at the root, grouping three projects: the application in `src/ToyRobot/`, the unit and Runner whole-flow tests in `tests/ToyRobot.Tests/`, and the console end-to-end tests in `tests/ToyRobot.ConsoleEndToEndTests/`. The tests use xUnit. `dotnet build` and `dotnet test` run from the repository root using `ToyRobot.sln`. The solution file is in the classic `.sln` format with exactly that name (newer SDKs create `.slnx` by default, which page 5 does not name).
- **SK-02**<a id="sk-02"></a> ([DEC-37][dec-37]; page 9; supports [NFR-03][nfr-03]): The application MUST target .NET 8 and roll forward to a newer installed major version (`RollForward` = `LatestMajor`).
- **SK-03**<a id="sk-03"></a> ([DEC-39][dec-39]; rule 7 of the Spec Kit brief; supports [NFR-06][nfr-06]): No other library, dependency injection container or logging framework is added. The application has no package references; the test projects reference only xUnit and the packages `dotnet test` needs to run it (the test SDK and the xUnit runner), and not the coverage package that the test template adds.

### Testing (this spec plans unit tests; contributes to NFR-06 in spec 002)

- **UT-01**<a id="ut-01"></a> ([DEC-31][dec-31] level 1; page 9): Unit tests MUST cover Table, Direction, CommandParser, RobotOperator and each command class, and MUST check every acceptance scenario above at component level: the exact parse result (command object with its values, EXIT, or the failure reason) and the exact outcome (position text, warning text, or done with nothing to show), with the robot's state after each command. Expected texts MUST be compared character for character with [FR-02][fr-02], [FR-04][fr-04], [FR-06][fr-06], [FR-07][fr-07] and [FR-10][fr-10].
- **UT-02**<a id="ut-02"></a> ([DEC-31][dec-31]; [NFR-06][nfr-06]): The unit tests MUST cover these [DEC-31][dec-31] cases: the three challenge examples and the hand-traced Brainstorm examples (2,1,NORTH; the blocked moves on Brainstorm pages 3 and 5; 0,3,WEST); every edge and corner; full LEFT and RIGHT cycles; a 2 x 2 table; MOVE, LEFT, RIGHT and REPORT each before PLACE; an invalid first PLACE; an invalid later PLACE; a second valid PLACE; a negative position; several REPORTs in one run; case-insensitivity including `north` and REPORT in upper case after lower-case input; every row of the [DEC-08][dec-08] table; tabs and non-breaking spaces as whitespace; every bad-line warning in [DEC-22][dec-22] with its exact text, including `> MOVE`, `MOVE now`, `PLACEHOLDER`, `PLACE 99999999999,0,NORTH`, `PLACE 0,0,N`, blank lines and the off-table warning with parsed values; `EXIT now` parsed as EXIT and `EXITNOW` as an unknown command; and every item in Edge Cases above.
- **UT-03**<a id="ut-03"></a> ([DEC-31][dec-31]; [NFR-06][nfr-06]): Test names use the challenge wording where possible, for example `Robot_not_on_table_ignores_MOVE`.
- **UT-04**<a id="ut-04"></a> ([DEC-32][dec-32], [DEC-20][dec-20]; [NFR-01][nfr-01]): No unit test input makes any component in this spec throw: the CommandParser returns a failure for every bad line, including very large numbers.

### Key Entities

- **Table**: the tabletop; its width and height (5 x 5 in the application) and whether a position is on it.
- **Direction**: NORTH, SOUTH, EAST or WEST, with a left turn and a right turn.
- **RobotState**: an immutable X, Y and Direction.
- **Robot**: the one robot; its placement is nothing (not on the table) or a RobotState.
- **`IRobotCommand`** and the command classes **`PlaceCommand`**, **`MoveCommand`**, **`LeftCommand`**, **`RightCommand`**, **`ReportCommand`**: one object per valid robot command line.
- **`IRobotOperator`**: what a command may use to reach the robot (`CurrentState`, `TryMoveTo(next)`).
- **RobotOperator**: holds the only Robot and the shared rules; returns an outcome.
- **CommandParser**: turns a line into a parse result.
- **Parse result** (`ParseResult`): a command object (`ParseResult.CommandObject`), EXIT (`ParseResult.Exit`), or a failure with its reason (`ParseResult.Failure`).
- **Outcome** (`Outcome`): the position text (`Outcome.PositionText`), a warning text (`Outcome.Warning`), or done with nothing to show (`Outcome.Done`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**<a id="sc-001"></a> ([FR-01][fr-01] to [FR-07][fr-07], [FR-09][fr-09], [FR-10][fr-10]): 100% of the acceptance scenarios in this spec have a passing unit test that checks the exact parse result or outcome text.
- **SC-002**<a id="sc-002"></a> ([FR-02][fr-02], [FR-04][fr-04], [FR-07][fr-07], [FR-10][fr-10]; [DEC-22][dec-22]): All 12 kinds of warning in [DEC-22][dec-22] are produced with text that matches [DEC-22][dec-22] character for character (the nine bad-line reasons by the CommandParser, the off-table PLACE and blocked MOVE warnings by the command classes, the "not on the table" warning by RobotOperator).
- **SC-003**<a id="sc-003"></a> ([FR-09][fr-09], [FR-10][fr-10]; [DEC-08][dec-08]): All 9 rows of the [DEC-08][dec-08] table give the result the table states, in a unit test each.
- **SC-004**<a id="sc-004"></a> ([FR-04][fr-04], [FR-02][fr-02]; [DEC-13][dec-13]): In 0 tests does a refused PLACE or MOVE change the robot's position or direction.
- **SC-005**<a id="sc-005"></a> ([UT-04][ut-04]; [DEC-32][dec-32]): 0 unit tests observe an exception thrown by any component in this spec for any input line, when the line goes through `CommandParser.Parse` and any command object through `RobotOperator.Run`.
- **SC-006**<a id="sc-006"></a> ([NFR-07][nfr-07]): A review against page 6 finds every component and member named exactly as listed, and no component beyond them; adding a robot command needs one new class and a CommandParser change only.
- **SC-007**<a id="sc-007"></a> ([SK-01][sk-01], [SK-02][sk-02]): From a fresh clone with a .NET SDK 8 or later, one build command and one test command at the repository root build all three projects and run every unit test with all passing.

## Assumptions

- The acceptance scenarios show console text as page 2 does, so that each one reads the same way in all three specs; in this spec they are checked at component level (see "How to read the scenarios"), and spec 002 checks them as console text.
- The solution design's technology choices (C#, .NET 8, `RollForward` = `LatestMajor`, xUnit, the folder layout) are fixed constraints from pages 5 and 9, not choices made in this spec.
- <a id="decided-console-text"></a>**Decided in the run: console text in this spec's scenarios.** The sources say acceptance scenarios are copied from page 2 with console output (stdout) and console messages (stderr) (brief rules 3 and 5), but the components that write console text (Runner, Program) belong to spec 002. Chosen: copy the console text exactly, and state that this spec's unit tests check the parse result and outcome texts that the Runner writes, while spec 002's Runner whole-flow tests check the console text. Other options: state only component outcomes here (breaks rule 5); move Runner whole-flow tests into this spec (they need the Runner, owned by spec 002).
- <a id="decided-exit-in-parser"></a>**Decided in the run: EXIT in the CommandParser.** [FR-15][fr-15] (EXIT ends the session) is owned by spec 002, but the CommandParser that recognises EXIT is a component of this spec (page 6). Chosen: this spec specifies the parse result EXIT ([FR-10a][fr-10a], [DEC-08][dec-08], [DEC-10][dec-10]) and its unit tests; spec 002 specifies what the session does with it. Other options: specify the parser's EXIT rule in spec 002 (would make spec 002 specify a component it does not own); leave it unstated (the parse result on page 6 would be incomplete).
- <a id="decided-brainstorm-inputs"></a>**Decided in the run: inputs of the hand-traced Brainstorm examples.** [DEC-31][dec-31] names them only by result (2,1,NORTH; the blocked moves on Brainstorm pages 3 and 5; 0,3,WEST). Their inputs are taken from the "Hand-traced examples checked" table in `docs/design-analysis/review-trail.md` (the handwritten PDF itself is background only). Each trace is ended with a REPORT line so its end state can be checked; the page 5 trace is the same input as [FR-04][fr-04]'s scenario, so that scenario covers it. Other options: read the inputs from the handwritten PDF (same content, harder to cite); leave the traces out (would miss a [DEC-31][dec-31] case).
- <a id="decided-edge-corner-positions"></a>**Decided in the run: edge and corner positions.** [DEC-31][dec-31] asks for "every edge and corner" without naming positions. Chosen: the middle of each edge (2,4,NORTH; 2,0,SOUTH; 4,2,EAST; 0,2,WEST) and, at each corner, both outward directions. Other options: every position on every edge (more tests, no extra rule covered).
- <a id="decided-e2e-project-skeleton"></a>**Decided in the run: the console end-to-end test project in the skeleton.** The brief gives this spec "the three projects", but console end-to-end tests are planned by spec 002. Chosen: this spec creates `tests/ToyRobot.ConsoleEndToEndTests/` as an xUnit project in the solution that builds, with no tests yet; spec 002 adds its tests. Other options: create the project in spec 002 (contradicts the brief's table).
- <a id="decided-rollforward-tests"></a>**Decided in the run: `RollForward` on the test projects.** [DEC-37][dec-37] requires `RollForward` = `LatestMajor` for the application. Chosen: the application project has it ([SK-02][sk-02]); the two test projects also set it, so that `dotnet test` from the README works on a machine that has only a newer .NET ([DEC-37][dec-37]'s reason, [NFR-09][nfr-09]). This changes no behaviour of the application. Other options: only the application project (tests would need the .NET 8 runtime to be installed).
- <a id="decided-code-names"></a>**Decided: code names of the parse result, the outcome and the unnamed members.** Page 6 describes the parse result, the outcome, the parse and run steps and the Robot's replace method without code names. Chosen: `ParseResult` (`CommandObject`, `Exit`, `Failure`), `Outcome` (`PositionText`, `Warning`, `Done`), `CommandParser.Parse(line)`, `RobotOperator.Run(command)`, `Robot.Placement` and `Robot.Replace(state)`, each taken from page 6's own wording (Clarifications). Spec 002's Runner uses these names. The remaining code names (`Table.IsOnTable(x, y)`, `Table.Width`, `Table.Height`, `RobotState(X, Y, Facing)`, `PlaceCommand(X, Y, F)`, and Direction's `North`, `East`, `South`, `West`, `All`, `Name`, `TurnLeft()`, `TurnRight()`) are listed in `research.md` R4. Other options: leave the names to implementation (specs 001 and 002 could disagree); names not found on page 6, such as `Result` or `Apply` (harder to trace).
- <a id="decided-culture-independent"></a>**Decided: culture-independent case comparison and number writing.** [DEC-08][dec-08] says number reading must be "the same on every machine" but does not say how letter case is compared or how numbers are written back in warnings and REPORT. Chosen: both use fixed, culture-independent rules (Clarifications), each with a unit test under a regional setting where the usual rules differ (Turkish for case, Swedish for the minus sign). Other options: the machine's regional settings (results would differ by machine, against [DEC-08][dec-08]'s reason and [NFR-04][nfr-04]).
- <a id="decided-placeholder-entry-point"></a>**Decided: the application's entry point in the skeleton.** A console application needs an entry point to build, but Program belongs to spec 002. Chosen: this spec's skeleton task creates `src/ToyRobot/Program.cs` as a placeholder entry point that reads nothing, writes nothing and returns exit code 0; spec 002 replaces its body with Program as page 6 describes. Other options: create the application as a library and change it to a console application in spec 002 (changes the project type later); use the template's "Hello, World!" program (writes to console output (stdout), against [DEC-23][dec-23]).
- <a id="decided-replace-refuses-null"></a>**Decided: `Robot.Replace` refuses a missing state at run time.** [DEC-14][dec-14] says the replace method "accepts only a real `RobotState` (never `null`)". Chosen: the parameter is declared as never nothing (checked by the compiler), and a missing state is also refused at run time with an argument error, with a unit test; no input line can reach this, because only RobotOperator calls it with a state built by a command. Other options: compiler check only (no run-time evidence for the rule).
- <a id="decided-table-size"></a>**Decided: the Table does not check its own size.** Page 6 gives the Table only its width, height and the on-table rule. Chosen: no check of the width and height; the rule 0 ≤ X < width and 0 ≤ Y < height applies as written. Program always passes 5 and 5 (spec 002) and tests pass positive sizes. Other options: refuse a size of 0 or less (new behaviour no source asks for).
- <a id="decided-position-text"></a>**Decided: one text form for a position.** [DEC-22][dec-22] requires the blocked MOVE warning to use the same `X,Y,F` form as REPORT, and the off-table PLACE warning uses that form with the parsed values. Chosen: RobotState gives its own text form `X,Y,F` (numbers written with culture-independent rules, F in upper case), used by `ReportCommand`, `MoveCommand` and `PlaceCommand` (`research.md` R8). Other options: each command formats the text itself (three copies of one rule); a separate formatter (a component not on page 6).
- <a id="decided-direction-class"></a>**Decided: Direction is a class with four fixed values.** Page 6 gives Direction turning and its upper-case name. Chosen: a class with exactly four instances, `Name`, `TurnLeft()`, `TurnRight()` and `All` (`research.md` R7); the one-step offsets stay in `MoveCommand`, because Direction does not know positions (page 6). Other options: an enumeration plus a helper type (a type not on page 6).
- <a id="decided-command-without-robot"></a>**Decided: what a command does if run directly with no placed robot.** Page 6 says command classes do not check whether the robot is on the table; RobotOperator does it first ([FR-07][fr-07]). Chosen: `MoveCommand`, `LeftCommand`, `RightCommand` and `ReportCommand`, if run directly with no current state, raise an invalid-operation error instead of a warning; no input line can reach this, and [FR-07][fr-07]'s tests prove RobotOperator never runs them unplaced (`research.md` R12). [SC-005][sc-005] and [UT-04][ut-04] concern input lines, which never cause an exception. Other options: a warning from the command (repeats RobotOperator's rule, against [DEC-27][dec-27]).
- <a id="decided-gitignore"></a>**Decided: `.gitignore` for build output.** The repository has no `.gitignore`. Chosen: the skeleton adds one listing `bin/`, `obj/`, `TestResults/` and `.vs/`, so build output is never committed (`research.md` R3). Other options: none with less.
- <a id="decided-us4-scenario-8"></a>**Decided: US4 scenario 8 states the console text of page 2's [FR-15][fr-15] example for reference, so every scenario states both channels (brief rule 3); spec 002 owns and tests it.**
- <a id="decided-requirements-cite-nfr"></a>**Decided: every SK and UT requirement cites its NFR on its own line.** [SK-01][sk-01] and [SK-03][sk-03] cite [NFR-06][nfr-06], as the heading "Solution skeleton" says the skeleton supports it; [UT-02][ut-02] and [UT-03][ut-03] cite [NFR-06][nfr-06] ([DEC-31][dec-31]'s list and the challenge test names are [NFR-06][nfr-06]'s check, page 3); [UT-04][ut-04] cites [NFR-01][nfr-01], whose unit-level part spec 002 places here. This adds citations only; no behaviour changes. Other options: rely on the section headings (the requirement line itself would not trace to an FR or NFR).
- <a id="decided-tasks-cite-ids"></a>**Decided: every task cites its FR or NFR ID and its DEC IDs.** The 19 tasks that lacked one or both (skeleton, test-helper, Outcome and run-the-tests tasks) now cite them, as brief rule 4 and the constitution require.
- <a id="decided-source-folders"></a>**Decided (developer, 2026-10-05): source folders ([DEC-40][dec-40]).** The application's files were first all directly in `src/ToyRobot/`. Chosen: group them by what they do, with namespaces that follow the folders, as the tests already do: `Domain/` (the table, the robot, its state and direction, and RobotOperator with `IRobotOperator`), `Commands/` (`IRobotCommand`, the five command classes and `Outcome`), `Parsing/` (`ParseResult` and `CommandParser`) and spec 002's `Session/` (Runner); Program stays at the project root. A reader finds a component by its job, and a new command has an obvious place. Other options: keep all files in one folder (16 small files are easy to scan, but the grouping by job is not visible); one folder per component (folders of one file).
- <a id="decided-public-type-handoff"></a>**Decided: the public-type check hand-off to spec 002.** Spec 002 adds `Runner` to the public-type list in `DesignRulesTests` (T054) in its task T020a, just before the task that creates the Runner (T021), and makes Program `internal static class Program` (spec 002 T022), so T054 stays green. This is stated in tasks.md under Dependencies & Execution Order.

[dec-01]: ../../docs/design-analysis/design-decisions.md#dec-01-table-size-and-valid-positions
[dec-02]: ../../docs/design-analysis/design-decisions.md#dec-02-origin-and-directions
[dec-03]: ../../docs/design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-06]: ../../docs/design-analysis/design-decisions.md#dec-06-commands
[dec-07]: ../../docs/design-analysis/design-decisions.md#dec-07-case-insensitive
[dec-08]: ../../docs/design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-09]: ../../docs/design-analysis/design-decisions.md#dec-09-a-leading--is-not-part-of-a-command
[dec-10]: ../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-11]: ../../docs/design-analysis/design-decisions.md#dec-11-the-robot-exists-from-the-start-not-on-the-table
[dec-12]: ../../docs/design-analysis/design-decisions.md#dec-12-robot-state-is-an-immutable-value
[dec-13]: ../../docs/design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../../docs/design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-15]: ../../docs/design-analysis/design-decisions.md#dec-15-place
[dec-16]: ../../docs/design-analysis/design-decisions.md#dec-16-move
[dec-17]: ../../docs/design-analysis/design-decisions.md#dec-17-left-and-right
[dec-18]: ../../docs/design-analysis/design-decisions.md#dec-18-report
[dec-19]: ../../docs/design-analysis/design-decisions.md#dec-19-robot-commands-before-the-robot-is-on-the-table
[dec-20]: ../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-25]: ../../docs/design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-26]: ../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../../docs/design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-28]: ../../docs/design-analysis/design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position
[dec-30]: ../../docs/design-analysis/design-decisions.md#dec-30-exit-is-handled-by-the-runner
[dec-31]: ../../docs/design-analysis/design-decisions.md#dec-31-testing
[dec-32]: ../../docs/design-analysis/design-decisions.md#dec-32-never-crash
[dec-34]: ../../docs/design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-36]: ../../docs/design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
[dec-37]: ../../docs/design-analysis/design-decisions.md#dec-37-deliverables
[dec-38]: ../../docs/design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
[dec-39]: ../../docs/design-analysis/design-decisions.md#dec-39-not-included
[dec-40]: ../../docs/design-analysis/design-decisions.md#dec-40-source-folders
[fr-01]: #fr-01
[fr-02]: #fr-02
[fr-03]: #fr-03
[fr-04]: #fr-04
[fr-05]: #fr-05
[fr-06]: #fr-06
[fr-07]: #fr-07
[fr-08]: ../../docs/solution-design/02-functional-requirements.md#fr-08
[fr-09]: #fr-09
[fr-10]: #fr-10
[fr-10a]: #fr-10a
[fr-11]: ../../docs/solution-design/02-functional-requirements.md#fr-11
[fr-15]: ../../docs/solution-design/02-functional-requirements.md#fr-15
[fr-16]: ../../docs/solution-design/02-functional-requirements.md#fr-16
[fr-17]: ../../docs/solution-design/02-functional-requirements.md#fr-17
[nfr-01]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-01
[nfr-02]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-02
[nfr-03]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-03
[nfr-04]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-04
[nfr-05]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-05
[nfr-06]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-06
[nfr-07]: #nfr-07
[nfr-07d]: #nfr-07d
[nfr-07h]: #nfr-07h
[nfr-07i]: #nfr-07i
[nfr-08]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-08
[nfr-09]: ../../docs/solution-design/03-non-functional-requirements.md#nfr-09
[sc-005]: #sc-005
[sk-01]: #sk-01
[sk-02]: #sk-02
[sk-03]: #sk-03
[ut-02]: #ut-02
[ut-03]: #ut-03
[ut-04]: #ut-04
