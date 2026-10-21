---
type: story
color: orange
priority: high
---

# TUI Improvements, part 1

Small improvements to the TUI to expose more details and clean up the appearance.


## Acceptance Criteria

- [x] Status pane shows tool calls for the most recent LLM exchange, which should update as soon as the tool call is detected
- [x] Status pane shows tokens and elapsed time for most recent LLM exchange
- [x] Status pane shows cumulative tokens
- [x] Status pane shows current room and inventory (first few items)
- [ ] Where labels are used, add a "plain label" helper that sets the hotkey, to avoid `_` disappearing, like the tool calls label
- [ ] Chat pane shows fewer borders
- [ ] Input pane uses a different color rather than a border
- [ ] Input pane accepts CMD+V to paste


## Layout Sketch

The status pane should look something like this:

```
Current Room: briar-gate
Inventory:
   Ancient Scroll

Cumulative Stats
   Input tokens:  n,nnn,nnn
   Output tokens: n,nnn,nnn

Most Recent Exchange
   Input tokens:  n,nnn,nnn
   Output tokens: n,nnn,nnn
   Elapsed time:  mm:ss

Recent Tool Calls
   get_current_scene
   roll_dice
   inspect_visible_item
   inspect_visible_item
   roll_dice
```

Basically, when the player submits an input string, the values (but not the labels) in "Most Recent Exchange" are set to blank.
The "Recent Tool Calls" list is cleared.

If the code enters the local parsing/execution, the "Most Recent Exchange" and "Recent Tool Calls" sections are removed (as they do not apply).

As soon as a tool call is detected, it should be added to the "Recent Tool Calls" list.

When the LLM exchange is complete, the "Most Recent Exchange" values are updated.


