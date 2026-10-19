---
color: blue
priority: high
epic: local-command-loop
depends_on:
  - parser-part-1
---

# Execution, Part 1

Execute the initial set of commands.

## Acceptance Criteria

- [x] Executing "look" shows a description of the current room and any items it contains.
- [x] Executing "move north" moves to the room to the north, if it exists
- [x] Executing "move north" reports an error, if there is no north exit
- [ ] Executing "inventory" shows the items currently in the player's inventory.
- [ ] Executing "take scroll" moves the scroll to the player's inventory, if it exists in the current location.
- [ ] Executing "take scroll" reports an error, if there is no scroll in the current location.

