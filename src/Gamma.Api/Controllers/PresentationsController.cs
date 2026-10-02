using Gamma.Api.Contracts;
using Gamma.Core.Abstractions;
using Gamma.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gamma.Api.Controllers;

[ApiController]
[Route("api/presentations")]
public sealed class PresentationsController : ControllerBase
{
    private readonly ISlideContentGenerator _contentGenerator;
    private readonly RendererResolver _rendererResolver;
    private readonly RendererProvider _defaultProvider;
    private readonly ILogger<PresentationsController> _logger;

    public PresentationsController(
        ISlideContentGenerator contentGenerator,
        RendererResolver rendererResolver,
        IOptions<RenderingOptions> renderingOptions,
        ILogger<PresentationsController> logger)
    {
        _contentGenerator = contentGenerator;
        _rendererResolver = rendererResolver;
        _defaultProvider = renderingOptions.Value.DefaultProvider;
        _logger = logger;
    }

    /// <summary>
    /// Step 1 of the Gamma-style flow: turn a prompt into structured deck
    /// content via the configured AI provider, without producing any file
    /// yet. Lets a caller review/edit the JSON before rendering.
    /// </summary>
    [HttpPost("draft")]
    [ProducesResponseType(typeof(PresentationDocument), StatusCodes.Status200OK)]
    public async Task<ActionResult<PresentationDocument>> Draft([FromBody] CreatePresentationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest("Prompt is required.");
        }

        try
        {
            var document = await _contentGenerator.GenerateAsync(request.ToDomain(), cancellationToken);
            return Ok(document);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "AI drafting failed.");
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway, title: "AI provider error");
        }
    }

    /// <summary>
    /// Renders an already-built (or already-edited) <see cref="PresentationDocument"/>
    /// to a .pptx file. No AI call happens here — this is Gamma's "export" step.
    /// </summary>
    [HttpPost("render/pptx")]
    [Produces("application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    public async Task<IActionResult> RenderPptx([FromBody] PresentationDocument document, [FromQuery] RendererProvider? renderer, CancellationToken cancellationToken)
    {
        var provider = renderer ?? _defaultProvider;
        var pptxRenderer = _rendererResolver.ResolvePresentationRenderer(provider);

        try
        {
            var bytes = await pptxRenderer.RenderAsync(document, cancellationToken);
            return File(bytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation", BuildFileName(document.Title, "pptx"));
        }
        catch (NotImplementedException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status501NotImplemented, title: $"{provider} renderer not implemented yet");
        }
    }

    /// <summary>Renders an already-built <see cref="PresentationDocument"/> directly to PDF.</summary>
    [HttpPost("render/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> RenderPdf([FromBody] PresentationDocument document, [FromQuery] RendererProvider? renderer, CancellationToken cancellationToken)
    {
        var provider = renderer ?? _defaultProvider;
        var pdfRenderer = _rendererResolver.ResolvePdfRenderer(provider);

        try
        {
            var bytes = await pdfRenderer.RenderAsync(document, cancellationToken);
            return File(bytes, "application/pdf", BuildFileName(document.Title, "pdf"));
        }
        catch (NotImplementedException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status501NotImplemented, title: $"{provider} renderer not implemented yet");
        }
    }

    /// <summary>
    /// The one-shot, Gamma-style endpoint: prompt in, file out. Drafts
    /// content with AI, then immediately renders it in the requested
    /// format. format=pptx|pdf (default pptx); renderer=OpenSource|Aspose|Syncfusion
    /// (default from config).
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePresentationRequest request,
        [FromQuery] string format = "pptx",
        [FromQuery] RendererProvider? renderer = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest("Prompt is required.");
        }

        PresentationDocument document;
        try
        {
            document = await _contentGenerator.GenerateAsync(request.ToDomain(), cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "AI drafting failed.");
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status502BadGateway, title: "AI provider error");
        }

        var provider = renderer ?? _defaultProvider;

        try
        {
            if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                var pdfBytes = await _rendererResolver.ResolvePdfRenderer(provider).RenderAsync(document, cancellationToken);
                return File(pdfBytes, "application/pdf", BuildFileName(document.Title, "pdf"));
            }

            var pptxBytes = await _rendererResolver.ResolvePresentationRenderer(provider).RenderAsync(document, cancellationToken);
            return File(pptxBytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation", BuildFileName(document.Title, "pptx"));
        }
        catch (NotImplementedException ex)
        {
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status501NotImplemented, title: $"{provider} renderer not implemented yet");
        }
    }

    private static string BuildFileName(string title, string extension)
    {
        var safe = string.Join("-", (string.IsNullOrWhiteSpace(title) ? "presentation" : title)
            .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(safe) ? $"presentation.{extension}" : $"{safe}.{extension}";
    }
}
