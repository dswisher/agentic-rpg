# TUI integration sketch

This document sketches how to connect `src/AgenticRpg.Tui` to the existing
agent, while keeping Terminal.Gui layout code out of `Program.cs` and keeping
agent orchestration separate from the view.

## Current state

- The CLI builds a dependency-injection container, resolves `GameApp`, and
  creates one game session for its run.
- The TUI currently builds its window and controls directly in `Program.cs`.
  Submitting text adds a player entry and a hard-coded GM response.
- `IGameAgent.CreateSession()` creates a session handle. A session's
  `SendAsync(input, cancellationToken)` returns a `ChatResponse`.
- `GameSession` currently buffers the model's streaming updates and returns a
  complete response. The TUI can show a pending/thinking state, then render the
  response when it is complete; it cannot display tokens incrementally through
  the current interface.
- The current agent is not yet an RPG world engine. It has a dice-rolling tool,
  but not the `Look`, `Move`, and `Take` tools or client-owned world state in
  `docs/RPG.md`.

## Suggested responsibilities

Keep the first version small:

- **`Program.cs`**: entry point; build the TUI container, resolve `GameApp`, and
  run it.
- **`Container.cs`**: register logging, `AddCoreServices()`, and the TUI's
  `GameApp`; build the service provider with scope validation enabled.
- **`GameApp.cs`**: own application/session lifetimes and coordinate submitted
  turns, cancellation, errors, and view updates.
- **`GameWindow.cs`**: build the Terminal.Gui window, panes, controls, and layout;
  manage chat rendering and UI state. It should not know about `IGameAgent`,
  Bedrock, or configuration.

Start with private layout-building methods in `GameWindow` (for example,
`BuildLayout`, `BuildChatPane`, and `BuildInputPane`). Split out a dedicated chat
view only if its rendering and interaction logic grow enough to merit it.

The window can expose a small interface to the coordinator, such as a
`MessageSubmitted` event and methods to append a player message, show a pending
response, render a completed response or error, and enable/disable input. Keep
the specific API simple and shaped by what the TUI actually needs.

## Lifetime and composition

The TUI should have its own composition root rather than reference the CLI
executable to reuse its `Container`. Register the same Core services with
`AddCoreServices()` and configure logging in the TUI container. The current CLI
uses Serilog; either configure equivalent logging for the TUI or extract shared
composition later if both applications need it.

Create one session for one playthrough and keep it alive while the UI runs. The
session owns the conversation history for that run. Dispose the session before
disposing the service provider, and make sure no turn is still running when the
Terminal.Gui application is disposed.

## Turn flow and UI-thread updates

1. The input's Enter handler runs on the Terminal.Gui UI thread. It can append
   the player's message, clear or disable the input, and show a pending state.
2. Start an asynchronous turn operation that awaits
   `session.SendAsync(input, cancellationToken)`. Do not use `.Wait()`, `.Result`,
   or otherwise block the UI thread waiting for the model. `Task.Run` is not
   needed just to call this asynchronous network API.
3. After the await, update Terminal.Gui controls through the application's
   `Invoke` method. In Terminal.Gui 2.5, `Application.Invoke(Action)` executes
   immediately when already on the UI thread, or queues the action for the next
   UI-loop iteration when called from another thread.
4. In the invoked action, render `response.Text`, update scroll position, and
   restore the input state. Use the same approach for cancellation and error
   messages.

Conceptually:

```csharp
var response = await session.SendAsync(input, cancellationToken);

app.Invoke(() =>
{
    gameWindow.ShowResponse(response.Text);
    gameWindow.SetBusy(false);
});
```

The agent request itself stays outside `Invoke`; only the short UI updates are
dispatched. Catch failures in the turn operation so they are presented or
logged rather than becoming unobserved task errors. Initially, allow only one
request at a time: `GameSession` has mutable conversation history and should
not receive overlapping sends.

If cancellation is added, pass a token to `SendAsync` and provide a way for the
player to cancel the active request. On shutdown, cancel and await any active
turn before disposing the UI application and session. Otherwise a late
completion could try to invoke an update after the application has been
disposed.

## Rendering considerations

- The current GM placeholder uses a fixed-height entry. Real responses can be
  longer, so calculate or otherwise accommodate the content height before
  scrolling to the bottom.
- Keep “thinking,” cancelled, and error states visible to the player; restore
  input in all completion paths.
- The existing agent instructions request plain text, while the TUI uses a
  Markdown view. Decide whether to keep plain text or intentionally support
  Markdown as the agent prompt evolves.
- Token usage is available on `ChatResponse.Usage`; it can initially remain a
  log detail, as in the CLI.

## Incremental path

1. Move the existing layout and placeholder rendering into `GameWindow` without
   connecting the agent yet.
2. Add the TUI container and `GameApp`; create and dispose one agent session per
   UI run.
3. Replace the placeholder response with the asynchronous turn flow and handle
   busy, error, and cancellation states.
4. Exercise the UI with a fake session before making a Bedrock request, then
   smoke-test against the configured client.
5. Separately, implement the RPG milestone from `docs/RPG.md`: authoritative
   client-owned rooms/state and game-level `Look`, `Move`, and `Take` tools.
   Wiring the current agent into the TUI does not implement those game
   mechanics.
