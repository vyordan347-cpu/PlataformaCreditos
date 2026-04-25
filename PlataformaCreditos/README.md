# Plataforma de Créditos

Sistema web desarrollado con ASP.NET Core MVC para la gestión de solicitudes de crédito y su evaluación por analistas de riesgo.

---

## 🌐 URL del sistema

https://plataformacreditos-xz9p.onrender.com/

---

## 📦 Repositorio

https://github.com/vyordan347-cpu/PlataformaCreditos.git

---

## 🧰 Tecnologías utilizadas

* ASP.NET Core MVC (.NET 8)
* Entity Framework Core
* SQLite
* Identity (autenticación y roles)
* Razor Views
* Session
* Redis (Render KeyValue)
* Docker
* Render (deploy)

---

## ⚙️ Funcionalidades

### Usuario

* Registro e inicio de sesión
* Registro de solicitudes de crédito
* Validaciones de negocio:

  * No monto negativo
  * No más de una solicitud pendiente
  * Monto ≤ 10 veces ingresos
* Listado de solicitudes con filtros:

  * Estado
  * Monto
  * Fecha
* Visualización de detalle

### Sesión y Cache

* Guarda la última solicitud visitada
* Muestra acceso rápido en el layout
* Cache distribuido de solicitudes (60s)

### Analista

* Panel exclusivo para rol "Analista"
* Visualiza solicitudes pendientes
* Aprobar solicitudes
* Rechazar solicitudes con motivo obligatorio
* Validación:

  * No aprobar si monto > 5x ingresos
  * No procesar solicitudes ya resueltas

---

## Credenciales de prueba

Correo: **[analista@test.com](mailto:analista@test.com)**
Contraseña: **Password123!**

---

## Pruebas realizadas

* Login de usuario
* Registro de solicitud
* Validaciones de formulario
* Restricción de solicitud pendiente
* Filtros de búsqueda
* Detalle de solicitud
* Sesión de última solicitud
* Cache funcionando correctamente
* Panel de analista
* Aprobación y rechazo
* Despliegue en Render

---

## Variables de entorno (Render)

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__DefaultConnection=Data Source=app.db
Redis__ConnectionString=red-d7l9v6t7vvec73e4c7ig:6379
```

---

## Explicación de variables

* **ASPNETCORE_ENVIRONMENT** → Define el entorno de ejecución
* **ASPNETCORE_URLS** → Puerto requerido por Render
* **ConnectionStrings__DefaultConnection** → Base de datos SQLite
* **Redis__ConnectionString** → Conexión a Redis (KeyValue)

---

## Redis

* En desarrollo local: usa `DistributedMemoryCache`
* En producción: usa Redis (Render KeyValue)

---

## Docker

El despliegue se realiza mediante un `Dockerfile` ubicado en la raíz del proyecto.

---

## Notas finales

* El sistema cumple con todas las reglas de negocio del caso
* Se implementó arquitectura MVC correctamente
* Se integró autenticación, roles, sesión y cache distribuido
* Se realizó despliegue en producción con Docker y Render

---
