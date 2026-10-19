namespace incidenskezelo_backend.Models.Incident;

public static class IncidentValidator
{
    public static string? TitleError(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Title is required.";
        }

        return title.Length > IncidentLimits.TitleMaxLength
            ? $"Title must be at most {IncidentLimits.TitleMaxLength} characters."
            : null;
    }

    public static string? DescriptionError(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Description is required.";
        }

        return description.Length > IncidentLimits.DescriptionMaxLength
            ? $"Description must be at most {IncidentLimits.DescriptionMaxLength} characters."
            : null;
    }
}
