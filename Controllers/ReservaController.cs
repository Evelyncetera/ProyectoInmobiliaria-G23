using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models;
using System.Security.Claims;

namespace Proyecto_Inmobiliaria.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva _repositorioReserva;
        private readonly IRepositorioInquilino _repositorioInquilino;
        private readonly IRepositorioInmueble _repositorioInmueble;
        private readonly IRepositorioPago _repositorioPago;
        private readonly ILogger<ReservasController> _logger;


        public ReservasController(IRepositorioReserva repositorio,
                                    IRepositorioInquilino repositorioInquilino,
                                    IRepositorioInmueble repositorioInmueble,
                                    IRepositorioPago repositorioPago,
                                    ILogger<ReservasController> logger)
        {
            _repositorioReserva = repositorio;
            _repositorioInquilino = repositorioInquilino;
            _repositorioInmueble = repositorioInmueble;
            _repositorioPago = repositorioPago;
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
                var lista = _repositorioReserva.ObtenerTodos();
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

                var reserva = new Reserva
                {
                    FechaDesde = DateTime.Today,
                    FechaHasta = DateTime.Today.AddDays(1)
                };

                return View(reserva);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al cargar el formulario de reserva");
                TempData["Error"] = "Ocurrió un error de conexión a la base de datos";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar el formulario de reserva");
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
            if (reserva.FechaDesde.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "FechaDesde",
                    "La fecha de inicio no puede ser anterior a la fecha actual.");
            }

            if (reserva.FechaHasta < reserva.FechaDesde)
            {
                ModelState.AddModelError(
                    "FechaHasta",
                    "La fecha de finalización no puede ser anterior a la fecha de inicio.");
            }

            if (!ModelState.IsValid)
            {
                CargarSelectLists();
                return View(reserva);
            }

            try
            {
                // Volver a verificar que el inmueble no esté ocupado en esas fechas
                if (!_repositorioReserva.EstaDisponible(reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta))
                {
                    CargarSelectLists();
                    ViewBag.Error = "El inmueble seleccionado ya está ocupado en esas fechas.";
                    return View(reserva);
                }

                int idReserva = _repositorioReserva.Alta(reserva, ObtenerIdUsuarioActual());

                var inmueble = _repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
                if (inmueble != null && inmueble.PorcentajeReserva > 0)
                {
                    int diasTotales = (reserva.FechaHasta.Date - reserva.FechaDesde.Date).Days + 1;
                    decimal totalReserva = reserva.MontoPorDia * diasTotales;
                    decimal importeSenia = Math.Round(totalReserva * (inmueble.PorcentajeReserva / 100m), 2);

                    if (importeSenia > 0)
                    {
                        var senia = new Pago
                        {
                            IdReserva = idReserva,
                            Concepto = "Seña",
                            FechaPago = DateTime.Today,
                            Importe = importeSenia,
                            Anulada = false
                        };
                        _repositorioPago.Alta(senia, ObtenerIdUsuarioActual());
                    }
                }

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
                var reserva = _repositorioReserva.ObtenerPorId(id);

                if (reserva == null)
                {
                    return NotFound();
                }

                if (reserva.Anulada)
                {
                    TempData["Error"] = "No se puede editar una reserva anulada.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaTerminacion.HasValue)
                {
                    TempData["Error"] = "No se puede editar una reserva terminada anticipadamente.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaHasta.Date < DateTime.Today)
                {
                    TempData["Error"] = "No se puede editar una reserva vencida.";
                    return RedirectToAction(nameof(Detalles), new { id });
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
                //var reserva = _repositorioReserva.ObtenerPorId(id);
                var reserva = _repositorioReserva.ObtenerPorId(id);
                if (reserva == null) return NotFound();

                var vm = new ReservaDetalleViewModel
                {
                    Reserva = reserva,
                    Pagos = _repositorioPago.ObtenerPorReserva(id)
                };

                return View(vm);
                //return reserva == null ? NotFound() : View(reserva);
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

            try
            {
                var reservaOriginal = _repositorioReserva.ObtenerPorId(id);

                if (reservaOriginal == null)
                {
                    return NotFound();
                }

                if (reservaOriginal.Anulada)
                {
                    TempData["Error"] = "No se puede editar una reserva anulada.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reservaOriginal.FechaTerminacion.HasValue)
                {
                    TempData["Error"] = "No se puede editar una reserva terminada anticipadamente.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reservaOriginal.FechaHasta.Date < DateTime.Today)
                {
                    TempData["Error"] = "No se puede editar una reserva vencida.";
                    return RedirectToAction(nameof(Detalles), new { id });
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

                // Re-verificar disponibilidad excluyendo la propia reserva
                if (!_repositorioReserva.EstaDisponible(
                        reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta,
                        id))
                {
                    CargarSelectLists();

                    ViewBag.Error = "El inmueble seleccionado ya está ocupado en esas fechas.";

                    return View(reserva);
                }

                _repositorioReserva.Modificacion(reserva);

                TempData["Mensaje"] = "Reserva modificada con éxito.";

                return RedirectToAction(nameof(Index));

            }
            catch (MySqlException ex)
            {
                _logger.LogError(
                    ex, "Error de base de datos al modificar la reserva {IdReserva}",
                    id);

                ViewBag.Error = "Ocurrió un error de conexión a la base de datos";

                CargarSelectLists();

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error inesperado al modificar la reserva {IdReserva}",
                    id);

                ViewBag.Error = "Ocurrió un error al modificar la reserva";

                CargarSelectLists();

                return View(reserva);
            }
        }



        // GET: /Reservas/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            try
            {
                var reserva = _repositorioReserva.ObtenerPorId(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                if (reserva.Anulada)
                {
                    TempData["Error"] = "La reserva ya se encuentra anulada.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaTerminacion.HasValue)
                {
                    TempData["Error"] = "No se puede anular una reserva terminada anticipadamente.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaHasta.Date < DateTime.Today)
                {
                    TempData["Error"] = "No se puede anular una reserva vencida.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaDesde.Date <= DateTime.Today)
                {
                    TempData["Error"] =
                        "No se puede anular una reserva que ya comenzó. Debe utilizar la terminación anticipada.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                return View(reserva);
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
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            try
            {
                var reserva = _repositorioReserva.ObtenerPorId(id);

                if (reserva == null)
                {
                    return NotFound();
                }

                if (reserva.Anulada)
                {
                    TempData["Error"] = "La reserva ya se encuentra anulada.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaTerminacion.HasValue)
                {
                    TempData["Error"] = "No se puede anular una reserva terminada anticipadamente.";

                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaHasta.Date < DateTime.Today)
                {
                    TempData["Error"] = "No se puede anular una reserva vencida.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaDesde.Date <= DateTime.Today)
                {
                    TempData["Error"] = "No se puede anular una reserva que ya comenzó. Debe utilizar la terminación anticipada.";

                    return RedirectToAction(nameof(Detalles), new { id });
                }

                int resultado =
                    _repositorioReserva.Baja(id, ObtenerIdUsuarioActual());

                if (resultado == 0)
                {
                    TempData["Error"] = "La reserva no pudo ser anulada porque su estado cambió.";
                }
                else
                {
                    TempData["Mensaje"] = "Reserva anulada correctamente.";
                }
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
                var original = _repositorioReserva.ObtenerPorId(id);
                if (original == null)
                {
                    return NotFound();
                }

                if (original.Anulada)
                {
                    TempData["Error"] = "No se puede renovar o extender una reserva anulada.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                var nueva = new Reserva
                {
                    IdInquilino = original.IdInquilino,
                    IdInmueble = original.IdInmueble,
                    FechaDesde = original.FechaHasta.AddDays(1),
                    FechaHasta = original.FechaHasta.AddDays(31),
                    MontoPorDia = original.MontoPorDia
                };


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
        public IActionResult Extender(Reserva reserva, int idReservaOriginal)
        {

            Reserva? original = null;

            try
            {
                original = _repositorioReserva.ObtenerPorId(idReservaOriginal);

                if (original == null)
                {
                    return NotFound();
                }

                if (original.Anulada)
                {
                    TempData["Error"] = "No se puede renovar o extender una reserva anulada.";

                    return RedirectToAction(
                        nameof(Detalles),
                        new { id = idReservaOriginal });
                }


                reserva.IdInquilino = original.IdInquilino;
                reserva.IdInmueble = original.IdInmueble;

                if (reserva.FechaHasta < reserva.FechaDesde)
                {

                    ModelState.AddModelError("FechaHasta", "La fecha de finalización no puede ser anterior a la fecha de inicio.");
                }

                if (!ModelState.IsValid)
                {

                    ViewBag.Original = original;
                    return View(reserva);
                }

                if (!_repositorioReserva.EstaDisponible(reserva.IdInmueble, reserva.FechaDesde, reserva.FechaHasta))
                {

                    ViewBag.Original = original;
                    ViewBag.Error = "El inmueble seleccionado ya está ocupado en esas fechas.";

                    return View(reserva);
                }

                int idReserva = _repositorioReserva.Alta(reserva, ObtenerIdUsuarioActual());

                var inmueble = _repositorioInmueble.ObtenerPorId(reserva.IdInmueble);
                if (inmueble != null && inmueble.PorcentajeReserva > 0)
                {
                    int diasTotales = (reserva.FechaHasta.Date - reserva.FechaDesde.Date).Days + 1;
                    decimal totalReserva = reserva.MontoPorDia * diasTotales;
                    decimal importeSenia = Math.Round(totalReserva * (inmueble.PorcentajeReserva / 100m), 2);

                    if (importeSenia > 0)
                    {
                        var senia = new Pago
                        {
                            IdReserva = idReserva,
                            Concepto = "Seña",
                            FechaPago = DateTime.Today,
                            Importe = importeSenia,
                            Anulada = false
                        };
                        _repositorioPago.Alta(senia, ObtenerIdUsuarioActual());
                    }
                }

                TempData["Mensaje"] = "Reserva renovada/extendida con éxito. Se generó un nuevo alquiler.";
                return RedirectToAction(nameof(Index));

            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al renovar la reserva {idReserva}", idReservaOriginal);
                ViewBag.Error = "Ocurrió un error de conexión a la base de datos";

                ViewBag.Original = original;

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al renovar la reserva {idReserva}", idReservaOriginal);
                ViewBag.Error = "Ocurrió un error al renovar la reserva";
                ViewBag.Original = original;
                return View(reserva);
            }
        }

        // GET: /Reservas/Terminar/5
        [HttpGet]
        public IActionResult Terminar(int id)
        {
            try
            {
                var reserva = _repositorioReserva.ObtenerPorId(id);

                if (reserva == null)
                {
                    return NotFound();
                }

                if (reserva.Anulada)
                {
                    TempData["Error"] = "No se puede terminar una reserva anulada.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (reserva.FechaTerminacion.HasValue)
                {
                    TempData["Error"] = "Esta reserva ya fue terminada anticipadamente.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (DateTime.Today < reserva.FechaDesde.Date)
                {
                    TempData["Error"] = "La reserva todavía no comenzó. Si desea cancelarla, debe anularla.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                if (DateTime.Today >= reserva.FechaHasta.Date)
                {
                    TempData["Error"] = "La reserva ya finalizó y no puede terminarse anticipadamente.";
                    return RedirectToAction(nameof(Detalles), new { id });
                }

                // Solo propone hoy como fecha inicial.
                // Todavía NO se guarda en la base de datos - Solo para prueba
                reserva.FechaTerminacion = DateTime.Today;

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la terminación anticipada de la reserva {IdReserva}", id);
                TempData["Error"] = "Ocurrió un error al cargar la reserva.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarTerminacion(int idReserva, DateTime fechaTerminacion)
        {
            try
            {
                var reserva = _repositorioReserva.ObtenerPorId(idReserva);

                if (reserva == null)
                {
                    return NotFound();
                }

                if (reserva.Anulada)
                {
                    TempData["Error"] = "No se puede terminar una reserva anulada.";

                    return RedirectToAction(nameof(Detalles), new { id = idReserva });
                }

                if (reserva.FechaTerminacion.HasValue)
                {
                    TempData["Error"] = "La reserva ya fue terminada anticipadamente.";

                    return RedirectToAction(nameof(Detalles), new { id = idReserva });
                }

                if (fechaTerminacion.Date < DateTime.Today ||
                    fechaTerminacion.Date < reserva.FechaDesde.Date ||
                    fechaTerminacion.Date >= reserva.FechaHasta.Date)
                {
                    TempData["Error"] = "La fecha de terminación no es válida.";

                    return RedirectToAction(nameof(Terminar), new { id = idReserva });
                }

                // IMPORTANTE:
                // La penalización vuelve a calcularse en el servidor.
                decimal penalizacion = CalcularPenalizacion(reserva, fechaTerminacion);

                var pago = new Pago
                {
                    IdReserva = reserva.IdReserva,
                    Concepto = "Penalización por terminación anticipada",
                    FechaPago = DateTime.Today,
                    Importe = penalizacion,
                    Anulada = false
                };

                int idUsuario = ObtenerIdUsuarioActual();

                _repositorioPago.RegistrarPenalizacionYTerminarReserva(pago, fechaTerminacion, idUsuario);

                TempData["Mensaje"] = "La penalización fue registrada y la reserva terminó anticipadamente.";

                return RedirectToAction(
                    nameof(Detalles),
                    new { id = idReserva });
            }
            catch (MySqlException ex)
            {
                _logger.LogError(
                    ex,
                    "Error de base de datos al finalizar anticipadamente la reserva {IdReserva}",
                    idReserva);

                TempData["Error"] = "Ocurrió un error de base de datos al finalizar la reserva.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al finalizar anticipadamente la reserva {IdReserva}",
                    idReserva);

                TempData["Error"] =
                    "No se pudo completar la terminación anticipada.";
            }

            return RedirectToAction(
                nameof(Terminar),
                new { id = idReserva });
        }

        // POST: /Reservas/CalcularTerminacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CalcularTerminacion(int idReserva, DateTime fechaTerminacion)
        {
            try
            {
                var reserva = _repositorioReserva.ObtenerPorId(idReserva);

                if (reserva == null)
                {
                    return NotFound();
                }

                if (reserva.Anulada)
                {
                    TempData["Error"] = "No se puede terminar una reserva anulada.";
                    return RedirectToAction(nameof(Detalles), new { id = idReserva });
                }

                if (reserva.FechaTerminacion.HasValue)
                {
                    TempData["Error"] = "La reserva ya fue terminada anticipadamente.";
                    return RedirectToAction(nameof(Detalles), new { id = idReserva });
                }

                if (fechaTerminacion.Date < DateTime.Today)
                {
                    ViewBag.Error = "La fecha de terminación no puede ser anterior a la fecha actual.";

                    reserva.FechaTerminacion = DateTime.Today;
                    return View("Terminar", reserva);
                }

                if (fechaTerminacion.Date < reserva.FechaDesde.Date)
                {
                    ViewBag.Error = "La fecha de terminación no puede ser anterior al inicio de la reserva.";

                    reserva.FechaTerminacion = fechaTerminacion;
                    return View("Terminar", reserva);
                }

                if (fechaTerminacion.Date >= reserva.FechaHasta.Date)
                {
                    ViewBag.Error = "La fecha de terminación debe ser anterior a la fecha de finalización original.";

                    reserva.FechaTerminacion = fechaTerminacion;
                    return View("Terminar", reserva);
                }

                decimal penalizacion = CalcularPenalizacion(reserva, fechaTerminacion);

                int diasTotales = (reserva.FechaHasta.Date - reserva.FechaDesde.Date).Days + 1;
                int diasCumplidos = (fechaTerminacion.Date - reserva.FechaDesde.Date).Days + 1;
                int diasRestantes = (reserva.FechaHasta.Date - fechaTerminacion.Date).Days;
                decimal mitadPeriodo = diasTotales / 2m;
                int porcentajeAplicado = diasCumplidos < mitadPeriodo ? 50 : 25;

                ViewBag.DiasTotales = diasTotales;
                ViewBag.DiasCumplidos = diasCumplidos;
                ViewBag.DiasRestantes = diasRestantes;
                ViewBag.PorcentajeAplicado = porcentajeAplicado;
                ViewBag.Penalizacion = penalizacion;


                // Solo guardamos la fecha en el objeto que vuelve a la vista.
                // NO se modifica todavía la base de datos hasta no tener el modelo de Pagos
                reserva.FechaTerminacion = fechaTerminacion;

                ViewBag.Penalizacion = penalizacion;

                return View("Terminar", reserva);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(
                    ex,
                    "Error de base de datos al calcular la terminación de la reserva {IdReserva}",
                    idReserva);

                TempData["Error"] =
                    "Ocurrió un error de conexión a la base de datos.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error inesperado al calcular la terminación de la reserva {IdReserva}",
                    idReserva);

                TempData["Error"] =
                    "No se pudo calcular la terminación anticipada.";

                return RedirectToAction(nameof(Index));
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
                var lista = _repositorioReserva.ObtenerMasReservados();
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
                var lista = _repositorioReserva.ObtenerInmueblesSinReservas(diasValidados);
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
                var lista = _repositorioReserva.ObtenerVigentes();
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
                var lista = _repositorioReserva.ObtenerPorTerminar(diasValidados);
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
                var lista = _repositorioReserva.ObtenerInmueblesDisponibles(desde.Value, hasta.Value);
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

        private decimal CalcularPenalizacion(Reserva reserva, DateTime fechaTerminacion)
        {

            int diasTotales = (reserva.FechaHasta.Date - reserva.FechaDesde.Date).Days + 1;

            int diasCumplidos = (fechaTerminacion.Date - reserva.FechaDesde.Date).Days + 1;

            int diasRestantes = (reserva.FechaHasta.Date - fechaTerminacion.Date).Days;

            decimal mitadPeriodo = diasTotales / 2m;

            decimal porcentajePenalizacion = diasCumplidos < mitadPeriodo ? 0.50m : 0.25m;

            decimal alquilerRestante = diasRestantes * reserva.MontoPorDia;

            return Math.Round(alquilerRestante * porcentajePenalizacion, 2);
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