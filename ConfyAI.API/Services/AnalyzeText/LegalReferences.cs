namespace ConfyAI.API.Services.AnalyzeText;

// Vetted legal references written by us. The model only picks a category; it never writes citations.
public static class LegalReferences
{
    private static readonly Dictionary<string, string> ByCategory = new(StringComparer.OrdinalIgnoreCase)
    {
        ["probationary_period"] =
            "Labor Code, Article 296 (formerly Article 281): probationary employment shall not exceed " +
            "six (6) months from the date the employee started working, unless covered by an " +
            "apprenticeship agreement stipulating a longer period.",
        ["termination_without_cause"] =
        "Labor Code, Article 297 (formerly Article 282) and Articles 298-299: an employee may only be " +
        "dismissed for a just cause or an authorized cause, and with due process.",
        ["employee_charges"] =
        "Labor Code, Article 113 (Wage Deduction): an employer may not deduct from an employee's wages " +
        "except in cases authorized by law or with the employee's written authorization. " +
        "Deductions, penalties and bond clauses should be checked against this rule.",
        ["post_employment_restriction"] =
        "Civil Code, Article 1306: parties may agree on any terms that are not contrary to law, morals, " +
        "good customs, public order or public policy. Philippine courts generally enforce post-employment " +
        "non-compete clauses only if they are reasonable (for example in duration, territory and scope of work; " +
        "see Rivera v. Solidbank). Check these limits in the clause."
    };

    public static string? For(string? category) =>
        category is not null && ByCategory.TryGetValue(category, out var basis) ? basis : null;
}
