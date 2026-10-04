# Prompts: design phase

The challenge asks for any prompts or artefacts used as inputs to code generation, where AI wrote any part of the code ([What to deliver](../challenge/README.md#what-to-deliver)).

This file lists the prompts given to the AI assistant (Claude Code, model Claude Opus 5.5) during the design phase, where the developer **made a decision** or **gave an instruction**. The resulting decisions in [design-decisions.md](design-decisions.md) are the input to code generation.

- Prompts are quoted **exactly as typed**, including typos. When the repository was published, names, folder paths and links in them were edited. Where a message also contained questions, only the decision or instruction part is quoted.
- Prompts that only asked for an explanation (for example "what do you mean by…") are not included.
- Where an answer was chosen from options the assistant offered, the chosen option is shown as **[Selected: …]**, and short replies such as "Option 2" are explained in the line above the quote.
- Some prompts reuse the assistant's wording for an option (for example P12): the developer typed or pasted it as their choice.
- Where the developer delegated a choice ("you decide"), the decision is marked as the assistant's pick in the Outcome column.

Other artefacts used as inputs: the handwritten analysis (`docs/design-analysis/handwritten/*.pdf`) and the clarification documents (`docs/design-analysis/clarifications/*.pdf`).

Decision numbers in the Outcome lines are the current numbers in `design-decisions.md`.

Back to [README](README.md).

---

## 1. Starting the review

<a id="p1"></a>**P1. Review the handwritten documents against the challenge**
> I am doing a coding challenge to learn spec-driven development: the Toy Robot Simulator. I will build it as a C# .NET 8 console application.
>
> Before writing any code, I have created these design documents myself (they are handwritten, so please read every page):
> 1.C:\Dev\ToyRobot\SystemDesign\IntiialBrainStorming_ReadingtheDocument_Understanding_Requirements.pdf
>    - my initial brainstorming: worked examples I traced by hand to understand the rules.
>    - Note: on page 2, the bottom example is crossed out on purpose; it was my first wrong attempt, which I corrected on page 3.
> 2. C:\Dev\ToyRobot\SystemDesign\ToyRobotRequirements_FR.pdf - functional requirements in my own words.
> 3. C:\Dev\ToyRobot\SystemDesign\NFR_AND_HLD_ToyRobot.pdf - non-functional requirements and high-level system design.
>
> Original requirement document is located at
> C:\Dev\ToyRobot\OriginalRequirementDocument\Toy Robot Challenge - 2026.pdf
>
> Review each document against the challenge and report your findings only:
> - anything missing (requirements, edge cases, NFRs, design parts)
> - anything incorrect or not matching the challenge
> - anything inconsistent between my documents
> - anything unclear or ambiguous
>
> For each finding give: the document and page, what the issue is, why it matters, and severity (high / medium / low).
>
> Group the findings into three types:
> 1. Definite gaps or errors: things that clearly don't match the challenge.
> 2. Judgement calls: things the challenge leaves open, where more than one answer is reasonable. For these, list the options and their trade-offs, but do not pick one. I will decide.
> 3. Possible extras: things you considered that are NOT required by the challenge. List them so I can decide whether they are worth including or are over-engineering.
>
> For each finding, also state any assumption you made, so I can check it.
>
> Do not suggest fixes, do not write code and do not change my documents. I will decide how to address each finding and then ask you to review or challenge my decision.

Outcome: the review findings (see [review-trail.md](review-trail.md)).

## 2. High-level design

<a id="p2"></a>**P2.**
> Design (HLD) gaps and so I intentionally left out putting all the details flow. detail flow can go into sequence diagrams

Outcome: the HLD stays high-level. The questions behind H2–H5 were answered as decisions (DEC-11, DEC-13, DEC-20, DEC-26 to DEC-28), and the detailed flows are drawn as sequence diagrams (DEC-37).

<a id="p3"></a>**P3.**
> I have intentionally not added the test flow in the HLD diagram as I hand draw the current HLD.  I definitely have plan to do the test and add a new diagram separately for test flow

Outcome: H7 closed; separate test diagram.

<a id="p4"></a>**P4.**
> what do u mean nothing writes to the screen see the arrow from Report to Runner.  Once runner get reponse back from the repot command. Runner will send it to the console and console will print it correct

Outcome: H1 withdrawn.

## 3. Organising the review

<a id="p5"></a>**P5.** On the table of design findings:
> update above with by adding one more column which show the  reasoning I gave it to u

Outcome: the developer's reasoning recorded next to each finding.

<a id="p6"></a>**P6.**
> document is very big split into small document per category so that easy to read and digest

Outcome: findings split into one file per category (now in `archive/`).

<a id="p7"></a>**P7.**
> please update to more meaningful  like toyrobotanalysis

Outcome: the folder name `toyrobotanalysis`.

<a id="p8"></a>**P8.**
> no look at the updated files
> ToyRobot_FunctionalRequirements
> ToyRobot_NonFunctionalRequirements
>
> and update the Definite gaps

Outcome: the gap findings re-checked against the developer's updated FR and NFR.

<a id="p9"></a>**P9.**
> Also look at the all the clarification this should answer most of the questiosn

Outcome: the clarification PDFs were used as answers to the findings.

## 4. Rules and behaviour

<a id="p10"></a>**P10.**
> my very first FR says that the robot must not fall in any case

Outcome: DEC-15 (off-table PLACE).

<a id="p11"></a>**P11.**
> I don't think the original document says to ignore silently. It make sense to inform the user

Outcome: DEC-19, DEC-20 (ignored commands produce a warning).

<a id="p12"></a>**P12.**
> Messages go to standard error, kept separate from the REPORT results

Outcome: DEC-23.

<a id="p13"></a>**P13.**
> it is label but I think even if the ask is not print in above format I still want to go with above format as it is easy to read

Outcome: DEC-21 (`Output:` label).

<a id="p14"></a>**P14.**
> yes let just choose from standard input

Outcome: DEC-04.

<a id="p15"></a>**P15.** Choosing option B ("Table takes its size in its constructor") over a configuration file with a feature flag; the second paragraph is the option text, pasted by the developer:
> we can choose B and still in future constructor can read from future
>
> Table takes its size in its constructor, fixed to 5x5 by the app; no config, no flag -

Outcome: DEC-03.

<a id="p16"></a>**P16.**
> Exit mean should be able to exit at any point

Outcome: DEC-10 (EXIT works at any point, including before PLACE).

<a id="p17"></a>**P17.**
> EXIT in a file: does it stop the rest of the file? (I've assumed yes.) yes correct assumption

Outcome: DEC-10.

<a id="p18"></a>**P18.** On extra spaces in commands. Option 1 was "extra spaces only at the start and end of a line"; option 2 was "also between the parts of a command":
> Option 2

Outcome: DEC-08.

<a id="p19"></a>**P19.** On the channel for echoed commands:
> I am not sure on this one u decide

Outcome: DEC-25 (echo on standard error; assistant's pick).

<a id="p20"></a>**P20.** On the prompt: **[Selected: Only when typing]**, **[Selected: Standard error]**

Outcome: DEC-24.

<a id="p21"></a>**P21.** On a file name passed as an argument: **[Selected: Show usage hint, exit]**

Outcome: DEC-04.

<a id="p22"></a>**P22.** On a pasted `>` in input: **[Selected: No preference]**

Outcome: DEC-09 (treated as a bad command; assistant's pick).

<a id="p23"></a>**P23.** Confirming the meaning of the D2 clarification ("does mention" should read "does not mention"):
> yes I mean does not #1 above in the new point

Outcome: DEC-15 (no default position).

## 5. Robot model and state

<a id="p24"></a>**P24.**
> by replace with new state what i meant was we create clone that will determine the next state and then replace the robot current state if the next state is valid which mean updating existing robot correct?

Outcome: DEC-13, DEC-15 (second PLACE replaces the state of the existing robot).

<a id="p25"></a>**P25.**
> Please review and challenge each decision against the challenge and my other decisions. Do not update any document until I confirm.
>
> 1. First PLACE: check before creating the Robot.
>    Decision: for the first PLACE, the order is: build the proposed state, ask the Table if the position is on the table, and only if it is valid, create the Robot. If the first PLACE is invalid (for example PLACE 9,9,NORTH), no Robot is created, the robot stays "not placed", and a warning goes to standard error.
>    Reason: creating the Robot first and checking afterwards would let an invalid robot exist, even for a moment. This keeps my H3 rule (no Robot before a valid PLACE) true at all times.
>
> 2. LEFT and RIGHT do not need the Table check.
>    Decision: LEFT and RIGHT replace the Robot's state through the same single method as PLACE and MOVE, but they skip the Table check, because turning never changes the position.
>    Reason: turning can never move the robot off the table, so the check would always pass. Skipping it makes it clear that LEFT and RIGHT can never be refused once the robot is placed.
>
> 3. Only one way to change the Robot's state.
>    Decision: the Robot's position and direction cannot be set directly from outside. There is exactly one method that replaces the state, and only the RobotOperator calls it, and only after the Table check (for PLACE and MOVE).
>    Reason: because I update the same Robot object instead of creating a new one, its state can change. If X, Y or direction could be set from anywhere, other code could put the robot off the table and bypass my rule.
>
> For each decision, tell me:
> - whether you agree, and why or why not
> - any case or risk I have missed
> - whether it conflicts with any other decision in my decisions log
>
> After I confirm, update the decisions log with these three points, linked to H3 and J11.

Outcome: DEC-13, DEC-14. Proposal 3 was adopted as written. Proposals 1 and 2 were refined in the follow-ups below: P27 replaced "LEFT and RIGHT skip the Table check" with one path for all four commands, and P29 replaced "no Robot is created" with a robot that exists from the start but is not on the table.

<a id="p26"></a>**P26.**
> Option 2: warn (on standard error, like your other warnings):

Outcome: DEC-19.

<a id="p27"></a>**P27.** Question A was whether LEFT and RIGHT skip the Table check (two paths) or go through it like PLACE and MOVE (one path):
> for A one single path make sense as checking for left and right does not changes anything and avoid us to maintain 2 different paths.

Outcome: DEC-13 (one path for PLACE, MOVE, LEFT and RIGHT).

<a id="p28"></a>**P28.** On how to protect "only RobotOperator changes the robot". Options were C1 (public method, protected by convention), C2 (`internal` method) and C3 (RobotOperator holds the only reference to the robot and never hands it out):
> option c3

Outcome: DEC-14 (RobotOperator holds the only reference to the Robot).

<a id="p29"></a>**P29.** On the robot model: **[Selected: Option C]** (the robot exists from the start, not on the table)

Outcome: DEC-11.

<a id="p30"></a>**P30.**
> I recommend we should create our decision in such a way that if there is requirment to add more than one robot then we can support it? without of changes. what to do u think?

Outcome: discussed; the assistant advised against building for several robots now, which led to P31.

<a id="p31"></a>**P31.**
> yes I agree we don't build it now but we should be able to support the addition of new report with minimal code changes i.e. it should not make us rewrite the whole code again

Followed by **[Selected: Yes, new robot]** (clarifying "report" meant "robot").

Outcome: DEC-38.

## 6. Components and testing

<a id="p32"></a>**P32.**
> I don't believe we need to log exit as an exception as it is not an error it is a choice so Runner should exist it. What do think

Outcome: DEC-30.

<a id="p33"></a>**P33.**
> extra text, exatra space everthing is ignored and still exits

Outcome: DEC-10.

<a id="p34"></a>**P34.**
> our requirement is to be albe to test the whole flow without console app

Outcome: DEC-29, DEC-31.

<a id="p35"></a>**P35.** On a number too large to read (for example `PLACE 99999999999,0,NORTH`). Option A was "treat it as a bad line"; option B was "read it as a larger number and reject it as off the table":
> I agree option A

Outcome: DEC-20 (a number too large to read is a bad line).

## 7. Documentation and delivery

<a id="p36"></a>**P36.**
> remove  it completely
> U5: the NFR photo is hard to read.
> A typo in your clarifications doc: D2 part 2 should say "does not mention".
> Three low HLD points: H1a (output end not drawn), H1b (testing the Runner) and H6 (one input box).

Outcome: these points withdrawn from the findings.

<a id="p37"></a>**P37.**
> don't make any decision out of date just update it as if it was the original decision

Outcome: `design-decisions.md` always states each decision as current, without "revised from" notes. The history stays in this log and in `archive/`, which is never edited.

<a id="p38"></a>**P38.**
> my hand written docs are the initial analysis and brain storming using which we came up with all the subsequence question and generated the new files and so these newly generated files are the source of truth

Outcome: `design-decisions.md` is the single source of truth.

<a id="p39"></a>**P39.**
> why we have multiple place for recording decision e.g. u recorded in Jugement call as well but we also have sepearte  08-decisions-log would be good to have one centralized document.
>
> Also if the reviewer or new person reading all these document it will not make sense to them  how can we resolve that?

Followed by:
> if we delete it than all my hard work will be gone. what u recommend?

and **[Selected: Yes, go ahead]** (keep the original notes in `archive/`).

Outcome: this folder's structure (README, design-decisions, review-trail, archive).

<a id="p40"></a>**P40.**
> Include your handwritten analysis documents in the deliverables? Yes

Outcome: DEC-37 (handwritten analysis included as development-process material).

<a id="p41"></a>**P41.**
> in regards to clarification PDFs - yes

Outcome: DEC-37.

<a id="p42"></a>**P42.** On how to prove cross-platform support. Option 1 was "a CI pipeline (GitHub Actions) on all three systems"; option 2 was "a README note of what was tested":
> yes I like the Option 1 and we can then add in the read me that we have tested it  as part of option 1

Outcome: DEC-33 (GitHub Actions on Windows, Linux and macOS).

<a id="p43"></a>**P43.** On a log file, after the assistant's recommendation:
> yes

Outcome: DEC-39 (no log file).

<a id="p44"></a>**P44.**
> Guidelines in the original docs says
>  please also include any prompts
> or artefacts used as inputs to code generation.
>
> I have given u some prompts so add those prompts as well as part of the deliverables.
>
> Not all of them but the one where I have made the decision and where i have instructed u what to do

Outcome: this file.

<a id="p45"></a>**P45.**
> yes but u aske another agent to review it so that it start the review with no prior knowledge. What do u think?

Outcome: an independent review of this folder by a separate AI agent with no prior context, simulating a reviewer or new engineer reading it for the first time.

<a id="p46"></a>**P46.**
> I also realized we did not added anything in the Claude.md. Ideally we should add the instruction in claude.md correct?

Outcome: agreed to add a `CLAUDE.md` with project rules only; its location was settled in P47.

<a id="p47"></a>**P47.** On where the code will live:
> C:\Dev\ToyRobot

Outcome: `CLAUDE.md` at the root of the repository, with the project rules for AI-assisted implementation; listed as a deliverable in DEC-37.

<a id="p48"></a>**P48.** After the first independent review (findings F1–F20, listed in [review-trail.md](review-trail.md)). "DEC-22" was the number of the output-channels decision at the time; it is now DEC-23:
> reword DEC-22 for F1 and and fix all the recommendation

Outcome: DEC-23 reworded so standard output carries only the REPORT output (label and position); the other review findings fixed, including Table and RobotOperator wiring (DEC-03, DEC-26, DEC-29), exact warning wording (DEC-22), echo rules (DEC-25), exit codes and argument handling (DEC-04, DEC-10), whitespace and number rules (DEC-08), end-to-end tests (DEC-31, DEC-33) and the expanded A1 assumption.

<a id="p49"></a>**P49.**
> Claude.md is the very first file we add correct and so it would no idea about the Decision then but u have reference the decision number to it which will make it look like we added the Claude.md later in the phase

Outcome: `CLAUDE.md` refers to `design-decisions.md` and its sections by name instead of specific decision numbers, so it stays valid if decisions are renumbered.

<a id="p50"></a>**P50.** On when to write the README (skeleton at the start of implementation, finalised at the end), and on adding a run script:
> ok add it I also want to add some powershell script that the reviewer can run to start the console application by that way he don't need to go and understand the build and run details. We still add these details and so if he wish he can read it but ideally he don't need to spend time figuring out how to so an instruction to run the powershell or any generic script which runs on any OS that will start the toy robot

Followed by **[Selected: A: run.cmd + run.sh]**.

Outcome: DEC-37 (README plan and run scripts).

<a id="p51"></a>**P51.** On a demo mode for the run scripts, after the assistant recommended none and listing the three challenge example commands in the README instead:
> sure

Outcome: DEC-37 (no demo mode; example commands in the README).

<a id="p52"></a>**P52.** After the assistant explained the eight choices it had made while fixing the review findings (warning wording, exit codes, any argument rejected, number format, blank lines, echoing EXIT, end-to-end tests, and the Runner receiving RobotOperator), and confirmed that each line goes to the CommandParser before RobotOperator:
> ok go ahead

Outcome: the eight choices accepted (DEC-04, DEC-08, DEC-10, DEC-20, DEC-22, DEC-25, DEC-27, DEC-29, DEC-31); a second independent review started.

<a id="p53"></a>**P53.** On the developer's question about where spec-driven development with Spec Kit is stated:
> when and where do we say I want to do spec driven development so use spec kit?

Outcome: DEC-36 and the Spec Kit rules in `CLAUDE.md`.

<a id="p54"></a>**P54.** After the second independent review (findings N1–N13, listed in [review-trail.md](review-trail.md)): **[Selected: Yes, apply all]** for the proposed fixes, and **[Selected: No preference]** on the Spec Kit details (the design analysis is the source of truth, the constitution holds only lasting principles, the developer runs the `/speckit-*` commands), so those details are the assistant's pick.

Outcome: N1–N13 fixed; the decision with a letter suffix renumbered (now DEC-01 to DEC-39); DEC-36 added.

<a id="p55"></a>**P55.**
> so what I am going to do is all the work which we did helps us doing the design analysis and decision. Once the review is finish and all clean we will push the changes to git and merge it. Then I am going to submit another prompt to create a solution design document based on our analysis and details. This SD will have have HLD, logical flow and implementation details and using this solution desing spec will be created

Outcome: DEC-36 (the chain design analysis → solution design document → Spec Kit specification → implementation, with that order of precedence).

<a id="p56"></a>**P56.**
> yes run the third review

Outcome: a third independent review (findings R1–R8, listed in [review-trail.md](review-trail.md)).

<a id="p57"></a>**P57.** After the third independent review: **[Selected: Yes, apply all]** for the proposed fixes.

Outcome: R1–R8 fixed (DEC-04, DEC-10, DEC-20, DEC-26, DEC-33, DEC-37, the README and `CLAUDE.md`).
