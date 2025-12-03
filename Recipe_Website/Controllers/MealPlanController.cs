using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Recipe_Website.Controllers
{
    // UI controller: serves the Razor page at /MealPlan
    [Authorize] // require login to access the Meal Plan UI
    public class MealPlanController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // This loads Views/MealPlan/Index.cshtml
            return View();
        }
    }
}
