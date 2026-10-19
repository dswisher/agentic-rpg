---
type: story
color: blue
priority: high
epic: grounded-llm-player-interaction
depends_on:
  - local-command-loop
---

# Provide Game Session to Tools

Make the active game `Session` available to the game tools registered for an
LLM request. The tools need access to the current player and world state to
answer questions and validate actions.

## Acceptance Criteria

- [ ] Tools invoked for a player request can access that game's active `Session`.
- [ ] Tool access is scoped to the correct game session and cannot leak state
  between sessions.
- [ ] Chat sessions without a game session can continue to use non-game tools,
  such as dice rolling.
- [ ] Tests verify that a tool sees the active session's state.

## Out of Scope

Recording events or providing the LLM with the full world map.
