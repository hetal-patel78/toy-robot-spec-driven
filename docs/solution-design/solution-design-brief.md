# Solution design brief

The developer's instructions for writing the solution design document (step 2 of the development approach, DEC-36). Follow them exactly.

---

I am doing a coding challenge to learn spec-driven development: the Toy Robot Simulator, built as a C# .NET 8 console application.

I have finished my analysis and design. I now want you to turn it into a solution design document.

SOURCES (use only these; do not invent requirements):
1. Source of truth for every rule and design decision:
   docs\design-analysis\design-decisions.md (DEC-01 to DEC-39, and assumption A1)
2. The original challenge from the challenge:
   docs\challenge\README.md
3. Background only (read every page, but do not take rules from them):
   - My handwritten initial analysis, in docs\design-analysis\handwritten\:
     IntiialBrainStorming_ReadingtheDocument_Understanding_Requirements.pdf,
     ToyRobot_FunctionalRequirements.pdf, ToyRobot_NonFunctionalRequirements.pdf, ToyRobot_SD_HLD.pdf
   - My clarifications, in docs\design-analysis\clarifications\ (two PDFs)
   - How each decision was reached: docs\design-analysis\review-trail.md
   - Project rules for the AI assistant: CLAUDE.md
Do NOT use docs\design-analysis\archive\ (superseded working notes).

Precedence: design-decisions.md wins. Where the handwritten documents or clarifications differ from it, follow design-decisions.md; this is not an open question.
List an open question only if design-decisions.md does not cover something, contradicts itself, or contradicts the challenge. Do not choose; ask me.
Do not change design-decisions.md. The solution design adds detail (how), it must not change or contradict any decision (what).

WHERE: Save the document as Markdown files in docs\solution-design\

STRUCTURE (parent and child pages):
- One parent page (README.md) that a reviewer can read in 5 minutes and understand the whole design. For each section it gives a 2-4 line summary and a link to the child page with the details.
- One child page per section. Each child page starts with a one-line purpose and ends with a link back to the parent.
- Use relative links so they work on GitHub.
- Wherever a requirement, component or design point comes from a decision, cite its ID (for example DEC-15) and link to design-decisions.md.

SECTIONS (one child page each):
1. Request summary: what the challenge asked for, in a few sentences.
2. Functional requirements: numbered FR-01, FR-02, ... Each one cites the DEC IDs it comes from and has acceptance criteria in Given / When / Then form with exact example input and exact expected output (standard output and standard error shown separately, with the exact warning text from DEC-22).
3. Non-functional requirements: numbered NFR-01, ... Each one cites its DEC IDs and says how it will be checked.
4. Scope: in scope and out of scope (DEC-38, DEC-39), with a reason for each out-of-scope item.
5. High-level design: a Mermaid architecture diagram and a short explanation.
6. Components: one section per component in DEC-26 (Program, Runner, CommandParser, the command classes with IRobotCommand (PlaceCommand, MoveCommand, LeftCommand, RightCommand, ReportCommand), RobotOperator with IRobotOperator, Robot, RobotState, Direction, Table). For each: its responsibility, what goes in, what comes out, and what it does NOT do. Describe the outcome that RobotOperator returns (REPORT position, warning, or nothing) and the parse result that CommandParser returns (a command object, EXIT, or a failure with its reason) under the component that returns them.
7. Logical flow and sequence diagrams: a Mermaid flowchart of how one line is processed, and Mermaid sequence diagrams for these scenarios: valid PLACE then REPORT; MOVE blocked at the edge; commands before a valid PLACE; a bad line (invalid text); PLACE off the table; EXIT; start-up (Program, argument check, typed versus redirected input).
8. Alternative solutions: I want these covered, in two groups. Other overall designs: (a) everything in one class, (b) commands as data, with all rules in RobotOperator, (c) an event-based design with a pure reducer. Other choices within the chosen design: (d) each command is given the Robot and the Table (the textbook Command pattern, without IRobotOperator), (e) a separate validator before the parser, (f) the Robot checking the table itself. For each: a short description, a Mermaid sequence diagram, pros, cons, and why I did not choose it (link the decision it relates to).
9. Considerations: error handling and never crashing; standard output, standard error and exit codes; testing strategy (the three test levels in DEC-31, with a Mermaid test-flow diagram); cross-platform and CI (DEC-33); run scripts (DEC-37); extensibility (for example adding a new robot command, more than one robot per DEC-38, and how a configurable table size could be added later through the Table constructor; none of these is built); assumptions; risks and known limitations.
10. Traceability: a table linking each FR and NFR to its DEC IDs, the component(s) that implement it and the kind of test that proves it (unit, whole-flow in memory, or end-to-end).
11. Glossary: every term and component name, defined once.
12. Assumptions and open questions: A1 from design-decisions.md (with its default), plus anything unclear or contradictory found under the precedence rule above.

RULES:
- Simple, plain English that anyone can understand. Short sentences. Explain any technical term the first time it is used.
- Use the component names exactly as in design-decisions.md (for example RobotOperator).
- Use "must" for requirements. Avoid vague words like "should", "etc." or "appropriate".
- Keep requirements (WHAT) separate from design (HOW): pages 1-4 describe only what the system must do, with no technology or class names. Pages 5-9 describe how.
- Keep each Mermaid diagram small and readable (about 8 boxes at most). Split big diagrams into smaller ones.
- No code, except very small snippets when they explain a design point.

HOW TO WORK:
Step 1: Read all sources. Then show me only the proposed outline: the list of files and a one-line purpose for each, plus any open questions you found. Stop and wait for my approval.
Step 2: After I approve, write the parent page and stop for my review.
Step 3: Then write the child pages. After each one, stop so I can review it before you continue.
