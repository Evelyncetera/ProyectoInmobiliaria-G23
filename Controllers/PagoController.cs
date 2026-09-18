using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models;
using System.Security.Claims;

namespace Proyecto_Inmobiliaria.Controllers
{
    [Authorize]
    public class PagoController : Controller
    {
        private readonly IRepositorioPago _repositorio;
        private readonly IRepositorioReserva _repositorioReserva;

        private readonly ILogger<PagoController> _logger;

        public PagoController(IRepositorioPago repositorio, IRepositorioReserva repositorioReserva, ILogger<PagoController> logger)
        {
            _repositorio = repositorio;
            _repositorioReserva = repositorioReserva;
            _logger = logger;
        }
        // GET: /Pago/Crear/5
        // Genera o carga un nuevo pago para una reserva de alquiler
        // insertando concepto de pago, la fecha de pago y el importe
        [HttpGet]
        public IActionResult Crear(int id)
        {
            try
            {
                var reserva = _repositorioReserva.ObtenerPorId(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                var pago = new Pago
                {
                    IdReserva = reserva.IdReserva,
                    FechaPago = DateTime.Today,
                    Anulada = false
                };

                return View(pago);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al cargar el formulario de pago");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction("Detalles", "Reservas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar el formulario de pago");
                TempData["Error"] = "No se pudo cargar el formulario";
                return RedirectToAction("Detalles", "Reservas");
            }
        }

        [HttpPost]
        public IActionResult Crear(Pago pago)
        {

            if (!ModelState.IsValid)
            {
                return View(pago);
            }

            try
            {
                _repositorio.Alta(pago, ObtenerIdUsuarioActual());
                TempData["Mensaje"] = "Pago registrado con éxito.";
                return RedirectToAction("Detalles", "Reservas", new { id = pago.IdReserva });
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al guardar el pago");
                ViewBag.Error = "Ocurrió un error de conexión a la base de datos";
                return View(pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar el pago");
                ViewBag.Error = "Ocurrió un error al guardar";
                return View(pago);
            }
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            Pago pago = null;
            try
            {
                pago = _repositorio.ObtenerPorId(id);
                if (pago == null)
                {
                    return NotFound();
                }
                return View(pago);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el pago {IdPago}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction("Detalles", "Reservas", new { id = pago.IdReserva});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el pago {IdPago}", id);
                TempData["Error"] = "Error al buscar el pago";
                return RedirectToAction("Detalles", "Reservas", new { id = pago.IdReserva});
            }
        }

        [HttpPost]
        public IActionResult Editar(int id, Pago pago)
        {
            if (id != pago.IdPago)
            {
                return BadRequest();
            }

            try
            {
                _repositorio.Modificacion(pago);
                TempData["Mensaje"] = "Pago modificado con éxito.";
                return RedirectToAction("Detalles", "Reservas", new {id = pago.IdReserva});
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al modificar el pago {IdPago}", id);
                ViewBag.Error = "Ocurrió un error de conexión a la base de datos";
                return View(pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al modificar el pago {IdPago}", id);
                ViewBag.Error = "Ocurrió un error al modificar el pago";
                return View(pago);
            }
        }

        // GET: /Pago/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            Pago pago = null;
            try
            {
                pago = _repositorio.ObtenerPorId(id);
                if (pago == null)
                {
                    return NotFound();
                }
                return View(pago); // Retorna vista de confirmación
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar el pago {IdPago} para anularla", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction("Detalles", "Reservas", new { id = pago.IdReserva });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar el pago {IdPago} para anularla", id);
                TempData["Error"] = "Error al buscar la reserva";
                return RedirectToAction("Detalles", "Reservas", new { id = pago.IdReserva });
            }
        }

        // POST: /Pago/Eliminar/5 (Baja lógica: anula el pago)
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var pago = _repositorio.ObtenerPorId(id);
            try
            {
                _repositorio.Baja(id, ObtenerIdUsuarioActual());
                TempData["Mensaje"] = "Pago anulado correctamente.";
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al anular el pago {IdPago}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al anular el pago {IdPago}", id);
                TempData["Error"] = "No se pudo anular el pago";
            }
            return RedirectToAction("Detalles", "Reservas", new {id = pago.IdReserva});
        }

        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var idUsuario)
                ? idUsuario
                : throw new InvalidOperationException("La sesión no contiene un usuario válido.");
        }
    }

}