// File: Models/Setting.cs
using System.ComponentModel.DataAnnotations;

namespace RecipeApp.Api.Models
{
    public class Setting
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Value { get; set; }
    }
}
