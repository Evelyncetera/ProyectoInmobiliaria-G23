using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlConnector;
using Proyecto_Inmobiliaria.Models;
using Proyecto_Inmobiliaria.Services;

namespace Proyecto_Inmobiliaria.Controllers
{
    [Authorize]
    public class InmueblesController : Controller
    {
        private readonly IRepositorioInmueble _repositorio;
        private readonly CompresorImagenes _compresor;
        private readonly ILogger<InmueblesController> _logger;

        public InmueblesController(IRepositorioInmueble repositorio, CompresorImagenes compresor,
            ILogger<InmueblesController> logger)
        {
            _repositorio = repositorio;
            _compresor = compresor;
            _logger = logger;
        }

        private const string Campos = "IdInmueble,IdPropietario,IdTipoInmueble,Direccion,Cupo,Latitud,Longitud,PrecioPorDia,PorcentajeReserva,Disponible";

        [HttpGet]
        public IActionResult Index([FromQuery] InmueblesListadoViewModel filtro)
        {
            try
            {
                _repositorio.ObtenerPagina(filtro);
                ViewBag.Propietarios = new SelectList(filtro.Propietario.HasValue
                    ? _repositorio.BuscarPropietarios(null, filtro.Propietario) : new List<OpcionInmueble>(), "Id", "Texto", filtro.Propietario);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error al listar inmuebles");
                TempData["Error"] = "No se pudo cargar el listado de inmuebles.";
                filtro.Items = new List<Inmueble>();
                filtro.Total = 0;
            }
            return View(filtro);
        }

        private void CargarSelectLists(Inmueble inmueble)
        {
            ViewBag.Propietarios = new SelectList(inmueble.IdPropietario > 0
                ? _repositorio.BuscarPropietarios(null, inmueble.IdPropietario) : new List<OpcionInmueble>(), "Id", "Texto", inmueble.IdPropietario);
            ViewBag.TiposInmuebles = new SelectList(inmueble.IdTipoInmueble > 0
                ? _repositorio.BuscarTipos(null, inmueble.IdTipoInmueble) : new List<OpcionInmueble>(), "Id", "Texto", inmueble.IdTipoInmueble);
        }

        [HttpGet]
        public IActionResult BuscarPropietarios(string? buscar) => Json(_repositorio.BuscarPropietarios(buscar));

        [HttpGet]
        public IActionResult BuscarTipos(string? buscar) => Json(_repositorio.BuscarTipos(buscar));

        [HttpGet]
        public IActionResult Crear()
        {
            var inmueble = new Inmueble();
            CargarSelectLists(inmueble);
            return View(inmueble);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(35 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 35 * 1024 * 1024)]
        public Task<IActionResult> Crear([Bind(Campos)] Inmueble inmueble, List<IFormFile> imagenes, CancellationToken ct)
            => Guardar(inmueble, imagenes, true, ct);

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var inmueble = _repositorio.ObtenerPorId(id);
            if (inmueble == null || !inmueble.Estado) return NotFound();
            CargarSelectLists(inmueble);
            return View(inmueble);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(35 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 35 * 1024 * 1024)]
        public Task<IActionResult> Editar(int id, [Bind(Campos)] Inmueble inmueble, List<IFormFile> imagenes, CancellationToken ct)
        {
            if (id != inmueble.IdInmueble) return Task.FromResult<IActionResult>(BadRequest());
            return Guardar(inmueble, imagenes, false, ct);
        }

        private async Task<IActionResult> Guardar(Inmueble inmueble, List<IFormFile> imagenes, bool nuevo, CancellationToken ct)
        {
            try
            {
                if (!nuevo)
                {
                    var actual = _repositorio.ObtenerPorId(inmueble.IdInmueble);
                    if (actual == null || !actual.Estado) return NotFound();
                }
                if (ModelState.IsValid)
                {
                    if (!_repositorio.BuscarPropietarios(null, inmueble.IdPropietario).Any())
                        ModelState.AddModelError(nameof(Inmueble.IdPropietario), "El propietario seleccionado no existe.");
                    if (!_repositorio.BuscarTipos(null, inmueble.IdTipoInmueble).Any())
                        ModelState.AddModelError(nameof(Inmueble.IdTipoInmueble), "El tipo seleccionado no existe.");
                }
                if (ModelState.IsValid)
                {
                    var comprimidas = await _compresor.ComprimirAsync(imagenes, ct);
                    _repositorio.Guardar(inmueble, comprimidas, nuevo);
                    TempData["Mensaje"] = nuevo ? "Inmueble registrado." : "Cambios guardados.";
                    return RedirectToAction(nameof(Detalles), new { id = inmueble.IdInmueble });
                }
            }
            catch (ArgumentException ex) { ModelState.AddModelError("", ex.Message); }
            catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error al guardar inmueble {Id}", inmueble.IdInmueble);
                ModelState.AddModelError("", "No se guardaron los cambios. Revisá los datos e intentá nuevamente.");
            }
            if (imagenes.Count > 0)
                ModelState.AddModelError("", "Volvé a seleccionar las imágenes antes de enviar el formulario.");
            CargarSelectLists(inmueble);
            return View(nuevo ? "Crear" : "Editar", inmueble);
        }

        [HttpGet]
        public IActionResult Detalles(int id)
        {
            var inmueble = _repositorio.ObtenerPorId(id);
            return inmueble == null ? NotFound() : View(new InmuebleDetallesViewModel
            {
                Inmueble = inmueble, Imagenes = _repositorio.ObtenerImagenes(id)
            });
        }

        [HttpGet]
        public IActionResult Imagen(int id)
        {
            var base64 = _repositorio.ObtenerImagenBase64(id);
            if (base64 == null) return NotFound();
            Response.Headers.CacheControl = "private, no-store";
            return File(Convert.FromBase64String(base64), "image/webp");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Disponibilidad(int id, bool disponible)
            => Ejecutar(() => _repositorio.CambiarDisponibilidad(id, disponible),
                disponible ? "El inmueble vuelve a estar disponible." : "Oferta suspendida. Las reservas existentes se conservan.", id);

        [Authorize(Roles = "Administrador")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
            => Ejecutar(() => _repositorio.Baja(id) > 0, "Inmueble dado de baja. Su historial se conserva.");

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Portada(int id, int imagenId)
            => Ejecutar(() => _repositorio.CambiarPortada(id, imagenId), "Portada actualizada.", id);

        [Authorize(Roles = "Administrador")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult EliminarImagen(int id, int imagenId)
            => Ejecutar(() => _repositorio.BajaImagen(id, imagenId), "Imagen dada de baja.", id);

        private IActionResult Ejecutar(Func<bool> accion, string mensaje, int? id = null)
        {
            try
            {
                if (!accion()) return NotFound();
                TempData["Mensaje"] = mensaje;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Error al actualizar inmueble {Id}", id);
                TempData["Error"] = "No se pudo completar la operación. Intentá nuevamente.";
            }
            return id.HasValue ? RedirectToAction(nameof(Detalles), new { id }) : RedirectToAction(nameof(Index));
        }
    }
}
