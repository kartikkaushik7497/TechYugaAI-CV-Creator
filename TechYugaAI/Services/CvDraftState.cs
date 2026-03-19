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

        draft.FullName = MergeString(draft.FullName, update.FullName);
        draft.Headline = MergeString(draft.Headline, update.Headline);
        draft.Summary = MergeString(draft.Summary, update.Summary);
        draft.RoleTarget = MergeString(draft.RoleTarget, update.RoleTarget);
        draft.YearsOfExperience = MergeString(draft.YearsOfExperience, update.YearsOfExperience);

        if (update.Contact is not null)
        {
            draft.Contact ??= new CvContact();
            draft.Contact.Email = MergeString(draft.Contact.Email, update.Contact.Email);
            draft.Contact.Phone = MergeString(draft.Contact.Phone, update.Contact.Phone);
            draft.Contact.Location = MergeString(draft.Contact.Location, update.Contact.Location);
            draft.Contact.LinkedIn = MergeString(draft.Contact.LinkedIn, update.Contact.LinkedIn);
            draft.Contact.Portfolio = MergeString(draft.Contact.Portfolio, update.Contact.Portfolio);
            draft.Contact.Website = MergeString(draft.Contact.Website, update.Contact.Website);
        }

        if (update.Skills is not null)
        {
            draft.Skills = update.Skills;
        }

        if (update.SkillGroups is not null)
        {
            draft.SkillGroups = update.SkillGroups;
        }

        if (update.Experiences is not null)
        {
            draft.Experiences = update.Experiences;
        }

        if (update.Education is not null)
        {
            draft.Education = update.Education;
        }

        if (update.Projects is not null)
        {
            draft.Projects = update.Projects;
        }

        if (update.Certifications is not null)
        {
            draft.Certifications = update.Certifications;
        }

        if (update.AdditionalNotes is not null)
        {
            draft.AdditionalNotes = update.AdditionalNotes;
        }

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

    private static string? MergeString(string? target, string? source)
        => source is null ? target : source;
}
