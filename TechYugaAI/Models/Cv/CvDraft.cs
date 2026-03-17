namespace TechYugaAI.Models.Cv;

public sealed class CvDraft
{
    public string? FullName { get; set; }
    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? RoleTarget { get; set; }
    public string? YearsOfExperience { get; set; }

    public CvContact Contact { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<CvSkillGroup> SkillGroups { get; set; } = new();
    public List<CvExperience> Experiences { get; set; } = new();
    public List<CvEducation> Education { get; set; } = new();
    public List<CvProject> Projects { get; set; } = new();
    public List<CvCertification> Certifications { get; set; } = new();
    public List<string> AdditionalNotes { get; set; } = new();
}

public sealed class CvContact
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? LinkedIn { get; set; }
    public string? Portfolio { get; set; }
    public string? Website { get; set; }
}

public sealed class CvSkillGroup
{
    public string? Name { get; set; }
    public List<string> Items { get; set; } = new();
}

public sealed class CvExperience
{
    public string? Title { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<string> Highlights { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
}

public sealed class CvEducation
{
    public string? Degree { get; set; }
    public string? Institution { get; set; }
    public string? Location { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<string> Details { get; set; } = new();
}

public sealed class CvProject
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? Description { get; set; }
    public string? Link { get; set; }
    public List<string> Highlights { get; set; } = new();
    public List<string> Technologies { get; set; } = new();
}

public sealed class CvCertification
{
    public string? Name { get; set; }
    public string? Issuer { get; set; }
    public string? Date { get; set; }
}
