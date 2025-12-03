// File: Models/Comment.cs
using Recipe_Website.Models;
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Api.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;

        public bool IsHidden { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
