using MediatR;

namespace ConfyAI.API.DTO;

public record AnalyzeTextRequest(string Text) : IRequest<AnalyzeTextResponse>;
