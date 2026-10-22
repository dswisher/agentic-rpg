# Agentic RPG

Exploring the use of LLMs to act like a GM to help run an RPG.


## Status

I am putting this project on hold, at least for now.
The primary goal was for me to learn about MEAI and how agents work at a deeper level than just using OpenCode provides.
That goal has been achieved - I am no expert, but I've learned a lot.

The premise of advancing to an RPG was to see if world state exposed through AI tools could be leveraged by an agent.
So far, the answer seems to be "no".
The agents I've experimented with (at least the ones cheap enough to use for this purpose) seem to skip calling the tools, and just make stuff up.
I'm new to context engineering, so I'm sure things could be improved.
But even a simple setup, where the context explicitly tells the agent to call the tool does not suffice.

I'd love to come back to this at some point, but I have no idea when that might be.


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

## Project Board

Implementation stories are tracked as Markdown cards in `project-board/`, using
the [`markban`](https://github.com/dswisher/markban) tool. Card colors are
visual category hints and may become more meaningful as the project grows:

- `green` — hand-crafted world
- `blue` — local command loop
- `purple` — event recording and action history
- `red` — NPC conversations
- `yellow` — LLM command integration
- `magenta` — NPC world simulation
- `cyan` — generated world content
- `orange` — TUI improvements

Epic types and dependencies describe workflow and prerequisites; colors are not
used for ordering or dependency behavior.

## Links

- [Terminal.Gui](https://github.com/tui-cs/Terminal.Gui)
