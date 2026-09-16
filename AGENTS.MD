```
# AGENTS.md - Proyecto Inmobiliaria (Reservas Temporales)

## 📌 Descripción del Proyecto
Sistema de informatización y gestión de alquileres temporarios para agencia inmobiliaria [1, 2], construido con **ASP.NET Core MVC** (.NET 10 LTS, C# 14) [1] y **MySQL** [1, 3].

---

## 🛠️ Tech Stack &amp; Arquitectura
* **Plataforma:** .NET 10 LTS (C# 14) [1].
* **Capa de Presentación:** ASP.NET Core MVC (Vistas Razor) [1, 4].
* **Acceso a Datos:** ADO.NET puro utilizando el paquete `MySqlConnector` [1, 3].
* **Patrón de Arquitectura:** **Patrón Repositorio** (`IRepositorioX` → `RepositorioX : RepositorioBase`) [1, 5].
* **Seguridad:** Autenticación por **Cookies** (`CookieAuthenticationDefaults`) [1, 6] y Autorización basada en **Roles** (`Administrador` y `Empleado`) [1, 7, 8].

---

## 📏 Reglas Obligatorias de Desarrollo
1. **Acceso a Datos:** Queda estrictamente prohibido escribir consultas SQL dentro de los controladores [1, 5, 9]. Todo el SQL debe residir en la clase Repositorio correspondiente [1, 5].
2. **Seguridad SQL:** Cualquier valor dinámico en sentencias SQL debe pasarse mediante **parámetros (`@param`)** para prevenir inyecciones SQL [1, 9, 10].
3. **Gestión de Conexiones:** Toda apertura de conexión a MySQL debe gestionarse dentro de un bloque **`using`** para asegurar su liberación [1, 11, 12].
4. **Autorización y Bajas:**
   * Las bajas de registros son lógicas (modificación del campo `estado`) [1, 7].
   * La eliminación de entidades y la administración de usuarios están reservadas para el rol **`Administrador`** (`[Authorize(Roles = "Administrador")]`) [1, 7, 8].
   * El rol **`Empleado`** solo puede realizar operaciones habituales de ABM y gestionar su propio perfil [1, 7, 8].

---

## 📁 Estructura Principal
* `Controllers/`: Manejo de peticiones HTTP, ruteo y coordinación de la vista [1, 13, 14].
* `Models/`: Entidades de dominio, ViewModels e interfaces/clases del Repositorio [1, 5, 15].
* `Views/`: Vistas Razor (`.cshtml`) e interfaz gráfica de usuario [1, 16, 17].
* `wwwroot/`: Recursos estáticos (hojas de estilo CSS, JS y descargas/avatars) [1, 18].

```

---
