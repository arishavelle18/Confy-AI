using ConfyAI.API.DTO;
using ConfyAI.API.Services.AnalyzeText;
using FluentValidation;
using MediatR;

namespace ConfyAI.API.Features.AnalyzeContract;

public record AnalyzeContractRequest(string Text) : IRequest<AnalyzeContractResponse>;
public class AnalyzeContractRequestValidator : AbstractValidator<AnalyzeContractRequest>
{
    public const int MaxChars = 30_000;

    public AnalyzeContractRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("The contract text is empty.")
            .MaximumLength(MaxChars).WithMessage($"The contract is too long. The limit is {MaxChars:N0} characters.");
    }
}

public record AnalyzeContractResponse(int ClauseCount, int RiskyClauseCount, int NeedsReviewCount, List<AnalyzeTextResponse> Clauses);

internal class AnalyzeContractRequestHandler(IClauseAnalyzer clauseAnalyzer) : IRequestHandler<AnalyzeContractRequest, AnalyzeContractResponse>
{
    public async Task<AnalyzeContractResponse> Handle(AnalyzeContractRequest request, CancellationToken cancellationToken)
    {
        var clauses = ClauseSplitter.Split(request.Text);
        var results = new List<AnalyzeTextResponse>();
        int needsReviewCount = 0;
        // Step 3: reuse the single-clause analysis, one clause at a time.
        foreach (var clause in clauses)
        {
            var result = await clauseAnalyzer.AnalyzeAsync(clause, cancellationToken);
            if (result.Risky || result.NeedsReview)
                results.Add(result);

            if (result.NeedsReview)
                needsReviewCount++;
        }

        return new AnalyzeContractResponse(clauses.Count, results.Count(r => r.Risky), needsReviewCount, results);
    }
}