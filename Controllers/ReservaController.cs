using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models;

namespace Proyecto_Inmobiliaria.Controllers
{
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva _repositorio;
        private readonly IRepositorioInquilino _repositorioInquilino;
        private readonly IRepositorioInmueble _repositorioInmueble;
        private readonly ILogger<ReservasController> _logger;


        public ReservasController(IRepositorioReserva repositorio,
                                  IRepositorioInquilino repositorioInquilino,
                                  IRepositorioInmueble repositorioInmueble,
                                  ILogger<ReservasController> logger)
        {
            _repositorio = repositorio;
            _repositorioInquilino = repositorioInquilino;
            _repositorioInmueble = repositorioInmueble;
            _logger = logger;
        }

      
        private void CargarSelectLists()
        {
            var inquilinos = _repositorioInquilino.ObtenerTodos();
            ViewBag.Inquilinos = new SelectList(
                inquilinos.Select(i => new { i.IdInquilino, Texto = $"{i.Nombre} {i.Apellido} ({i.Dni})" }),
                "IdInquilino", "Texto");

            var inmuebles = _repositorioInmueble.ObtenerTodos();
            ViewBag.Inmuebles = new SelectList(
                inmuebles.Select(m => new { m.IdInmueble, Texto = $"{m.Direccion} - {m.PrecioPorDia:C}/día" }),
                "IdInmueble", "Texto");
        }

        // GET: /Reservas
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
                _logger.LogError(ex, "Error de base de datos al recuperar las reservas");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<Reserva>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar las reservas");
                TempData["Error"] = "No se pudo recuperar la lista";
                return View(new List<Reserva>());
            }
        }

        // GET: /Reservas/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            try
            {
                CargarSelectLists();
                return View(new Reserva());
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al preparar el alta de una reserva");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al preparar el alta de una reserva");
                TempData["Error"] = "No se pudo cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken] // Evita ataques CSRF
        public IActionResult Crear(Reserva reserva)
        {
            // Control de fechas
            if (reserva.FechaHasta < reserva.FechaDesde)
            {
                ModelState.AddModelError("FechaHasta", "La fecha de finalización no puede ser anterior a la fecha de inicio.");
            }

            if (!ModelState.IsValid)
            {
                CargarSelectLists();
                return View(reserva);
            }

            try
            {
                // Volver a verificar que el inmueble no esté ocupado en esas fechas
                if (!_repositorio.EstaDisponible(reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta))
                {
                    CargarSelectLists();
                    ViewBag.Error = "El inmueble seleccionado ya está ocupado en esas fechas.";
                    return View(reserva);
                }

                _repositorio.Alta(reserva);
                TempData["Mensaje"] = "Reserva registrada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al guardar la reserva");
                ViewBag.Error = "Ocurrió un error de conexión a la base de datos";
                CargarSelectLists();
                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar la reserva");
                ViewBag.Error = "Ocurrió un error al guardar";
                CargarSelectLists();
                return View(reserva);
            }
        }

        // GET: /Reservas/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var reserva = _repositorio.ObtenerPorId(id);
                if (reserva == null)
                {
                    return NotFound();
                }
                CargarSelectLists();
                return View(reserva);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar la reserva {IdReserva}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar la reserva {IdReserva}", id);
                TempData["Error"] = "Error al buscar la reserva";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Detalles(int id)
        {
            try
            {
                var reserva = _repositorio.ObtenerPorId(id);
                return reserva == null ? NotFound() : View(reserva);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar la reserva {IdReserva}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar la reserva {IdReserva}", id);
                TempData["Error"] = "Error al buscar la reserva";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Reserva reserva)
        {
            if (id != reserva.IdReserva)
            {
                return BadRequest();
            }

            // Control de fechas
            if (reserva.FechaHasta < reserva.FechaDesde)
            {
                ModelState.AddModelError("FechaHasta", "La fecha de finalización no puede ser anterior a la fecha de inicio.");
            }

            if (!ModelState.IsValid)
            {
                CargarSelectLists();
                return View(reserva);
            }

            try
            {
                // Re-verificar disponibilidad excluyendo la propia reserva
                if (!_repositorio.EstaDisponible(reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta, id))
                {
                    CargarSelectLists();
                    ViewBag.Error = "El inmueble seleccionado ya está ocupado en esas fechas.";
                    return View(reserva);
                }

                _repositorio.Modificacion(reserva);
                TempData["Mensaje"] = "Reserva modificada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al modificar la reserva {IdReserva}", id);
                ViewBag.Error = "Ocurrió un error de conexión a la base de datos";
                CargarSelectLists();
                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al modificar la reserva {IdReserva}", id);
                ViewBag.Error = "Ocurrió un error al modificar la reserva";
                CargarSelectLists();
                return View(reserva);
            }
        }

        // GET: /Reservas/Eliminar/5
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var reserva = _repositorio.ObtenerPorId(id);
                if (reserva == null)
                {
                    return NotFound();
                }
                return View(reserva); // Retorna vista de confirmación
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al buscar la reserva {IdReserva} para anularla", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al buscar la reserva {IdReserva} para anularla", id);
                TempData["Error"] = "Error al buscar la reserva";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Eliminar/5 (Baja lógica: anula la reserva)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                _repositorio.Baja(id);
                TempData["Mensaje"] = "Reserva anulada correctamente.";
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al anular la reserva {IdReserva}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al anular la reserva {IdReserva}", id);
                TempData["Error"] = "No se pudo anular la reserva";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Reservas/Extender/5
        // Renueva/extiende una reserva: genera una NUEVA con el mismo inquilino e inmueble,
        // sin modificar la reserva original.
        [HttpGet]
        public IActionResult Extender(int id)
        {
            try
            {
                var original = _repositorio.ObtenerPorId(id);
                if (original == null)
                {
                    return NotFound();
                }

                var nueva = new Reserva
                {
                    IdInquilino = original.IdInquilino,
                    IdInmueble = original.IdInmueble,
                    FechaDesde = original.FechaHasta.AddDays(1),
                    FechaHasta = original.FechaHasta.AddDays(31),
                    MontoPorDia = original.MontoPorDia
                };

                CargarSelectLists();
                ViewBag.Original = original;
                return View(nueva);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al preparar la renovación de la reserva {IdReserva}", id);
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al preparar la renovación de la reserva {IdReserva}", id);
                TempData["Error"] = "No se pudo preparar la renovación";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Extender
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Extender(Reserva reserva)
        {
            if (reserva.FechaHasta < reserva.FechaDesde)
            {
                ModelState.AddModelError("FechaHasta", "La fecha de finalización no puede ser anterior a la fecha de inicio.");
            }

            if (!ModelState.IsValid)
            {
                CargarSelectLists();
                return View(reserva);
            }

            try
            {
                if (!_repositorio.EstaDisponible(reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta))
                {
                    CargarSelectLists();
                    ViewBag.Error = "El inmueble seleccionado ya está ocupado en esas fechas.";
                    return View(reserva);
                }

                _repositorio.Alta(reserva); // Genera una nueva reserva (la original queda intacta)
                TempData["Mensaje"] = "Reserva renovada/extendida con éxito. Se generó un nuevo alquiler.";
                return RedirectToAction(nameof(Index));
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al renovar la reserva");
                ViewBag.Error = "Ocurrió un error de conexión a la base de datos";
                CargarSelectLists();
                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al renovar la reserva");
                ViewBag.Error = "Ocurrió un error al renovar la reserva";
                CargarSelectLists();
                return View(reserva);
            }
        }

        /* ----- REPORTES ----- */

        // GET: /Reservas/ReporteMasReservados
        // Listar los inmuebles más reservados en los últimos 365 días
        [HttpGet]
        public IActionResult ReporteMasReservados()
        {
            try
            {
                var lista = _repositorio.ObtenerMasReservados();
                return View(lista);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al recuperar el reporte de inmuebles más reservados");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<InmuebleConReservas>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar el reporte de inmuebles más reservados");
                TempData["Error"] = "No se pudo recuperar el reporte";
                return View(new List<InmuebleConReservas>());
            }
        }

        // GET: /Reservas/ReporteSinReservas?dias=30
        // Listar inmuebles sin reservas en los últimos X días (30, 60, etc.)
        [HttpGet]
        public IActionResult ReporteSinReservas(int? dias)
        {
            int diasValidados = dias.HasValue && dias.Value > 0 ? dias.Value : 30;
            try
            {
                var lista = _repositorio.ObtenerInmueblesSinReservas(diasValidados);
                ViewBag.Dias = diasValidados;
                return View(lista);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al recuperar inmuebles sin reservas");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                ViewBag.Dias = diasValidados;
                return View(new List<Inmueble>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar inmuebles sin reservas");
                TempData["Error"] = "No se pudo recuperar el reporte";
                ViewBag.Dias = diasValidados;
                return View(new List<Inmueble>());
            }
        }

        // GET: /Reservas/ReporteVigentes
        // Listar reservas vigentes (hoy dentro de [desde, hasta])
        [HttpGet]
        public IActionResult ReporteVigentes()
        {
            try
            {
                var lista = _repositorio.ObtenerVigentes();
                return View(lista);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al recuperar reservas vigentes");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<Reserva>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar reservas vigentes");
                TempData["Error"] = "No se pudo recuperar el reporte";
                return View(new List<Reserva>());
            }
        }

        // GET: /Reservas/ReportePorTerminar?dias=7
        // Listar reservas que terminan en X días (plazo elegible)
        [HttpGet]
        public IActionResult ReportePorTerminar(int? dias)
        {
            int diasValidados = dias.HasValue && dias.Value > 0 ? dias.Value : 7;
            try
            {
                var lista = _repositorio.ObtenerPorTerminar(diasValidados);
                ViewBag.Dias = diasValidados;
                return View(lista);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al recuperar reservas por terminar");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                ViewBag.Dias = diasValidados;
                return View(new List<Reserva>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar reservas por terminar");
                TempData["Error"] = "No se pudo recuperar el reporte";
                ViewBag.Dias = diasValidados;
                return View(new List<Reserva>());
            }
        }

        // GET: /Reservas/ReporteDisponibles?desde=...&hasta=...
        // Dadas dos fechas, listar inmuebles no ocupados en alguna reserva entre esas fechas
        [HttpGet]
        public IActionResult ReporteDisponibles(DateTime? desde, DateTime? hasta)
        {
            // Si faltan fechas, solo muestra el formulario
            if (!desde.HasValue || !hasta.HasValue)
            {
                return View(new List<Inmueble>());
            }

            if (hasta < desde)
            {
                TempData["Error"] = "La fecha final no puede ser anterior a la fecha inicial.";
                return View(new List<Inmueble>());
            }

            try
            {
                ViewBag.Desde = desde.Value.ToString("yyyy-MM-dd");
                ViewBag.Hasta = hasta.Value.ToString("yyyy-MM-dd");
                var lista = _repositorio.ObtenerInmueblesDisponibles(desde.Value, hasta.Value);
                return View(lista);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al recuperar inmuebles disponibles");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return View(new List<Inmueble>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al recuperar inmuebles disponibles");
                TempData["Error"] = "No se pudo recuperar el reporte";
                return View(new List<Inmueble>());
            }
        }
    }
}