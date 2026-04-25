# Plataforma de Créditos

Sistema web desarrollado con ASP.NET Core MVC para gestionar solicitudes de crédito y su evaluación por analistas.

## Tecnologías

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQLite
- Identity
- Razor Views
- Session
- Redis (Render KeyValue)
- Docker
- Render

## Funcionalidades

- Registro e inicio de sesión
- Registro de solicitudes de crédito
- Listado de solicitudes con filtros
- Detalle de solicitud
- Validaciones de negocio
- Sesión de última solicitud visitada
- Cache distribuida
- Panel de analista (aprobar / rechazar)

## Variables de entorno

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__DefaultConnection=Data Source=app.db
Redis__ConnectionString=HOST:6379