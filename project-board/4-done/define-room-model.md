---
priority: high
color: green
epic: hand-crafted-world
done: 2026-10-15
---

# Define Room Model

Add a minimal model for rooms, named exits, and room descriptions.

## Context

The room model is part of the authoritative in-memory world state. The engine,
not the LLM, owns room identity, descriptions, and exits. See `docs/RPG.md`,
especially **Core architecture** and **First milestones**.

## Acceptance Criteria

- [x] A room has a stable identifier.
- [x] A room has a player-facing description.
- [x] A room can define named exits that point to other rooms.
- [x] The model does not require persistence or LLM-generated content.
- [x] The model is sufficient to represent the initial three-room world.

## Out of Scope

Item placement, player state, persistence, generated worlds, and room-specific
game rules belong to other cards or later work.
