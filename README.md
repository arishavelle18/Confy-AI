# Confy

**Read your employment contract before you sign it. Confy marks the clauses worth a second look.**

Confy is a local-first tool for Filipino workers. You paste a contract (or upload a PDF or Word file), and Confy flags clauses that deserve a closer look, explains why in plain English, and points to the Philippine law you can check the clause against.

Built for the **AppBuildersPH Hackathon 2026** (Local AI theme).

> Confy is **not legal advice**. It flags clauses that *should be checked*. It does not say whether a clause is valid or enforceable. For real decisions, ask a lawyer or the DOLE.

---

## At a glance

- **Project name:** Confy
- **Short description:** A local-first contract checker. It reads an employment contract on your own computer and marks clauses worth a second look, with plain-English reasons and Philippine law to check against.
- **Team members:** Solo build by Arishavelle Karl D. Villanueva ([@arishavelle18](https://github.com/arishavelle18))
- **Repository:** https://github.com/arishavelle18/Confy-AI
- **Demo video:** _add link_
- **X / LinkedIn video post:** _add link_

---

## Why local

An employment contract holds your name, salary and personal terms. Confy runs on your own computer:

- The language model runs through [Ollama](https://ollama.com) on `localhost`.
- The API and the web app both run on `localhost`.
- Your contract is never sent to any server on the internet. There are no cloud AI calls, no analytics and no external fonts or scripts.

You can switch off Wi-Fi and Confy still works (after the one-time setup below).

### Why does this product benefit from running AI locally?

An employment contract is private. It has your salary, your employer's name and terms you may not want anyone else to see, and the people who need this tool most are often worried about their job. Sending that file to a cloud AI means trusting a third party with it. Running the model locally means the contract never leaves the computer, which removes that risk, costs nothing per check, and works without internet. A small 3B model is enough here because the job is narrow: the code handles routing, the numbers and the legal references, and the model only answers one small question at a time.

### What runs locally and what needs internet

| | |
|---|---|
| **Runs locally (always)** | Model inference (Ollama), the API, the web app, PDF and DOCX text extraction, clause splitting, legal reference lookup |
| **Needs internet (one time only, setup)** | Installing the .NET SDK and Ollama, restoring NuGet packages, `ollama pull qwen2.5:3b` |
| **Needs internet while using Confy** | Nothing |
| **Cloud services / third-party APIs** | None. No API keys, no accounts, no telemetry |

## What it checks (4 checks)

| Check | What Confy looks for |
|---|---|
| Probationary period | A probation (including any extension) longer than 6 months |
| Termination without cause | The employer may end employment at will, with no stated cause |
| Charges and penalties | Deductions, penalties, bonds or liquidated damages charged to the employee |
| Restriction after you leave | Non-compete or similar limits after employment ends |

Legal references shown in the app (for you to check against):
- Labor Code Art. 296 (formerly 281) on probationary employment, Arts. 297 to 299 (formerly 282 to 284) on termination causes
- Labor Code Art. 113 on wage deductions
- Civil Code Art. 1306 and *Rivera v. Solidbank* on reasonableness of post-employment restrictions

## How it works

Small local models are easy to confuse, so Confy does not ask one model one big question.

1. **Split.** The contract is split into numbered clauses.
2. **Route by rules.** Keyword rules decide which of the 4 checks apply to a clause.
3. **One focused question per check.** The model sees one clause and one narrow criterion at a time, with a few examples.
4. **Code decides what it can.** For probation, the model only extracts the numbers (length and extension). Plain code converts and compares them to 6 months.
5. **Verified references.** Legal references are attached by code from a fixed list. The model never writes law citations.
6. **No silent "safe".** If the model fails or returns something unusable for a clause, Confy marks it **"could not be checked"** instead of calling it safe.

## Tech stack

- **API:** .NET 10, ASP.NET Core Minimal APIs, Carter, MediatR, FluentValidation, OllamaSharp, PdfPig (PDF text), DocumentFormat.OpenXml (DOCX text)
- **Web app:** Blazor WebAssembly (.NET 10)
- **Model:** `qwen2.5:3b` via Ollama (temperature 0, JSON output)

## Run it

**You need:** [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Ollama](https://ollama.com).

```bash
# one time, needs internet
ollama pull qwen2.5:3b

# 1. start the API (https://localhost:7267)
cd ConfyAI.API
dotnet run

# 2. in another terminal, start the web app (https://localhost:7151)
cd ConfyAI.UI
dotnet run
```

Open https://localhost:7151. Make sure Ollama is running first.

After the first restore and model download, no internet is needed.

## Disclosures (hackathon rules)

### Existing code before the hackathon
A few days before the build window (about Oct 7), I made a small learning experiment to understand how local LLMs work: a single-clause `/analyze-text` endpoint and a clause splitter. It helped me prepare for this project, and I reused that project skeleton and clause splitter as my starting point. The 4-check design, code-decided probation, legal references, validation, file upload and the entire Blazor web app were built during the hackathon.

### Assets
The Confy logo is an inline SVG drawn for this project and the UI design is my own. There are no stock images or external icon packs.

### AI tools used
- **Claude** (Anthropic): used as a pair-programming assistant for architecture advice, code, prompt design for the local model, test cases, UI code and this README. I reviewed, ran and tested the code myself on my machine.

### Models
- **Qwen2.5-3B-Instruct** (`qwen2.5:3b` on Ollama), by Alibaba Cloud. It runs locally. Its license is **`qwen-research`** (see the model card: https://huggingface.co/Qwen/Qwen2.5-3B-Instruct). Please read that license before any commercial use.

### APIs and cloud services
None at runtime. Confy calls no cloud API. The only network traffic is between the browser, the Confy API and Ollama, all on `localhost`.

### Open-source libraries
Carter, MediatR, FluentValidation, OllamaSharp, PdfPig, DocumentFormat.OpenXml and the .NET / Blazor framework. Each is used under its own license; see the respective package pages.

### Fonts
The app uses system fonts by default. If Fraunces and DM Sans are bundled, they are licensed under the SIL Open Font License.

## Known limitations

- Only **4 checks**. Confy says nothing about other problems in a contract.
- Keyword routing can miss unusual wording, and a 3B model can miss or misread a clause (for example, a termination clause that says "either party may terminate with 30 days notice").
- Confy flags clauses that *should be checked*. A flagged clause is not necessarily illegal, and an unflagged clause is not necessarily fine.
- **Scanned PDFs (images) are not supported** (no OCR). Use a text PDF, a DOCX, or paste the text.
- Text inside **DOCX tables** is not read.
- Long contracts take a while because the model runs on your own machine. Input is limited to 30,000 characters and 5 MB per file.
- English contracts only for now.

## Demo

- Demo video: _add link_

## License

MIT License. See [LICENSE](LICENSE). The Qwen2.5-3B model is covered by its own license (see "Models" above).
