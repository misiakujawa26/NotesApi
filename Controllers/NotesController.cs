using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using NotesApi.Models;

namespace NotesApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("notes")]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class NoteResponseDto
        {
            public int Id { get; set; }
            public string Content { get; set; } = string.Empty;
        }

        public class NoteCreateDto
        {
            public string Content { get; set; } = string.Empty;
        }

        private string CurrentUserId =>
            User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid")
            ?? User.FindFirstValue("sub")
            ?? string.Empty;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteResponseDto>>> GetNotes()
        {
            return await _context.Notes
                .Where(n => n.UserId == CurrentUserId)
                .Select(n => new NoteResponseDto
                {
                    Id = n.Id,
                    Content = n.Content
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteResponseDto>> GetNote(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
                return NotFound();

            if (note.UserId != CurrentUserId)
                return Forbid();

            return new NoteResponseDto
            {
                Id = note.Id,
                Content = note.Content
            };
        }

        [HttpPost]
        public async Task<ActionResult<NoteResponseDto>> CreateNote([FromBody] NoteCreateDto dto)
        {
            var note = new Note
            {
                Content = dto.Content,
                UserId = CurrentUserId
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var response = new NoteResponseDto
            {
                Id = note.Id,
                Content = note.Content
            };

            return CreatedAtAction(nameof(GetNote), new { id = note.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteCreateDto dto)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
                return NotFound();

            if (note.UserId != CurrentUserId)
                return Forbid();

            note.Content = dto.Content;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);

            if (note == null)
                return NotFound();

            if (note.UserId != CurrentUserId)
                return Forbid();

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}