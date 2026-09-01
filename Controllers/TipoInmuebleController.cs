using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models;

namespace Proyecto_Inmobiliaria.Controllers
{
    public class TipoInmuebleController : Controller
    {
        private readonly IRepositorioTipoInmueble _repositorio;

        private readonly ILogger<TipoInmuebleController> _logger;

        // El framework inyecta automáticamente el repositorio configurado
        public TipoInmuebleController(IRepositorioTipoInmueble repositorio, ILogger<TipoInmuebleController> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        // GET: /TipoInmuebles
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
                _logger.LogError(ex, "Error de base de datos al recuperar los inmuebles de este tipo");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<TipoInmueble>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar los inmuebles de este tipo");
                TempData["Error"] = "No se pudo recuperar la lista";
                return View(new List<TipoInmueble>());
            }
        }

        // GET: /TipoInmuebles/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /TipoInmuebles/Crear
        [HttpPost]
        [ValidateAntiForgeryToken] // Evita ataques CSRF
        public IActionResult Crear(TipoInmueble tipoInmueble)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoInmueble);
            }

            try
            {
                _repositorio.Alta(tipoInmueble);
                TempData["Mensaje"] = "Tipo de Inmueble registrado con éxito.";
                return RedirectToAction(nameof(Index)); // Redirección tras guardar (POST-Redirect-GET)
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al guardar el tipo de inmueble");
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe un tipo de inmueble con este nombre."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(tipoInmueble);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar el tipo de inmueble");
                ViewBag.Error = "Ocurrió un error al guardar";
                return View(tipoInmueble);
            }
        }

        // GET: /TipoInmuebles/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var tipoInmueble = _repositorio.ObtenerPorId(id);
                if (tipoInmueble == null)
                {
                    return NotFound();
                }
                return View(tipoInmueble);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error en la base de datos al buscar el tipo de inmueble {IdTipoInmueble}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el tipo de inmueble {IdTipoInmueble}", id);
                TempData["Error"] = "Error al buscar tipo de inmueble";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /TipoInmuebles/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, TipoInmueble tipoInmueble)
        {
            if (id != tipoInmueble.IdTipoInmueble)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(tipoInmueble);
            }

            try
            {
                _repositorio.Modificacion(tipoInmueble);
                TempData["Mensaje"] = "Tipo de Inmueble modificado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al modificar el tipo de inmueble {IdTipoInmueble}", id);
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe un tipo de inmueble con este nombre."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(tipoInmueble);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al modificar el tipo de inmueble {IdTipoInmueble}", id);
                ViewBag.Error = "Ocurrió un error al modificar el tipo de inmueble";
                return View(tipoInmueble);
            }
        }

        // GET: /TipoInmuebles/Eliminar/5
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var tipoInmueble = _repositorio.ObtenerPorId(id);
                if (tipoInmueble == null)
                {
                    return NotFound();
                }
                return View(tipoInmueble); // Retorna vista de confirmación
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el tipo de inmueble {IdTipoInmueble} para eliminarlo", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el tipo de inmueble {IdTipoInmueble} para eliminarlo", id);
                TempData["Error"] = "Error al buscar tipo de inmueble";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /TipoInmuebles/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                _repositorio.Baja(id);
                TempData["Mensaje"] = "Tipo de Inmueble eliminado correctamente.";
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar el tipo de inmueble {IdTipoInmueble}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar el tipo de inmueble {IdTipoInmueble}", id);
                TempData["Error"] = "No se pudo dar de baja al tipo de inmueble";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}