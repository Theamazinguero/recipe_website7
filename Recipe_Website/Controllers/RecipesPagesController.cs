using Microsoft.AspNetCore.Mvc;

namespace Recipe_Website.Controllers
{
    // All routes under /Recipes
    [Route("Recipes")]
    public class RecipesPagesController : Controller
    {
        // GET /Recipes
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Views/Recipes/Index.cshtml");
        }

        // GET /Recipes/Search
        [HttpGet("Search")]
        public IActionResult Search()
        {
            return View("~/Views/Recipes/Search.cshtml");
        }

        // GET /Recipes/Create
        // No [Authorize] because the frontend uses JWT stored in localStorage.
        // The API handles authorization; MVC pages stay public.
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Recipes/Create.cshtml");
        }

        // GET /Recipes/Details/5
        [HttpGet("Details/{id:int}")]
        public IActionResult Details(int id)
        {
            // Details.cshtml uses: @model int
            return View("~/Views/Recipes/Details.cshtml", id);
        }

        // GET /Recipes/Edit/5
        [HttpGet("Edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            // Edit.cshtml uses: @model int
            return View("~/Views/Recipes/Edit.cshtml", id);
        }

        // GET /Recipes/Favorites
        [HttpGet("Favorites")]
        public IActionResult Favorites()
        {
            return View("~/Views/Recipes/Favorites.cshtml");
        }

        // GET /Recipes/MyRecipes
        [HttpGet("MyRecipes")]
        public IActionResult MyRecipes()
        {
            return View("~/Views/Recipes/MyRecipes.cshtml");
        }
    }
}
