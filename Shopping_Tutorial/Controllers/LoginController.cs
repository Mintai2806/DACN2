using Microsoft.AspNetCore.Mvc;

namespace Shopping_Tutorial.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
