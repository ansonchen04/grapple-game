# Learnings

## Setting Up & Learning to Use AI-Assisted Development

### Why Local, Not Cloud

I decided to run my own local LLM for a few reasons:
- **Cost** — running Claude/ChatGPT (Codex)/whatever gets expensive fast if you're using it heavily for personal projects.
- **Privacy**
- **Education** — I wanted to learn how to actually set up and configure a personal LLM stack. As frontier model costs add up, I think it's valuable to know how to run your own — both as a fallback and to understand the tradeoffs (quantization, serving engines, hardware) that cloud APIs abstract away.

### The Hardware/Infra Journey

- I started with a basic setup — a single 3090 running Ollama. This got me ~30 tokens/sec and usable context up to about 32K before performance degraded.
- Wanting more context and throughput, I added a second 3090 and switched to vLLM.
- I went with 2x 3090s and skipped NVLink — NVLink would've cost an extra ~$200 for only about 15% more throughput, which wasn't worth it for my use case.
- With 48GB of combined VRAM, 70B-class models became an option, but I stuck with Qwen3.6-27B:
  - At the time, the newest Qwen release for the 70B tier was still version 2.5, while 27B had the newer 3.6.
  - Sticking with 27B (vs. a 70B model) left much more VRAM headroom for context length, which mattered more to me than raw parameter count for coding tasks.
  - It was already working well for me on a single GPU, so there was no urgent reason to switch model size.
- Migrating from Ollama to vLLM was the bigger lever, not just adding a second GPU: vLLM's more configurable memory/KV-cache handling took me from ~30 tok/s and 32K context (Ollama, single GPU) to 65+ tok/s and 262K context (vLLM, dual GPU, tensor-parallel).

### Learning to Work With a Local (Not Frontier) Model

- **Core lesson:** local models need different handling than something like Claude or ChatGPT. Early on, I tried just prompting Qwen with a big task (like I have in the past with Claude) and it would just hit its context limit mid-way, stop, and lose track of what it had already done. I had to learn to break tasks into much smaller pieces.
- **Architect mode** (`/architect` in Aider) made a big difference. Forcing the model to write a plan before touching any code meant each step could be implemented and tested individually - and if something went wrong, I could fix just that step rather than the whole task. It also meant that if the model did run out of context mid-task, I could just point it back at the plan to see what was done and pick up where it left off.
- Adding relevant files (and sometimes links) explicitly with `/add` helped Aider stay focused with the right context for a given problem. For me, this is still manual right now, but I could see some kind of agent eventually automating what context gets pulled in and what context I can drop.
- I put together a `CONVENTIONS.md` file - similar in spirit to Claude Code's Skills - to consistently prompt Aider toward the coding patterns and habits that worked best for this project.
- Sometimes the local model just isn't smart enough to work out a solution on its own. When that happened, I'd ask Claude or ChatGPT to sketch a plan, then feed that plan into Aider to implement.
- Asking Aider to explain *why* a change would work before implementing it helped cut down on nonsense solutions and hallucinated fixes.

### Where It Helped vs. Where It Didn't

- Aider was strong at targeted, well-defined changes - fixing a specific bug, or a mechanical task like parsing dimensions out of a batch of `.tscn` files into a clean table. Anything with a clear, bounded scope, it handled quickly.
- It struggled more with very abstract or open-ended problems. Getting the Verlet rope simulation stable took significantly longer than a well-scoped bug fix would have. In situations where it needed to iteratively try things until it actually worked, it seemed to struggle a lot more, since it's a lot more open-ended.
- Aider commits after every change, which made it easy to roll back anything I didn't like. Occasionally, though, this rapid commit/read cycle exposed real git issues on my end — a few times I had to clear out corrupted pack files and re-fetch from origin to get the repo healthy again.

## Game Dev Notes (brief)

- Rope/swing physics: the visual rope uses Verlet integration for the sag/flex you actually see, but the player's swing motion is governed separately by a unidirectional, critically-damped spring-damper constraint with some slack tolerance — the two systems are deliberately decoupled so the visual rope doesn't have to double as the actual gameplay constraint. I found this was the most fun to play, so that's why it's like this.