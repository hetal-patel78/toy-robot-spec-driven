# 04 HLD questions H2–H5 explained

> Category: the questions behind H2–H5, with examples and options. Source doc: HLD.
> [Back to index](00-index.md) · Your answers: [08-decisions-log.md](08-decisions-log.md) (H2, H3, H4, H5, SC1–SC4)

Your chosen options: H2 = B, H3 = C, H4 = B, H5 = A.

Suggested order: **H5 → H3 → H2 → H4**, because the H5 answer usually settles where H2 and H3 live.

One line followed through the design touches all four:
```
"MOVE" → Parser (H4: valid line?) → Command (H5: data or action?)
       → "placed?" check (H3: who / how stored?) → edge check (H2: who asks Table?)
       → Robot updated, or move ignored
```

## H2. Who enforces the table edges?
**Example:** the robot is at 4,2,EAST and the user types `MOVE`. The next square, 5,2, is off the table, so the move is ignored.

**Questions:**
1. Which part *knows* the table size (0..4)?
2. Which part *asks* whether the new position is valid?
3. Which part *decides* to apply or ignore the move?
4. Does PLACE use the same check as MOVE?

| Option | Flow | Trade-off |
|---|---|---|
| A. Robot checks itself | Robot works out 5,2, asks Table, and ignores the move itself | Few parts. But Robot depends on Table, so it can't be tested alone. |
| B. RobotOperator checks | It asks Robot for the next position, asks Table if it is valid, and updates Robot only if yes | Robot stays unaware of the table and the rules sit in one place. RobotOperator does more. |
| C. Each command checks | The MOVE command asks Table and updates Robot only if valid | Self-contained. But the check is repeated in MOVE and PLACE unless shared. |

**The sequence diagram should show:** which lifeline calls Table, and what happens on "no".

## H3. How is "not placed yet" represented?
**Example:** the input is `MOVE`, then `REPORT`, then `PLACE 0,0,NORTH`.

**Questions:**
1. Does a Robot object exist before PLACE?
2. If so, what are its X, Y and F? (Not 0,0,NORTH, because that is a real position.)
3. Who checks "placed?" before MOVE, LEFT, RIGHT and REPORT?
4. After an invalid first PLACE (for example 9,9,NORTH), is the robot still unplaced?

| Option | What "not placed" looks like | Trade-off |
|---|---|---|
| A. No Robot until PLACE | RobotOperator holds null until the first valid PLACE | Simple. Every command must check for null, and missing one check causes a crash. |
| B. IsPlaced flag | The Robot exists with IsPlaced = false | Familiar. But the position can be read while it is meaningless. |
| C. Position is "empty or real" | A position that is either empty or a real X,Y,F | You can't read a position that doesn't exist. Needs slightly more thought in C#. |

**The sequence diagram should show:** MOVE before PLACE, where the check stops it, and then a valid PLACE after which the robot is placed.

## H4. What does the parser do with bad input?
**Examples:** `HELLO`, `PLACE 1,2`, `PLACE A,B,NORTH`, `PLACE 1,2,UP`, `move`, a blank line.

**Questions:**
1. What does the parser return for a bad line: nothing, an "invalid" result, or an error?
2. Who decides what happens next: the parser or the Runner?
3. Does processing continue with the next line? (The challenge implies yes.)
4. Is anything printed? (J3/J6)
5. Is `move` (lower case) valid? (J5)
6. Does the parser check table bounds, or only the format? `PLACE 9,9,NORTH` is well-formed but not allowed.

| Option | What happens with `HELLO` | Trade-off |
|---|---|---|
| A. Returns "nothing" | Runner skips it | Simple. But the reason for the failure is lost, so no warnings later. |
| B. Returns an "invalid command" result | Runner ignores or logs it | Keeps the reason. Adds one more type. |
| C. Throws an error | Runner catches it and continues | Common in C#. But using errors for expected bad input is often seen as poor practice, and an uncaught one would break NFR item 1 ("never crash"). |

**The sequence diagram should show:** a bad line arriving, what the parser returns, and the Runner moving on to the next line.

## H5. How do "Command" and RobotOperator relate?
**Example:** the line `LEFT` arrives.

**Questions:**
1. Is a Command just data ("this is LEFT"), or does it act ("I turn the robot")?
2. Who holds the rule that LEFT turns NORTH into WEST: the Command, RobotOperator or Robot?
3. Does the Runner pass the Command to RobotOperator, or does the Command run itself?
4. If a new command (for example BACK) were added later, which parts would change?

| Option | Flow for LEFT | Adding BACK | Trade-off |
|---|---|---|---|
| A. Commands are data, RobotOperator acts | Parser → LeftCommand (data) → Runner → RobotOperator → Robot turns | Change the parser and RobotOperator | Easy to follow, with the rules in one place. RobotOperator grows with each command. |
| B. Commands act themselves | Parser → LeftCommand → Runner calls `Execute(robot)` | Add one class, plus the parser | Easy to extend (command pattern). Rules are spread across classes, and RobotOperator may become unnecessary. |
| C. Mix | Commands act through RobotOperator, which checks "placed?" and edges first | Add one class, plus the parser | Shared rules (H2, H3) stay in one place. More parts to explain. |
