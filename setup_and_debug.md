# Startup Instructions

To setup the LLM after a reboot or if it says the backend isn't connected, run the following in the command line:

1. Open PowerShell and enter WSL:
```powershell
   wsl
```
2. Navigate to the project and launch:
```bash
   cd ~/club-3090
   bash scripts/launch.sh
```

Then, just say yes to all the prompts.

# Debugging Aider

## Git Repo Corrupted

For some reason, sometimes Aider will make a mistake on a commit or something, and then it will be unable to make any other changes (Aider saves every single one of its changes as a commit so you can easily `/undo` any mistakes it makes). If it happens to do this, an easy fix is as follows:

1. Go to the `grapple-game/.git/objects` folder. If you're navigating via file explorer, you may need to enable the "show hidden folders" option.
2. Delete the `pack` folder.
3. Go to the terminal, navigate to your `grapple-game` folder, and run:
```bash
   git fetch origin
```

Please note, this will also get rid of any changes since the last push. Be sure to save your changes frequently!


# Local LLM Inference Configs 

## 1. Software Stack

| Layer | What |
|---|---|
| OS | Windows host running **WSL2** (Ubuntu-22.04, user `anson`) |
| Containerization | Docker Desktop |
| Inference server | **vLLM**, tensor-parallel across both GPUs |
| Model | `Qwen3.6-27B-AutoRound-INT4` |
| Serving repo | [`noonghunna/club-3090`](https://github.com/noonghunna/club-3090) — using the `dual.yml` variant |
| Coding assistant | [Aider](https://aider.chat), in **architect mode** |

**Model server config:**
- Context length: 262K
- KV cache: fp8
- Endpoint: `http://localhost:8010/v1`
- Model aliases exposed: `qwen3.6-27b` and `qwen3.6-27b-autoround`

**Exact compose file:** `models/qwen3.6-27b/vllm/compose/dual/autoround-int4/fp8-mtp.yml` in `club-3090`

- vLLM image: `vllm/vllm-openai:v0.21.0` (pinned stable release, not nightly)
- Port mapping: host `8010` → container `8000`
- Model path: `/root/.cache/huggingface/qwen3.6-27b-autoround-int4`
- `--served-model-name qwen3.6-27b-autoround`
- `--quantization auto_round`, `--dtype float16`
- `--tensor-parallel-size 2`, `--pipeline-parallel-size 1`
- `--max-model-len 262144`
- `--gpu-memory-utilization 0.92`
- `--max-num-seqs 2` (2 concurrent streams at full context)
- `--max-num-batched-tokens 8192`
- `--kv-cache-dtype fp8_e5m2`
- `--speculative-config {"method":"mtp","num_speculative_tokens":3}` (MTP n=3 draft/speedup)
- `--chat-template` — custom froggeric fixed chat template (patches several default-template bugs)
- `--reasoning-parser qwen3`, `--default-chat-template-kwargs {"enable_thinking": false}`
- `--enable-auto-tool-choice`, `--tool-call-parser qwen3_coder`
- `--enable-prefix-caching`, `--enable-chunked-prefill`
- Sampling defaults: `temperature 0.6`, `top_p 0.95`, `top_k 20`, `min_p 0.0`, `repetition_penalty 1.0`
- NVLink: auto-detected at container boot via `detect_nvlink.sh`; falls back to `--disable-custom-all-reduce` on PCIe-only (which is our case — no NVLink installed)
- `PYTORCH_CUDA_ALLOC_CONF=expandable_segments:True,max_split_size_mb:512`
- `shm_size: 16gb`, `ipc: host`
- All values are overridable via `.env` (e.g. `MAX_MODEL_LEN`, `GPU_MEMORY_UTILIZATION`, `TP`, `KV_CACHE_DTYPE`, `TEMP`) without editing the compose file directly

**What is AutoRound:** a post-training INT4 quantization method (originally Intel) that uses a sign-gradient-descent optimization pass to pick weight roundings that minimize output error, rather than naive nearest-value rounding. `Lorbus/Qwen3.6-27B-int4-AutoRound` is the pre-quantized checkpoint being used — ~4 bits/weight instead of 16/32, which is what makes it fit in the TP=2 VRAM budget. `--quantization auto_round` tells vLLM which kernel to use to correctly decode those weights at inference time.

**Flag glossary (compose command):**

| Flag | What it controls |
|---|---|
| `--model` | Path to the model weights to load |
| `--served-model-name` | The name/alias clients use in API requests (`"model": "qwen3.6-27b-autoround"`) — doesn't have to match the folder name |
| `--quantization auto_round` | Which quant format/kernel to use to decode the weights |
| `--dtype float16` | Compute dtype for the *unquantized* parts of the model (activations, etc.) |
| `--tensor-parallel-size 2` | Split the model's weights across 2 GPUs, computing each layer in parallel across both |
| `--pipeline-parallel-size 1` | Alternate way to split a model across GPUs (by layer ranges instead of within each layer) — set to 1, i.e. not used here |
| `--max-model-len 262144` | Max context length (prompt + generation) the server accepts, in tokens |
| `--gpu-memory-utilization 0.92` | Fraction of each GPU's VRAM vLLM is allowed to claim for weights + KV cache |
| `--max-num-seqs 2` | Max number of requests/streams batched and served concurrently |
| `--max-num-batched-tokens 8192` | Max tokens processed per scheduling step across all concurrent sequences (throughput/latency tuning) |
| `--kv-cache-dtype fp8_e5m2` | Precision for the attention KV cache — fp8 roughly halves KV cache memory vs fp16/bf16, enabling the 262K context |
| `--speculative-config {"method":"mtp","num_speculative_tokens":3}` | Speculative decoding: a draft mechanism (MTP) guesses 3 tokens ahead, main model verifies in one pass |
| `--chat-template` | Path to the Jinja template formatting messages into the model's expected prompt format (custom froggeric fixed version here) |
| `--reasoning-parser qwen3` | Tells vLLM how to parse this model's `<think>`-style reasoning output into a structured field |
| `--default-chat-template-kwargs {"enable_thinking": false}` | Default chat-template setting — reasoning mode off by default |
| `--enable-auto-tool-choice` + `--tool-call-parser qwen3_coder` | Enables function/tool calling and how to parse this model's tool-call output format |
| `--enable-prefix-caching` | Reuses KV cache across requests sharing a common prefix (e.g. same system prompt) |
| `--enable-chunked-prefill` | Breaks long prompt processing into chunks, interleaved with ongoing generation for other requests |
| `--override-generation-config {...}` | Default sampling params (temperature, top_p, top_k, min_p, repetition_penalty) when a client doesn't specify them |
| `--host 0.0.0.0` / `--port 8000` | Interface/port vLLM binds to *inside* the container |

## 2. Aider Configuration

- **`.aider.conf.yml`** — points Aider at `http://localhost:8010/v1` and the model alias instead of Ollama/cloud defaults.
- Running in **architect mode**.
- Context commands in use: `/drop`, `/clear`, `/tokens`.
- Aider auto-commits every change to git — use `/undo` to revert a single bad edit.
- TODO: add a `.aider.model.settings.yml` to suppress the "unknown model" warning Aider throws for this model (not set up yet).

## 3. Multi-User / Web Interface

Since vLLM already exposes an OpenAI-compatible endpoint (`http://localhost:8010/v1`), Open WebUI (or similar) should be able to point at it directly as a custom OpenAI API connection — no separate inference setup needed, just a second client hitting the same server.
