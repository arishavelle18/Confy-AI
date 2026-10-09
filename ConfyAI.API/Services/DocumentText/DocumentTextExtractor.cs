using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace ConfyAI.API.Services.DocumentText;

public static class DocumentTextExtractor
{
    public static string Extract(Stream stream, string extension)
    {
        // Word needs a seekable stream, so copy the upload into memory first.
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        memory.Position = 0;

        return extension.ToLowerInvariant() switch
        {
            ".pdf" => ExtractPdf(memory),
            ".docx" => ExtractDocx(memory),
            _ => throw new NotSupportedException("Only .pdf and .docx files are supported.")
        };
    }

    private static string ExtractPdf(Stream stream)
    {
        var text = new StringBuilder();
        using var pdf = PdfDocument.Open(stream);
        foreach (var page in pdf.GetPages())
            text.AppendLine(ContentOrderTextExtractor.GetText(page));
        return text.ToString();
    }

    private static string ExtractDocx(Stream stream)
    {
        var text = new StringBuilder();
        using var doc = WordprocessingDocument.Open(stream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return string.Empty;

        // A blank line between paragraphs lets ClauseSplitter tell the clauses apart.
        foreach (var paragraph in body.Elements<Paragraph>())
            text.AppendLine(paragraph.InnerText).AppendLine();
        return text.ToString();
    }
}