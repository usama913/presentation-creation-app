# Gamma Clone — AI presentation & PDF generator (.NET 10)

A Gamma-style backend: send a prompt, get a drafted slide deck back as a
real `.pptx` or `.pdf`. Built as an ASP.NET Core 10 Web API, split into
clean layers so the rendering engine can be swapped later without
touching the API or the AI drafting logic.

## What it does

1. **Draft** — `ISlideContentGenerator` sends your prompt to an LLM
   (OpenAI or Anthropic, your choice) and gets back a structured deck:
   a title, a subtitle, and a list of slides, each with a layout, a
   title, and either bullets or body text.
2. **Render** — that structured deck is handed to a renderer that turns
   it into bytes: `IPresentationRenderer` for `.pptx`, `IPdfRenderer` for
   `.pdf`. The default renderer is 100% free and dependency-free (see
   below); Aspose.Slides and Syncfusion.Presentation are wired up as
   swappable alternatives you can turn on later.

You can also skip step 1 entirely and POST your own deck JSON straight to
the render endpoints — the AI step is optional, not load-bearing.

## Why it builds with zero NuGet packages

The open-source renderer doesn't use DocumentFormat.OpenXml or QuestPDF —
it hand-assembles the `.pptx` (an Open Packaging Convention zip of XML
parts) with `System.IO.Compression`, and hand-writes the `.pdf` byte
format directly (objects, xref table, content streams, the built-in
Helvetica core font — no font embedding needed). The AI layer talks to
OpenAI/Anthropic with plain `HttpClient` + `System.Text.Json` instead of
their SDKs. Every project also uses `<FrameworkReference Include="Microsoft.AspNetCore.App" />`
to get `Microsoft.Extensions.*` (DI, Options, Logging, HttpClientFactory)
from the ASP.NET Core shared framework already on your machine, instead
of from NuGet.

The practical upshot: **this solution restores and builds with the .NET
SDK alone, no internet access required**, which is also exactly why it
was built and verified this way in the sandbox this was generated in
(that environment's network policy blocks `nuget.org`; your own machine
almost certainly won't have that restriction, but you never need to find
out). Every renderer, every JSON parser, every HTTP call in this
solution was built and tested against real output — generated files were
verified by opening them with `python-pptx` and `pypdf`, and by running
the Web API for real and hitting its endpoints with `curl`. It is not
speculative code.

The only genuinely external pieces are the two commercial engines
(Aspose, Syncfusion) — those are stubs on purpose, see below.

## Project layout

```
GammaClone.slnx
src/
  Gamma.Core/                  Domain models + interfaces. No implementation, no dependencies.
    Models/                    PresentationDocument, Slide, SlideLayout, ThemeOptions,
                                PresentationRequest, RendererProvider, AiProvider, options classes.
    Abstractions/               ISlideContentGenerator, IPresentationRenderer, IPdfRenderer,
                                RendererResolver (keyed-DI lookup).

  Gamma.Ai/                    The "draft with AI" step.
    Clients/                   IChatCompletionClient + OpenAI/Anthropic HTTP implementations.
    SlideContentGenerator.cs   Prompts the model for strict JSON, parses it into PresentationDocument.

  Gamma.Rendering.OpenSource/  The default, free, dependency-free renderer.
    Pptx/                      Hand-built OOXML .pptx writer.
    Pdf/                       Hand-built PDF writer (Helvetica core font, real word-wrap).

  Gamma.Rendering.Commercial/  Stubs for Aspose.Slides / Syncfusion.Presentation.
                                Throw NotImplementedException with instructions until you wire them up.

  Gamma.Api/                   ASP.NET Core Web API tying it together.
    Controllers/PresentationsController.cs   /draft, /render/pptx, /render/pdf, /create

  Gamma.Samples/               Console smoke-test: builds a sample deck by hand (no AI call,
                                no API key needed) and writes sample-deck.pptx/.pdf to disk.
```

Dependency direction is one-way: `Gamma.Api` → `Gamma.Ai` / `Gamma.Rendering.*` → `Gamma.Core`.
Nothing outside `Gamma.Core` depends on any other layer, so the API and
the AI drafting code never know or care which rendering engine is
actually running.

## Running it

Requires the **.NET 10 SDK**.

```bash
# Quick sanity check — no API key needed, no network needed:
dotnet run --project src/Gamma.Samples -- ./out
# writes ./out/sample-deck.pptx and ./out/sample-deck.pdf

# Build everything:
dotnet build GammaClone.slnx

# Run the API:
dotnet run --project src/Gamma.Api
# Swagger/OpenAPI isn't wired up (see "Adding API docs" below) — call it
# directly, e.g. with curl or Postman, against http://localhost:5034 (see
# src/Gamma.Api/Properties/launchSettings.json for the exact port).
```

### Configuring the AI provider

Edit `src/Gamma.Api/appsettings.json`, or (recommended for the API key)
use an environment variable or `dotnet user-secrets`:

```json
{
  "Ai": {
    "Provider": "OpenAi",           // or "Anthropic"
    "OpenAi": { "ApiKey": "", "Model": "gpt-4o-mini" },
    "Anthropic": { "ApiKey": "", "Model": "claude-sonnet-4-5" }
  }
}
```

Environment-variable override (standard ASP.NET Core config binding,
double underscore = nested key):

```bash
export Ai__OpenAi__ApiKey=sk-...
# or
export Ai__Provider=Anthropic
export Ai__Anthropic__ApiKey=sk-ant-...
```

`OpenAiOptions.BaseUrl` can point at an Azure OpenAI chat-completions-compatible
endpoint instead of `api.openai.com` if that's what you use.

## API

All endpoints are under `/api/presentations`. `PresentationDocument` is the
JSON shape shown below — see `src/Gamma.Core/Models/` for every field.

### `POST /draft` — prompt → structured content (no file yet)

```bash
curl -X POST http://localhost:5034/api/presentations/draft \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Why competitive intelligence matters for SaaS companies",
    "slideCount": 6,
    "audience": "sales leaders",
    "tone": "confident, concise"
  }'
```

Returns a `PresentationDocument` JSON object you can inspect or hand-edit
before rendering.

### `POST /render/pptx` and `POST /render/pdf` — content → file

```bash
curl -X POST http://localhost:5034/api/presentations/render/pptx \
  -H "Content-Type: application/json" \
  -o deck.pptx \
  -d '{
    "title": "Why CI Matters",
    "slides": [
      { "title": "Why CI Matters", "layout": "TitleSlide" },
      { "title": "The problem", "layout": "TitleAndBullets",
        "bullets": ["Competitors move fast", "Manual tracking doesn'\''t scale"] },
      { "title": "Let'\''s talk", "layout": "Closing", "body": "Book a demo" }
    ]
  }'
```

Add `?renderer=OpenSource|Aspose|Syncfusion` to either render endpoint to
pick the engine for that one call (defaults to `Rendering:DefaultProvider`
in config). Asking for `Aspose`/`Syncfusion` before you've implemented
them returns **501 Not Implemented** with a message telling you what to
do next — it never silently falls back or crashes the process.

### `POST /create?format=pptx|pdf` — the one-shot Gamma-style call

Same body as `/draft`, but drafts with AI and renders in one round trip:

```bash
curl -X POST "http://localhost:5034/api/presentations/create?format=pdf" \
  -H "Content-Type: application/json" \
  -o deck.pdf \
  -d '{ "prompt": "Onboarding plan for new enterprise customers", "slideCount": 7 }'
```

### Slide layouts

`SlideLayout` (used in both AI output and hand-built JSON):
`TitleSlide`, `TitleAndBullets`, `SectionHeader`, `Quote`, `Closing`.

## Switching to Aspose.Slides or Syncfusion later

Everything is already wired for this — it's a config flip plus filling
in one class, not a rewrite:

1. Get the license + NuGet package:
   - `dotnet add package Aspose.Slides.NET --project src/Gamma.Rendering.Commercial`, or
   - `dotnet add package Syncfusion.Presentation.Net.Core --project src/Gamma.Rendering.Commercial`
2. Implement the corresponding stub in `src/Gamma.Rendering.Commercial/`
   (`AsposePresentationRenderer.cs`, `AsposePdfRenderer.cs`,
   `SyncfusionPresentationRenderer.cs`, `SyncfusionPdfRenderer.cs`) — each
   has a doc comment sketching the real Aspose/Syncfusion API calls to
   use.
3. Set `Rendering:DefaultProvider` to `"Aspose"` or `"Syncfusion"` in
   `appsettings.json` (or pass `?renderer=Aspose` per request without
   changing the default at all).

`Gamma.Api` and `Gamma.Ai` never reference Aspose/Syncfusion directly —
they only see `IPresentationRenderer`/`IPdfRenderer` from `Gamma.Core`, so
nothing else in the solution changes.

## Adding API docs (Swagger/OpenAPI)

Deliberately left out so the solution needs zero network access to build.
Once you have NuGet access:

```bash
dotnet add package Microsoft.AspNetCore.OpenApi --project src/Gamma.Api
```

then in `Program.cs`: `builder.Services.AddOpenApi();` and
`app.MapOpenApi();` (or swap in `Swashbuckle.AspNetCore` for classic
Swagger UI).

## Known limitations / good next steps

- **One slide master/layout, absolutely-positioned shapes.** Solid for a
  generated deck; if you want it editable in PowerPoint's Outline view
  with real placeholder inheritance, that's a natural next step in
  `PptxSlideXmlBuilder`.
- **No images, charts, or icons yet.** `Slide.ImagePrompt` exists as a
  reserved field for a future image-generation step; both renderers
  currently ignore it safely.
- **PDF text is Helvetica-only** (the built-in core font, no embedding).
  Good enough for clean, professional slides; custom font embedding is a
  real chunk of extra work if you need brand-exact typography in the PDF.
- **No auth/rate-limiting on the API.** Add whatever your deployment
  needs (API key middleware, ASP.NET Core Identity, etc.) — nothing here
  precludes it.
- **`AddKeyedSingleton`** is used for renderer selection (.NET 8+ keyed
  DI) — if you ever need per-request renderer *state* rather than a
  stateless singleton, switch those registrations to keyed-scoped.
