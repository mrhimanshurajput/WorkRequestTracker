using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Data;
using WorkRequestTracker.Api.DTOs;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Controllers;

[ApiController]
[Route("api/work-requests")]
public class WorkRequestsController(WorkRequestDbContext db, ILogger<WorkRequestsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<WorkRequestDto>>> GetAll(
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(ApiError("page must be >= 1 and pageSize must be between 1 and 100.", "validation_error"));

        var query = db.WorkRequests.AsNoTracking().Include(x => x.Notes).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!WorkRequestValues.Statuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return BadRequest(ApiError("Invalid status.", "invalid_status"));
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Title.Contains(search) || x.ClientName.Contains(search));

        var total = await query.CountAsync();
        var entities = await query.OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var items = entities.Select(ToDto).ToList();

        return Ok(new PagedResult<WorkRequestDto>(items, page, pageSize, total));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkRequestDto>> GetById(int id)
    {
        var item = await db.WorkRequests.AsNoTracking().Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound(ApiError("Work request not found.", "not_found")) : Ok(ToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<WorkRequestDto>> Create(CreateWorkRequestDto dto)
    {
        if (!WorkRequestValues.Priorities.Contains(dto.Priority, StringComparer.OrdinalIgnoreCase))
            return BadRequest(ApiError("Priority must be Low, Medium, or High.", "invalid_priority"));
        if (!WorkRequestValues.Statuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(ApiError("Status must be New, InProgress, Blocked, or Completed.", "invalid_status"));
        if (dto.DueDate == default)
            return BadRequest(ApiError("DueDate is required.", "validation_error"));

        var now = DateTime.UtcNow;
        var item = new WorkRequest
        {
            Title = dto.Title.Trim(), ClientName = dto.ClientName.Trim(),
            Description = dto.Description?.Trim(), Priority = Normalize(dto.Priority),
            Status = Normalize(dto.Status), DueDate = dto.DueDate,
            CreatedDate = now, UpdatedDate = now
        };
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            item.Notes.Add(new WorkRequestNote { Text = dto.Notes.Trim(), CreatedDate = now });

        db.WorkRequests.Add(item);
        await db.SaveChangesAsync();
        var result = await db.WorkRequests.AsNoTracking().Include(x => x.Notes)
            .FirstAsync(x => x.Id == item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToDto(result));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<WorkRequestDto>> UpdateStatus(int id, UpdateStatusDto dto)
    {
        if (!WorkRequestValues.Statuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            return BadRequest(ApiError("Status must be New, InProgress, Blocked, or Completed.", "invalid_status"));

        var item = await db.WorkRequests.Include(x => x.Notes).FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound(ApiError("Work request not found.", "not_found"));

        item.Status = Normalize(dto.Status);
        item.UpdatedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ToDto(item));
    }

    [HttpPost("{id:int}/notes")]
    public async Task<ActionResult<WorkRequestNoteDto>> AddNote(int id, AddNoteDto dto)
    {
        var item = await db.WorkRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound(ApiError("Work request not found.", "not_found"));

        var note = new WorkRequestNote
        {
            WorkRequestId = id, Text = dto.Text.Trim(), CreatedDate = DateTime.UtcNow
        };
        db.WorkRequestNotes.Add(note);
        item.UpdatedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Created($"/api/work-requests/{id}", new WorkRequestNoteDto(note.Id, note.Text, note.CreatedDate));
    }

    private static string Normalize(string value) =>
        value.Equals("inprogress", StringComparison.OrdinalIgnoreCase) ? "InProgress" :
        char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();

    private static WorkRequestDto ToDto(WorkRequest x) =>
        new(x.Id, x.Title, x.ClientName, x.Description, x.Priority, x.Status, x.DueDate,
            x.CreatedDate, x.UpdatedDate,
            x.Notes.OrderByDescending(n => n.CreatedDate)
                .Select(n => new WorkRequestNoteDto(n.Id, n.Text, n.CreatedDate)).ToList());

    private static object ApiError(string message, string code) =>
        new { error = new { code, message, traceId = Guid.NewGuid().ToString("N") } };
}
