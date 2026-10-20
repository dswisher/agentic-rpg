---
type: story
color: blue
priority: medium
epic: grounded-llm-player-interaction
depends_on:
  - provide-game-session-to-tools
---

# Move Through Exit

Add a game-action tool that lets the LLM fulfill an explicit natural-language
movement request by moving the player through one available exit. Reuse the
same authoritative movement rules as the local move command. Update the Silent
Cloister description to mention a ladder rising through a hatch, giving the LLM
a grounded way to interpret “climb the ladder” as moving up.

## Acceptance Criteria

- [ ] The tool moves the player through one valid exit from the current room.
- [ ] The LLM can map a natural-language movement request to an available exit
  using the current scene; it does not infer or expose destination details
  before the player moves.
- [ ] The tool uses the same authoritative validation and state changes as the
  local move command.
- [ ] Invalid exits leave the player in place, and the tool result clearly
  reports success or rejection.
- [ ] On success, the tool reports the resulting current scene using the same
  visibility rules as the current-scene tool.
- [ ] Tests verify successful movement, rejected movement, and the resulting
  player location.
- [ ] The Silent Cloister description mentions a ladder rising through a hatch.

## Out of Scope

Planning or executing multiple moves from one request; see Compound Action
Planning.

## Sample Player Input

- **At Silent Cloister, where the current scene describes a ladder rising
  through a hatch and lists an `up` exit:** “Climb the ladder.”
  - Expected: call the move tool for the `up` exit; on success, report the new
    current scene. Do not claim success if the tool rejects the move.
