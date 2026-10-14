# Local Model Candidates

Notes from a model survey on 2026-10-13. Nothing here is tested yet. Sizes come from the Ollama library on that date. Many of these models are newer than the assistant's training data, so treat this as a list of things to try, not recommendations.

## Hardware

- **M1 Mac, 32 GB**: by default the GPU can use about 21–22 GB. Models around 20 GB are a tight fit once the context cache is added.
- **M5 Mac, 48 GB**: the GPU can use about 36 GB. Use it for the 20–25 GB models.

## Already configured

`qwen3.5:2b`, `:4b`, `:9b` (default), `:35b-a3b`, plus Bedrock Sonnet 4.6 and Haiku 4.5.

Note: `35b-a3b` is a mixture-of-experts model. It holds 35B weights but uses only about 3B for each token. It's fast, but it often reasons more like a mid-sized model than a 35B one. A dense 27B model (one that uses all its weights for every token) may follow the rules better, but it runs slower.

## What matters for this game

The first milestone tests **rule-following, not prose**. The model should narrate only what tools return and must not get through a locked door. Judge models on reliable tool calling and following the system prompt first.

## Qwen models between 9b and 35b

| Model | Size | Notes |
|---|---|---|
| `qwen3.5:27b` | 17 GB | Dense, the obvious middle option. Slow on the M1, better on the M5. |
| `qwen3.8:27b` | 18 GB | Newer generation (late Sept 2026), aimed at agent tasks. Compare with 3.5:27b on the M5. |
| `qwen3.5:9b-q8_0` | 10 GB | Same 9b with lighter compression. A cheap check of whether quality improves. |

## Models from other families

| Model | Size | Why |
|---|---|---|
| `gemma4:12b` | about 8 GB | A good M1 step up from qwen 9b, from a different model family. |
| `gemma4:26b` / `gemma4:31b` | 16–20 GB | Larger Gemma models for the M5. |
| `gpt-oss:20b` | 14 GB | Mixture-of-experts, strong at tool calls, fast. Fits well on the M1. |
| `mistral-small3.2:24b` | 15 GB | Dense, about a year old. Solid function calling and good prose. A useful baseline. |
| `granite4.1:8b` / `:30b` | 5 / 17 GB | IBM model focused on tool use and structured JSON. Interesting for world generation (`GetResponseAsync<Room>`). |
| `muse-glimmer:30b` | 18 GB | New Meta model tuned for tool use and recovering from errors. M5. |
| `nemotron-3.5-lightning:30b` | 25 GB | NVIDIA mixture-of-experts model (about 3B active) built for agents. M5 only. |

**Shortlist to start with:** `gpt-oss:20b` and `gemma4:12b` on the M1, and `qwen3.5:27b` vs `qwen3.8:27b` on the M5.

## Things that affect results

1. **Context length.** Ollama's OpenAI-compatible endpoint can't set the context size per request. If the prompt goes over the server's limit, Ollama silently drops older text, which can look like the model "forgetting the rules". Set the `OLLAMA_CONTEXT_LENGTH` environment variable, or create a Modelfile variant with a larger `num_ctx`.
2. **Thinking models.** Each tool round trip pays for the thinking tokens again, so a turn with several tool calls gets slow. Try each model with thinking on and off.
3. **Using the M5 remotely.** Start Ollama on the M5 with `OLLAMA_HOST=0.0.0.0`. Then add config entries whose `endpoint` is `http://<m5-host>:11434/v1/`.

## Next steps

- [ ] Write a short fixed script of player inputs, for example:
  - "go north" into the locked door
  - "take the sword"
  - "give the sword to the troll then go west"
- [ ] Run the script against each model. Log tool calls and token counts with `.UseLogging()`.
- [ ] For each model, record:
  - whether it made up exits or items
  - whether it got through the locked door
  - tool-call accuracy
  - latency per turn
- [ ] Set up the M5 endpoint and add config entries for the larger models.
- [ ] Check the context-length setting on both machines.

## Results

_(fill in as models are tested)_

| Model | Machine | Thinking | Cheated? | Made things up? | Tool accuracy | Latency/turn | Notes |
|---|---|---|---|---|---|---|---|
