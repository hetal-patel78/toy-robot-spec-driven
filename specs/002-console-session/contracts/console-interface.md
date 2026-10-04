# Contract: the application's console interface

What the built application (`ToyRobot`) promises to a person, a shell or a test. Every string is copied character for character from design-decisions.md (brief rule 2). Command rules and warning texts for robot commands and bad lines belong to spec 001 ([DEC-22][dec-22]) and are not restated here.

## Invocation

| Form | Meaning | Source |
|---|---|---|
| `ToyRobot` (console input not redirected) | a person types commands | [DEC-04][dec-04], [DEC-24][dec-24] |
| `ToyRobot < commands.txt` (cmd, bash, zsh), or a pipe | commands come from a file or pipe | [DEC-04][dec-04] |
| `ToyRobot <anything>` | any argument: how-to-run message, exit code 1, nothing read | [DEC-04][dec-04] |

## Channels (DEC-23)

| Channel | Carries |
|---|---|
| Console input (stdin) | the commands, one per line ([DEC-05][dec-05]); UTF-8 when redirected, a leading byte-order mark ignored ([DEC-20][dec-20]) |
| Console output (stdout) | only the REPORT result: `Output:` then `X,Y,F`, each on its own line ([DEC-21][dec-21]) |
| Console messages (stderr) | the prompt, the echo, warnings, the how-to-run message and the unexpected-problem error line |

## Exact strings

| Item | Text | Channel | Source |
|---|---|---|---|
| How-to-run message | `How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)` (three spaces before the bracket), one line | stderr | [DEC-04][dec-04] |
| REPORT label | `Output:` | stdout | [DEC-21][dec-21] |
| REPORT position | `X,Y,F`, for example `0,1,NORTH`, upper case | stdout | [DEC-21][dec-21], [DEC-07][dec-07] |
| Prompt (typing only) | `> ` (`>` and a space, no line break), before each read, including the read that finds the end of the input | stderr | [DEC-24][dec-24], [DEC-10][dec-10] |
| Echo (redirected only) | `> ` followed by the line read with whitespace removed from both ends; a blank line is echoed as `> ` | stderr | [DEC-25][dec-25], [DEC-08][dec-08] |
| Warning | `Warning: ` followed by the reason or warning text from spec 001 | stderr | [DEC-22][dec-22] |
| Unexpected-problem error line | `Error: unexpected problem - <exception message>`, where `<exception message>` is the exception's `Message`; exactly one line, nothing after it | stderr | [DEC-23][dec-23], [DEC-32][dec-32] |

## Exit codes (page 9)

| Exit code | When | Source |
|---|---|---|
| 0 | the input ends, or EXIT is read (any case; anything after it on the line is ignored) | [DEC-10][dec-10] |
| 1 | any argument is given (after the how-to-run message) | [DEC-04][dec-04] |
| 2 | an unexpected problem, not caused by any input (after the error line; also when the error line cannot be written) | [DEC-32][dec-32] |
| non-zero, set by the system | Ctrl+C (not handled by the application; not tested) | [DEC-10][dec-10] |

## Ordering

- With redirected input, each line's echo is written before the line is processed, so its warning (if any) follows directly under it ([DEC-25][dec-25]).
- After EXIT nothing more is read, echoed or run ([DEC-10][dec-10]).
- With an argument, console input is never read ([DEC-04][dec-04]).

[dec-04]: ../../../docs/design-analysis/design-decisions.md#dec-04-standard-input-only
[dec-05]: ../../../docs/design-analysis/design-decisions.md#dec-05-one-command-per-line
[dec-07]: ../../../docs/design-analysis/design-decisions.md#dec-07-case-insensitive
[dec-08]: ../../../docs/design-analysis/design-decisions.md#dec-08-how-a-line-is-read
[dec-10]: ../../../docs/design-analysis/design-decisions.md#dec-10-ending-a-session
[dec-20]: ../../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-21]: ../../../docs/design-analysis/design-decisions.md#dec-21-report-format
[dec-22]: ../../../docs/design-analysis/design-decisions.md#dec-22-warning-wording
[dec-23]: ../../../docs/design-analysis/design-decisions.md#dec-23-output-channels
[dec-24]: ../../../docs/design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../../../docs/design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-32]: ../../../docs/design-analysis/design-decisions.md#dec-32-never-crash
