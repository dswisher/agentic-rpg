---
type: epic
color: red
priority: high
depends_on:
  - event-recording
---

# NPC Conversations

Add conversation mode, NPC-specific context, and LLM-backed dialogue in the TUI.

## Stories

- **Define NPC Model** — Add a minimal NPC model with identity, location, personality, and knowledge.
- **Add Generic Test NPC** — Place one hand-crafted generic NPC in the demo world.
- **Parse Talk Command** — Recognize `talk to <npc>` and resolve a nearby NPC locally.
- **Enter Conversation Mode** — Switch the game session into explicit conversation mode after a valid talk command.
- **Build Conversation Window** — Add a TUI subwindow that clearly displays an active NPC conversation.
- **Route Conversation Input** — Send player input to the active NPC conversation instead of normal command parsing.
- **Leave Conversation Mode** — Return to normal command mode when the player enters `leave` or `quit`.
- **Build NPC Context** — Provide the active NPC's identity, knowledge, attitude, and local game state to the LLM.
- **Start NPC Dialogue** — Use the configured LLM to produce dialogue for the active NPC.
- **Record Conversation Events** — Record conversation lifecycle and meaningful consequences as in-memory events.
