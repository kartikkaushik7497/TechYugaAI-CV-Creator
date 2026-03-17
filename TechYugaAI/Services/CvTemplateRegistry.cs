namespace TechYugaAI.Services;

public sealed record CvTemplateDefinition(
    string Id,
    string Name,
    string Description,
    string AccentHex,
    string PreviewClass
);

public sealed class CvTemplateRegistry
{
    public const string ModernId = "modern";
    public const string MinimalId = "minimal";
    public const string ExecutiveId = "executive";
    public const string SlateId = "slate";
    public const string ZenId = "zen";
    public const string AtlasId = "atlas";

    private static readonly IReadOnlyList<CvTemplateDefinition> TemplateList =
    [
        new CvTemplateDefinition(
            ModernId,
            "Modern Split",
            "Two-column layout with a bold sidebar for skills and contact.",
            "#1f2937",
            "template-preview-modern"
        ),
        new CvTemplateDefinition(
            SlateId,
            "Slate Split",
            "Modern split layout with a refined navy palette.",
            "#1e3a8a",
            "template-preview-slate"
        ),
        new CvTemplateDefinition(
            MinimalId,
            "Minimal Clean",
            "Single-column layout focused on clarity and ATS readability.",
            "#0f766e",
            "template-preview-minimal"
        ),
        new CvTemplateDefinition(
            ZenId,
            "Zen Minimal",
            "Streamlined ATS layout with softer section accents.",
            "#047857",
            "template-preview-zen"
        ),
        new CvTemplateDefinition(
            ExecutiveId,
            "Executive",
            "Sophisticated header with structured experience emphasis.",
            "#4c1d95",
            "template-preview-executive"
        ),
        new CvTemplateDefinition(
            AtlasId,
            "Atlas Executive",
            "Traditional executive layout with rich accent tones.",
            "#7c2d12",
            "template-preview-atlas"
        )
    ];

    public IReadOnlyList<CvTemplateDefinition> Templates => TemplateList;

    public CvTemplateDefinition Get(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return TemplateList[0];
        }

        return TemplateList.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            ?? TemplateList[0];
    }
}
