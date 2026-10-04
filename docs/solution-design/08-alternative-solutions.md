# 8. Alternative solutions

Six other ways the application could have been built, and why each was not chosen.

## The chosen design, in one line

The Runner reads each line; the CommandParser turns it into a command object, one class per command; RobotOperator holds the only Robot and the rules shared by all commands, runs the command through the narrow `IRobotOperator` interface, asks the Table whether the next position is on the table, and replaces the robot's state only if it is ([DEC-26][dec-26], [DEC-27][dec-27], [DEC-13][dec-13], [DEC-14][dec-14]). Pages [5](05-high-level-design.md) and [6](06-components.md) describe it in full.

## How to read the alternative pages

Each alternative has its own page with:
- a short description;
- a small sequence diagram of how it would work, using the same conventions as [page 7](07-logical-flow.md#how-to-read-the-diagrams);
- its pros and cons;
- why it was not chosen, with a link to the decision it relates to;
- the **trade-off** the chosen design accepts instead. Where a decision records one in its "Trade-off" line, it is quoted in quotation marks; otherwise it is explained in plain words, with a link to the closest decision where there is one. This keeps the comparison fair: the chosen design has costs too.

The alternatives fall into two groups.

## Other overall designs

Each of these would replace the whole chosen design with a different way of building the application.

| # | Alternative | In one line | Why not, in short | Decisions |
|---|---|---|---|---|
| 8.1 | [Everything in one class](08-alternative-solutions/01-everything-in-one-class.md) | One class reads, parses, applies the rules and prints | Hard to read, change and test without the console | [DEC-26][dec-26], [DEC-29][dec-29], [DEC-34][dec-34] |
| 8.2 | [Commands as data, with all rules in RobotOperator](08-alternative-solutions/02-commands-as-data.md) | Commands hold only data; one class holds every rule for every command | One class with many jobs that changes with every new command | [DEC-14][dec-14], [DEC-27][dec-27], [DEC-34][dec-34] |
| 8.3 | [Event-based design with a pure reducer](08-alternative-solutions/03-event-based-reducer.md) | Each command becomes an event; a function works out the new state | More parts than a five-command application needs | [DEC-12][dec-12], [DEC-13][dec-13], [DEC-27][dec-27], [DEC-36][dec-36] |

## Other choices within the chosen design

Each of these keeps the chosen components and changes how one part of them works.

| # | Alternative | In one line | Why not, in short | Decisions |
|---|---|---|---|---|
| 8.4 | [Each command is given the Robot and the Table](08-alternative-solutions/04-commands-get-the-robot.md) | The textbook Command pattern: each command checks every rule and changes the Robot itself | Repeats the shared rules in each command and hands the robot out | [DEC-13][dec-13], [DEC-14][dec-14], [DEC-27][dec-27], [DEC-34][dec-34] |
| 8.5 | [A separate validator before the parser](08-alternative-solutions/05-separate-validator.md) | One step checks the line, a second step builds the command object | The line is read twice by two sets of rules that must agree | [DEC-08][dec-08], [DEC-26][dec-26], [DEC-28][dec-28] |
| 8.6 | [The Robot checks the table itself](08-alternative-solutions/06-robot-checks-the-table.md) | The Robot knows the Table and refuses moves off it | Mixes rules into the Robot and makes more robots harder later | [DEC-13][dec-13], [DEC-14][dec-14], [DEC-28][dec-28], [DEC-38][dec-38] |

---

Back to the [solution design overview](README.md).

[dec-08]: ../design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-12]: ../design-analysis/design-decisions.md#dec-12-robot-state-is-an-immutable-value
[dec-13]: ../design-analysis/design-decisions.md#dec-13-state-is-replaced-never-changed-in-place
[dec-14]: ../design-analysis/design-decisions.md#dec-14-only-one-way-to-change-the-robot
[dec-26]: ../design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-28]: ../design-analysis/design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position
[dec-29]: ../design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-34]: ../design-analysis/design-decisions.md#dec-34-readable-code-solid
[dec-36]: ../design-analysis/design-decisions.md#dec-36-development-approach-design-first-then-spec-driven-with-github-spec-kit
[dec-38]: ../design-analysis/design-decisions.md#dec-38-one-robot-now-room-for-more-later
