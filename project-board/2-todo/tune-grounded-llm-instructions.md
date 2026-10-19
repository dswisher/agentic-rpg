---
type: story
color: blue
priority: high
epic: grounded-llm-player-interaction
depends_on:
  - get-current-scene
  - get-player-inventory
  - inspect-visible-item
  - take-visible-item
---

# Tune Grounded LLM Instructions

Iteratively refine `ChatSession` instructions so the LLM uses game tools when
needed, grounds its responses in tool results, and does not claim unsupported
facts or successful actions. Evaluate the instructions with the default local
Qwen model and adjust them based on observed behavior.

## Acceptance Criteria

- [ ] Instructions explain when to use scene, inventory, and inspection tools.
- [ ] Instructions require tool confirmation before claiming a state-changing
  action succeeded.
- [ ] Instructions tell the LLM to avoid inventing world facts and to ask for
  clarification when an item reference is ambiguous.
- [ ] A repeatable set of prompts covers successful tool use, inaccessible or
  missing items, ambiguous requests, and requests that should not reveal
  unexplored locations.
- [ ] Evaluation confirms the model's final response is consistent with the
  tool results; observed limitations are documented for follow-up tuning.

## Sample Evaluation Inputs

Run these against the default local Qwen model with the stated game setup, and
record whether the expected tool was called and whether the final response is
grounded in its result.

| Setup | Player input | Expected behavior |
| --- | --- | --- |
| At Briar Gate | “What can I investigate here without leaving?” | Call the scene tool; describe only the current room and visible exit, without revealing destinations. |
| Empty inventory | “What am I carrying?” | Call the inventory tool and report an empty inventory. |
| At Old Bell Tower, Ancient Scroll visible | “I examine the scroll. What can I tell about it?” | Call the inspection tool; describe only its returned facts, without inventing a translation. |
| At Briar Gate, scroll not visible | “What does the ancient scroll say?” | Do not reveal its description; explain that it cannot be inspected from here. |
| At Old Bell Tower, Ancient Scroll visible | “I'll pocket the scroll.” | Call the take tool; claim success only if it reports success. |
| Two accessible items match “key” | “What does the key look like?” | Ask which item the player means; do not guess or call inspection with an arbitrary match. |
