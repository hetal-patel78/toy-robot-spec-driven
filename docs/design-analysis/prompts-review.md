# Prompts: review

Questions the developer asked while reviewing the delivered work, quoted exactly as typed, with a short summary of the answer. When the repository was published, names, folder paths and links in the quotes were edited. Decisions and instructions are in [prompts-implementation-phase.md](prompts-implementation-phase.md); numbers there are cited as Pnnn.

<a id="r1"></a>**R1.** On the README's "Run it" section.
> the Run it section in the read me say 
>
> on the command prompt 
> run commands.txt
>
> commands.txt is just an e.g. file name correct?

Answer: Yes: `commands.txt` stands for any file of commands, for example `testdata\challenge-example-1.txt`. In PowerShell the script must be started as `.\run.cmd <file>` (the developer then hit this, and the README already shows it).

<a id="r2"></a>**R2.**
> any reason why we don't have an try catch exception handling?

Answer: No component throws for any input, so the never-crash requirement (DEC-32) was met without a catch-all; the assistant offered a top-level handler for unexpected failures (for example a failing console). The developer asked for it (P143).

<a id="r3"></a>**R3.** On section 3 of the run report.
> I am looking  at the report Decided overnight sectio
>
> can u explain #1 properly
>
> 1. End-to-end tests compare console output (stdout) and the exit code; console messages (stderr) only for the usage hint (002 line 413). Page 10 and page 9 say end-to-end compares stdout; the byte-order-mark case is proved end-to-end by stdout matching Challenge Example 1, and the echo by the whole-flow tests.

Answer: Explained the three test levels (unit, whole-flow in memory, end-to-end with the real app), the two output channels, and that the end-to-end tests compare the printed results and exit codes while the other messages are checked exactly by the whole-flow tests.

<a id="r4"></a>**R4.**
> by Adding the safeguard do u mean try catch?

Answer: Yes: the safeguard is the try/catch added for P143 (DEC-32). Adding its text to spec 002 moved the later lines down.

<a id="r5"></a>**R5.**
> we are talking about
>
> 1. End-to-end tests compare console output (stdout) and the exit code; console messages (stderr) only for the usage hint (002 line 413). Page 10 and page 9 say end-to-end compares stdout; the byte-order-mark case is proved end-to-end by stdout matching Challenge Example 1, and the echo by the whole-flow tests.
>
>
> so why u saying
> The reference "(002 line 413)" in the report is out of date

Answer: "(002 line 413)" pointed to where the rule is written in spec 002; after the safeguard text was added the rule moved to lines 402 and 408, so the line number no longer pointed at it. This led to the clickable links (P149) that replace bare line numbers.

<a id="r6"></a>**R6.**
> what is usage hint?

Answer: The one-line message printed when the application is started with an argument. The developer found the word "Usage" unclear (P144 to P146); it is now the how-to-run message, starting `How to run:`.

<a id="r7"></a>**R7.** On the how-to-run message.
> when does app print above?

Answer: Only when the application is started with anything after its name (a file name, `--help`); the run scripts print their own version when given more than one file name.

<a id="r8"></a>**R8.** On the references in section 3 of the run report.
> what does 750, 752 means?
> 001-robot-rules: console text in the scenarios (750)

Answer: Line numbers in that spec's `spec.md` where each decision is written in full. They were replaced by links to anchors on the decisions (P152), so they no longer go stale.

<a id="r9"></a>**R9.**
> so what do u want me to check for
>
> 1. End-to-end tests compare console output (stdout) and the exit code; console messages (stderr) only for the how-to-run message (002 line 413). Page 10 and page 9 say end-to-end compares stdout; the byte-order-mark case is proved end-to-end by stdout matching Challenge Example 1, and the echo by the whole-flow tests. and 
>
> I compares stdout and exit codes of the script runs; stderr only in the script error cases (003 lines 239, 241, 254), as DEC-33 says.
>
> ?

Answer: For each "Decided in the run" item: whether the developer agrees with the choice. Item 1 (what the end-to-end tests compare) and item 2 (what CI compares when it runs the scripts) were explained, with the note that CI also checks the progress lines (CI-07). The developer accepted both (P153).

<a id="r10"></a>**R10.**
> what is below about?
>
> 1. Case comparison and number writing are culture-independent (001 line 757): ordinal ignore-case matching and invariant-culture numbers, tested under tr-TR and sv-SE (NFR-04).

Answer: Command words are matched ignoring case by fixed rules and numbers are read and written with fixed rules, so Turkish settings (dotted capital I) and Swedish settings (a different minus sign) give the same results; tests run under both. The developer accepted it (P155).

<a id="r11"></a>**R11.**
> also explain the remainings? 4, 5,6, and 7

Answer: Item 4: names for what the solution design describes but does not name. Item 5: Direction is a class with four fixed instances, so a number such as `PLACE 1,2,1` is an invalid direction. Item 6: the six test-data pairs. Item 7: the root README is the README. The developer accepted all four (P154).

<a id="r12"></a>**R12.** On the code of `MoveCommand`.
> I am looking at the implementation of the MOVECOMMAND
>
> I see you have used record.  which is reference type. This MoveCommand also uses RobotState and Step function accepting RobotState as the parameter.
>
> so if there are 100 MoveCommand it will create 100 instance of the RobotState include 100 Instances of the MoveCommand + 100 Outocome as well 100 warning incase of all the invalid state.
>
>
> I understand we will not see any memory issue for this toy Robot as all the unsuse dinstance will be garbage collect  soon and we will not see a difference at all but I am just curous
>
>
> Any reason for not chosing
> public readonly record struct RobotState(int X, int Y, Direction Facing) as well as struct for all?
>
> Struct will avoid creation of object on the heap as it is structure it will get created on stack that mean not memory allocation heap and no garbage collection neeeded.

Answer: The allocations are tiny and short-lived. A struct would not help the commands (used through `IRobotCommand`, so boxed) or `Outcome` (a family of types built on inheritance, which structs cannot use). For `RobotState` a `readonly record struct` would work, but a struct always has a default value (0,0 facing nothing) that could reach `ToString()`, and "not placed" is simply `null` with a class; DEC-13 asks only for immutability. Cheaper savings were offered instead: one shared instance of `Outcome.Done` and of each command with no data.

<a id="r13"></a>**R13.**
> u mean create a static class of each of above?

Answer: Not static classes: each class keeps its type and gets one shared instance (`public static readonly MoveCommand Instance = new();` with a private constructor), like `Direction.North`. Safe because these objects have no data.

<a id="r14"></a>**R14.**
> yes sorry that is what i mean I phrased it incorrectly. How many places u will need to make changes and how long it will take?

Answer: About 10 lines in 7 code files, about 60 lines in 7 test files, one sentence in `design-decisions.md` and four spec 001 files; about 45 minutes with CI and review. The developer chose to leave it (P156).

<a id="r15"></a>**R15.** On testing the application by hand.
> I have one question how will I enter the command manually and test it ?

Answer: Run a script with no file name (`run` in cmd, `.\run.cmd` in PowerShell, `./run.sh` in bash); after the build the app shows a `> ` prompt; type one command per line and press Enter; end with `EXIT`. Challenge example 3 was given as a session to try.

<a id="r16"></a>**R16.** On a typed session.
> i used below
>
> Checking for a .NET SDK, version 8 or later...
> Building the application (Release)...
> Running the application: type commands, one per line; end with EXIT or Ctrl+Z then Enter.
> > PLACE 1,2,EAST
> > MOVE
> > MOVE
> > LEFT
> > Report
> Output:
> 3,2,NORTH
> > MOVE
> > Report
> Output:
> 3,3,NORTH
> > LEFT
> > report
> Output:
> 3,3,WEST
> > right
> > report
> Output:
> 3,3,NORTH
> >
>
> Is the output correct?

Answer: Yes, every line: 1,2 EAST, two moves to 3,2, LEFT to NORTH (3,2,NORTH), MOVE to 3,3 (3,3,NORTH, as challenge example 3), LEFT to WEST, RIGHT back to NORTH. Mixed letter case is accepted, and LEFT and RIGHT turn without moving. The last `> ` is the app waiting for the next command.

<a id="r17"></a>**R17.**
> give me bad cases

Answer: A list of lines that are ignored with a warning (before any PLACE, badly formed lines, a PLACE off the table, a MOVE blocked at the edge), each with the exact warning, checked by running them through the built app. It became try-it-by-typing.md, renamed later to [testing-by-hand.md](../testing-by-hand.md) ([P179](prompts-implementation-phase.md#p179); [P163](prompts-implementation-phase.md#p163) to [P165](prompts-implementation-phase.md#p165)).

<a id="r18"></a>**R18.** On the test levels.
> I know whole flow is testing the runner and end to end is testing from Console end to end correct?

Answer: Yes. The whole-flow tests run the whole application in memory, from the Runner inward (some through Program.Run), with in-memory input and output; the end-to-end tests start the real built application as a separate process and check console input and output, exit codes, the test data and the run scripts. The names were then made self-explanatory ([P173](prompts-implementation-phase.md#p173), [P174](prompts-implementation-phase.md#p174)).

## Issues found while testing by hand

| Issue | Found in | Outcome |
|---|---|---|
| Ctrl+Z then Enter did not end a typed session on Windows | [P161](prompts-implementation-phase.md#p161) | Fixed: a line that is only Ctrl+Z ends the input ([DEC-10](design-decisions.md#dec-10-ending-a-session), [P162](prompts-implementation-phase.md#p162)) |
| A number too large to read was reported as "must be a whole number" | [P167](prompts-implementation-phase.md#p167), [P168](prompts-implementation-phase.md#p168) | Its own reason: "X is too large" / "Y is too large" ([DEC-20](design-decisions.md#dec-20-bad-lines), [DEC-22](design-decisions.md#dec-22-warning-wording), [P169](prompts-implementation-phase.md#p169)) |
| A line with two problems reported only the first | [P168](prompts-implementation-phase.md#p168) | Kept by design: one reason per line, the first problem only ([DEC-22](design-decisions.md#dec-22-warning-wording), [P169](prompts-implementation-phase.md#p169)) |
| Page 8.3 (event-based alternative): the CommandParser returned an event | [P171](prompts-implementation-phase.md#p171), [P172](prompts-implementation-phase.md#p172) | The CommandParser returns a command, as in the chosen design; the Runner or a dispatcher turns it into an event for the store and reducer |
| The names "whole-flow" and "end-to-end" for the two test levels were confusing | [P173](prompts-implementation-phase.md#p173) | Renamed: RunnerWholeFlow folder and ToyRobot.ConsoleEndToEndTests project ([P174](prompts-implementation-phase.md#p174)) |
| Page 8.4 (each command is given the Robot and the Table; removed later, P177) read like the chosen design without RobotOperator | [P175](prompts-implementation-phase.md#p175) | It is an alternative that was not built; a "Compared with the chosen design" paragraph now says what it removes and links to the chosen flow |
