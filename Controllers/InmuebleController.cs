using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models;          

namespace Proyecto_Inmobiliaria.Controllers
{
    public class InmuebleController : Controller
    {
        private readonly IRepositorioInmueble _repositorio;

        private readonly ILogger<InmuebleController> _logger;

        // El framework inyecta automáticamente el repositorio configurado
        public InmuebleController(IRepositorioInmueble repositorio, ILogger<InmuebleController> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        // GET: /Inmuebles
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var lista = _repositorio.ObtenerTodos();
                return View(lista);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al recuperar los inmuebles");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<Inmueble>());
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar los inmuebles");
                TempData["Error"] = "No se pudo recuperar la lista";
                return View(new List<Inmueble>());
            }
        }

        // GET: /Inmuebles/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Inmuebles/Crear
        [HttpPost]
        [ValidateAntiForgeryToken] // Evita ataques CSRF
        public IActionResult Crear(Inmueble inmueble)
        {
            if (!ModelState.IsValid)
            {
                return View(inmueble);
            }

            try
            {
                _repositorio.Alta(inmueble);
                TempData["Mensaje"] = "Inmueble registrado con éxito.";
                return RedirectToAction(nameof(Index)); // Redirección tras guardar (POST-Redirect-GET)
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al guardar el inmueble");
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe un inmueble con esta dirección."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(inmueble);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar el inmueble");
                ViewBag.Error = "Ocurrió un error al guardar";
                return View(inmueble);
            }
        }

        // GET: /Inmuebles/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var inmueble = _repositorio.ObtenerPorId(id);
                if(inmueble == null)
                {
                    return NotFound();
                }
                return View(inmueble);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error en la base de datos al buscar el inmueble {IdInmueble}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el inmueble {IdInmueble}", id);
                TempData["Error"] = "Error al buscar inmueble";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Detalles(int id)
        {
            try
            {
                var inmueble = _repositorio.ObtenerPorId(id);
                return inmueble == null ? NotFound() : View(inmueble);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el inmueble {IdInmueble}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el inmueble {IdInmueble}", id);
                TempData["Error"] = "Error al buscar inmueble";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Inmuebles/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Inmueble inmueble)
        {
            if (id != inmueble.IdInmueble)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(inmueble);
            }

            try
            {
                _repositorio.Modificacion(inmueble);
                TempData["Mensaje"] = "Inmueble modificado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al modificar el inmueble {IdInmueble}", id);
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe un inmueble con esta dirección."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(inmueble);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al modificar el inmueble {IdInmueble}", id);
                ViewBag.Error = "Ocurrió un error al modificar el inmueble";
                return View(inmueble);
            }
        }

        // GET: /Inmuebles/Eliminar/5
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var inmueble = _repositorio.ObtenerPorId(id);
                if (inmueble == null)
                {
                    return NotFound();
                }
                return View(inmueble); // Retorna vista de confirmación
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el inmueble {IdInmueble} para eliminarlo", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el inmueble {IdInmueble} para eliminarlo", id);
                TempData["Error"] = "Error al buscar inmueble";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Inmuebles/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                _repositorio.Baja(id);
                TempData["Mensaje"] = "Inmueble eliminado correctamente.";
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar el inmueble {IdInmueble}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar el inmueble {IdInmueble}", id);
                TempData["Error"] = "No se pudo dar de baja al inmueble";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}