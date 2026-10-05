# Prompts: implementation phase

Prompts the developer gave in the implementation phase (Spec Kit specification and code), quoted exactly as typed, with their outcome. When the repository was published, names, folder paths and links in the quotes were edited. Numbering continues from [prompts-design-phase.md](prompts-design-phase.md).

<a id="p133"></a>**P133.** The prompt that started the Spec Kit run:
> Read CLAUDE.md, then follow docs/spec-kit/spec-kit-brief.md exactly: write, review, fix, re-review and check the whole Spec Kit specification, then implement and test the application from it, so that by the morning I have a full, working, error-free system. Use a workflow and run the three specs in parallel. I am not available until the morning: do not wait for me. Where the sources do not settle something, including clarifying questions, decide it yourself; where a document blocks a working system, change it as the brief allows; list everything in the morning report. Keep working until everything is finished; after every step where everything passes, commit, push the branch you are on and update the morning report. Push main only after a separate review agent has approved, as the brief says. Never delete anything.

Outcome: the assistant found that its checkout was behind `main` and that the brief it had read was an earlier version, and asked which rules to follow (P134). It then ran the brief's steps, with these Spec Kit commands (each run read that command's `SKILL.md` in `.claude/skills/` and followed it; the inputs are in [docs/spec-kit/specify-inputs.md](../spec-kit/specify-inputs.md)):
- `/speckit-constitution`, input: the DEC-36 principles (clean code and SOLID; three test levels, tests first; never crash; cross-platform; no over-engineering; standard output for REPORT output only). Result: `.specify/memory/constitution.md` v1.0.0.
- `/speckit-specify` three times, one after another, input: the 001, 002 and 003 sections of `specify-inputs.md` (each spec's owned requirements, components, planned tests and solution design pages, with the brief's rules). Result: `specs/001-robot-rules`, `specs/002-console-session`, `specs/003-delivery`.
- For each spec, in parallel in its own worktree: `/speckit-clarify` (questions answered from the sources, or decided under brief rule 10), `/speckit-plan`, `/speckit-tasks`, then `/speckit-analyze` after every review round. Reviews: 001 two rounds, 002 four, 003 four; each ended with no CRITICAL or HIGH issue.
- Cross-check of the three specs together (six rounds) and the checks on the whole specification (`docs/spec-kit/checks/Test-Specification.ps1`, all passing), then `/speckit-analyze` once more on each spec.
- `/speckit-implement` for 001 (58 tasks, 337 unit tests), 002 (51 tasks, whole-flow and end-to-end tests, 447 tests in all) and 003 (37 tasks: test data, `run.cmd`, `run.sh`, CI on Windows, Linux and macOS, `.gitattributes`, the README; 455 tests in all), each followed by a code review, and `main` pushed after a separate review agent approved.
Details, decisions and results: [docs/spec-kit/implementation-run-report.md](../spec-kit/implementation-run-report.md).

<a id="p134"></a>**P134.** Answer to "Which should I follow?" (the pasted prompt or the brief the assistant had read):
> Your worktree is out of date: it is at 3e12578, but main is at 9722151. Bring your branch up to date with main first, then re-read CLAUDE.md and docs/spec-kit/spec-kit-brief.md. The current brief matches my prompt: it allows implementation, deciding open questions, changing blocking documents, pushing branches, and pushing main after a separate review agent approves. Then follow the current brief exactly.

Outcome: the checkout was fast-forwarded to `main` and the current brief followed.

<a id="p135"></a>**P135.** On where the worktrees are made:
> i want the specs and work tree to be part of ToyRobot repo

Outcome: the assistant explained that a worktree is already part of the same repository, and asked where the folders should go (P136).

<a id="p136"></a>**P136.**
> yes move it inside the repo and continue

Outcome: the `specs` worktree moved from `C:\Dev\codefoundry-specs` to `.claude\worktrees\specs` with `git worktree move`; later worktrees made inside `.claude\worktrees\` (listed in `.git/info/exclude`, so never committed).

<a id="p137"></a>**P137.** On the desktop app not letting the session edit files in another worktree, choosing the first of three options:
> yes go with option 1

Outcome: the session's own worktree became the working checkout; each spec's parallel work ran in its own isolated worktree; the `specs` branch was moved forward by git only.

<a id="p138"></a>**P138.**
> i don't think u need to look at the handwritten document and braining storming document as they were created in the verify beginning

Outcome: every agent was told not to read `docs/design-analysis/handwritten/` or `clarifications/`. The hand-traced Brainstorm examples that DEC-31 requires as tests were taken from `review-trail.md`.

<a id="p139"></a>**P139.**
> don't ask for approval work as originally proposed

Outcome: the run continued as the brief describes.

<a id="p140"></a>**P140.**
> Get the whole app ready by tomorrow morning full tested and working

Outcome: the specification, implementation, tests, real runs and CI completed as listed under P133.

<a id="p141"></a>**P141.** After the run, on the run scripts:
> please update the command to include the progress when the run is execute so that user know what is happening

Outcome: spec 003 updated first (a clarification, the scenarios, the run-script contract, a new task T038 and a CI check CI-07 written before the change), then `run.cmd` and `run.sh`. Each script now writes progress lines to console messages (stderr): `Checking for a .NET SDK, version 8 or later...`, `Building the application (Release)...`, then `Running the application with commands from <file>` or, with no argument, `Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.` (`Ctrl+D` in `run.sh`). The build output stays hidden unless the build fails, and console output (stdout) is unchanged (DEC-23, DEC-37). The README's "Run it" section says so.

<a id="p142"></a>**P142.** On the two `run.cmd` weaknesses a review had found (a wildcard argument, and delayed expansion turned on by a user's settings):
> fix it

Outcome: spec 003 updated first (a clarification, the argument-check requirement, the run-script contract, research R-03, task T039, and a CI-02 case written before the change), then `run.cmd`: a file argument containing `*` or `?` is rejected with `The file name must not contain * or ?: "<name>"` on console messages (stderr) and exit code 1, before the application starts; the script turns delayed expansion off itself (`setlocal DisableDelayedExpansion`), so a `!` in a file name stays as typed. `run.sh` needed no change (a quoted `*` is not a regular file, so it gives "File not found" and exit code 1).

<a id="p143"></a>**P143.** After the assistant explained why the application had no try/catch (no component throws for any input, so DEC-32 was met without one) and offered to draft a decision for a top-level handler:
> yes please definitley need try cacth for unhandled exceptions

Outcome: a decision change confirmed by the developer (CLAUDE.md). Recorded in DEC-32 (with DEC-23, DEC-26 and DEC-31 to match), then the solution design pages, then spec 002, then the code with its tests written first: `Program` catches any exception that escapes its work, writes one line to standard error, `Error: unexpected problem - <exception message>`, and exits with code 2 (0 normal end, 1 any argument). Bad input still never reaches it: every bad line keeps its defined warning. Ctrl+C is still not handled (DEC-10). `Program.Main` stays a thin wrapper of the real console and calls an internal `Program.Run`, which the new tests call with a failing reader and failing writers.

<a id="p144"></a>**P144.** After the assistant explained the one-line message the application prints when started with an argument (`Usage: ToyRobot < commands.txt ...`):
> Usage is not self explanatory

Outcome: the assistant offered two changes, rewording the printed message and renaming the term "usage hint" in the documents, and asked which.

<a id="p145"></a>**P145.**
> how to run the app message?

Outcome: the assistant offered A (rename the term to "how-to-run message" only) or B (also make the printed message start `How to run:`), recommending B.

<a id="p146"></a>**P146.**
> go with B

Outcome: a decision change confirmed by the developer (CLAUDE.md). DEC-04 now gives the message as `How to run: ToyRobot < commands.txt   (or run ToyRobot with no arguments and type commands)` and calls it the how-to-run message; the solution design, the three specs and the checks script were updated to match, then the tests (written first), then `Program`, `run.cmd` (`How to run: run.cmd [commands.txt]`), `run.sh` (`How to run: ./run.sh [commands.txt]`) and the README. 460 tests pass.

<a id="p147"></a>**P147.**
> can u create new image from each of those PDF under clarifications and hand written directory?

Outcome: one PNG image per PDF page (14 images), rendered with the Windows PDF renderer, in `docs/design-analysis/clarifications/images/` and `docs/design-analysis/handwritten/images/`, named `<PDF name> - page <n>.png`. The PDFs themselves are unchanged.

<a id="p148"></a>**P148.**
> rotate
> ToyRobot_SD_HLD - page 1 and page 2 to potrait

Outcome: both images turned 90 degrees anticlockwise, so the writing reads upright (each is now 3840 x 2880). The PDF is unchanged.

<a id="p149"></a>**P149.**
> now one thing I noticed is I see your referecing dependency  and the FR and NFR in the report and other document but what if user want to click and see what is those reference about?

Outcome: the assistant proposed making the references clickable: every `DEC-nn`, `FR-nn`, `NFR-nn`, spec-local ID (for example `TD-01`, `CI-02`) and `Pnn` prompt number in the Markdown documents would become a link to its definition, without changing any wording.

<a id="p150"></a>**P150.** On which model should do the link work:
> yes using  sonnet

Outcome: the link work was done by a Sonnet 5.5 agent. It wrote a script (kept outside the repository) that adds reference-style links with the definitions at the end of each file, adds invisible `<a id>` anchors at each requirement on solution design pages 2 and 3, at each ID defined in a spec and at each prompt number in the two prompt logs, and keeps each file's line endings. A checker resolved every link target (file and anchor) and confirmed that the text is unchanged once the link syntax and anchors are removed. 460 tests pass and the spec checks 1 to 5 pass. Left unlinked on purpose: the recorded `/speckit-specify` inputs, the scenario tags such as `**[FR-01; DEC-01]**` that the spec checks read as raw text, and two phrases on page 2 ("as for FR-nn", "(stderr) of FR-nn") that the checks also read as raw text.

<a id="p151"></a>**P151.**
> End-to-end tests compare console output (stdout) and the exit code; console messages (stderr) only for the how-to-run message
>
> above will still not make to a new reader?

Outcome: the assistant proposed a plain-language version of the item and offered to rewrite all decided items.

<a id="p152"></a>**P152.**
> rewrite all of them in plain language delegat it to seperate agent and use the appropriate model

Outcome: the "Decided in the run" section of the run report was rewritten in plain language by a Sonnet 5.5 agent. Each item now has a plain-words title, one or two sentences on what it means in practice and why, and its sources; the terms console output (stdout), console messages (stderr), exit code, end-to-end test, whole-flow test and CI are explained once at the start of the section. Each bare line-number reference became a clickable link to the decision itself, through an invisible `<a id>` anchor added at the start of that bullet in the spec's Assumptions. The CI item was also corrected: CI also checks the run scripts' progress lines in a normal run (CI-07).

After the assistant explained decided items 1 and 2 (what the end-to-end tests and CI compare):

<a id="p153"></a>**P153.**
> accepted

Outcome: decided items 1 and 2 are marked "Confirmed by the developer" in the run report; the other items stay unconfirmed.

<a id="p154"></a>**P154.** After the assistant explained decided items 4 to 7 (code names, Direction as a class, the test-data set, the root README):
> I am happy with #4 as it definitely make sense to return the self explanatory names
>
> #5 Yes If we use enum then we will need to add some extra guardrail to check if the entered value is string or not by doing a length check but it is still not a great option so using a sealed class for Direction is right move
>
> #6 that is one of the reason why I did the detail analysis so that implementation phase can figure out what needs easily
>
> #7 yes that is why I created README at the root when I started on the analysis phase

Outcome: items 4 to 7 marked confirmed by the developer in the report (section 3).

<a id="p155"></a>**P155.** After the assistant explained decided item 3 (letter case and numbers not depending on regional settings):
> item 3 accepted

Outcome: item 3 marked confirmed by the developer in the report (section 3); all seven "most worth checking" items are now confirmed.

<a id="p156"></a>**P156.** After the assistant estimated the work for one shared instance of `Outcome.Done` and of each command with no data ([R12](prompts-review.md#r12) to [R14](prompts-review.md#r14)):
> na leave it as I need to focus on testing and sumbit the test but add the question which i aske u about updating the links , and above  in the implemenation prompt or create  new for review prompt and add it there and also update the read me and push the changes

<a id="p157"></a>**P157.**
> and the decision as well

Outcome: decided by the developer: the code stays as it is. `RobotState` stays a `sealed record` (not a `readonly record struct`), and the commands and `Outcome.Done` are not changed to shared instances; no decision, spec or code changed. The review questions were logged in a new file, [prompts-review.md](prompts-review.md) (R1 to R14, quoted exactly as typed, each with a summary of the answer and, where there was one, the decision it led to), and the README links it.

<a id="p158"></a>**P158.** After the assistant explained the decided item "the Table does not check its own size" (spec 001):
> Yes that is the reason I left it out as the values were hard coded

Outcome: confirmed by the developer. The Table size is fixed at 5 x 5 by Program, so no input can reach a size of 0 or less; a guard in the Table's constructor is the place to add one when the size becomes configurable ([DEC-38](design-decisions.md#dec-38-one-robot-now-room-for-more-later), [DEC-39](design-decisions.md#dec-39-not-included)).

<a id="p159"></a>**P159.** After the assistant explained the decided item "a command run directly with no placed robot throws" (spec 001):
> yes confirmed, move to decision 3

Outcome: confirmed by the developer. The same answer, with the next number, then confirmed "`Robot.Replace` refuses a missing state" and "one text form for a position, `X,Y,F`, written by `RobotState`".

<a id="p160"></a>**P160.** After the assistant explained the decided item "a placeholder entry point until spec 002":
> confirm all at this stage as I want to test the changes from the new repo and then finish

Outcome: confirmed by the developer: the placeholder entry point and every other item in section 3 of the [run report](../spec-kit/implementation-run-report.md#3-decided-in-the-run), as they stand. None of them changes behaviour, so design-decisions.md is unchanged.

<a id="p161"></a>**P161.** While testing the typed session on Windows:
> control + Z and the i press enter on command prompt does not exit the application
> Exit command work in power shell and command prompt

Outcome: on Windows the console gives Ctrl+Z then Enter to the app as a line holding only the character U+001A, not as the end of input, so the Runner treated it as a bad line and kept reading. EXIT was not affected; Ctrl+D on Linux and macOS was not affected.

<a id="p162"></a>**P162.** On the proposed fix (a line that is only Ctrl+Z ends the input; DEC-10, spec 002, test first, then Runner):
> ask other agent to implement the fix and use Sonnet 5.5

Outcome: a decision change confirmed by the developer, so it was made in the order the chain asks: [DEC-10](design-decisions.md#dec-10-ending-a-session) (a line that is exactly the single character U+001A ends the input like the end of a file, typed or redirected, with no warning and exit code 0); then the solution design pages (functional requirement FR-16, the components page, the exit flow page, the glossary); then spec 002 (FR-16, two acceptance scenarios, an edge case, task T058); then the tests, written first and failing (2 of the 462 tests failed; the other 460 passed); then the Runner. All 462 tests pass (437 + 25); spec checks 1 to 5 pass. Done by a Sonnet 5.5 agent.

<a id="p163"></a>**P163.** After the developer ran a typed session by hand and asked for bad cases to try:
> can u update the read to show how to test directly in the command line as also add commands

Outcome: a "Try it by typing" section was drafted in the README (P164 moved it to its own page).

<a id="p164"></a>**P164.** On the draft README section:
> actually add a subpage other wise it will polluste readme unless u are sure it will not

Outcome: the section is its own page, docs/try-it-by-typing.md (renamed later to [docs/testing-by-hand.md](../testing-by-hand.md), [P179](#p179)): how to start a typed session in cmd, PowerShell and bash, a table of the commands, a normal session (challenge example 3), a table of commands that are ignored with the exact warning each prints, and a session blocked at the edge. The README links to it (P165). Every line on the page was checked by running the same commands through the built application; no behaviour changed.

<a id="p165"></a>**P165.** On where the README links to the new page:
> it should be part of run it right?
>
> Also the  Where to look table is missing the prompt-review section i.e. the prompt I supplied when I was reviewing the changes

Outcome: the link is a sentence at the end of the README's "Run it" section, after how to end a typed session, and a row in "Where to look"; there is no separate section. The "Where to look" row for [prompts-review.md](prompts-review.md) was already there under "Questions asked while reviewing the delivered work"; it now reads "Prompts used while reviewing the delivered work (questions and answers)", in line with the rows for the other two prompt logs.

<a id="p166"></a>**P166.** On the README's "Where to look" row for the handwritten analysis:
> Handwritten initial analysis	docs/design-analysis/handwritten/ (PDFs) this to 
> https://github.com/hetal-patel78/toy-robot-spec-driven/tree/main/docs/design-analysis/handwritten/images

Outcome: the row links to the page images first, with the PDFs alongside; after the assistant asked, the developer chose "Yes, both, then push", so the Clarifications row does the same.

<a id="p167"></a>**P167.** While testing by hand:
> when i enter > place 9999999999999999999,0,east
> Warning: 'place 9999999999999999999,0,east' ignored - X must be a whole number
>
> i think it need to be our of range correct?

Outcome: the behaviour matched [DEC-20](design-decisions.md#dec-20-bad-lines) (the developer's choice of option A, [P35](prompts-design-phase.md#p35) in the design-phase log) and [DEC-22](design-decisions.md#dec-22-warning-wording); the assistant offered keeping it, a clearer "X is too large" message (still a bad line), or treating it as off the table.

<a id="p168"></a>**P168.** On the options:
> X is too large and also  when i enter 
> > place 9999999999999999999,111111111111111111111111111,east
> Warning: 'place 9999999999999999999,111111111111111111111111111,east' ignored - X must be a whole number
>
> it only give me message for X

Outcome: chose the clearer message; the assistant explained that only the first problem on a line is reported ([DEC-22](design-decisions.md#dec-22-warning-wording), spec 001) and asked whether to change that.

<a id="p169"></a>**P169.** On reporting several problems on one line: **[Selected: First problem only (Recommended)]**

Outcome: a decision change confirmed by the developer (CLAUDE.md). In order: [DEC-20](design-decisions.md#dec-20-bad-lines) and [DEC-22](design-decisions.md#dec-22-warning-wording) (a number written as a whole number but too large to read, in either direction, is still a bad line and gets `X is too large` or `Y is too large`; anything not written as a whole number keeps `must be a whole number`; only the first problem on a line is reported, as before); then the solution design pages that list the warnings and the parse failures; then spec 001 and the other specs that list the texts (scenarios, edge cases, contract, research, task T059) and the check script; then the tests, written first (22 failed); then `CommandParser`; then the try-it page. 495 tests pass (470 unit and whole-flow, 25 end-to-end). Done by a Sonnet 5.5 agent.

<a id="p170"></a>**P170.** While testing by hand:
> update the review prompt with the issues I am noticing

Outcome: [prompts-review.md](prompts-review.md) has R15 to R17 (how to type commands, whether a typed session's output is correct, and lines to try that are ignored) and a section "Issues found while testing by hand" with each issue, the prompt it was raised in and its outcome.

<a id="p171"></a>**P171.** On solution design page 8.3:
> There is also an issue with the sequence diagram of even based reducer
>
> Runner call command parser and command parser returns event that does not make sense commanparser just returns the command

Outcome: the assistant proposed a diagram in which the reducer produced the event; the developer answered "no u are not getting my point" and was asked what the diagram should show (P172).

<a id="p172"></a>**P172.** Asked what the event-based diagram should show after the CommandParser returns the command:
> it only matter if we were going to implement it but we should still document the correct approach i.e.
> Runner passes a raw string line to the CommandParser.
> CommandParser returns a Command (e.g., MoveCommand, PlaceCommand).
> Runner (or a Dispatcher) turns that Command into an Event (or the Store/Dispatcher handles the translation).
> Store invokes the Reducer with (currentState, event).
> Reducer returns (newState, warning)

Outcome: [page 8.3](../solution-design/08-alternative-solutions/03-event-based-reducer.md) follows these steps: the CommandParser returns a command, the Runner turns it into an event (or a dispatcher or the store does), the store calls the reducer with the current state and the event, and the reducer returns the new state and any warning. The description, diagram and numbered steps changed; the pros, cons and the reason it was not chosen did not. No decision changed: page 8.3 describes a design that was not built.

<a id="p173"></a>**P173.** While reviewing the tests (the first message was interrupted, then the question [R18](prompts-review.md#r18) was asked, then the suggestion):
> I checked the test and the naming is confusing
> ToyRobotEndToEnd and wholeflow can create confusion
>
> I suggest we add runner in the wholeflow and similarly for end to end to make if self explanatory

Outcome: the assistant offered three scopes: folder and project, folder only with new wording, or wording only.

<a id="p174"></a>**P174.** On the scope of the renaming: **[Selected: Folder + project (Recommended)]**

Outcome: a decision change confirmed by the developer (CLAUDE.md). In order: [DEC-31](design-decisions.md#dec-31-testing) (the two levels are named "Runner whole-flow tests" and "console end-to-end tests" and defined once, with their folder and project); solution design pages 3, 5, 9, 10, 11 and 12 (folder layout, test levels, glossary); the Spec Kit files in `specs/` (paths, project names, namespaces and level names, and the new task T059 in spec 002); the code (`git mv` of `tests/ToyRobot.Tests/WholeFlow/` to `tests/ToyRobot.Tests/RunnerWholeFlow/` and of `tests/ToyRobot.EndToEndTests/` to `tests/ToyRobot.ConsoleEndToEndTests/`, with the project file, namespaces, `ToyRobot.sln` and the CI workflow); the specification check script; the README (the table of test levels); the run report. Totals after the change: 470 tests in `ToyRobot.Tests` and 25 in `ToyRobot.ConsoleEndToEndTests`, all passing, in Debug and Release. Done by a Sonnet 5.5 agent.

<a id="p175"></a>**P175.** On solution design page 8.4:
> I think this is incorrect
> 04-commands-get-the-robot.md under 08-alternative-solutions
>
> this is the chose design but it is missing robotoperator

Outcome (page 8.4 removed later, [P177](#p177)): page 8.4 described an alternative that was not built; leaving out RobotOperator and `IRobotOperator` is the difference it shows. So that it cannot be read as the chosen design, the developer chose to add one paragraph, "Compared with the chosen design", that says what is removed and links to the chosen flow on pages 7.1 and 7.2. No decision or code changed.

<a id="p176"></a>**P176.** On solution design page 8:
> I suggest we remove
> https://github.com/hetal-patel78/toy-robot-spec-driven/blob/main/docs/solution-design/08-alternative-solutions/06-robot-checks-the-table.md
>
> as to many alternative can lead to confusions

Outcome: alternative 8.6 (the Robot checks the table itself) removed, so page 8 has five alternatives: three other overall designs and two other choices within the chosen design. Page 8's introduction and table, the overview page and item 8 of the solution-design brief now list five. The rule it illustrated stays in the decisions: the Table checks positions and the Robot only holds its state ([DEC-28](design-decisions.md#dec-28-the-parser-checks-format-the-table-checks-position)). No decision or code changed.

<a id="p177"></a>**P177.** On the alternatives:
> no I just keep it simple
>
> having 04 will add confusions  as well I believe so ho about it we just keep 01, 02 and 03 and make 05 ->04

Outcome: alternative 8.4 (each command is given the Robot and the Table) removed, and the separate-validator page renamed from 8.5 to 8.4 (`04-separate-validator.md`). Page 8 now has four alternatives: three other overall designs and one other choice within the chosen design. Page 8, the overview page and item 8 of the solution-design brief list four. Earlier records that name the removed page 8.4 ([P175](#p175), the review log, the report) say it was removed later. No decision or code changed.

<a id="p178"></a>**P178.** On the solution design:
> can u also confirm all the sequence diagram matches with our current implementation

Outcome: the sequence diagrams on pages 7.1 to 7.7 were compared with the code, line by line. Pages 7.1 to 7.6 match. Page 7.7 (start-up) showed Program choosing the reader after the argument check and after creating the parts; `Program.Main` chooses it first, before the argument check, as the page's own note on the safeguard already said. The diagram and its steps now show the reader chosen first (steps renumbered). The alternatives on page 8 describe designs that were not built, so they were not compared. No decision or code changed.

<a id="p179"></a>**P179.** After the assistant suggested keeping the typed-session page in `docs/` rather than `testdata/` (which holds only the input and expected-output pairs the tests read), and renaming it so its purpose is clear:
> ye good ideas to rename

Outcome: `docs/try-it-by-typing.md` renamed to `docs/testing-by-hand.md`, with the heading "Testing by hand". The README links ("Run it" and "Where to look") point to the new name; earlier records that name the old page say it was renamed. The page's content is unchanged. No decision or code changed.

<a id="p180"></a>**P180.** On the test data:
> can u add the a new file in test data that show multiple operation not 1 or not but a complicated check

Outcome: a seventh test-data pair, `testdata/long-session.txt` and `long-session.expected.txt` (spec 003, [TD-01](../../specs/003-delivery/spec.md#td-01); task T040). The 36 lines mix every kind of line: commands before PLACE, a PLACE off the table, the robot walked along two edges with a MOVE blocked at each corner, an unknown command, a PLACE with values missing, a blank line, a second PLACE in lower case, a line starting with spaces, a full turn, a negative PLACE, and four REPORTs. The expected file holds the four reports (console output, stdout), produced by the built application and checked by hand. Test first: the TD-02 test and both CI-01 lists name the new pair. Spec 003 (TD-01, TD-02, SC-002, SC-006, a new decision), its contract, data model, plan, quickstart and the README's test-data table now list seven pairs. No decision in design-decisions.md or application code changed.

<a id="p181"></a>**P181.** On the README section "The three challenge examples":
> add all the test data file in the
> The three challenge examples
> Copy and run; each prints the two lines shown

Outcome: the README section is now "Run the test data": the ready-to-copy cmd, PowerShell and bash commands and the expected console output (stdout) for all seven test-data files, the three challenge examples first. The table names each file; the "Test data" section still says what each file shows. Spec 003 is unchanged: its README requirements ([NFR-09](../../specs/003-delivery/spec.md#nfr-09), [SC-004](../../specs/003-delivery/spec.md#sc-004)) ask for at least the three challenge examples. No decision or code changed.

<a id="p182"></a>**P182.** After the README listed every test-data file:
> yes push it also do the same for testing by hand

Outcome: [docs/testing-by-hand.md](../testing-by-hand.md) has a new section, "Run the test data": the command in each shell, then for each of the seven test-data files what the console shows, line by line (each line echoed, then its warning or report). Every block was produced by running the file through the built application; the reports in each block are exactly the file's expected output. No decision or code changed.
