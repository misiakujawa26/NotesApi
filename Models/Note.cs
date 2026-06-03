using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NotesApi.Models
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
    }
}