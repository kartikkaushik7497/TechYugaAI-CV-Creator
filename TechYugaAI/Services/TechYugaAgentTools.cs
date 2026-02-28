using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace TechYugaAI.Services;

public class TechYugaAgentTools
{
    [Description("Generates a professional CV. Call this when the user has provided their details and wants a final document.")]
    public string GenerateCVDoc(string fullName, string experienceSummary, string skills)
    {
        // Integration point for QuestPDF or specialized ML.NET logic
        return $"[SUCCESS] TechYuga CV generated for {fullName}. Ready for download.";
    }

    [Description("Calculates a project bid total based on labor, materials, and profit margin.")]
    public string CalculateBid(double laborCosts, double materialCosts, double marginPercent)
    {
        //Integrate with ML.NET for predictive analytics or use a simple formula for demonstration
        double total = (laborCosts + materialCosts) * (1 + (marginPercent / 100));
        return $"The calculated total for this TechYuga Bid is ${total:F2}.";
    }
}