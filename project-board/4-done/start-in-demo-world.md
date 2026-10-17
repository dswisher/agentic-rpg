---
priority: high
color: green
epic: hand-crafted-world
done: 2026-10-17
depends_on:
  - define-player-state
  - create-demo-world
---

# Start In Demo World

Initialize each new game session in the starting room of the demo world.

## Context

Every new session should begin with a known hand-crafted world and a
predictable player state so the first milestone can be exercised through the
TUI. See `docs/RPG.md`, especially **State, events, and history** and **First
milestones**.

## Acceptance Criteria

- [x] A new game session loads the hand-crafted demo world.
- [x] The player starts in the designated starting room.
- [x] The player's inventory starts empty.
- [x] The initial state is available to the TUI.
- [x] Creating a second session does not reuse mutable state from the first.

## Out of Scope

Initial room rendering, command parsing, persistence, save games, and event
recording belong to other cards or later work.
