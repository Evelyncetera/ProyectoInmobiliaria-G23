using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models; 

namespace Proyecto_Inmobiliaria.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario _repositorio;
        private readonly ILogger<PropietariosController> _logger;

        // El framework inyecta automáticamente el repositorio configurado
        public PropietariosController(IRepositorioPropietario repositorio, ILogger<PropietariosController> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        // GET: /Propietarios
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
                _logger.LogError(ex, "Error de base de datos al recuperar los propietarios");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<Propietario>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar los propietarios");
                TempData["Error"] = "No se pudo recuperar la lista";
                return View(new List<Propietario>());
            }
        }

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
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al guardar el propietario");
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe un propietario registrado con ese DNI."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(propietario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar el propietario");
                ViewBag.Error = "Ocurrió un error al guardar";
                return View(propietario);
            }
        }

        // GET: /Propietarios/Editar/5
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
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el propietario {IdPropietario}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el propietario {IdPropietario}", id);
                TempData["Error"] = "Error al buscar propietario";
                return RedirectToAction(nameof(Index));
            }
        }

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
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al modificar el propietario {IdPropietario}", id);
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe otro propietario registrado con ese DNI."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(propietario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al modificar el propietario {IdPropietario}", id);
                ViewBag.Error = "Ocurrió un error al modificar";
                return View(propietario);
            }
        }

        // GET: /Propietarios/Eliminar/5
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
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el propietario {IdPropietario} para eliminarlo", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el propietario {IdPropietario} para eliminarlo", id);
                TempData["Error"] = "Error al buscar propietario";
                return RedirectToAction(nameof(Index));
            }
        }

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
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar el propietario {IdPropietario}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar el propietario {IdPropietario}", id);
                TempData["Error"] = "No se pudo dar de baja al propietario";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}