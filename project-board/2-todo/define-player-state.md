---
priority: high
color: green
epic: hand-crafted-world
depends_on:
  - define-room-model
  - define-item-model
---

# Define Player State

Track the player's current room and inventory in the game session.

## Context

Player state belongs to the authoritative in-memory game session. It should be
available to local command execution and later LLM tools without relying on
chat history. See `docs/RPG.md`, especially **Core architecture**, **State,
events, and history**, and **First milestones**.

## Acceptance Criteria

- [ ] Player state records the current room.
- [ ] Player state records the player's inventory.
- [ ] Player state can be updated when the player changes rooms.
- [ ] Player state can be updated when the player receives an item.
- [ ] State is isolated between newly created game sessions.
- [ ] State remains in memory; persistence is not required yet.

## Out of Scope

Event recording, command parsing, persistence, health, combat, and story
objectives belong to other cards or later work.
