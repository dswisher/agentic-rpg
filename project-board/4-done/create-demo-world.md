---
priority: high
color: green
epic: hand-crafted-world
depends_on:
  - define-room-model
  - define-item-model
---

# Create Demo World

Create three hand-crafted connected rooms and one takeable item.

## Context

This is the smallest playable world used to exercise the TUI and the first
local commands. The world is hand-crafted and deterministic; the LLM is not
involved. See `docs/RPG.md`, especially **Core architecture** and **First
milestones**.

GPT suggestion for three starting rooms, a small abandoned abbey:

- Briar Gate (briar-gate)
- Silent Cloister (silent-cloister)
- Old Bell Tower (old-bell-tower)


## Acceptance Criteria

- [x] The world contains exactly three rooms.
- [x] The rooms have stable identifiers and useful descriptions.
- [x] The rooms have connected, named exits that support movement.
- [x] The world contains one visible, takeable item.
- [x] The item has a stable identifier, name, and description.
- [x] The world can be created repeatedly with the same results.

## Out of Scope

NPCs, locked exits, puzzles, generated content, persistence, and story state
are not part of this card.
