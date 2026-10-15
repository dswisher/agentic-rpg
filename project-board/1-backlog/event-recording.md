---
type: epic
color: purple
priority: high
depends_on:
  - local-command-loop
---

# Event Recording

Record meaningful local and future LLM-driven actions as structured in-memory events.

## Stories

- **Define Domain Events** — Add event types for movement and item pickup.
- **Record Movement Events** — Emit a structured event after each successful player movement.
- **Record Item Events** — Emit a structured event after each successful item pickup.
- **Expose Recent Events** — Make recent events available to future LLM context builders.
