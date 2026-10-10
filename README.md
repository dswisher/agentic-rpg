# `Microsoft.Extensions.AI` Proof of Concept

Exploring LLM chat in C# with `Microsoft.Extensions.AI` (MEAI), with a focus on managing conversation context.

## Setup

Config lives outside the repo, in `~/.config/agentic-rpg/config.jsonc` (`chmod 600`):

```jsonc
{
  "bearerToken": "<bedrock-api-key>",
  "modelId": "<inference-profile-id>",
  "regionName": "us-east-1"
}
```

## Steps

- [x] Stateless calls: the model has no memory between calls
- [x] Manual history loop: every turn re-sends the whole conversation
- [x] System prompts: `ChatRole.System` message vs. `ChatOptions.Instructions`
- [x] Streaming: `GetStreamingResponseAsync`, rebuilding the message for history
- [x] Token usage: watch input tokens grow each turn
- [x] Tool calling: `AIFunctionFactory`, `.UseFunctionInvocation()`
- [ ] Middleware: `ChatClientBuilder`, `.UseLogging()`, a trimming `DelegatingChatClient`
- [ ] Context strategies: sliding window, token budget, summarization

