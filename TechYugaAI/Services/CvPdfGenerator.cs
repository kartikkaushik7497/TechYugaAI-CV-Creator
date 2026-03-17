using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TechYugaAI.Models.Cv;

namespace TechYugaAI.Services;

public sealed class CvPdfGenerator
{
    private readonly IWebHostEnvironment environment;
    private readonly CvTemplateRegistry templates;

    public CvPdfGenerator(IWebHostEnvironment environment, CvTemplateRegistry templates)
    {
        this.environment = environment;
        this.templates = templates;
    }

    public string GeneratePdf(CvDraft draft, string templateId)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var template = templates.Get(templateId);
        var outputDirectory = Path.Combine(environment.WebRootPath, "generated");
        Directory.CreateDirectory(outputDirectory);

        var safeName = SanitizeFilePart(draft.FullName) ?? "candidate";
        var fileName = $"cv-{safeName}-{Guid.NewGuid():N}.pdf";
        var filePath = Path.Combine(outputDirectory, fileName);

        IDocument document = template.Id switch
        {
            CvTemplateRegistry.MinimalId or CvTemplateRegistry.ZenId => new MinimalCvDocument(draft, template),
            CvTemplateRegistry.ExecutiveId or CvTemplateRegistry.AtlasId => new ExecutiveCvDocument(draft, template),
            _ => new ModernCvDocument(draft, template)
        };

        document.GeneratePdf(filePath);
        return $"/generated/{fileName}";
    }

    private static string? SanitizeFilePart(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var cleaned = new string(input.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned.Replace(' ', '-').ToLowerInvariant();
    }

    private abstract class CvDocumentBase : IDocument
    {
        protected readonly CvDraft Draft;
        protected readonly CvTemplateDefinition Template;

        protected CvDocumentBase(CvDraft draft, CvTemplateDefinition template)
        {
            Draft = draft;
            Template = template;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public abstract void Compose(IDocumentContainer container);

        protected static string Safe(string? value, string fallback = "")
            => string.IsNullOrWhiteSpace(value) ? fallback : value!;

        protected static bool HasItems<T>(ICollection<T>? items) => items is { Count: > 0 };

        protected static void SectionTitle(ColumnDescriptor column, string title, string accentHex)
        {
            column.Item().Text(title).FontSize(11).SemiBold().FontColor(accentHex);
            column.Item().LineHorizontal(1).LineColor(accentHex);
            column.Item().PaddingBottom(4);
        }

        protected static void BulletList(ColumnDescriptor column, IEnumerable<string> items, string? color = null)
        {
            foreach (var item in items.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                column.Item().Row(row =>
                {
                    row.ConstantItem(10).Text("-").FontColor(color ?? Colors.Grey.Darken2);
                    row.RelativeItem().Text(item).FontSize(9.5f);
                });
            }
        }

        protected static bool HasContactInfo(CvContact? contact)
        {
            return contact is not null
                && (!string.IsNullOrWhiteSpace(contact.Email)
                    || !string.IsNullOrWhiteSpace(contact.Phone)
                    || !string.IsNullOrWhiteSpace(contact.Location)
                    || !string.IsNullOrWhiteSpace(contact.LinkedIn)
                    || !string.IsNullOrWhiteSpace(contact.Portfolio)
                    || !string.IsNullOrWhiteSpace(contact.Website));
        }

        protected static IEnumerable<(string Label, string? Url)> BuildContactEntries(CvContact? contact)
        {
            if (contact is null)
            {
                yield break;
            }

            if (!string.IsNullOrWhiteSpace(contact.Email))
            {
                yield return (contact.Email, $"mailto:{contact.Email}");
            }
            if (!string.IsNullOrWhiteSpace(contact.Phone))
            {
                yield return (contact.Phone, $"tel:{NormalizePhone(contact.Phone)}");
            }
            if (!string.IsNullOrWhiteSpace(contact.Location))
            {
                yield return (contact.Location, null);
            }
            if (!string.IsNullOrWhiteSpace(contact.LinkedIn))
            {
                yield return (contact.LinkedIn, NormalizeUrl(contact.LinkedIn));
            }
            if (!string.IsNullOrWhiteSpace(contact.Portfolio))
            {
                yield return (contact.Portfolio, NormalizeUrl(contact.Portfolio));
            }
            if (!string.IsNullOrWhiteSpace(contact.Website))
            {
                yield return (contact.Website, NormalizeUrl(contact.Website));
            }
        }

        protected static string NormalizeUrl(string value)
        {
            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return $"https://{value}";
        }

        protected static string NormalizePhone(string value)
            => new string(value.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }

    private sealed class ModernCvDocument : CvDocumentBase
    {
        public ModernCvDocument(CvDraft draft, CvTemplateDefinition template) : base(draft, template) { }

        public override void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9.5f));
                page.Content().Row(row =>
                {
                    row.ConstantItem(190).Background(Template.AccentHex).Padding(16).Column(left =>
                    {
                        left.Spacing(8);
                        left.Item().Text(Safe(Draft.FullName, "Resume Draft")).FontSize(18).FontColor(Colors.White).SemiBold();
                        if (!string.IsNullOrWhiteSpace(Draft.Headline ?? Draft.RoleTarget))
                        {
                            left.Item().Text(Safe(Draft.Headline ?? Draft.RoleTarget)).FontColor(Colors.Grey.Lighten4);
                        }

                        if (HasContactInfo(Draft.Contact))
                        {
                            left.Item().PaddingTop(6).Text("Contact").FontColor(Colors.Grey.Lighten4).FontSize(10).SemiBold();
                            foreach (var entry in BuildContactEntries(Draft.Contact))
                            {
                                if (!string.IsNullOrWhiteSpace(entry.Url))
                                {
                                    var linkItem = left.Item();
                                    linkItem.Hyperlink(entry.Url);
                                    linkItem.Text(entry.Label).FontColor(Colors.White);
                                }
                                else
                                {
                                    left.Item().Text(entry.Label).FontColor(Colors.White);
                                }
                            }
                        }

                        if (HasItems(Draft.SkillGroups))
                        {
                            left.Item().PaddingTop(8).Text("Skills").FontColor(Colors.Grey.Lighten4).FontSize(10).SemiBold();
                            foreach (var group in Draft.SkillGroups.Where(g => !string.IsNullOrWhiteSpace(g.Name)))
                            {
                                left.Item().Text(Safe(group.Name)).FontColor(Colors.White).FontSize(9.5f).SemiBold();
                                left.Item().Text(string.Join(", ", group.Items)).FontColor(Colors.Grey.Lighten4);
                            }
                        }
                        else if (HasItems(Draft.Skills))
                        {
                            left.Item().PaddingTop(8).Text("Skills").FontColor(Colors.Grey.Lighten4).FontSize(10).SemiBold();
                            left.Item().Text(string.Join(", ", Draft.Skills)).FontColor(Colors.Grey.Lighten4);
                        }
                    });

                    row.RelativeItem().PaddingLeft(20).Column(right =>
                    {
                        right.Spacing(8);
                        if (!string.IsNullOrWhiteSpace(Draft.Summary))
                        {
                            SectionTitle(right, "Profile", Template.AccentHex);
                            right.Item().Text(Draft.Summary).FontSize(9.5f);
                        }

                        if (HasItems(Draft.Experiences))
                        {
                            SectionTitle(right, "Experience", Template.AccentHex);
                            foreach (var exp in Draft.Experiences)
                            {
                                right.Item().Text($"{Safe(exp.Title)} - {Safe(exp.Company)}").SemiBold();
                                right.Item().Text($"{Safe(exp.StartDate)} - {Safe(exp.EndDate, "Present")} - {Safe(exp.Location)}")
                                    .FontColor(Colors.Grey.Darken1).FontSize(9);
                                if (HasItems(exp.Highlights))
                                {
                                    BulletList(right, exp.Highlights);
                                }
                            }
                        }

                        if (HasItems(Draft.Projects))
                        {
                            SectionTitle(right, "Projects", Template.AccentHex);
                            foreach (var project in Draft.Projects)
                            {
                                right.Item().Text(Safe(project.Name)).SemiBold();
                                if (!string.IsNullOrWhiteSpace(project.Link))
                                {
                                    var linkItem = right.Item();
                                    linkItem.Hyperlink(NormalizeUrl(project.Link));
                                    linkItem.Text(project.Link).FontColor(Colors.Blue.Darken2).FontSize(9);
                                }
                                right.Item().Text(Safe(project.Description)).FontSize(9.5f);
                                if (HasItems(project.Highlights))
                                {
                                    BulletList(right, project.Highlights);
                                }
                            }
                        }

                        if (HasItems(Draft.Education))
                        {
                            SectionTitle(right, "Education", Template.AccentHex);
                            foreach (var edu in Draft.Education)
                            {
                                right.Item().Text($"{Safe(edu.Degree)} - {Safe(edu.Institution)}").SemiBold();
                                right.Item().Text($"{Safe(edu.StartDate)} - {Safe(edu.EndDate)} - {Safe(edu.Location)}")
                                    .FontColor(Colors.Grey.Darken1).FontSize(9);
                            }
                        }
                    });
                });
            });
        }
    }

    private sealed class MinimalCvDocument : CvDocumentBase
    {
        public MinimalCvDocument(CvDraft draft, CvTemplateDefinition template) : base(draft, template) { }

        public override void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontFamily("Calibri").FontSize(10));
                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text(Safe(Draft.FullName, "Resume Draft")).FontSize(22).SemiBold();
                    if (!string.IsNullOrWhiteSpace(Draft.Headline ?? Draft.RoleTarget))
                    {
                        column.Item().Text(Safe(Draft.Headline ?? Draft.RoleTarget)).FontColor(Colors.Grey.Darken1);
                    }
                    if (HasContactInfo(Draft.Contact))
                    {
                        var entries = BuildContactEntries(Draft.Contact).ToList();
                        if (entries.Count > 0)
                        {
                            column.Item().Row(row =>
                            {
                                for (var i = 0; i < entries.Count; i++)
                                {
                                    var entry = entries[i];
                                    if (!string.IsNullOrWhiteSpace(entry.Url))
                                    {
                                        var linkItem = row.AutoItem();
                                        linkItem.Hyperlink(entry.Url);
                                        linkItem.Text(entry.Label).FontColor(Colors.Grey.Darken2).FontSize(9.5f);
                                    }
                                    else
                                    {
                                        row.AutoItem().Text(entry.Label).FontColor(Colors.Grey.Darken2).FontSize(9.5f);
                                    }

                                    if (i < entries.Count - 1)
                                    {
                                        row.AutoItem().Text(" | ").FontColor(Colors.Grey.Darken2).FontSize(9.5f);
                                    }
                                }
                            });
                        }
                    }

                    column.Item().LineHorizontal(1).LineColor(Template.AccentHex);

                    if (!string.IsNullOrWhiteSpace(Draft.Summary))
                    {
                        SectionTitle(column, "Summary", Template.AccentHex);
                        column.Item().Text(Draft.Summary).FontSize(9.8f);
                    }

                    if (HasItems(Draft.Experiences))
                    {
                        SectionTitle(column, "Experience", Template.AccentHex);
                        foreach (var exp in Draft.Experiences)
                        {
                            column.Item().Text($"{Safe(exp.Title)} - {Safe(exp.Company)}").SemiBold();
                            column.Item().Text($"{Safe(exp.StartDate)} - {Safe(exp.EndDate, "Present")} - {Safe(exp.Location)}")
                                .FontColor(Colors.Grey.Darken1).FontSize(9);
                            if (HasItems(exp.Highlights))
                            {
                                BulletList(column, exp.Highlights);
                            }
                        }
                    }

                    if (HasItems(Draft.Projects))
                    {
                        SectionTitle(column, "Projects", Template.AccentHex);
                        foreach (var project in Draft.Projects)
                        {
                            column.Item().Text(Safe(project.Name)).SemiBold();
                            if (!string.IsNullOrWhiteSpace(project.Role))
                            {
                                column.Item().Text(project.Role).FontColor(Colors.Grey.Darken1).FontSize(9);
                            }
                            if (!string.IsNullOrWhiteSpace(project.Link))
                            {
                                var linkItem = column.Item();
                                linkItem.Hyperlink(NormalizeUrl(project.Link));
                                linkItem.Text(project.Link).FontColor(Colors.Blue.Darken2).FontSize(9);
                            }
                            column.Item().Text(Safe(project.Description)).FontSize(9.5f);
                        }
                    }

                    if (HasItems(Draft.SkillGroups) || HasItems(Draft.Skills))
                    {
                        SectionTitle(column, "Skills", Template.AccentHex);
                        if (HasItems(Draft.SkillGroups))
                        {
                            foreach (var group in Draft.SkillGroups)
                            {
                                var groupItems = string.Join(", ", group.Items);
                                var label = string.IsNullOrWhiteSpace(group.Name) ? groupItems : $"{group.Name}: {groupItems}";
                                column.Item().Text(label);
                            }
                        }
                        else
                        {
                            column.Item().Text(string.Join(", ", Draft.Skills));
                        }
                    }

                    if (HasItems(Draft.Education))
                    {
                        SectionTitle(column, "Education", Template.AccentHex);
                        foreach (var edu in Draft.Education)
                        {
                            column.Item().Text($"{Safe(edu.Degree)} - {Safe(edu.Institution)}").SemiBold();
                            column.Item().Text($"{Safe(edu.StartDate)} - {Safe(edu.EndDate)}").FontColor(Colors.Grey.Darken1).FontSize(9);
                        }
                    }
                });
            });
        }
    }

    private sealed class ExecutiveCvDocument : CvDocumentBase
    {
        public ExecutiveCvDocument(CvDraft draft, CvTemplateDefinition template) : base(draft, template) { }

        public override void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(9.5f));
                page.Content().Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(header =>
                        {
                            header.Item().Text(Safe(Draft.FullName, "Resume Draft")).FontSize(20).SemiBold();
                            header.Item().Text(Safe(Draft.Headline ?? Draft.RoleTarget)).FontColor(Colors.Grey.Darken1);
                        });
                        if (HasContactInfo(Draft.Contact))
                        {
                            row.ConstantItem(200).Column(contact =>
                            {
                                foreach (var entry in BuildContactEntries(Draft.Contact))
                                {
                                    if (!string.IsNullOrWhiteSpace(entry.Url))
                                    {
                                        var linkItem = contact.Item();
                                        linkItem.Hyperlink(entry.Url);
                                        linkItem.Text(entry.Label).FontSize(9.2f);
                                    }
                                    else
                                    {
                                        contact.Item().Text(entry.Label).FontSize(9.2f);
                                    }
                                }
                            });
                        }
                    });

                    column.Item().LineHorizontal(1).LineColor(Template.AccentHex);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(6);
                            if (!string.IsNullOrWhiteSpace(Draft.Summary))
                            {
                                SectionTitle(left, "Executive Summary", Template.AccentHex);
                                left.Item().Text(Draft.Summary).FontSize(9.5f);
                            }

                            if (HasItems(Draft.SkillGroups) || HasItems(Draft.Skills))
                            {
                                SectionTitle(left, "Core Skills", Template.AccentHex);
                                if (HasItems(Draft.SkillGroups))
                                {
                                    foreach (var group in Draft.SkillGroups)
                                    {
                                        var groupItems = string.Join(", ", group.Items);
                                        var label = string.IsNullOrWhiteSpace(group.Name) ? groupItems : $"{group.Name}: {groupItems}";
                                        left.Item().Text(label);
                                    }
                                }
                                else
                                {
                                    left.Item().Text(string.Join(", ", Draft.Skills));
                                }
                            }

                            if (HasItems(Draft.Certifications))
                            {
                                SectionTitle(left, "Certifications", Template.AccentHex);
                                foreach (var cert in Draft.Certifications)
                                {
                                    left.Item().Text($"{Safe(cert.Name)} - {Safe(cert.Issuer)}").FontSize(9.5f);
                                }
                            }

                            if (HasItems(Draft.Projects))
                            {
                                SectionTitle(left, "Projects", Template.AccentHex);
                                foreach (var project in Draft.Projects)
                                {
                                    left.Item().Text(Safe(project.Name)).SemiBold();
                                    if (!string.IsNullOrWhiteSpace(project.Link))
                                    {
                                        var linkItem = left.Item();
                                        linkItem.Hyperlink(NormalizeUrl(project.Link));
                                        linkItem.Text(project.Link).FontColor(Colors.Blue.Darken2).FontSize(9);
                                    }
                                    left.Item().Text(Safe(project.Description)).FontSize(9.5f);
                                }
                            }
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Spacing(6);
                            if (HasItems(Draft.Experiences))
                            {
                                SectionTitle(right, "Experience", Template.AccentHex);
                                foreach (var exp in Draft.Experiences)
                                {
                                    right.Item().Text($"{Safe(exp.Title)} - {Safe(exp.Company)}").SemiBold();
                                    right.Item().Text($"{Safe(exp.StartDate)} - {Safe(exp.EndDate, "Present")}").FontColor(Colors.Grey.Darken1).FontSize(9);
                                    if (HasItems(exp.Highlights))
                                    {
                                        BulletList(right, exp.Highlights);
                                    }
                                }
                            }

                            if (HasItems(Draft.Education))
                            {
                                SectionTitle(right, "Education", Template.AccentHex);
                                foreach (var edu in Draft.Education)
                                {
                                    right.Item().Text($"{Safe(edu.Degree)} - {Safe(edu.Institution)}").SemiBold();
                                    right.Item().Text($"{Safe(edu.StartDate)} - {Safe(edu.EndDate)}").FontColor(Colors.Grey.Darken1).FontSize(9);
                                }
                            }
                        });
                    });
                });
            });
        }
    }
}
