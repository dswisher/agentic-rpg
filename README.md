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

