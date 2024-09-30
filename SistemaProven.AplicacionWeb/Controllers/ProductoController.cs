using Microsoft.AspNetCore.Mvc;

namespace SistemaProven.AplicacionWeb.Controllers
{
    public class ProductoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
