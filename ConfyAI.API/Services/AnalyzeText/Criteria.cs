namespace ConfyAI.API.Services.AnalyzeText;

// One criterion = one risk the model checks. The code routes a clause to a criterion by keywords,
// so the 3B model only ever sees ONE small, focused question at a time.
public record Criterion(string Category, string[] Keywords, string Rules)
{
    public bool Matches(string clause) =>
        Keywords.Any(k => clause.Contains(k, StringComparison.OrdinalIgnoreCase));
}

public static class Criteria
{
    public static readonly IReadOnlyList<Criterion> All =
    [
        new("probationary_period", ["probation"], 
            //"""
            //Risk to detect: PROBATIONARY PERIOD TOO LONG
            //- Flag risky=true ONLY if the clause sets a probationary period LONGER than 6 months (for example 7 months, 9 months, or 1 year).
            //- If the clause allows an extension, ADD the extension to the base period. Flag risky=true if the total is longer than 6 months.
            //- Do NOT flag a probationary period of 6 months (about 180 days) or less.
            //- Do NOT flag a clause that does not state the length of the probationary period.
            //- Do NOT flag clauses that do not mention probation at all.
            //- Convert days to months first (30 days = 1 month). 90 days is 3 months, which is NOT risky.
            //- If the base period plus the extension is exactly 6 months or less, it is NOT risky.


            //Examples:
            //Clause: "The Employee shall serve a probationary period of twelve (12) months from the date of hiring."
            //Answer: {"risky": true, "category": "probationary_period", "explanation": "The probationary period is 12 months, which is longer than 6 months."}

            //Clause: "The probationary period is four (4) months and may be extended by up to four (4) more months."
            //Answer: {"risky": true, "category": "probationary_period", "explanation": "The base period is 4 months, but the extension of up to 4 more months makes it 8 months in total, which is longer than 6 months."}

            //Clause: "The Employee shall serve a probationary period of six (6) months."
            //Answer: {"risky": false, "category": null, "explanation": null}

            //Clause: "The Employee will receive a monthly salary of PHP 25,000."
            //Answer: {"risky": false, "category": null, "explanation": null}

            //Clause: "The Employee shall be on probation for sixty (60) days."
            //Answer: {"risky": false, "category": null, "explanation": null}

            //Clause: "The probationary period is two (2) months and may be extended by up to four (4) more months."
            //Answer: {"risky": false, "category": null, "explanation": null}
            //"""
            "Handled in code: see ClauseAnalyzer.RunProbationAsync."
            ),

        new("termination_without_cause", ["terminat", "dismiss", "end employment", "discretion", "at will"], """
            Risk to detect: TERMINATION WITHOUT A STATED CAUSE
            - Flag risky=true if the clause lets the COMPANY end the employment without a stated valid cause, for example "at any time", "for any reason", "at will", "without cause", "without notice", or "at the Company's discretion". This applies even if a notice period is given, as long as no specific cause is required.
            - Do NOT flag a clause that allows termination only for specific causes (for example serious misconduct, willful disobedience, gross neglect of duties, or redundancy).
            - Do NOT flag a clause where only the EMPLOYEE resigns or gives notice.
            - Do NOT flag clauses that do not talk about ending the employment.

            Examples:
            Clause: "The Company may terminate the Employee at any time for any reason."
            Answer: {"risky": true, "category": "termination_without_cause", "explanation": "The Company can terminate for any reason, without a specific cause."}

            Clause: "The Company may terminate this contract without prior notice or cause."
            Answer: {"risky": true, "category": "termination_without_cause", "explanation": "The Company can terminate without notice and without a cause."}

            Clause: "Employment may be terminated for serious misconduct after due notice and hearing."
            Answer: {"risky": false, "category": null, "explanation": null}

            Clause: "The Employee may resign by giving thirty (30) days written notice."
            Answer: {"risky": false, "category": null, "explanation": null}
            """),

            new("employee_charges", ["deduct", "penalt", "liquidated", "bond", "reimburs", "forfeit", "withhold"], """
            Risk to detect: CHARGES OR PENALTIES AGAINST THE EMPLOYEE
            - Flag risky=true if the clause lets the COMPANY deduct money from the Employee's pay for losses, damages, shortages or penalties.
            - Flag risky=true if the clause makes the EMPLOYEE pay a penalty, liquidated damages, a bond, or repay training costs, especially when the Employee resigns or leaves early.
            - Do NOT flag deductions required by law, such as SSS, PhilHealth, Pag-IBIG or income tax.
            - Do NOT flag the Company reimbursing or paying the Employee.
            - Do NOT flag clauses that do not talk about deductions, penalties, bonds or repayment by the Employee.

            Examples:
            Clause: "The Company may deduct from the Employee's pay the cost of any equipment that is lost or damaged."
            Answer: {"risky": true, "category": "employee_charges", "explanation": "The Company can deduct the cost of lost or damaged equipment from the Employee's pay."}

            Clause: "If the Employee resigns before one (1) year, the Employee must pay the Company PHP 50,000 as a penalty."
            Answer: {"risky": true, "category": "employee_charges", "explanation": "The Employee must pay a penalty for resigning early."}

            Clause: "The Employee signs a bond of PHP 30,000 which is forfeited if the Employee leaves within one year."
            Answer: {"risky": true, "category": "employee_charges", "explanation": "The Employee forfeits a bond for leaving early."}

            Clause: "The Company shall withhold income tax and SSS contributions from the salary as required by law."
            Answer: {"risky": false, "category": null, "explanation": null}

            Clause: "The Company will reimburse the Employee for transportation expenses."
            Answer: {"risky": false, "category": null, "explanation": null}
            """),

            new("post_employment_restriction", ["compet", "engage in", "similar business", "similar trade", "same industry", "shall not work", "after leaving", "after resignation", "after employment", "after termination"], """
            Risk to detect: RESTRICTION ON WORKING AFTER EMPLOYMENT ENDS (NON-COMPETE)
            - Flag risky=true if the clause stops the Employee, AFTER the employment ends (after resignation, termination or leaving), from working for a competitor or doing similar business or work.
            - Do NOT flag a restriction that only applies WHILE the Employee is employed.
            - Do NOT flag clauses that only protect confidential information or only forbid soliciting customers or employees.
            - Do NOT flag clauses where "competitive" only describes pay or benefits.
            - Do NOT flag clauses that do not restrict the Employee's future work.

            Examples:
            Clause: "After leaving the Company, the Employee shall not work for any competing business for three (3) years."
            Answer: {"risky": true, "category": "post_employment_restriction", "explanation": "The Employee is barred from working for a competitor after leaving the Company."}

            Clause: "The Employee agrees not to engage in any similar business after resignation."
            Answer: {"risky": true, "category": "post_employment_restriction", "explanation": "The Employee cannot do similar business after resigning."}

            Clause: "The Employee shall not work for a competitor while employed by the Company."
            Answer: {"risky": false, "category": null, "explanation": null}

            Clause: "The Company offers a competitive benefits package."
            Answer: {"risky": false, "category": null, "explanation": null}

            Clause: "The Employee shall not solicit the Company's customers after employment ends."
            Answer: {"risky": false, "category": null, "explanation": null}
            """),
    ];
}