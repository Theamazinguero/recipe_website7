// File: DTOs/RecipeDtos.cs
using System.ComponentModel.DataAnnotations;
using RecipeApp.Api.Models;

namespace RecipeApp.Api.DTOs
{
    public class RecipeIngredientDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Quantity { get; set; }
        public string? Unit { get; set; }
    }

    public class RecipeStepDto
    {
        [Required]
        public int StepNumber { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;
    }

    public class RecipeCreateRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? ShortDescription { get; set; }

        public string? InstructionsSummary { get; set; }

        public string? ImageUrl { get; set; }

        public List<string> Tags { get; set; } = new();
        public List<RecipeIngredientDto> Ingredients { get; set; } = new();
        public List<RecipeStepDto> Steps { get; set; } = new();
    }

    public class RecipeResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDisabled { get; set; }
        public string CreatedByDisplayName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public IEnumerable<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
        public IEnumerable<RecipeStepDto> Steps { get; set; } = new List<RecipeStepDto>();
    }
}
