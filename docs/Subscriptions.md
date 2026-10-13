# Model Provider Plans: Replacing Bedrock

Bedrock access ends in a couple of months. Local Ollama may not be strong enough for the full game master experience (see `RPG.md`). These notes compare hosted options at roughly $20/month.

Researched Oct 2026. Plans, prices and terms change often, so re-check before subscribing.

## The catch: coding plans

Many cheap subscriptions are licensed for **coding agents only**, and a game master doesn't qualify.

- **OpenCode Go** ($10/mo): the [docs](https://opencode.ai/v2/docs/console/go) say it's "designed for OpenCode and other coding agents that produce similar types of requests. Traffic is monitored for abuse." Clients must send "typical coding agent traffic", a custom user agent, and an `x-opencode-session` header.
  - Our traffic doesn't look like a coding agent's. We send small contexts and many short tool round trips. Go's estimates assume 50–85K cached tokens per request.
  - Reddit reports it's "no longer general API access".
  - Fine for *building* this repo with OpenCode, not for *running* the game.
  - For a definitive answer, email help@anoma.ly.
- The same applies to the Z.ai GLM Coding Plan, Kimi Code, and similar plans.
- ChatGPT and Claude subscriptions don't include API access.

## Options that allow general use

| Option | Cost | What you get | Notes |
|---|---|---|---|
| **Ollama Cloud Pro** | $20/mo ($16.67 paid yearly) | $60 of usage credits per month. DeepSeek V4.1 Flash/V4 Pro, GLM-5.3, Kimi K3, MiniMax M3, gpt-oss-120b, ... | No coding-only clause in the [terms](https://ollama.com/terms). Lower prices off-peak (weekends, and weekdays outside 12:00–18:00 UTC). |
| **NanoGPT Pro** | $12/mo | 60M input tokens/week on the included open models, through web and API | No restriction shown on the pricing page (full terms not reviewed). Popular for roleplay. |
| **Synthetic.new** | $30/mo | Flat rate, 500 requests every 5 hours, 1 concurrent request per model. Kimi K3, GLM-5.3, DeepSeek V4.1 Flash, Qwen3.8 | "Works with any agent framework." Over budget, and some users complain about past price increases. |
| **Pay-as-you-go** (OpenRouter, Chutes, DeepSeek directly, NanoGPT without a plan) | Per token | General-purpose APIs | Probably cheapest at our scale (see below). |

**Avoid:** Featherless Chat ($25). Its terms say "Not for … app/API traffic", and the context is capped at 32K. Their Developer plan allows apps but starts at $50/mo.

## Rough cost estimate

Our design keeps each turn small: 200–500 tokens of injected state, trimmed history, and tool round trips. Assume about 5K input and 300 output tokens per model call, with 3 calls per turn. On DeepSeek V4.1 Flash (about $0.14–0.30 per 1M input tokens), that's **well under a cent per turn**. Even heavy play is probably a few dollars a month.

A subscription mainly pays off for premium models (Kimi K3, GLM-5.3) or a fixed bill.

## Recommendation

1. **Ollama Cloud Pro fits the code best.** `OllamaClientFactory` already uses the OpenAI-compatible client with a configurable `Endpoint`. To switch:
   - Point `Endpoint` at Ollama Cloud's OpenAI-compatible URL (believed to be `https://ollama.com/v1/`; verify in their docs).
   - Read a real API key from `~/.config/agentic-rpg/auth.jsonc`. The factory currently hard-codes `"ollama"`.
   - Keep local Ollama for cheap development runs.
2. **Measure before committing.** Build per-turn token tracking (already planned in `RPG.md`) while Bedrock is still available. Then pick flat-rate or per-token pricing from real numbers.
3. Ollama Cloud and NanoGPT are both cheap enough to try for a month. Both are OpenAI-compatible, so the same factory works with either one through `Endpoint`.
