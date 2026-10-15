---
priority: high
color: green
epic: hand-crafted-world
---

# Define Item Model

Add a minimal model for items and their location in the world or player inventory.

## Context

Items are authoritative engine state. The initial model only needs to support a
visible item in a room and an item carried by the player. See `docs/RPG.md`,
especially **Core architecture**, **State, events, and history**, and **First
milestones**.

## Acceptance Criteria

- [ ] An item has a stable identifier.
- [ ] An item has a display name and description.
- [ ] An item has an engine-owned location.
- [ ] An item can be located in a room or in the player's inventory.
- [ ] The model is sufficient to represent the initial takeable item.

## Out of Scope

Containers, item properties, equipment, item interactions, persistence, and
LLM-generated items are not part of this card.
