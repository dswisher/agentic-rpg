# Agentic RPG

Exploring the use of LLMs to act like a GM to help run an RPG.


## Setup

The config lives outside the repo, in `~/.config/agentic-rpg/`, and comes in two parts.

The first part, `config.jsonc`, specifies the providers and models that are available for use.
You can see [my version](https://github.com/dswisher/dotfiles/blob/main/dot_config/agentic-rpg/config.jsonc) in my dotfiles repo.
It looks something like:

```jsonc
{
  "defaultModel": "local-qwen-9b",
  "models": [
    {
      "name": "local-qwen-9b",
      "provider": "ollama",
      "modelId": "qwen3.5:9b",
      "options": {
        "endpoint": "http://localhost:11434/v1/"
      }
    },
    {
      "name": "bedrock-haiku",
      "provider": "bedrock",
      "modelId": "global.anthropic.claude-haiku-4-5-20251001-v1:0",
      "options": {
        "region": "us-east-1"
      },
      "authProfile": "bedrock"
    }
  ]
}
```

The bedrock model above requires authentication, and that goes in `auth.jsonc` in the same config directory, and looks something like:

```jsonc
{
  "profiles": {
    "bedrock": {
      "bearerToken" : "<your token>"
    }
  }
}
```

You should be sure to run `chmod 600` on `auth.jsonc`.


## Local Models with Ollama

Install with `brew install ollama`.

When you wish to use the models, run:

```
brew services start ollama
```

When done, run:

```
brew services stop ollama
```

The command `brew services list` will list all running services.

To use a model, you must first pull it, with something like
```
ollama pull qwen3.5:9b
```

## Links

- [Terminal.Gui](https://github.com/tui-cs/Terminal.Gui)

