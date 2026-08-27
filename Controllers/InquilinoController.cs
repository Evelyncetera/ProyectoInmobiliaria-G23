using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models; 

namespace Proyecto_Inmobiliaria.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino _repositorio;
        private readonly ILogger<InquilinosController> _logger;

        public InquilinosController(IRepositorioInquilino repositorio, ILogger<InquilinosController> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }

        // GET: /Inquilinos
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
                _logger.LogError(ex, "Error de base de datos al recuperar los inquilinos");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<Inquilino>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar los inquilinos");
                TempData["Error"] = "No se pudo recuperar la lista";
                return View(new List<Inquilino>());
            }
        }

        // GET: /Inquilinos/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Inquilinos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Inquilino inquilino)
        {
            if (!ModelState.IsValid)
            {
                return View(inquilino);
            }

            try
            {
                _repositorio.Alta(inquilino);
                TempData["Mensaje"] = "Inquilino registrado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al registrar el inquilino");
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe un inquilino registrado con ese DNI."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(inquilino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al registrar el inquilino");
                ViewBag.Error = "Ocurrió un error al registrar";
                return View(inquilino);
            }
        }

        // GET: /Inquilinos/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var inquilino = _repositorio.ObtenerPorId(id);
                if (inquilino == null)
                {
                    return NotFound();
                }
                return View(inquilino);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el inquilino {IdInquilino}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el inquilino {IdInquilino}", id);
                TempData["Error"] = "Error al buscar inquilino";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Inquilinos/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Inquilino inquilino)
        {
            if (id != inquilino.IdInquilino)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(inquilino);
            }

            try
            {
                _repositorio.Modificacion(inquilino);
                TempData["Mensaje"] = "Datos del inquilino modificados con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al modificar el inquilino {IdInquilino}", id);
                ViewBag.Error = ex.Number == 1062
                    ? "Ya existe otro inquilino registrado con ese DNI."
                    : "Ocurrió un error de conexión a la base de datos";
                return View(inquilino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al modificar el inquilino {IdInquilino}", id);
                ViewBag.Error = "Ocurrió un error al guardar los cambios";
                return View(inquilino);
            }
        }

        // GET: /Inquilinos/Eliminar/5
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var inquilino = _repositorio.ObtenerPorId(id);
                if (inquilino == null)
                {
                    return NotFound();
                }
                return View(inquilino);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el inquilino {IdInquilino} para eliminarlo", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el inquilino {IdInquilino} para eliminarlo", id);
                TempData["Error"] = "Error al buscar inquilino";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Inquilinos/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                _repositorio.Baja(id);
                TempData["Mensaje"] = "Inquilino eliminado del sistema.";
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar el inquilino {IdInquilino}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar el inquilino {IdInquilino}", id);
                TempData["Error"] = "No se pudo dar de baja al inquilino";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}