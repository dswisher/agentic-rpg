---
type: story
color: blue
priority: high
epic: grounded-llm-player-interaction
done: 2026-10-19
depends_on:
  - local-command-loop
---

# Provide Game Session to Tools

Make the active game `Session` available to the game tools registered for an
LLM request. The tools need access to the current player and world state to
answer questions and validate actions.

## Acceptance Criteria

- [x] Tools invoked for a player request can access that game's active `Session`.
- [x] Tool access is scoped to the correct game session and cannot leak state
  between sessions.
- [x] Tests verify that a tool sees the active session's state.

## Out of Scope

Recording events or providing the LLM with the full world map.
