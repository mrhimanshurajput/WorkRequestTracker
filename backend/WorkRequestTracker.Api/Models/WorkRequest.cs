using System.ComponentModel.DataAnnotations;

namespace WorkRequestTracker.Api.Models;

public static class WorkRequestValues
{
    public static readonly string[] Priorities = ["Low", "Medium", "High"];
    public static readonly string[] Statuses = ["New", "InProgress", "Blocked", "Completed"];
}

public class WorkRequest
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string Priority { get; set; } = "Medium";

    [Required, MaxLength(20)]
    public string Status { get; set; } = "New";

    public DateTime DueDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public List<WorkRequestNote> Notes { get; set; } = [];
}

public class WorkRequestNote
{
    public int Id { get; set; }
    public int WorkRequestId { get; set; }
    [Required, MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public WorkRequest? WorkRequest { get; set; }
}
