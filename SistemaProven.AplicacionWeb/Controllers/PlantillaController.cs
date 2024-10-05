using Microsoft.AspNetCore.Mvc;

namespace SistemaProven.AplicacionWeb.Controllers
{
    public class PlantillaController : Controller
    {
        public IActionResult EnviarClave(string correo, string clave)
        {
            ViewData["Correo"] = correo;
            ViewData["Clave"] = clave;
            ViewData["url"] = $"{this.Request.Scheme}://{this.Request.Host}";

            return View();
        }
        
        public IActionResult RestablecerClave(string clave)
        {
            ViewData["Clave"] = clave;
            
            return View();
        }
    }
}
