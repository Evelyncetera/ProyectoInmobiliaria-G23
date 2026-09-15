using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Inmobiliaria.Models; 

namespace Proyecto_Inmobiliaria.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _repoUsuario;

        public UsuariosController(IRepositorioUsuario repoUsuario)
        {
            _repoUsuario = repoUsuario;
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

                if (usuario == null || usuario.Clave != clave)
                {
                    ViewBag.Error = "Correo electrónico o contraseña incorrectos.";
                    return View();
                }

                // Armar las claims
                var claims = new List<Claim>
                {
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
            catch (Exception ex)
            {
                ViewBag.Error = "Error al intentar iniciar sesión: " + ex.Message;
                return View();
            }
        }

        // GET: /Usuarios/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuarios");
        }
    }
}