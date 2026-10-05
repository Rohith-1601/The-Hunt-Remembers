# The Hunt Remembers

A third-person psychological sci-fi horror prototype for the LILA Games Game Designer Written Test.

## Pitch

You are trapped in an abandoned research facility with a nearly blind creature that has exceptional hearing and gradually learns repeated player habits.

**You do not become stronger. You become harder to predict.**

## Core Loop

Restore Power → Recover Research Sample → Send Emergency Signal → Escape

Moment-to-moment:

Move → Listen → Decide → Make Noise / Stay Quiet → Hunter Reacts → Re-route

## Key Features

- Third-person movement
- Walk / Sprint / Crouch / Crouch-walk / Leap
- Noise-driven Hunter hearing
- Patrol → Hear → Investigate → Search → Return to Patrol
- Vision cone + line-of-sight detection
- Chase + attack
- Adaptive memory of repeated player noise locations
- Throwable distraction
- Main Menu + Guide
- Pause Menu
- Death / Restart
- Win state
- WebGL browser build

## Prototype Scope

- 1 facility
- 1 Hunter
- 4 objectives
- 6 patrol points
- 1 distraction tool

## Controls

- WASD — Move
- Shift — Sprint
- Ctrl — Crouch
- Space — Leap
- Q — Throw distraction
- E — Interact
- Esc — Pause

## Development

Engine: Unity 2022 LTS  
Language: C#  
Render Pipeline: Built-in  
Target: WebGL  
Version Control: Git / GitHub

## Design Principle

The Hunter is not a machine-learning model. Adaptive behavior is implemented as a deterministic gameplay system designed to create the perception that the Hunter is learning the player's habits.

## AI Usage

AI was used for brainstorming, code debugging, design iteration, test-case generation, and documentation refinement. Final gameplay design, tuning, implementation, and scope were selected by the designer.

## Credits

Design / Programming / Prototype: Rohith Dhanesh S
