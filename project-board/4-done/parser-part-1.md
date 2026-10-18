---
color: blue
priority: high
epic: local-command-loop
done: 2026-10-18
---

# Parser, Part 1

Implement the initial version of the local parser, handling a small, but useful set of commands.

## Acceptance Criteria

- [x] The `Parser` class exists that accepts a string input and returns a `ParseResult`
- [x] Parsing "look" returns `Local(LookCommand)`
- [x] Parsing "move north" returns `Local(MoveCommand("north"))`
- [x] Parsing "take loot" returns `Local(TakeCommand("loot"))`
- [x] Parsing "inventory" returns `Local(InventoryCommand)`
- [x] Unit tests exist for the above cases

## Out of Scope

Execution of the parsed local commands will be an upcoming card.
