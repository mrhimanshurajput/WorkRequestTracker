using System.ComponentModel.DataAnnotations;

namespace WorkRequestTracker.Api.DTOs;

public class CreateWorkRequestDto
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 1)]
    public string ClientName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public string Priority { get; set; } = "Medium";

    [Required]
    public string Status { get; set; } = "New";

    public DateTime DueDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdateStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class AddNoteDto
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;
}

public record WorkRequestNoteDto(int Id, string Text, DateTime CreatedDate);

public record WorkRequestDto(
    int Id, string Title, string ClientName, string? Description,
    string Priority, string Status, DateTime DueDate, DateTime CreatedDate,
    DateTime UpdatedDate, IReadOnlyList<WorkRequestNoteDto> Notes);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
