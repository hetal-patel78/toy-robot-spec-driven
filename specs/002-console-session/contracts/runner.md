# Contract: Runner

The in-memory contract the Runner whole-flow tests rely on ([DEC-29][dec-29], [NFR-05][nfr-05]). Names exactly as page 6.

## Construction

```text
Runner(TextReader input, TextWriter output, TextWriter error, RobotOperator robotOperator, bool interactive)
```

- `input`: where lines are read from (console input, or a `StringReader` in tests).
- `output`: console output (stdout), or a `StringWriter` in tests. Receives only `Output:` and position lines.
- `error`: console messages (stderr), or a `StringWriter` in tests. Receives the prompt, the echo and warnings.
- `robotOperator`: spec 001's RobotOperator, created with any Table (5 x 5 in the application; any size in tests, [DEC-03][dec-03]).
- `interactive`: true shows the prompt; false echoes each line ([DEC-24][dec-24], [DEC-25][dec-25]).

The Runner creates its own CommandParser ([DEC-26][dec-26]).

## `Run()`

Returns nothing. For each line, in this order (page 6):

1. If `interactive`: write `> ` to `error`.
2. Read one line from `input`. If there is none, or the line is exactly the single character U+001A (Ctrl+Z): return.
3. If it is the first line: remove one U+FEFF at its start, if present.
4. If not `interactive`: write `> ` + the line trimmed with `Trim()`, as a line, to `error`.
5. Parse the line with the CommandParser:
   - failure (`ParseResult.Failure`, `Reason`) → write `Warning: ` + reason, as a line, to `error`;
   - EXIT (`ParseResult.Exit`) → return;
   - command object (`ParseResult.CommandObject`) → pass it to `robotOperator` (`RobotOperator.Run`); for its outcome:
     - position text (`Outcome.PositionText`) → write `Output:` as a line, then the text as a line, to `output`;
     - warning (`Outcome.Warning`) → write `Warning: ` + text, as a line, to `error`;
     - done with nothing to show (`Outcome.Done`) → write nothing.

## Guarantees

- Never reads ahead: it reads the next line only after the previous line has been processed ([DEC-20][dec-20], [FR-08][fr-08]).
- Never uses `Console` ([DEC-26][dec-26], [DEC-29][dec-29]).
- Never passes EXIT or a failure to RobotOperator ([DEC-27][dec-27], [DEC-30][dec-30]).
- Knows no robot rule and no table size (page 6).
- Writes nothing for EXIT and nothing at the end of the input (a line that is only U+001A counts as the end of the input: no echo, no warning); with empty input it writes nothing at all when `interactive` = false, and exactly `> ` when `interactive` = true.

[dec-03]: ../../../docs/design-analysis/design-decisions.md#dec-03-table-size-is-a-constructor-parameter
[dec-20]: ../../../docs/design-analysis/design-decisions.md#dec-20-bad-lines
[dec-24]: ../../../docs/design-analysis/design-decisions.md#dec-24-prompt
[dec-25]: ../../../docs/design-analysis/design-decisions.md#dec-25-echo-with-redirected-input
[dec-26]: ../../../docs/design-analysis/design-decisions.md#dec-26-responsibilities
[dec-27]: ../../../docs/design-analysis/design-decisions.md#dec-27-each-command-is-a-class-robotoperator-holds-the-shared-rules
[dec-29]: ../../../docs/design-analysis/design-decisions.md#dec-29-the-runner-receives-everything-it-uses
[dec-30]: ../../../docs/design-analysis/design-decisions.md#dec-30-exit-is-handled-by-the-runner
[fr-08]: ../spec.md#fr-08
[nfr-05]: ../spec.md#nfr-05
