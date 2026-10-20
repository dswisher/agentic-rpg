---
type: epic
color: purple
priority: medium
depends_on:
  - local-command-loop
---

# Grounded LLM Player Interaction

Give the LLM a small set of game tools so it can answer natural-language player
requests using authoritative current state, without inventing facts or revealing
information the player has not discovered.

This epic uses current world and player state; it does not depend on event
recording. Local commands remain the preferred path for routine actions.

## Stories

- **Provide Game Session to Tools** — Make the active game session available to chat tools.
- **Get Current Scene** — Expose only the player's current room and visible features.
- **Get Player Inventory** — Expose only the items the player carries.
- **Inspect Visible Item** — Return details for an item the player can currently access.
- **Move Through Exit** — Let the LLM move the player through one valid exit based on an explicit natural-language request.
- **Take Visible Item** — Let the LLM pick up one explicitly requested, accessible item.
- **Tune Grounded LLM Instructions** — Guide and evaluate tool use and factual grounding.

## Acceptance Criteria

- [ ] The LLM can answer supported natural-language requests using game tools.
- [ ] Tool results are based on authoritative current game state.
- [ ] Tools do not disclose information about inaccessible items or unexplored rooms.
- [ ] State-changing tools apply the same validation as local game commands.
- [ ] The LLM does not claim an action succeeded unless the tool reports success.
- [ ] This work does not require event recording.
