using Carter;
using MediatR;

namespace ConfyAI.API.Features.AnalyzeTextContract;

public class Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/analyze-contract", async (AnalyzeContractRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            return Results.Ok(result);
        });
    }
}
