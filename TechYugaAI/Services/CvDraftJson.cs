using System.Text.Json;
using TechYugaAI.Models.Cv;

namespace TechYugaAI.Services;

public static class CvDraftJson
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static bool TryDeserialize(string json, out CvDraft? draft, out string? error)
    {
        draft = null;
        error = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "Draft JSON was empty.";
            return false;
        }

        try
        {
            draft = JsonSerializer.Deserialize<CvDraft>(json, SerializerOptions);
            if (draft is null)
            {
                error = "Draft JSON could not be parsed.";
                return false;
            }

            return true;
        }
        catch (JsonException ex)
        {
            error = $"Draft JSON error: {ex.Message}";
            return false;
        }
    }
}
