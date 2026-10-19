---
type: story
color: blue
priority: high
epic: grounded-llm-player-interaction
depends_on:
  - provide-game-session-to-tools
---

# Inspect Visible Item

Add a read-only `inspect_visible_item` tool that returns the authoritative
description of an item the player can currently see or carry. For example, the
LLM can answer “I examine the ancient scroll closely. What can I tell about
it?” from the item's description instead of inventing its properties.

## Acceptance Criteria

- [ ] The tool resolves an item by a player-provided name or unambiguous alias.
- [ ] The tool returns the item's authoritative name and description only when
  it is in the current room or the player's inventory.
- [ ] The tool reports missing, inaccessible, or ambiguous items without
  returning hidden item details.
- [ ] Tests cover visible, carried, inaccessible, missing, and ambiguous items.

## Sample Player Inputs

- **At Old Bell Tower, with the Ancient Scroll visible:** “I examine the ancient
  scroll closely. What can I tell about it?”
  - Expected: call the inspection tool and describe only its returned facts:
    it is fragile, has faded runes, and emanates a faint magical aura. Do not
    invent a translation.
- **At Briar Gate, where the scroll is not visible:** “What does the ancient
  scroll say?”
  - Expected: the tool does not return the scroll's description; explain that
    it cannot be inspected from here rather than revealing its details.
- **With two accessible items matching “key”:** “What does the key look like?”
  - Expected: ask which item the player means; do not choose one arbitrarily.
