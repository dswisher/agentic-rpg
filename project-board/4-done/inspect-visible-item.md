---
type: story
color: blue
priority: high
epic: grounded-llm-player-interaction
done: 2026-10-19
depends_on:
  - provide-game-session-to-tools
---

# Inspect Visible Item

Add a read-only `inspect_visible_item` tool that returns the authoritative
description of an item the player can currently see or carry. For example, the
LLM can answer “I examine the ancient scroll closely. What can I tell about
it?” from the item's description instead of inventing its properties.

## Acceptance Criteria

- [x] The tool resolves an item by its full name or an unambiguous complete word
  from its name (for example, “scroll” for “Ancient Scroll”).
- [x] The tool returns the item's authoritative name and description only when
  it is in the current room or the player's inventory.
- [x] The tool reports when there is no accessible match or when the request is
  ambiguous, without returning item details for either result.
- [x] The tool searches only items in the current room and the player's
  inventory; it does not determine whether an inaccessible item exists
  elsewhere in the world.
- [x] Tests cover visible, carried, no-accessible-match, and ambiguous results.

## Sample Player Inputs

- **At Old Bell Tower, with the Ancient Scroll visible:** “I examine the ancient
  scroll closely. What can I tell about it?”
  - Expected: call the inspection tool and describe only its returned facts:
    it is fragile, has faded runes, and emanates a faint magical aura. Do not
    invent a translation.
- **At Briar Gate, where the scroll is not visible:** “What does the ancient
  scroll say?”
  - Expected: the tool returns no accessible match without searching elsewhere
    in the world; explain that the item cannot be inspected here or from the
    player's inventory, without claiming whether it exists elsewhere.
- **With two accessible items matching “key”:** “What does the key look like?”
  - Expected: ask which item the player means; do not choose one arbitrarily.
