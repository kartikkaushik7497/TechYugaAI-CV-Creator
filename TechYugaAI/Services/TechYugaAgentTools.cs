using System.ComponentModel;

namespace TechYugaAI.Services;

public class TechYugaAgentTools
{
    private readonly CvDraftState cvState;
    private readonly CvPdfGenerator pdfGenerator;
    private readonly CvTemplateRegistry templateRegistry;

    public TechYugaAgentTools(CvDraftState cvState, CvPdfGenerator pdfGenerator, CvTemplateRegistry templateRegistry)
    {
        this.cvState = cvState;
        this.pdfGenerator = pdfGenerator;
        this.templateRegistry = templateRegistry;
    }

    [Description("Updates the CV draft using JSON. Provide structured data for name, summary, skills, experience, education, projects, certifications, and contact.")]
    public string UpdateCvDraft(string cvJson)
    {
        if (!CvDraftJson.TryDeserialize(cvJson, out var draft, out var error) || draft is null)
        {
            return $"[ERROR] CV draft update failed. {error}";
        }

        cvState.ApplyUpdate(draft);
        return "[SUCCESS] CV draft updated.";
    }

    [Description("Generates a professional CV PDF using the selected template. Provide templateId and the full cvJson draft when possible.")]
    public string GenerateCVDoc(string templateId, string? cvJson = null)
    {
        // Integration point for QuestPDF or specialized ML.NET logic
        try
        {
            if (!string.IsNullOrWhiteSpace(cvJson))
            {
                if (!CvDraftJson.TryDeserialize(cvJson, out var draft, out var error) || draft is null)
                {
                    return $"[ERROR] CV draft could not be parsed. {error}";
                }

                cvState.ApplyUpdate(draft);
            }

            var template = templateRegistry.Get(templateId);
            var pdfUrl = pdfGenerator.GeneratePdf(cvState.Draft, template.Id);
            cvState.SetGeneratedPdf(pdfUrl, Path.GetFileName(pdfUrl));

            return $"[SUCCESS] TechYuga CV generated. Download: {pdfUrl}";
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException is null
                ? $"{ex.GetType().Name}: {ex.Message}"
                : $"{ex.GetType().Name}: {ex.Message} | Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
            Console.WriteLine($"[PDF ERROR] {detail}");
            return $"[ERROR] PDF generation failed. {detail}";
        }
    }

    [Description("Calculates a project bid total based on labor, materials, and profit margin.")]
    public string CalculateBid(double laborCosts, double materialCosts, double marginPercent)
    {
        //Integrate with ML.NET for predictive analytics or use a simple formula for demonstration
        double total = (laborCosts + materialCosts) * (1 + (marginPercent / 100));
        return $"The calculated total for this TechYuga Bid is ${total:F2}.";
    }
}
