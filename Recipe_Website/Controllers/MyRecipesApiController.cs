using Microsoft.AspNetCore.Mvc;

namespace Recipe_Website.Controllers
{
    public class MyRecipesApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
