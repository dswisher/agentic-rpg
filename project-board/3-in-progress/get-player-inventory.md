---
type: story
color: blue
priority: medium
epic: grounded-llm-player-interaction
depends_on:
  - provide-game-session-to-tools
---

# Get Player Inventory

Add a read-only tool that returns the items the player currently carries. This
lets the LLM answer questions about available possessions without guessing.

## Acceptance Criteria

- [ ] The tool returns only items currently in the player's inventory.
- [ ] Returned item information is sufficient for the LLM to reason about what
  the player has available.
- [ ] Items in rooms or otherwise inaccessible are not returned.
- [ ] Tests cover empty inventory and inventory changes after taking or dropping
  an item.

## Sample Player Inputs

- **With an empty inventory:** “What am I carrying right now?”
  - Expected: call the inventory tool and report that the inventory is empty.
- **After taking the Ancient Scroll:** “Do I have anything with a faint
  magical aura?”
  - Expected: call the inventory tool and identify the scroll from its
    description; do not claim the player has any other items.
