# RPG Game Master: Design Notes

Long-term idea: a text adventure in the style of Colossal Cave or Zork, with
rooms, exits, NPCs, items, and story state. The initial world is hand-crafted.
An LLM supplies interpretation, narration, complex planning, and NPC dialogue,
but it is not required for every player action.

## Core architecture: the engine owns the truth

- The world lives in code: rooms, exits, items, NPCs, inventory, and story
  state.
- The engine validates and applies every state change.
- The local command parser handles routine, deterministic commands without an
  LLM round trip.
- The LLM handles ambiguous or compound instructions, narration, and NPC
  conversations.
- The LLM reads and changes state only through game-level tools.
- The full map is never sent to the model. It fetches what it needs through
  context and tools.

This keeps the game responsive while preventing the model from inventing exits,
items, NPC knowledge, or successful actions.

## Command processing

Normal play uses a local-first pipeline:

1. Parse the player's input locally.
2. Resolve aliases and entities against the current world state.
3. Validate and execute recognized commands.
4. Record the resulting events.
5. Fall back to the LLM for unsupported, ambiguous, conversational, or
   compound input.

Initial local commands might include `look`, `move north`, `take sword`,
`inventory`, `open chest`, and `talk to Aldous`. The parser should remain small
and predictable at first. It should distinguish between:

- successfully parsed and executed;
- understood but invalid in the current state;
- ambiguous;
- not recognized.

Only the latter two categories generally need LLM interpretation.

Local commands can produce deterministic responses immediately. An LLM
narrator may optionally be used when additional atmosphere or story context is
valuable, but routine actions should not require one.

## State, events, and history

The game should maintain three separate kinds of information:

- **World state:** the authoritative current state used by game rules.
- **Event log:** structured facts about meaningful changes.
- **Transcripts:** the player's original commands and the text shown by the
  game, useful for presentation but not authoritative.

For example, `take sword` might produce:

```text
Player command: "take sword"
Event: ItemTaken(player, sword, chapel)
State: sword location = player inventory
```

Local actions and LLM-driven actions must produce the same domain events. The
LLM should not need the entire transcript to know what happened; each turn can
use current state plus a compact, relevant selection of recent events.

Chat history is disposable. Trimming or summarizing it may lose the wording of
past turns, but not facts that belong in world state, story state, or NPC
memory.

## Compound and complex instructions

The LLM is useful when a player expresses an intention that would otherwise
require several commands:

> Open the chest and take anything valuable inside.

The model should translate this into a constrained plan or sequence of
game-level actions, such as:

```text
Open(container = "chest")
TakeItems(container = "chest", filter = "valuable")
```

The engine then determines what is actually possible and valuable, validates
each step, applies successful steps, and records the resulting events. The
model must not be allowed to invent a sword, decide that a locked chest opened,
or mutate state directly.

Plans should stop or adapt when a step fails. The initial implementation can
use ordinary tool calls; a typed plan tool can be added once the action model
is clearer.

## NPC conversations

`talk to Aldous` is a local command that enters a dedicated conversation mode.
The TUI opens a conversation subwindow, and subsequent player input is sent to
the LLM as dialogue with Aldous until the player leaves the conversation.

The conversation model receives:

- Aldous's personality, goals, attitude, and knowledge;
- the immediate game context;
- relevant story state;
- the conversation transcript;
- tools for permitted actions and information.

It must not receive unrestricted world knowledge. Aldous can only reveal what
he knows, and any proposed world change must be validated by the engine.

Conversation mode should make the interaction boundary clear in the TUI.
Commands such as `leave` or `quit` end the conversation and return to normal
command mode. The game may pause normal player actions while the conversation
is active.

Conversation transcripts should be scoped to the conversation rather than
becoming the entire game history. Important consequences become structured
events, for example:

```text
ConversationStarted(player, Aldous)
InformationRevealed(Aldous, monastery_is_to_the_north)
NpcAttitudeChanged(Aldous, player, trust +1)
ConversationEnded(player, Aldous)
```

NPC memory can be updated when a conversation ends, but memory updates should
be constrained and validated rather than allowing the model to rewrite an
NPC's knowledge arbitrarily.

## NPC simulation

The LLM has three broad responsibilities:

1. **World generation** (later): create validated, persistent world content.
2. **NPC animation:** meaningful movement, decisions, goals, and dialogue.
3. **Complex player instructions:** interpret ambiguity and plan multi-step
   actions.

Routine NPC behavior does not necessarily need an LLM. Schedules, movement,
and simple reactions can be deterministic. LLM calls should be reserved for
important decisions, encounters with the player, and dialogue. A later
Director component could occasionally review story state and plan upcoming
beats, separate from the per-turn narrator or NPC actors.

## Story state and context

Story state is structured state owned by the engine:

- **Arc / beats:** the current act and next intended development.
- **Objectives:** id, description, status, and completion conditions.
- **Journal:** concise important events.
- **NPC memory:** what each NPC knows, attitude, and relevant conversations.

Do not rely on the model to remember to ask for critical context. Provide a
small state block on every relevant LLM call containing the current location,
nearby entities, active objectives, current story beat, a short story summary,
and recent relevant events.

Add deeper information to tool results when the engine knows it is relevant.
Rarely needed lore can be fetched on demand, but critical facts should not live
only behind an optional tool call.

The engine should complete objectives when their conditions are met, rather
than trusting a `CompleteObjective` request from the model. A `RecordEvent`
operation can capture story-significant consequences when appropriate.

## Tools and model boundaries

Prefer coarse, game-level tools such as `Look`, `Move`, `Take`, `Open`, `Talk`,
and `RecallEvents` over unrestricted CRUD operations such as
`SetItemLocation` or `UpdateRoomFlag`.

The model proposes; the engine decides. Every tool must enforce rules for
location, visibility, ownership, locks, capabilities, and story conditions.
Tool results should report authoritative facts that the model can safely
narrate.

## Generating the world (later)

- Use structured output plus validation for generated rooms, NPCs, and items.
- Once saved, generated world content is permanent; the model does not
  regenerate it casually.
- Keep a short world bible containing setting, tone, and immutable key facts.
- Start with generated scenery before attempting generated puzzles or plot
  structures.

## Pitfalls

1. **Made-up details:** tools and injected state must be authoritative; the
   model should describe only facts returned by them.
2. **Slow routine actions:** keep common commands local and measure LLM latency
   and token usage.
3. **Ambiguous entity references:** resolve names locally where possible and
   ask for clarification rather than guessing.
4. **Overpowered plans:** validate every step of a compound instruction and
   expose only permitted actions.
5. **Story drift:** persist important facts in structured state, journal, or
   NPC memory rather than relying on chat history.
6. **NPC omniscience:** provide each NPC only the knowledge justified by the
   world and that NPC's experience.

## First milestones

1. Three hand-made rooms, one item, and authoritative state for location and
   inventory.
2. Local `Look`, `Move`, `Take`, and `Inventory` commands.
3. A shared event model used by both local commands and future LLM tools.
4. `Talk` command and a TUI conversation subwindow with one hand-crafted NPC.
5. LLM-backed NPC dialogue with constrained context and validated effects.
6. LLM fallback for unsupported input and compound action planning.

Success means routine play is responsive, NPC conversations are engaging, and
the model cannot cheat its way through a locked door or invent facts that are
not present in the engine's state.

## How this maps to MEAI

- **Function calling** (`AIFunctionFactory`, `.UseFunctionInvocation()`): the
  interface between the model and authoritative game actions.
- **Context strategies:** inject fresh structured state; trim chat history
  aggressively because durable facts live in the engine.
- **Middleware:** a state-injecting `DelegatingChatClient` and `.UseLogging()`
  to debug model decisions and tool calls.
- **Multiple interaction modes:** normal commands use the local-first pipeline;
  conversation mode sends turns to the NPC-specific LLM interaction.
