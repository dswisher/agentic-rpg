---
type: story
color: blue
priority: medium
epic: grounded-llm-player-interaction
depends_on:
  - provide-game-session-to-tools
---

# Take Visible Item

Add a game-action tool that lets the LLM fulfill an explicit natural-language
pickup request, such as “I'll pocket the scroll.” The tool should apply the
same validation and state changes as the local take command.

## Acceptance Criteria

- [ ] The tool takes one specifically identified item from the current room.
- [ ] It does not take items that are absent, ambiguous, or not in the current
  room.
- [ ] It uses the same authoritative game rules as the local take command.
- [ ] The tool result clearly reports success or rejection so the LLM can
  accurately describe what happened.
- [ ] Tests verify both the result and the resulting item location.

## Out of Scope

Taking multiple items based on subjective criteria, or using items on other
objects.

## Sample Player Inputs

- **At Old Bell Tower, with the Ancient Scroll on the floor:** “I'll pocket the
  scroll.”
  - Expected: call the take tool; on success, report that the scroll was taken
    and verify it is now in the player's inventory.
- **At Briar Gate, where the scroll is not present:** “I'll pocket the scroll.”
  - Expected: the tool rejects the action, the scroll's location is unchanged,
    and the LLM does not claim success.
- **With two accessible items matching “key”:** “I'll take the key.”
  - Expected: ask which key the player means; do not take either item.
