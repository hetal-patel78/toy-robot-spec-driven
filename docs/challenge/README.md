# The challenge: Toy Robot Simulator

This is the problem statement this project builds, written in my own words. It is the classic "Toy Robot" coding kata, a well-known public exercise. I used it as a small, well-defined problem for learning spec-driven development with GitHub Spec Kit and Claude Code. Where the design documents quote "the challenge", they quote the usual wording of this kata.

## The problem

A toy robot moves on a square table of 5 x 5 units. Nothing else is on the table. The robot can move freely, but it must never fall off: any command that would make it fall is ignored, and later valid commands still work.

Write a console application that reads these commands:

| Command | What it does |
|---|---|
| `PLACE X,Y,F` | Puts the robot on the table at position X,Y, facing `NORTH`, `SOUTH`, `EAST` or `WEST`. Position 0,0 is the SOUTH WEST corner. |
| `MOVE` | Moves the robot one unit forward in the direction it faces. |
| `LEFT`, `RIGHT` | Turns the robot 90 degrees that way, without moving it. |
| `REPORT` | Shows the robot's X, Y and facing direction. Any format will do; printing it on the console is enough. |

## The rules

- The first valid command must be a `PLACE`. Until a valid `PLACE` has run, every other command is discarded. After that, commands can come in any order, including another `PLACE`.
- A robot that is not on the table may ignore `MOVE`, `LEFT`, `RIGHT` and `REPORT`.
- The robot must not fall off the table. This includes the first placement: a `PLACE` off the table is ignored, and so is any `MOVE` that would take the robot off the edge.
- Commands can come from a file or be typed at the console (standard input); the developer chooses.

## Examples

The `>` marks a line of input.

**Example 1**

```text
> PLACE 0,0,NORTH
> MOVE
> REPORT
```

Output: `0,1,NORTH`

**Example 2**

```text
> PLACE 0,0,NORTH
> LEFT
> REPORT
```

Output: `0,0,WEST`

**Example 3**

```text
> PLACE 1,2,EAST
> MOVE
> MOVE
> LEFT
> MOVE
> REPORT
```

Output: `3,3,NORTH`

## What to deliver

Besides the code, the challenge asks for the following. They suit the aim of learning the process as well as solving the problem:

- the complete source code;
- test data that exercises the application;
- a short README on how to build and run it;
- a description of the development process: the tools used and how, resources consulted, and the notable decisions and trade-offs;
- the prompts and artefacts given to the AI as inputs to code generation.
