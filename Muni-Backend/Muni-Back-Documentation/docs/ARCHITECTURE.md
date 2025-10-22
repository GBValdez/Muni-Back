# Arquitectura — Backend

- **Capa Web/API**: controladores por agregado (`users`, `roles`, `reports`, `votes`, `catalogues`).
- **Capa de Aplicación**: DTOs, servicios de dominio (AutoMapper, email service).
- **Capa de Dominio**: Entidades (`Reports`, `Votes`, `Status`, `Type`, `userEntity`, `rolEntity`, etc.).
- **Infraestructura**: `DBProyContext` (EF Core + Npgsql), migraciones, configuración.

**Autenticación**: JWT + ASP.NET Core Identity.  
**Persistencia**: PostgreSQL.  
**Estándares**: JSON y convenciones REST.  
