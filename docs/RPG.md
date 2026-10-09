# RPG Game Master: Design Notes

Long-term idea: an LLM acts as game master for a text adventure in the style of Colossal Cave or Zork, with a world made of rooms, NPCs and items. These notes collect the design threads so far. Nothing here is built yet.

## Core principle: the client owns the truth

- The world lives in code: rooms, exits, items, NPCs, story state.
- The model **narrates and interprets** what the player means. It only reads and changes state through tools.
- The full map is never sent. The model fetches what it needs with tool calls.
- Chat history is **disposable**. Trimming or summarizing it loses the wording of past turns, not the facts, because the facts are kept outside the chat.

This avoids the main way these projects fail (early AI Dungeon, for example): treating the conversation as the game state, so the world drifts as context fills up or gets trimmed.

## Pitfalls and how to handle them

1. **Made-up details** (exits, items, NPCs that don't exist)
   - Make tools authoritative: `Move("north")` returns a failure if there's no north exit.
   - System prompt: "Only describe what tools return."
   - Inject the current room's state every turn instead of hoping the model calls `Look`.
2. **Tool design**
   - Prefer coarse, game-level tools (`Look`, `Move`, `Take`, `Talk`) over fine-grained CRUD (`SetItemLocation`, `UpdateRoomFlag`).
   - The model proposes and the engine decides. The engine enforces the rules, and the model can't freely change state.
3. **Speed and cost**: each tool call is another round trip to the model. Track per-turn token usage.
4. **Ambiguous input** ("give the sword to the troll then go west"): handling this is a strength of LLMs, but expect to tune the prompt.

## Keeping the story arc coherent

Story state is world state, so it's structured and owned by the client:

- **Arc / beats**: e.g. "Act 2: find the missing heir. Next beat: learn she fled to the monastery."
- **Objectives**: id, description, status (active / done / failed), completion conditions.
- **Journal**: short list of key events ("Player spared the troll").
- **NPC memory**: per NPC, attitude plus what they know or have said.

### Getting story state to the model: three layers

Don't rely on the model to *remember to ask*.

1. **Always in context (pushed every turn).** A small block (about 200–500 tokens) rebuilt each turn: current room, active objectives, current beat, "story so far" summary, recent journal entries.
   - In MEAI: a `DelegatingChatClient` that adds a fresh state system message, or rebuilds `ChatOptions.Instructions`, before each call.
2. **Added to tool results (the engine decides what's relevant).** Tool results include related story context. The engine does the lookup, not the model. Example `Move` result:
   ```
   Room: Chapel. Exits: south.
   NPC present: Brother Aldous.
   Story note: Aldous knows where the heir went (objective: find-heir).
   Aldous's memory: distrusts the player after the fire.
   ```
3. **Fetched on demand (the model asks).** Deeper, rarely needed info: `GetLore(topic)`, `RecallEvents(about)`. Found through tool descriptions and the system prompt. Unreliable, so nothing critical should live only here.

### Writing the story back

- `RecordEvent(summary)`: the model adds journal entries for things that matter later.
- Prefer the **engine** completing objectives when conditions are met (item taken, NPC talked to) over a `CompleteObjective(id)` tool, so the model can't skip ahead.
- When trimming history, have the model summarize the dropped turns into "story so far", **saved in state**, not left as a chat message.

## Generating the world (later)

- Use **structured output plus validation**: `GetResponseAsync<Room>(...)` with lore and constraints. The engine checks the result and saves it.
- Once saved, it's **permanent**. The model never regenerates it.
- Keep a short "world bible" (setting, tone, key facts) in the system prompt for consistency.
- The hard part is coherence over time: plot threads, NPC motives, solvable puzzles. Start with generated scenery and leave generated puzzles for much later.

## Later: separate narrator and director

- **Narrator**: plays turn by turn.
- **Director**: a separate, occasional call that reviews story state, plans the next beats, adjusts objectives, plants hooks.

This keeps the per-turn prompt small and gives the story a dedicated planner.

## How this maps to the README steps

- **Tool calling** (`AIFunctionFactory`, `.UseFunctionInvocation()`): the core of the game.
- **Context strategies**: safe to trim aggressively because state lives in the client. Summaries go into story state.
- **Middleware**: a state-injecting `DelegatingChatClient`, and `.UseLogging()` to debug "why did the GM say that?"

## First milestone

Three hand-made rooms, one item, and the tools `Look`, `Move` and `Take`.

Success means the model narrates only what tools return and **can't cheat its way through a locked door**.
