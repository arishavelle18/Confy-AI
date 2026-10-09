namespace ConfyAI.API.DTO;

public record AnalyzeTextResponse(
    bool Risky,
    string? Category,
    string? Explanation,
    string? LegalBasis,
    string RawClauseText,
    bool NeedsReview = false
);