# 12. Assumptions

What the design assumes where the challenge is silent, with the default used and the fallback if the assumption proves wrong, and the questions settled while writing this document.

## Assumptions

### A1 How the solution is checked

The challenge does not say how solutions are checked. This affects three things this design has had to choose ([A1][a1]):

| What is not known | Default | If it proves otherwise |
|---|---|---|
| Is the output compared automatically against expected text, or read by a person? Is `Output:` part of the expected output? | REPORT prints `Output:` and then the position, for example `0,1,NORTH` ([DEC-21][dec-21]) | If console output (stdout) is compared automatically, print only `X,Y,F` |
| Is only console output (stdout) compared, or both streams merged? | Console output (stdout) carries only the REPORT result; the prompt, echo and warnings are console messages (stderr) ([DEC-23][dec-23]) | If both streams are merged, also stop the echo and warnings with redirected input |
| Are the example commands fed with the leading `>` shown in the challenge? | `>` is a prompt, so `> MOVE` is a bad line ([DEC-09][dec-09]) | If the examples are fed as `> PLACE …`, accept and remove a leading `> ` |

The design follows the defaults, and the fallback for each case is known (page [9](09-considerations.md#risks)). The format is also described in the README, so a person reading the output knows what to expect ([DEC-37][dec-37]).

### Found while writing this document

None open. Writing this document found no point that design-decisions.md leaves uncovered, contradicts itself on, or that contradicts the challenge. Each such point raised along the way was put to the developer and settled in design-decisions.md, and the pages were updated to match.

## Settled while writing this document

Questions about how to write this document, raised with the outline and answered by the developer:

| # | Question | Answer |
|---|---|---|
| A | The run scripts' messages are not worded in any decision. Should page 3 fix their wording? | No. The run scripts are a non-functional requirement checked by their exit codes and console output (stdout); the message wording is not fixed ([NFR-08][nfr-08], [DEC-37][dec-37]). |
| B | Where do the folder layout and the test framework go, as no decision names them? | The layout on page [5](05-high-level-design.md#folder-layout-and-the-built-application): `src/ToyRobot/`, `tests/ToyRobot.Tests/`, `tests/ToyRobot.ConsoleEndToEndTests/` and `testdata/`; the tests use xUnit. |
| C | The brief says pages 1–4 name no technology. May they name the chosen platform? | Yes: pages 1–4 may name C#, .NET 8, a console application, Windows, Linux and macOS, and CI on all three, but no design details such as component names. |

The full history of these and every other instruction is in the [design-phase prompt log](../design-analysis/prompts-design-phase.md).

---

Back to the [solution design overview](README.md).

[a1]: ../design-analysis/design-decisions.md#9-assumptions

[dec-09]: ../design-analysis/design-decisions.md#dec-09-a-leading--is-not-part-of-a-command
[dec-21]: ../design-analysis/design-decisions.md#dec-21-report-format
[dec-23]: ../design-analysis/design-decisions.md#dec-23-output-channels
[dec-37]: ../design-analysis/design-decisions.md#dec-37-deliverables
[nfr-08]: 03-non-functional-requirements.md#nfr-08
