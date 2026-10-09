namespace ConfyAI.UI.Models;

public record AnalyzeContractResult(int ClauseCount, int RiskyClauseCount, int NeedsReviewCount, List<ClauseResult> Clauses);

public record ClauseResult(bool Risky, string? Category, string? Explanation, string? LegalBasis, string RawClauseText, bool NeedsReview);

public record ApiError(string? Error);