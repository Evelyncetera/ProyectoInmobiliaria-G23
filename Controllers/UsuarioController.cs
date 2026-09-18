using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Proyecto_Inmobiliaria.Models; 

namespace Proyecto_Inmobiliaria.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _repoUsuario;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuariosController(
            IRepositorioUsuario repoUsuario,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _repoUsuario = repoUsuario;
            _passwordHasher = passwordHasher;
        }

        // GET: /Usuarios/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al inicio
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Usuarios/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string clave)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(clave))
                {
                    ViewBag.Error = "Por favor, ingresa el email y la contraseña.";
                    return View();
                }

                // Buscar usuario activo en BD
                var usuario = _repoUsuario.ObtenerPorEmail(email);

                if (usuario == null ||
                    _passwordHasher.VerifyHashedPassword(
                        usuario,
                        usuario.Clave,
                        clave) == PasswordVerificationResult.Failed)
                {
                    ViewBag.Error = "Correo electrónico o contraseña incorrectos.";
                    return View();
                }

                // Armar las claims
                var claims = new List<Claim>
                {   
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol), // "Administrador" o "Empleado"
                    new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                    new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}")
                };

                if (!string.IsNullOrEmpty(usuario.AvatarUrl))
                {
                    claims.Add(new Claim("AvatarUrl", usuario.AvatarUrl));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                // Firmar y crear la cookie de autenticación
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo iniciar sesión. Intente nuevamente.";
                return View();
            }
        }

        // GET: /Usuarios/Perfil
        [HttpGet]
        public IActionResult Perfil()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
            {
                return NotFound();
            }

            return View(new PerfilViewModel
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                AvatarUrl = usuario.AvatarUrl
            });
        }

        // POST: /Usuarios/Perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
            {
                return NotFound();
            }

            try
            {
                if (modelo.Avatar != null && modelo.Avatar.Length > 0)
                {
                    var extension = Path.GetExtension(modelo.Avatar.FileName).ToLowerInvariant();
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                    if (modelo.Avatar.Length > 2 * 1024 * 1024 ||
                        !extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError(nameof(modelo.Avatar), "El avatar debe ser JPG, PNG o WEBP y pesar como máximo 2 MB.");
                        return View(modelo);
                    }
                }

                _repoUsuario.ActualizarPerfil(
                    usuario.IdUsuario,
                    modelo.Nombre,
                    modelo.Apellido,
                    modelo.Email);

                if (modelo.Avatar != null && modelo.Avatar.Length > 0)
                {
                    var extension = Path.GetExtension(modelo.Avatar.FileName).ToLowerInvariant();
                    var carpeta = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "avatars");
                    Directory.CreateDirectory(carpeta);

                    var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
                    var ruta = Path.Combine(carpeta, nombreArchivo);
                    await using var stream = System.IO.File.Create(ruta);
                    await modelo.Avatar.CopyToAsync(stream);

                    var avatarUrl = $"/uploads/avatars/{nombreArchivo}";
                    _repoUsuario.ActualizarAvatar(usuario.IdUsuario, avatarUrl);
                    usuario.AvatarUrl = avatarUrl;
                }

                await ActualizarCookie(usuario, modelo);
                TempData["Mensaje"] = "Perfil actualizado correctamente.";
                return RedirectToAction(nameof(Perfil));
            }
            catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError(nameof(modelo.Email), "El correo electrónico ya está registrado.");
                return View(modelo);
            }
        }

        // POST: /Usuarios/CambiarClave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarClave(CambiarClaveViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                var usuarioConError = ObtenerUsuarioActual();
                modelo.Perfil ??= usuarioConError == null
                    ? new PerfilViewModel()
                    : new PerfilViewModel
                    {
                        Nombre = usuarioConError.Nombre,
                        Apellido = usuarioConError.Apellido,
                        Email = usuarioConError.Email,
                        AvatarUrl = usuarioConError.AvatarUrl
                    };
                return View("Perfil", modelo.Perfil);
            }

            var usuario = ObtenerUsuarioActual();
            if (usuario == null)
            {
                return NotFound();
            }

            if (_passwordHasher.VerifyHashedPassword(usuario, usuario.Clave, modelo.ClaveActual)
                == PasswordVerificationResult.Failed)
            {
                var perfil = new PerfilViewModel
                {
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email,
                    AvatarUrl = usuario.AvatarUrl
                };
                ModelState.AddModelError(nameof(modelo.ClaveActual), "La contraseña actual no es correcta.");
                ViewBag.Error = "La contraseña actual no es correcta.";
                return View("Perfil", perfil);
            }

            var hash = _passwordHasher.HashPassword(usuario, modelo.ClaveNueva);
            _repoUsuario.CambiarClave(usuario.IdUsuario, hash);
            TempData["Mensaje"] = "Contraseña actualizada correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // GET: /Usuarios/Administrar
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Administrar()
        {
            var usuarios = _repoUsuario.ObtenerTodosIncluyendoInactivos();
            return View(usuarios);
        }

        // GET: /Usuarios/CrearUsuario
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult CrearUsuario()
        {
            return View(new Usuario { Rol = "Empleado", Estado = 1 });
        }

        // POST: /Usuarios/CrearUsuario
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            if (usuario.Rol != "Administrador" && usuario.Rol != "Empleado")
            {
                ModelState.AddModelError(nameof(usuario.Rol), "El rol debe ser Administrador o Empleado.");
                return View(usuario);
            }

            usuario.Estado = 1;
            usuario.Clave = _passwordHasher.HashPassword(usuario, usuario.Clave);

            try
            {
                _repoUsuario.Alta(usuario);
                TempData["Mensaje"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Administrar));
            }
            catch (MySqlConnector.MySqlException ex) when (ex.Number == 1062)
            {
                ModelState.AddModelError(nameof(usuario.Email), "El correo electrónico ya está registrado.");
                return View(usuario);
            }
        }

        // GET: /Usuarios/EditarUsuario
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult EditarUsuario(int id)
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var modelo = new EditarUsuarioViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = usuario.Rol
            };

            return View(modelo);
        }

        // POST: /Usuarios/EditarUsuario
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarUsuario(EditarUsuarioViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            if (modelo.Rol != "Administrador" && modelo.Rol != "Empleado")
            {
                ModelState.AddModelError(nameof(modelo.Rol), "El rol debe ser Administrador o Empleado.");
                return View(modelo);
            }

            var usuarioOriginal = _repoUsuario.ObtenerPorId(modelo.IdUsuario);
            if (usuarioOriginal == null)
            {
                return NotFound();
            }

            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out var idUsuarioActual)
                && modelo.IdUsuario == idUsuarioActual
                && usuarioOriginal.Rol == "Administrador"
                && modelo.Rol == "Empleado")
            {
                TempData["Error"] = "No puede quitarse a sí mismo el rol de Administrador.";
                return RedirectToAction(nameof(Administrar));
            }

            usuarioOriginal.Nombre = modelo.Nombre;
            usuarioOriginal.Apellido = modelo.Apellido;
            usuarioOriginal.Rol = modelo.Rol;

            _repoUsuario.Modificacion(usuarioOriginal);

            TempData["Mensaje"] = "Usuario modificado correctamente.";
            return RedirectToAction(nameof(Administrar));
        }

        // POST: /Usuarios/CambiarEstado
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int id, int estado)
        {
            if (estado != 0 && estado != 1)
            {
                return BadRequest();
            }

            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(claim, out var idUsuarioActual) && id == idUsuarioActual && estado == 0)
            {
                TempData["Error"] = "No puede desactivar su propio usuario.";
                return RedirectToAction(nameof(Administrar));
            }

            _repoUsuario.CambiarEstado(id, estado);

            TempData["Mensaje"] = estado == 1
                ? "Usuario activado correctamente."
                : "Usuario desactivado correctamente.";
            return RedirectToAction(nameof(Administrar));
        }

        // POST: /Usuarios/Logout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuarios");
        }

        private Usuario? ObtenerUsuarioActual()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? _repoUsuario.ObtenerPorId(id) : null;
        }

        private async Task ActualizarCookie(Usuario usuario, PerfilViewModel modelo)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, modelo.Email),
                new(ClaimTypes.Role, usuario.Rol),
                new("FullName", $"{modelo.Nombre} {modelo.Apellido}")
            };

            if (!string.IsNullOrWhiteSpace(usuario.AvatarUrl))
            {
                claims.Add(new Claim("AvatarUrl", usuario.AvatarUrl));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}