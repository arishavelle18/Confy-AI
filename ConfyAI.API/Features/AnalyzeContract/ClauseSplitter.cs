using System.Text;
using System.Text.RegularExpressions;

namespace ConfyAI.API.Features.AnalyzeContract;

//public static class ClauseSplitter
//{
//    private const int MinClauseLength = 20; // Ignore clauses shorter than this length such as headings or bullet points

//    // Start the clause with a number, e.g., "1. ", "1.1 ", "Section 1.2 ", etc.
//    private static readonly Regex ClauseStart = new(@"^\s*(\d+(\.\d+)+|\d+[.)]|Section\s+\d+)\s+", RegexOptions.IgnoreCase);

//    public static List<string> Split(string text)
//    {
//        var clauses = new List<string>();
//        var current = new StringBuilder();

//        foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
//        {
//            var isBlank = string.IsNullOrWhiteSpace(line);

//            // A blank line or a numbered line ends the clause we were collecting.
//            if ((isBlank || ClauseStart.IsMatch(line)) && current.Length > 0)
//                Flush();

//            if (!isBlank)
//                current.Append(line.Trim()).Append(' ');
//        }

//        Flush(); // the last clause has no line after it to trigger the flush
//        return clauses;

//        void Flush()
//        {
//            var clause = current.ToString().Trim();
//            if (clause.Length >= MinClauseLength)
//                clauses.Add(clause);
//            current.Clear();
//        }
//    }
//}


public static class ClauseSplitter
{
    private const int MinClauseLength = 20; // Ignore clauses shorter than this length such as headings or bullet points

    // Start the clause with a number, e.g., "1. ", "1.1 ", "Section 1.2 ", etc.
    private static readonly Regex ClauseStart = new(@"^\s*(\d+(\.\d+)+|\d+[.)]|Section\s+\d+)\s+", RegexOptions.IgnoreCase);

    // Pasted text often has "1. ... 2. ... 3. ..." on one line. Put a line break before each inline number.
    private static readonly Regex InlineClauseStart =
        new(@"(?<=[.;])\s+(?=(\d+(\.\d+)+|\d+[.)]|Section\s+\d+)\s+[A-Z])");

    public static List<string> Split(string text)
    {
        var clauses = new List<string>();
        var current = new StringBuilder();

        var normalized = InlineClauseStart.Replace(text.Replace("\r\n", "\n"), "\n");

        foreach (var line in normalized.Split('\n'))
        {
            var isBlank = string.IsNullOrWhiteSpace(line);

            // A blank line or a numbered line ends the clause we were collecting.
            if ((isBlank || ClauseStart.IsMatch(line)) && current.Length > 0)
                Flush();

            if (!isBlank)
                current.Append(line.Trim()).Append(' ');
        }

        Flush(); // the last clause has no line after it to trigger the flush
        return clauses;

        void Flush()
        {
            var clause = current.ToString().Trim();
            if (clause.Length >= MinClauseLength)
                clauses.Add(clause);
            current.Clear();
        }
    }
}
