using Microsoft.AspNetCore.Mvc;

namespace SistemaProven.AplicacionWeb.Controllers
{
    public class NegocioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
