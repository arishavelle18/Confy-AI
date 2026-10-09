using ConfyAI.API.DTO;
using OllamaSharp;
using OllamaSharp.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConfyAI.API.Services.AnalyzeText;

public class ClauseAnalyzer(OllamaApiClient ollamaApiClient) : IClauseAnalyzer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // A clause that only says "do not solicit" and has none of these is not a non-compete.
    private static readonly string[] WorkWords =
        ["work for", "employed by", "engage in", "compet", "provide services", "similar business", "same industry"];
    public async Task<AnalyzeTextResponse> AnalyzeAsync(string clause, CancellationToken cancellationToken)
    {
        var needsReview = false;
        // Rules first: only criteria whose keywords appear in the clause are sent to the model.
        // find 
        foreach (var criterion in Criteria.All.Where(c => c.Matches(clause)))
        {
            var result = await RunCriterionAsync(criterion, clause, cancellationToken);
            if (result.Risky)
                return result;

            needsReview |= result.NeedsReview;
        }
        return new AnalyzeTextResponse(false, null, null, null, clause, needsReview);
    }

    private async Task<AnalyzeTextResponse> RunCriterionAsync(Criterion criterion, string clause, CancellationToken cancellationToken)
    {
        if (criterion.Category == "probationary_period")
            return await RunProbationAsync(clause, cancellationToken);
        // A non-solicit clause is decided in code, not by the model.
        if (criterion.Category == "post_employment_restriction" && IsOnlyNonSolicit(clause))
            return new AnalyzeTextResponse(false, null, null, null, clause);

        var generateRequest = new GenerateRequest
        {
            Prompt = BuildPrompt(criterion, clause),
            Format = "json",
            Options = new RequestOptions { Temperature = 0.0f }
        };

        var responseText = new StringBuilder();

        await foreach (var chunk in ollamaApiClient.GenerateAsync(generateRequest, cancellationToken))
        {
            if (chunk != null)
                responseText.Append(chunk.Response);
        }

        try
        {
            var result = JsonSerializer.Deserialize<AnalyzeTextResponse>(responseText.ToString(), JsonOptions)!;
            if (!result.Risky)
                return new AnalyzeTextResponse(false, null, null, null, clause);

            return result with
            {
                Category = criterion.Category,
                RawClauseText = clause,
                LegalBasis = LegalReferences.For(criterion.Category)
            };
        }
        catch
        {
            return new AnalyzeTextResponse(
                Risky: false,
                Category: null,
                Explanation: null,
                LegalBasis: null,
                RawClauseText: clause,
                true);
        }
    }

    private static string BuildPrompt(Criterion criterion, string clause) => $$"""
        You are an AI that checks ONE contract clause for ONE risk, for an employee.
 
        {{criterion.Rules}}
 
        Input Clause:
        "{{clause}}"
 
        Reply ONLY in raw JSON in this format:
        {"risky": true or false, "category": "{{criterion.Category}}" or null, "explanation": "Reason here" or null}
        """;

    private record ProbationExtract(
        [property: JsonPropertyName("base_value")] double? BaseValue,
        [property: JsonPropertyName("base_unit")] string? BaseUnit,
        [property: JsonPropertyName("extension_value")] double? ExtensionValue,
        [property: JsonPropertyName("extension_unit")] string? ExtensionUnit);

    private static double ToMonths(double? value, string? unit) => (value, unit?.ToLowerInvariant()) switch
    {
        (null, _) => 0,
        (var v, "days") => v!.Value / 30.0,
        (var v, "months") => v!.Value,
        (var v, "years") => v!.Value * 12,
        _ => 0
    };

    // The model only copies the numbers. The code adds them up and decides.
    private async Task<AnalyzeTextResponse> RunProbationAsync(string clause, CancellationToken cancellationToken)
    {
        var prompt = $$"""
            Extract the probationary period from this contract clause. Do NOT judge it. Only copy the numbers.
            Units must be exactly "days", "months" or "years". Use null for anything not stated.

            Clause: "The Employee shall be on probation for forty-five (45) days."
            Answer: {"base_value": 45, "base_unit": "days", "extension_value": null, "extension_unit": null}

            Clause: "The probationary period is two (2) months, extendable by two (2) months."
            Answer: {"base_value": 2, "base_unit": "months", "extension_value": 2, "extension_unit": "months"}

            Clause: "The probationary period is ten (10) months, extendable at the Company's discretion."
            Answer: {"base_value": 10, "base_unit": "months", "extension_value": null, "extension_unit": null}

            Clause: "Regularization depends on the Company's evaluation."
            Answer: {"base_value": null, "base_unit": null, "extension_value": null, "extension_unit": null}

            Clause: "{{clause}}"
            Answer:
            """;

        var request = new GenerateRequest
        {
            Prompt = prompt,
            Format = "json",
            Options = new RequestOptions { Temperature = 0.0f }
        };

        var text = new StringBuilder();
        await foreach (var chunk in ollamaApiClient.GenerateAsync(request, cancellationToken))
            if (chunk != null) text.Append(chunk.Response);

        try
        {
            var e = JsonSerializer.Deserialize<ProbationExtract>(text.ToString())!;
            var total = ToMonths(e.BaseValue, e.BaseUnit) + ToMonths(e.ExtensionValue, e.ExtensionUnit);

            if (total <= 6.0001)
                return new AnalyzeTextResponse(false, null, null, null, clause);

            return new AnalyzeTextResponse(
                Risky: true,
                Category: "probationary_period",
                Explanation: $"The probationary period is {total:0.#} months in total (including any stated extension), which is longer than 6 months.",
                LegalBasis: LegalReferences.For("probationary_period"),
                RawClauseText: clause);
        }
        catch
        {
            return new AnalyzeTextResponse(false, null, null, null, clause,true);
        }
    }

    //True when the clause only forbids soliciting clients or employees.
    private static bool IsOnlyNonSolicit(string clause) =>
        clause.Contains("solicit", StringComparison.OrdinalIgnoreCase) &&
        !WorkWords.Any(w => clause.Contains(w, StringComparison.OrdinalIgnoreCase));
}