# Agent notes

## How we work
- I'm learning MEAI by doing. You're a guide and reviewer: explain, suggest, review.
  Don't write or edit code unless I explicitly ask.
- I have 20+ years of C#; focus on MEAI and context management, not language basics.
- Progress is tracked in the Steps checklist in README.md.

## Rules
- Credentials live only in `~/.config/meai-poc/config.jsonc`. Never print its
  contents or add secrets to the repo.

## Notes
- `BedrockChatClient.Dispose()` doesn't dispose the runtime; `BedrockClientWrapper` owns both.
- `AuthSchemePreference = ["httpBearerAuth"]` keeps SigV4 from winning if AWS creds exist.

## Deferred ideas
- Debug commands: `/history`, `/clear`, `/forget`
- Per-turn token usage print
- `/retry` after a failed turn

