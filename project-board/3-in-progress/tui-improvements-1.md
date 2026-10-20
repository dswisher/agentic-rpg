---
type: story
color: orange
priority: high
---

# TUI Improvements, part 1

Small improvements to the TUI to expose more details and clean up the appearance.


## Acceptance Criteria

- [ ] Status pane shows tool calls for the most recent LLM exchange, which should update as soon as the tool call is detected
- [ ] Status pane shows tokens and elapsed time for most recent LLM exchange
- [ ] Status pane shows cumulative tokens
- [ ] Status pane shows current room and inventory (first few items)
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


## Status Pane Implementation Plan

### Threading model
- `OnMessageSubmitted` and the continuation after `await chat.SendAsync(...)` in `RunTurnAsync`
  both run on the UI thread (MainLoopSyncContext). All "before" and "after" status updates go there.
- Tool calls are spotted inside `ChatSession.SendAsync`, which runs on thread-pool threads
  (`ConfigureAwait(false)`). This is the only place that needs a callback.
- Use `IProgress<T>`: create a `Progress<T>` on the UI thread (in `RunTurnAsync`, before the first
  `await`) and its handler gets posted back to the UI thread automatically.
- Room and inventory only change between turns, so no callback is needed; re-read them after each turn.

### Core changes
1. Add `Agents/ChatProgress.cs`: a sealed record hierarchy, like `GameTurnResult`:
   - `ChatProgress.ToolCallRequested(string Name)`. More cases (e.g. text-chunk streaming) can be added later.
2. `IChatSession.SendAsync` / `ScopedChatSession.SendAsync` / `ChatSession.SendAsync`:
   add `IProgress<ChatProgress>? progress = null` before the `CancellationToken`.
3. In `ChatSession`'s `await foreach`, for each `FunctionCallContent` in `update.Contents`, call
   `progress?.Report(new ChatProgress.ToolCallRequested(call.Name))`.
   (Same loop as `LogAgentUpdate`; maybe pull the tool-call detection out of that method.)
4. CLI `GameApp`: no change needed (the parameter defaults to null).

### TUI changes
5. New `StatusPane : FrameView` class (pull it out of `GameWindow.BuildStatusPane`). It keeps its own
   state and redraws itself. API:
   - `SetLocation(string roomName, IReadOnlyList<string> inventory)`
   - `SetCumulative(long input, long output)`
   - `BeginExchange()`: show the exchange and tool-call sections, blank the values, clear the tool list
   - `AddToolCall(string name)`
   - `SetElapsed(TimeSpan elapsed)`: used by the ticking timer while the turn is running
   - `CompleteExchange(long? input, long? output, TimeSpan elapsed)`
   - `HideExchange()`: remove the exchange and tool-call sections (local commands)
   - Rendering: build the whole text from the state and set it on one multi-line `Label`
     (or a few labels). This avoids juggling visibility and layout when sections come and go.
     Formatting: `N0` right-aligned for tokens, `mm\:ss` for elapsed. Show at most ~5 inventory items,
     then "…and N more". Show the most recent N tool calls if the list gets longer than the pane.
6. `GameWindow` exposes `StatusPane Status { get; }`. `GameWindow` stays a dumb view, so no Core types
   are passed into it.
7. `GameApp`:
   - Fields: `long cumulativeInput, cumulativeOutput` (or a `UsageDetails` combined with `.Add()`).
   - Helper `RefreshLocation()`: looks up the room for `session.Player.CurrentRoomId` and shows
     `Room.Name`, plus the items where `Location is InPlayerInventory`, ordered by name. Call it after
     the initial look, after each local command (`DisplayGameResult`), and after each LLM turn
     (in `finally`).
   - Local path (`OnMessageSubmitted`, not `DelegateToLlm`): `Status.HideExchange()`.
   - LLM path (`RunTurnAsync`):
     - Before the await: `Status.BeginExchange()`, start a `Stopwatch`, create
       `new Progress<ChatProgress>(p => { if (p is ToolCallRequested t) Status.AddToolCall(t.Name); })`.
     - Ticking elapsed time: `var tick = app.AddTimeout(TimeSpan.FromSeconds(1), () => { Status.SetElapsed(sw.Elapsed); return true; });`
       Timeout callbacks run on the UI thread. In `finally`, call `app.RemoveTimeout(tick)` before the
       final `CompleteExchange`, so a late tick can't overwrite the final value.
     - After the await: `CompleteExchange(response.Usage?.InputTokenCount, ..., sw.Elapsed)`,
       add to the cumulative totals, then `SetCumulative(...)`.
     - On cancel or error: `CompleteExchange(null, null, sw.Elapsed)` (no usage is available, so the
       cumulative totals stay the same).

### Tests
- `ChatSessionTests`: a fake client streams a `FunctionCallContent`. Check that `progress` gets
  `ToolCallRequested` with the right name, and that it works with `progress: null`.
- (Optional) Pull the text-building in `StatusPane` into a pure function so it can be unit-tested
  without Terminal.Gui.

### Things to verify during implementation
- `response.Usage` after `ToChatResponse()` should be the **sum** across all the model calls
  `FunctionInvokingChatClient` makes in one turn (one per tool round-trip). Confirm with a turn that
  uses several tools, by comparing against the log.
- `Progress<T>` posts its handler asynchronously. Tool-call reports should land before the
  continuation after `SendAsync` runs (both are queued on the main loop in order). If they ever
  arrive late, add a turn id to `BeginExchange` and ignore reports from older turns.

