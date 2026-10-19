---
type: story
color: blue
priority: medium
epic: grounded-llm-player-interaction
done: 2026-10-19
depends_on:
  - provide-game-session-to-tools
---

# Get Current Scene

Add a read-only tool that returns the player's current room and what is
currently visible there. This helps the LLM answer requests such as “What can I
investigate here?” without disclosing unexplored parts of the world.

## Acceptance Criteria

- [x] The tool returns the current room's name and description.
- [x] The tool returns visible item names and available exit directions.
- [x] The tool does not return destination room names, unseen items, or other
  information outside the current scene.
- [x] Tests verify the returned scene changes when the player moves.

## Sample Player Inputs

- **At Briar Gate:** “I don't want to leave yet. What can I investigate here?”
  - Expected: call the scene tool; mention the visible carvings and north exit,
    but do not name or describe rooms beyond the exit.
- **At Old Bell Tower:** “What do I see around me?”
  - Expected: report the current room and the visible Ancient Scroll; do not
    disclose anything in other rooms.
