# 07 Possible extras (X1–X11)

> Category: things not required by the challenge. For you to decide whether they're worth including or are over-engineering.
> [Back to index](00-index.md) · Current decisions: [08-decisions-log.md](08-decisions-log.md)

Useful test for each extra: "If the reviewer asks why I built this, do I have a one-sentence answer tied to a requirement?"

| # | Extra | Where it appears | Note | Status |
|---|---|---|---|---|
| X1 | Supporting **both** file and stdin input | NFR item 2, HLD | The challenge needs only one | Not included: standard input only (J4) |
| X2 | Verified cross-platform support (Win/Linux/macOS) | NFR item 4 | .NET 8 gives most of this for free. A CI matrix to prove it would be extra. | **In scope** (U3); needs a proof: CI or a README note |
| X3 | Hardened "never crash" handling for huge numbers, empty file, etc. | NFR item 1 | Not stated in the challenge, but reviewers usually expect it. Arguably not an extra. | In scope (NFR item 1) |
| X4 | Configurable table size | D5/J1 discussion | The challenge fixes 5x5. | Not included: Table takes its size in its constructor; no config, no flag |
| X5 | Help, EXIT or usage text | J8, J4 | | EXIT and a usage hint for arguments are in scope |
| X6 | Warnings for ignored commands | J3 | | In scope, on standard error |
| X7 | Logging framework or DI container | Not in docs | Over-engineering risk for a console app this size | Not planned |
| X8 | CI pipeline (GitHub Actions), coverage thresholds, mutation or property-based tests | Not in docs | Shows engineering maturity. Costs time. | Open (a simple CI also serves X2) |
| X9 | Obstacles, multiple robots, extra commands | Not in docs | Common follow-up topics. Not required. | Not planned; describe in the README's "how this would evolve" |
| X10 | Dockerfile | Not in docs | Little value for a .NET console app | Not planned |
| X11 | Including the handwritten docs as part of the "development process" artefacts | n/a | Supports D6, but brings I6 (tidiness) into scope | Open |
| X12 | Log file for REPORT results and warnings | FR item 13, clarifications | Terminal redirection already covers it: `> results.txt`, `2> warnings.txt` | Undecided; most likely to read as over-engineering |
