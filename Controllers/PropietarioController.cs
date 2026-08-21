using Microsoft.AspNetCore.Mvc;
using Proyecto_Inmobiliaria.Models; 

namespace Proyecto_Inmobiliaria.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario _repositorio;

        // El framework inyecta automáticamente el repositorio configurado
        public PropietariosController(IRepositorioPropietario repositorio)
        {
            _repositorio = repositorio;
        }

       /*  // GET: /Propietarios
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var lista = _repositorio.ObtenerTodos();
                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo recuperar la lista: " + ex.Message;
                return View(new List<Propietario>());
            }
        }
 */
        // GET: /Propietarios/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Propietarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken] // Evita ataques CSRF
        public IActionResult Crear(Propietario propietario)
        {
            if (!ModelState.IsValid)
            {
                return View(propietario);
            }

            try
            {
                _repositorio.Alta(propietario);
                TempData["Mensaje"] = "Propietario registrado con éxito.";
                return RedirectToAction(nameof(Index)); // Redirección tras guardar (POST-Redirect-GET)
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al guardar: " + ex.Message;
                return View(propietario);
            }
        }

       /*  // GET: /Propietarios/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var propietario = _repositorio.ObtenerPorId(id);
                if (propietario == null)
                {
                    return NotFound();
                }
                return View(propietario);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al buscar propietario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
 */
        // POST: /Propietarios/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Propietario propietario)
        {
            if (id != propietario.IdPropietario)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(propietario);
            }

            try
            {
                _repositorio.Modificacion(propietario);
                TempData["Mensaje"] = "Propietario modificado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al modificar: " + ex.Message;
                return View(propietario);
            }
        }

       /*  // GET: /Propietarios/Eliminar/5
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var propietario = _repositorio.ObtenerPorId(id);
                if (propietario == null)
                {
                    return NotFound();
                }
                return View(propietario); // Retorna vista de confirmación
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al buscar propietario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        } */

        // POST: /Propietarios/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                _repositorio.Baja(id);
                TempData["Mensaje"] = "Propietario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo dar de baja al propietario: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}