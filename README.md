# Proyecto Inmobiliaria - G23

> Sistema web de gestión inmobiliaria desarrollado en ASP.NET Core MVC (.NET 10) como parte del plan de trabajos prácticos. El sistema permite la gestión integral de alquileres temporarios, administrando propietarios, inquilinos, inmuebles, reservas/contratos, pagos y control de usuarios con autenticación basada en roles.

---

## 👥 Integrantes del Grupo

* **Evelyn Cetera** - *evelyncetera@gmail.com* - [@Evelyncetera](https://github.com/Evelyncetera) - Discord: `evelyn_56580`
* **Matias Correa** - *matigc90@gmail.com* - [@mgc90](https://github.com/mgc90) - Discord: `mattyass90`
* **Christian Villegas** - *villegaschristian16@gmail.com* - [@christian-2001](https://github.com/christian-2001) - Discord: `christian_villegas_2001`

---
## Instrucciones para levantar la Base de Datos

  Clone el repositorio:

  Bash
    git clone [https://github.com/Evelyncetera/ProyectoInmobiliaria-G23]
    cd ProyectoInmobiliaria_G23

## Ejecute el script .sql ubicado en la raíz del proyecto, en su gestor de bases de datos:

/inmobiliariadb_g23.sql

## Configure la cadena de conexión en el archivo appsettings.json o mediante User Secrets:
  JSON
  {
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=inmobiliariadb_g23;User=root;Password=;"
    }
  }
  
## Ejecute el proyecto:

 dotnet run

 
## 📐 Modelado de Datos

A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación:

### Diagrama Entidad-Relación (DER) / Diagrama de Clases

![Diagrama del Proyecto](./diagram/diagrama%20de%20clases.png)
