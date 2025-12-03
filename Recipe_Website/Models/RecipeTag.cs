// File: Models/RecipeTag.cs
using Recipe_Website.Models;

namespace RecipeApp.Api.Models
{
    public class RecipeTag
    {
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}
