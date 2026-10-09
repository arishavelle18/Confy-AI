using ConfyAI.API.DTO;

namespace ConfyAI.API.Services.AnalyzeText;

public interface IClauseAnalyzer
{
    Task<AnalyzeTextResponse> AnalyzeAsync(string clause, CancellationToken cancellationToken);
}
