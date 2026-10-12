# Agentic RPG

Exploring the use of LLMs to act like a GM to help run an RPG.

## Setup

Config lives outside the repo, in `~/.config/agentic-rpg/config.jsonc` (`chmod 600`):

```jsonc
{
  "bearerToken": "<bedrock-api-key>",
  "modelId": "<inference-profile-id>",
  "regionName": "us-east-1"
}
```

## Local Models with Ollama

Install with `brew install ollama`.

When you wish to use the models, run `brew services start ollama`, and when done, run `brew services stop ollama`.
The command `brew services list` will list all running services.

To use a model, you must first pull it, with something like `ollama pull qwen3.5:35b-a3b`.

