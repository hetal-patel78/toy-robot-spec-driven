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
