# Proyecto Inmobiliaria - G23

Sistema web de gestión inmobiliaria desarrollado en **ASP.NET Core MVC (.NET 10)** para la administración de alquileres temporarios.

El sistema permite gestionar propietarios, inquilinos, inmuebles, reservas, pagos y usuarios, incluyendo control de disponibilidad, renovaciones, terminaciones anticipadas, señas y autenticación basada en roles.

> **Rama correspondiente a la versión final del proyecto:** `desarrollo`

---

## 👥 Integrantes del Grupo

- **Evelyn Cetera** - evelyncetera@gmail.com - [@Evelyncetera](https://github.com/Evelyncetera) - Discord: `evelyn_56580`
- **Matias Correa** - matigc90@gmail.com - [@mgc90](https://github.com/mgc90) - Discord: `mattyass90`
- **Christian Villegas** - villegaschristian16@gmail.com - [@christian-2001](https://github.com/christian-2001) - Discord: `christian_villegas_2001`
- **Mauricio Barca** - mauriciobarca1989@gmail.com - [@Mbarca89](https://github.com/Mbarca89)

---

## 🛠️ Tecnologías utilizadas

- ASP.NET Core MVC
- .NET 10
- C#
- MySQL / MariaDB
- MySqlConnector
- Razor Views
- Bootstrap
- JavaScript
- HTML / CSS
- Git y GitHub

---

## ⚙️ Funcionalidades principales

### Propietarios e Inquilinos
- Alta, baja y modificación.
- Consulta y listado.
- Validación de datos.

### Inmuebles
- Alta, modificación y baja lógica.
- Asociación con propietario y tipo de inmueble.
- Precio por día.
- Porcentaje de reserva.
- Disponibilidad.
- Latitud y longitud.
- Imagen de portada e imágenes adicionales.
- Filtros y búsqueda.

### Reservas
- Creación de reservas para un inmueble e inquilino.
- Validación de disponibilidad y superposición de fechas.
- Registro del monto por día al momento de realizar la reserva.
- Renovación/extensión mediante la creación de una nueva reserva.
- Conservación de la reserva original.
- Terminación anticipada.
- Cálculo de penalización por terminación anticipada.

### Pagos
- Registro de pagos asociados a una reserva.
- Registro de conceptos e importes.
- Anulación lógica de pagos.
- Generación automática de la **seña** al crear una reserva.
- Cálculo de la seña según el porcentaje configurado en el inmueble.
- Generación de una nueva seña al renovar/extender una reserva.

### Usuarios y Seguridad
- Inicio de sesión.
- Autenticación.
- Autorización basada en roles.
- Usuarios administradores y empleados.
- Registro de usuarios responsables de distintas operaciones.

---

## 🗄️ Instrucciones para levantar la Base de Datos

### 1. Clonar el repositorio

La versión final e integrada del proyecto se encuentra en la rama `desarrollo`.

```bash
git clone -b desarrollo https://github.com/Evelyncetera/ProyectoInmobiliaria-G23.git
cd ProyectoInmobiliaria-G23
```

---

### 2. Crear la Base de Datos

En la raíz del proyecto se encuentra el script:

```text
inmobiliariadb_g23.sql
```

Ejecute este archivo en **MySQL o MariaDB** utilizando DBeaver, MySQL Workbench u otro gestor compatible.

El script contiene la estructura y los datos necesarios para levantar la base utilizada por la aplicación.

---

### 3. Configurar la cadena de conexión

Configure la conexión a la base de datos en el archivo `appsettings.json` o mediante User Secrets.

Ejemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=inmobiliariadb_g23;User=root;Password=;"
  }
}
```

Modifique el usuario y la contraseña según la configuración local de MySQL/MariaDB.

---

### 4. Restaurar las dependencias

Desde la carpeta raíz del proyecto ejecute:

```bash
dotnet restore
```

---

### 5. Ejecutar el proyecto

```bash
dotnet run
```

Una vez iniciada la aplicación, la consola indicará la dirección local desde la cual puede accederse al sistema.

---

## 🔐 Credenciales de prueba

### Administrador

- **Email:** `admin@inmobiliaria.com`
- **Contraseña:** `123456`

### Empleado

- **Email:** `empleado@inmobiliaria.com`
- **Contraseña:** `123456`

> Las credenciales anteriores corresponden únicamente a usuarios de prueba incluidos para utilizar la aplicación en el entorno de desarrollo.

---

## ⚙️ Funcionalidades principales

### Propietarios e Inquilinos

- Alta, modificación y baja.
- Consulta y listado.
- Validación de datos.

### Inmuebles

- Alta, modificación y baja lógica.
- Asociación con propietario y tipo de inmueble.
- Precio por día.
- Porcentaje de reserva.
- Disponibilidad.
- Latitud y longitud.
- Imagen de portada e imágenes adicionales.
- Búsqueda y filtros.

### Reservas

- Creación de reservas asociadas a un inmueble y un inquilino.
- Validación de disponibilidad.
- Control de superposición de fechas.
- Registro del monto por día correspondiente a la reserva.
- Renovación o extensión mediante la creación de una nueva reserva.
- Conservación de la reserva original.
- Terminación anticipada.
- Cálculo de penalización por terminación anticipada.

### Pagos

- Registro de pagos asociados a una reserva.
- Registro de concepto, fecha e importe.
- Anulación lógica de pagos.
- Generación automática de la **seña** al crear una reserva.
- Cálculo de la seña utilizando el porcentaje de reserva configurado en el inmueble.
- Generación automática de una nueva seña al renovar o extender una reserva.

### Usuarios y Seguridad

- Inicio de sesión.
- Autenticación de usuarios.
- Autorización basada en roles.
- Usuarios con rol Administrador y Empleado.
- Registro del usuario responsable de determinadas operaciones del sistema.

---

## 📐 Modelado de Datos

A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación.

### Diagrama Entidad-Relación (DER) / Diagrama de Clases

![Diagrama del Proyecto](./diagram/DER%20inmobiliaria.png)

---

## 🌿 Rama de entrega

La versión integrada y actualizada del proyecto se encuentra en la rama:

```text
desarrollo
```

En caso de haber clonado el repositorio sin especificar una rama, puede cambiar a la versión final mediante:

```bash
git checkout desarrollo
```

