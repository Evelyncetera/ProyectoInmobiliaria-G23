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

                ViewBag.IdReserva = reserva.IdReserva;
                return View();
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

        private int ObtenerIdUsuarioActual()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var idUsuario)
                ? idUsuario
                : throw new InvalidOperationException("La sesión no contiene un usuario válido.");
        }
    }

}