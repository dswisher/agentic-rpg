# TUI integration sketch

This document sketches how to connect `src/AgenticRpg.Tui` to the existing
agent, while keeping Terminal.Gui layout code out of `Program.cs` and keeping
agent orchestration separate from the view.

## Current state

- Steps 1 and 2 of the incremental path below are done.
- `Program.cs` parses `--model`, builds the TUI container, resolves `GameApp`,
  and runs it. Exceptions are printed to the console and return exit code 1.
- `Container.cs` registers Serilog, `AddCoreServices()`, and `GameApp`. Logs go
  to `~/.local/state/agentic-rpg/tui.log`, which is recreated on every run.
- `GameApp` creates one agent session per run, *before* `app.Init()`, so
  configuration errors (missing config file, unknown model) are reported on the
  normal console rather than hidden behind the TUI. It then creates the
  Terminal.Gui application and `GameWindow`, and runs it. Exceptions are logged
  and rethrown.
- `GameWindow` builds the layout and exposes a `MessageSubmitted` event plus
  `AddPlayerMessage` and `AddGmMessage`. It ignores empty input.
- Submitting text still produces a hard-coded GM response; the session is
  created but not yet used.
- `IGameAgent.CreateSession()` creates a session handle. A session's
  `SendAsync(input, cancellationToken)` returns a `ChatResponse`.
- `GameSession` buffers the model's streaming updates and returns a complete
  response. The TUI can show a pending/thinking state, then render the response
  when it is complete; it cannot display tokens incrementally through the
  current interface.
- The current agent is not yet an RPG world engine. It has a dice-rolling tool,
  but not the `Look`, `Move`, and `Take` tools or client-owned world state in
  `docs/RPG.md`.

## Responsibilities

- **`Program.cs`**: entry point; parse command-line options, build the TUI
  container, resolve `GameApp`, and run it.
- **`Container.cs`**: register logging, `AddCoreServices()`, and `GameApp`;
  build the service provider with scope validation enabled.
- **`GameApp.cs`**: own application/session lifetimes and coordinate submitted
  turns, cancellation, errors, quitting, and view updates.
- **`GameWindow.cs`**: build the Terminal.Gui window, panes, controls, and
  layout; manage chat rendering and UI state. It does not know about
  `IGameAgent`, model providers, or configuration.

Split out a dedicated chat view only if its rendering and interaction logic grow
enough to merit it. Keep the window's API small and shaped by what the TUI
actually needs.

## Lifetime and composition

The TUI has its own composition root rather than referencing the CLI
executable. Both register the same Core services with `AddCoreServices()`.
Shared composition (for example, the Serilog setup) can be extracted later if
it is worth it.

Logging must not go to the console: Terminal.Gui owns the terminal, and console
output would corrupt the screen. Use `tail -f` on the log file while playing.

One session lives for one playthrough and owns that run's conversation
history. Disposal unwinds in reverse order of creation: window, then
Terminal.Gui application, then session, then (in `Program`) the service
provider. No turn may still be running when the window and application are
disposed; see "Quitting" below.

## Threading model

Terminal.Gui 2.5 installs its own `SynchronizationContext`
(`MainLoopSyncContext`) when a session begins. Its `Post` queues work onto the
main loop, much like `app.Invoke`. An `await` on the UI thread captures this
context, so the code after the `await` resumes on the UI thread on a later loop
iteration.

The design relies on that:

- **Core uses `ConfigureAwait(false)` on every `await`**, enforced by analyzer
  CA2007 for `src/AgenticRpg.Core` only (see `.editorconfig`). Library code has
  no reason to return to the caller's UI thread, and capturing it causes the
  classic deadlock if a caller ever blocks on a task.
- **The TUI does not use `ConfigureAwait(false)`**, and CA2007 is deliberately
  not enabled for it. `GameApp` wants its single `await` per turn to resume on
  the UI thread, so it can update the window directly, without `app.Invoke` and
  without `Task.Run`.
- **Never block the UI thread on a task** (`.Wait()`, `.Result`,
  `GetAwaiter().GetResult()`). The continuation that would complete the task is
  queued for the UI thread, which is busy waiting. Nothing throws; the program
  just hangs.

## Turn flow

1. The input's Enter handler runs on the UI thread. `GameWindow` trims the
   text, ignores it if empty, clears the input, and raises `MessageSubmitted`.
2. `GameApp` ignores the message if a turn is already active. `GameSession` has
   mutable conversation history and must not receive overlapping sends.
   Otherwise it appends the player's message, sets the window busy, creates a
   new `CancellationTokenSource` for the turn, and starts the turn without
   awaiting it.
3. The turn awaits `session.SendAsync(input, cancellationToken)` and resumes on
   the UI thread.
4. It renders `response.Text`, or a `[cancelled]` or error message, and logs
   token usage. The turn catches every exception, so it never becomes an
   unobserved task error.
5. In `finally`, it clears the busy state, marks the turn inactive and, if a
   quit is pending, asks the application to stop.

Conceptually:

```csharp
private async Task RunTurnAsync(string input, CancellationToken ct)
{
    try
    {
        var response = await session!.SendAsync(input, ct);   // resumes on UI thread
        window!.AddGmMessage(response.Text);
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested)
    {
        window!.AddSystemMessage("[cancelled]");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Turn failed.");
        window!.AddSystemMessage($"[error: {ex.Message}]");
    }
    finally
    {
        window!.SetBusy(false);
        turnActive = false;              // before RequestStop; see "Quitting"
        if (quitPending) app!.RequestStop(window);
    }
}
```

While busy, `GameWindow` ignores Enter but keeps the input field enabled, so it
keeps focus and still receives keys (needed for the cancel key).

## Quitting: deferred quit

`GameApp.Run` never waits for a turn to finish. Instead, the window is not
allowed to stop while a turn is active:

1. `GameApp` handles the window's `IsRunningChanging` event (a cancellable
   event in Terminal.Gui 2.5). When the window is stopping and a turn is active,
   it cancels the stop, sets `quitPending`, shows `[quitting…]`, and cancels the
   turn's token.
2. The turn finishes through the normal `[cancelled]` path. Its `finally` sets
   `turnActive = false` and then calls `app.RequestStop(window)`. The order
   matters: `RequestStop` raises `IsRunningChanging` again, and this time it
   must be allowed through.
3. When `app.Run` returns, no turn is running, so there is nothing to wait for.
   Log a warning if a turn is somehow still active, then dispose the turn's
   `CancellationTokenSource`.

Why not cancel and wait after `app.Run` returns? Once the loop has stopped, the
turn's queued continuations have nowhere useful to run. Terminal.Gui 2.5 falls
back to running them on the thread pool after a session ends, which avoids a
hang but means UI code would run on the wrong thread, possibly while the
window and application are being disposed (`app.Invoke` throws after
disposal). The deferred quit keeps every UI update on the UI thread while the
UI is alive.

If a provider ignores cancellation, the quit waits until the request finishes,
but the UI stays responsive meanwhile.

## Cancellation

`GameWindow` raises a `CancelRequested` event when the cancel key is pressed
while busy, and leaves the key unhandled when idle. `GameApp` cancels the
turn's token, and the turn reports `[cancelled]`. If the cancel key is also the
quit key, this mirrors the CLI's Ctrl+C: cancel while busy, quit while idle.

`GameSession` only records a turn in history when it succeeds, so a cancelled
or failed turn leaves the conversation consistent. Note that tools may already
have run during a cancelled turn; once tools change world state, the state
injected each turn (see `docs/RPG.md`) is what keeps the model correct.

## Rendering considerations

- GM entries currently have a fixed height. Real responses can be longer, so
  size entries to their content (try `Dim.Auto()`, otherwise calculate the
  wrapped line count) before scrolling to the bottom.
- Keep “thinking,” cancelled, quitting, and error states visible to the player;
  restore input in all completion paths.
- The agent instructions request plain text with lines of at most 80
  characters, while the TUI uses a Markdown view. Decide whether to keep plain
  text or intentionally support Markdown, and whether the line-length rule
  still makes sense in a resizable pane.
- Token usage is available on `ChatResponse.Usage`; it can remain a log detail,
  as in the CLI.

## Incremental path

1. **Done.** Move the existing layout and placeholder rendering into
   `GameWindow` without connecting the agent.
2. **Done.** Add the TUI container (file logging, `--model`) and `GameApp`;
   create and dispose one agent session per UI run, before `app.Init()`.
3. Replace the placeholder with the asynchronous turn flow, in small steps:
   1. **Core uses `ConfigureAwait(false)`.** Enable CA2007 for
      `src/AgenticRpg.Core` only and fix what it reports (currently
      `GameSession`'s `await foreach`). No behavior change; commit separately.
   2. **Async turn with a fake delay.** Add `SetBusy` and `AddSystemMessage` to
      `GameWindow`; implement the turn flow with `Task.Delay` and a canned
      reply, plus the deferred quit. Check: busy state shows and clears, Enter
      is ignored while busy, quitting mid-delay exits cleanly, and a temporary
      `throw` shows the error path.
   3. **Real model.** Replace the delay with `session.SendAsync`, render
      `response.Text`, log usage, and fix the entry heights. Start with Ollama.
      Check: a normal turn, a `roll_dice` turn, quitting mid-request, and a
      stopped Ollama server.
   4. **Cancel key.** Add `CancelRequested` to `GameWindow` and cancel the turn
      from `GameApp`. Check: cancel mid-reply, then send another message.
4. Separately, implement the RPG milestone from `docs/RPG.md`: authoritative
   client-owned rooms/state and game-level `Look`, `Move`, and `Take` tools.
   Wiring the current agent into the TUI does not implement those game
   mechanics.
