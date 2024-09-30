using Microsoft.AspNetCore.Mvc;

namespace SistemaProven.AplicacionWeb.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
