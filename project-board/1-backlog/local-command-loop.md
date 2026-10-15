---
type: epic
color: blue
priority: high
depends_on:
  - hand-crafted-world
---

# Local Command Loop

Add a local-first parser and execution pipeline for routine player commands.

## Stories

- **Define Command Result** — Represent successful, invalid, ambiguous, and unrecognized local command results.
- **Parse Look Command** — Recognize the `look` command without calling the LLM.
- **Execute Look Command** — Display the current room description, exits, and visible items.
- **Parse Move Command** — Recognize `move <direction>` and short directional aliases.
- **Execute Move Command** — Move the player through valid exits and report invalid directions locally.
- **Parse Take Command** — Recognize `take <item>` and resolve the item name locally.
- **Execute Take Command** — Move a visible item into the player's inventory when allowed.
- **Parse Inventory Command** — Recognize the `inventory` command locally.
- **Execute Inventory Command** — Display the player's current inventory without calling the LLM.
- **Route Local Commands** — Send recognized commands to the local engine before invoking the chat session.
- **Display Local Results** — Show local command responses in the existing TUI chat pane.
