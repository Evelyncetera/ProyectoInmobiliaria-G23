using Microsoft.AspNetCore.Mvc;
using Proyecto_Inmobiliaria.Models; 

namespace Proyecto_Inmobiliaria.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino _repositorio;

        public InquilinosController(IRepositorioInquilino repositorio)
        {
            _repositorio = repositorio;
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
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo recuperar la lista: " + ex.Message;
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
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al registrar: " + ex.Message;
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
            catch (Exception ex)
            {
                TempData["Error"] = "Error al buscar inquilino: " + ex.Message;
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
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al guardar los cambios: " + ex.Message;
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
            catch (Exception ex)
            {
                TempData["Error"] = "Error al buscar inquilino: " + ex.Message;
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
            catch (Exception ex)
            {
                TempData["Error"] = "No se pudo dar de baja al inquilino: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}