# Contract: test data (TD-01; DEC-31, DEC-37)

Each pair: `testdata/<name>.txt` (input) and `testdata/<name>.expected.txt` (exact console output (stdout)). `⏎` is not written in the files; every line, including the last, ends with a line break (LF).

| Name | Input file lines | Expected file lines |
|---|---|---|
| `challenge-example-1` | `PLACE 0,0,NORTH`, `MOVE`, `REPORT` | `Output:`, `0,1,NORTH` |
| `challenge-example-2` | `PLACE 0,0,NORTH`, `LEFT`, `REPORT` | `Output:`, `0,0,WEST` |
| `challenge-example-3` | `PLACE 1,2,EAST`, `MOVE`, `MOVE`, `LEFT`, `MOVE`, `REPORT` | `Output:`, `3,3,NORTH` |
| `bom-challenge-example-1` | bytes `EF BB BF`, then `PLACE 0,0,NORTH`, `MOVE`, `REPORT` | `Output:`, `0,1,NORTH` |
| `exit-mid-file` | `PLACE 0,0,NORTH`, `REPORT`, `EXIT`, `MOVE`, `REPORT` | `Output:`, `0,0,NORTH` |
| `empty` | (zero bytes) | (zero bytes) |

Who uses them: spec 002's [E2E-01][e2e-01] (every pair, built application, line-by-line comparison, exit code 0); [CI-01][ci-01] (every pair through each script on each system); [CI-03][ci-03] (`challenge-example-1` through console input with no argument); [CI-04][ci-04] and [CI-05][ci-05] (`challenge-example-1` as the argument); [CI-02][ci-02] (`challenge-example-1` and `challenge-example-2` as its two-argument case); the README (the three challenge examples). [TD-02][td-02] checks that all twelve files exist and that `bom-challenge-example-1.txt` starts with `EF BB BF`.

[ci-01]: ../spec.md#ci-01
[ci-02]: ../spec.md#ci-02
[ci-03]: ../spec.md#ci-03
[ci-04]: ../spec.md#ci-04
[ci-05]: ../spec.md#ci-05
[e2e-01]: ../../002-console-session/spec.md#e2e-01
[td-02]: ../spec.md#td-02
