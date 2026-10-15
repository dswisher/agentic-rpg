# TUI step 3 plan: the asynchronous turn

Step 3 of the incremental path in `docs/TUI-integration.md`: replace the
placeholder GM reply with a real asynchronous turn, including busy, error,
cancellation, and quit handling. The design rationale (threading model,
deferred quit) lives in `docs/TUI-integration.md`; this file is the working
checklist.

Each sub-step should leave the TUI working and be committed on its own.

## Starting point (end of step 2)

- `GameApp.Run` creates the session before `app.Init()`, then the
  Terminal.Gui application and `GameWindow`, and runs it. Per-run state lives in
  fields (`window`, `session`) that are cleared in a `finally`.
- `GameWindow` raises `MessageSubmitted` (trimmed, non-empty text) and has
  `AddPlayerMessage` and `AddGmMessage`.
- `OnMessageSubmitted` still adds a hard-coded GM reply.
- Logs go to `~/.local/state/agentic-rpg/tui.log`, recreated each run.

## Key ideas to keep in mind

- Terminal.Gui installs `MainLoopSyncContext` while a session runs. An `await`
  on the UI thread resumes on the UI thread on a later loop iteration (queued
  through `app.Invoke` → `TimedEvents`, run by `RunTimers` in each iteration).
- Core uses `ConfigureAwait(false)` everywhere (enforced by CA2007). The TUI
  does not: `GameApp`'s single `await` per turn should resume on the UI thread,
  so it can update the window directly. No `Task.Run`, no `app.Invoke`.
- Never block the UI thread on a task.
- Quitting is deferred while a turn is active, rather than cancelling and
  waiting after `app.Run` returns.

## 3a: Core uses `ConfigureAwait(false)`, enforced by CA2007

No behavior change; separate commit.

- [x] In `.editorconfig`, add a section that applies only to Core, with a
      comment explaining why the TUI is excluded:
      ```ini
      [src/AgenticRpg.Core/**.cs]
      dotnet_diagnostic.CA2007.severity = warning
      ```
- [x] Fix what it reports. Currently that's `GameSession`'s `await foreach`:
      add `.ConfigureAwait(false)` to `GetStreamingResponseAsync(...)`.
- [x] Do **not** enable CA2007 for `src/AgenticRpg.Tui`.

Checks:

- [x] Build has no warnings.
- [x] Temporarily remove the `ConfigureAwait` and confirm CA2007 fires.
- [x] CLI and TUI behave exactly as before.

## 3b: async turn with a fake delay, busy and error states, deferred quit

No model calls yet: `await Task.Delay(2000, ct)` and a canned reply.

### `GameWindow`

- [x] `SetBusy(bool busy)`: a `busy` flag that makes `OnInputAccepted` ignore
      Enter, plus a visible indicator (for example, change the input pane's
      title to "Input — GM is thinking…"; keep `inputPane` in a field).
      **Keep the `TextField` enabled**, so it keeps focus and still receives
      keys (needed for the cancel key in 3d).
- [x] `AddSystemMessage(string text)`: a third kind of entry, for
      `[cancelled]`, `[quitting…]`, and errors (for example, a plain `Label`
      with no frame), built on `AddEntry`.
- [x] Remove the "later steps" TODO comment.

### `GameApp`

- [x] Per-run fields: `app`, `window`, `session`, `turnCts`, `turnActive`,
      `quitPending`. Set and clear them like the existing ones.
- [x] `OnMessageSubmitted`: ignore if `turnActive`; otherwise add the player
      message, `SetBusy(true)`, set `turnActive`, dispose the old `turnCts` and
      create a new one, and start `RunTurnAsync` without awaiting it
      (`_ = RunTurnAsync(text, ct);`).
- [x] `RunTurnAsync`: no `ConfigureAwait(false)`. Catch everything so the task
      never faults.

```csharp
private async Task RunTurnAsync(string input, CancellationToken ct)
{
    try
    {
        await Task.Delay(2000, ct);         // 3c: session!.SendAsync(input, ct)
        window!.AddGmMessage("...");        // back on the UI thread: no Invoke needed
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
        window?.SetBusy(false);
        turnActive = false;                 // before RequestStop: see below
        if (quitPending) app?.RequestStop(window);
    }
}
```

### Deferred quit

- [x] Subscribe to the window's `IsRunningChanging` event:

```csharp
gameWindow.IsRunningChanging += OnWindowRunningChanging;

private void OnWindowRunningChanging(object? sender, CancelEventArgs<bool> e)
{
    if (e.NewValue || !turnActive) return;  // starting, or idle: let it stop

    e.Cancel = true;                        // not yet...
    quitPending = true;
    window!.AddSystemMessage("[quitting…]");
    _ = turnCts!.CancelAsync();             // ...the turn's finally will RequestStop
}
```

- [x] Order matters: `turnActive = false` before `RequestStop`. `RequestStop`
      only sets `StopRequested`; the loop then exits and `End` raises
      `IsRunningChanging` again, which must be allowed through this time.
- [x] Use `CancelAsync()`, not `Cancel()`, in the veto handler. `Cancel()` runs
      the token's callbacks synchronously, so `Task.Delay` completes inside the
      handler. Its `await` continuation captured `MainLoopSyncContext`, which
      is still current, so .NET runs it inline instead of posting it. The
      turn's `finally` then calls `RequestStop` *before* the handler returns,
      and `Run` resets `StopRequested = false` after the vetoed `End`, so the
      quit is lost (symptom: `[quitting…]` and `[cancelled]` appear, but the
      app keeps running). `CancelAsync()` runs the callbacks on the thread
      pool, so the continuation is posted and runs on a later loop iteration,
      after the reset. A plain `Cancel()` is fine anywhere a stop isn't being
      vetoed (for example, the 3d cancel key).
- [x] After `app.Run` returns: log a warning if `turnActive` is still true (a
      bug), then dispose `turnCts`. Do not wait on anything.

### To verify in the IDE

- [x] The exact event args type of `IsRunningChanging` (the docs describe
      `CancelEventArgs<bool>` with `NewValue` and `Cancel`).
      Confirmed: `EventHandler<CancelEventArgs<bool>>` (`IRunnable.cs`).
- [x] That the quit key actually goes through `IsRunningChanging`. It is
      documented as raised by both `RequestStop` and `End`.
      Confirmed, with a correction: `RequestStop` only sets `StopRequested`.
      `RunLoop` exits, `Run`'s `finally` calls `End`, and `End` raises
      `IsRunningChanging`. If a handler cancels, `End` returns early, and
      `Run` resets `StopRequested = false` and re-enters `RunLoop`
      (`ApplicationImpl.Run.cs`).
- [x] What the quit key is (I believe Esc is the v2 default).
      Confirmed: `[Command.Quit] = Bind.All (Key.Esc)` (`Application.cs`).
      Ctrl+C is not quit: the terminal is in raw mode, so it arrives as a key,
      and `TextField` binds it to Copy.

### Checks

- [x] The busy indicator appears and clears.
- [x] Enter does nothing while busy; typing still works.
- [x] Quitting mid-delay shows `[quitting…]` and then exits; nothing alarming
      in `tui.log`.
- [x] Quitting while idle exits immediately.
- [x] A temporary `throw` inside the turn shows the error path, and input
      works again afterwards.
- [x] Optional: a breakpoint after the `await` in `RunTurnAsync` shows the main
      thread, with `TimedEvents.RunTimersImpl` → `Invoke` lambda → `MoveNext`
      on the call stack.
      That holds for a normal turn and for a quit using `CancelAsync()`. With
      a plain `Cancel()` in the veto handler, the stack instead shows
      `OnWindowRunningChanging` → `CancellationTokenSource.Cancel` → … →
      `MoveNext`: the continuation ran inline.

## 3c: the real model

- [x] Replace the delay with `session!.SendAsync(input, ct)` and render
      `response.Text`.
- [x] Log `response.Usage` as the CLI does.
- [x] Fix the fixed GM entry height (5 rows): try `Height = Dim.Auto()` on the
      entry and the `Markdown` view; if `Markdown` doesn't report its height
      properly, calculate the wrapped line count from the text and width.
- [x] Look at how the plain-text, 80-column prompt renders in a `Markdown`
      view. Decide between plain text and Markdown, and whether the
      line-length rule still makes sense for the TUI.
- [x] Start with Ollama (free, local).

Checks:

- [x] A normal turn.
- [x] A turn that calls `roll_dice`.
- [x] Request and response visible in `tui.log` (enable `Debug` for
      `Microsoft.Extensions.AI` for full payloads, if wanted).
- [x] Quitting mid-request: tests whether the provider honors cancellation. If
      it doesn't, the quit waits for the request, but the UI stays responsive.
- [x] Stopping the Ollama server mid-session shows the error path.
- [x] Smoke-test against Bedrock once Ollama works.

## 3d: a key for cancelling

- [x] `GameWindow` adds a `CancelRequested` event. In `OnInputKeyDown`, when
      `busy` is set and the cancel key is pressed, raise it and set
      `Handled = true`. When idle, leave the key unhandled.
- [x] `GameApp` handles it with `turnCts?.Cancel()`; the existing
      `[cancelled]` path does the rest.
- [x] If the cancel key is also the quit key (Esc), this mirrors the CLI's
      Ctrl+C: cancel while busy, quit while idle. Verify that handling the key
      in the view stops the app-level quit binding.

Checks:

- [x] Cancel mid-reply, then send another message. The conversation should be
      consistent, because `GameSession` only records a turn in history when it
      succeeds.
- [x] Quitting while busy still works (deferred quit and cancel share the same
      path).

## After step 3

- Update `docs/TUI-integration.md`'s "Current state" and mark step 3 done.
- Next big step: the RPG milestone in `docs/RPG.md` (`Look`, `Move`, `Take`,
  client-owned world state).

## Next session: understanding async continuations

Before (or alongside) 3b, walk through Stephen Cleary's writing to firm up the
model behind this plan:

- "Don't Block on Async Code" (the classic UI/`.Result` deadlock).
- "ConfigureAwait FAQ" (Stephen Toub, .NET blog) and Cleary's
  "Async and Await" intro.
- "There Is No Thread".
- "Task.Run Etiquette" (why `Task.Run` is a smell in libraries but fine at the
  UI edge).

Relate each to the Terminal.Gui v2.5.0 source we looked at:

- `Terminal.Gui/App/MainLoop/MainLoopSyncContext.cs` (`Post` → `app.Invoke`,
  with the thread-pool fallback when nothing can pump; issue #5636).
- `Terminal.Gui/App/ApplicationImpl.Run.cs` (`Invoke`, `Begin`/`End`
  installing and restoring the context, the run loop, `CanPumpPostedWork`).
- `Terminal.Gui/App/MainLoop/ApplicationMainLoop.cs` (`IterationImpl`: input,
  draw, then `TimedEvents.RunTimers`).
- `Terminal.Gui/App/Timeout/TimedEvents.cs` (the queue: a `SortedList` keyed
  by due time).
