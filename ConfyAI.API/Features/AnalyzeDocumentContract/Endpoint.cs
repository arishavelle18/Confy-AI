using Carter;
using ConfyAI.API.Features.AnalyzeTextContract;
using ConfyAI.API.Services.DocumentText;
using MediatR;

namespace ConfyAI.API.Features.AnalyzeDocumentContract;

public class Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/analyze-contract/file", async (IFormFile file, IMediator mediator, CancellationToken cancellationToken) =>
        {
            const long maxBytes = 5 * 1024 * 1024;
            var extension = Path.GetExtension(file.FileName);

            if (extension is not (".pdf" or ".docx"))
                return Results.BadRequest(new { error = "Upload a .pdf or .docx file." });

            if (file.Length > maxBytes)
                return Results.BadRequest(new { error = "The file is too large. The limit is 5 MB." });

            string text;
            try
            {
                await using var stream = file.OpenReadStream();
                text = DocumentTextExtractor.Extract(stream, extension);
            }
            catch (Exception)
            {
                return Results.BadRequest(new { error = "Could not read the file. It may be damaged or password protected." });
            }

            if (string.IsNullOrWhiteSpace(text))
                return Results.BadRequest(new { error = "No text found in the file. Scanned documents are not supported yet." });

            var result = await mediator.Send(new AnalyzeContractRequest(text), cancellationToken);
            return Results.Ok(result);
        })
        .DisableAntiforgery();
    }
}
