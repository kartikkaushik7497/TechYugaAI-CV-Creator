using TechYugaAI.Models.Cv;

namespace TechYugaAI.Services;

public sealed class CvDraftState
{
    private CvDraft draft = new();

    public CvDraft Draft => draft;
    public string SelectedTemplateId { get; private set; } = CvTemplateRegistry.ModernId;
    public string? LastGeneratedPdfUrl { get; private set; }
    public string? LastGeneratedPdfFileName { get; private set; }
    public DateTimeOffset? LastGeneratedAt { get; private set; }

    public event Action? OnChange;

    public void Reset()
    {
        draft = new CvDraft();
        LastGeneratedPdfUrl = null;
        LastGeneratedPdfFileName = null;
        LastGeneratedAt = null;
        NotifyChanged();
    }

    public void SetTemplate(string templateId)
    {
        if (string.IsNullOrWhiteSpace(templateId))
        {
            return;
        }

        SelectedTemplateId = templateId;
        NotifyChanged();
    }

    public void ApplyUpdate(CvDraft update)
    {
        if (update is null)
        {
            return;
        }

        var clearSections = ExtractClearSections(update);

        draft.FullName = MergeString(draft.FullName, update.FullName, clearSections.Contains("fullname"));
        draft.Headline = MergeString(draft.Headline, update.Headline, clearSections.Contains("headline"));
        draft.Summary = MergeString(draft.Summary, update.Summary, clearSections.Contains("summary"));
        draft.RoleTarget = MergeString(draft.RoleTarget, update.RoleTarget, clearSections.Contains("roletarget"));
        draft.YearsOfExperience = MergeString(draft.YearsOfExperience, update.YearsOfExperience, clearSections.Contains("yearsofexperience"));

        if (update.Contact is not null)
        {
            draft.Contact ??= new CvContact();
            var clearContact = clearSections.Contains("contact");
            draft.Contact.Email = MergeString(draft.Contact.Email, update.Contact.Email, clearContact || clearSections.Contains("contact.email"));
            draft.Contact.Phone = MergeString(draft.Contact.Phone, update.Contact.Phone, clearContact || clearSections.Contains("contact.phone"));
            draft.Contact.Location = MergeString(draft.Contact.Location, update.Contact.Location, clearContact || clearSections.Contains("contact.location"));
            draft.Contact.LinkedIn = MergeString(draft.Contact.LinkedIn, update.Contact.LinkedIn, clearContact || clearSections.Contains("contact.linkedin"));
            draft.Contact.Portfolio = MergeString(draft.Contact.Portfolio, update.Contact.Portfolio, clearContact || clearSections.Contains("contact.portfolio"));
            draft.Contact.Website = MergeString(draft.Contact.Website, update.Contact.Website, clearContact || clearSections.Contains("contact.website"));
        }

        draft.Skills = MergeList(draft.Skills, update.Skills, clearSections.Contains("skills"));
        draft.SkillGroups = MergeList(draft.SkillGroups, update.SkillGroups, clearSections.Contains("skillgroups"));
        draft.Experiences = MergeList(draft.Experiences, update.Experiences, clearSections.Contains("experiences"));
        draft.Education = MergeList(draft.Education, update.Education, clearSections.Contains("education"));
        draft.Projects = MergeList(draft.Projects, update.Projects, clearSections.Contains("projects"));
        draft.Certifications = MergeList(draft.Certifications, update.Certifications, clearSections.Contains("certifications"));
        draft.AdditionalNotes = MergeList(draft.AdditionalNotes, StripClearTokens(update.AdditionalNotes), clearSections.Contains("additionalnotes"));

        ClearGeneratedPdf();
        NotifyChanged();
    }

    public void SetGeneratedPdf(string url, string fileName)
    {
        LastGeneratedPdfUrl = url;
        LastGeneratedPdfFileName = fileName;
        LastGeneratedAt = DateTimeOffset.UtcNow;
        NotifyChanged();
    }

    private void ClearGeneratedPdf()
    {
        LastGeneratedPdfUrl = null;
        LastGeneratedPdfFileName = null;
        LastGeneratedAt = null;
    }

    private void NotifyChanged() => OnChange?.Invoke();

    private static string? MergeString(string? target, string? source, bool clearRequested)
    {
        if (clearRequested)
        {
            return string.Empty;
        }

        if (source is null)
        {
            return target;
        }

        if (!string.IsNullOrWhiteSpace(target) && string.IsNullOrWhiteSpace(source))
        {
            return target;
        }

        if (!string.IsNullOrWhiteSpace(target) && IsPlaceholderLike(source))
        {
            return target;
        }

        return source;
    }

    private static List<T> MergeList<T>(List<T> current, List<T>? update, bool clearRequested)
    {
        if (clearRequested)
        {
            return new List<T>();
        }

        if (update is null)
        {
            return current;
        }

        if (update.Count == 0 && current.Count > 0)
        {
            return current;
        }

        return update;
    }

    private static HashSet<string> ExtractClearSections(CvDraft update)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (update.AdditionalNotes is null)
        {
            return set;
        }

        foreach (var note in update.AdditionalNotes)
        {
            if (note is null)
            {
                continue;
            }

            var trimmed = note.Trim();
            if (trimmed.StartsWith("__clear__:", StringComparison.OrdinalIgnoreCase))
            {
                var section = trimmed.Substring("__clear__:".Length).Trim();
                if (!string.IsNullOrWhiteSpace(section))
                {
                    set.Add(section);
                }
            }
        }

        return set;
    }

    private static List<string> StripClearTokens(List<string>? notes)
    {
        if (notes is null)
        {
            return new List<string>();
        }

        return notes
            .Where(n => n is not null && !n.Trim().StartsWith("__clear__:", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static bool IsPlaceholderLike(string value)
    {
        return value.Contains('[') && value.Contains(']');
    }
}
