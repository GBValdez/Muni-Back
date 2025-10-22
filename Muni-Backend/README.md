# Muni-Back — API REST (.NET 8 + EF Core + PostgreSQL)

API para **Participación Ciudadana** del proyecto municipal. Expone endpoints para usuarios, roles, reportes de problemas, catálogos y votos; implementa autenticación **JWT**, **EF Core** con PostgreSQL y CORS.

## Tecnologías

- .NET SDK 8
- ASP.NET Core Web API
- Entity Framework Core (Npgsql)
- Identity (usuarios/roles)
- AutoMapper
- JWT Bearer Auth
- Swagger/OpenAPI
- PostgreSQL (Neon u on‑prem)
- MailKit (recuperación de contraseña)

## Estructura

```txt
Muni-Back/
 ├─ Program.cs
 ├─ appsettings.json
 ├─ context/DBProyContext.cs
 ├─ users/ (controllers, entidades, servicios)
 ├─ roles/
 ├─ reports/
 ├─ votes/
 ├─ catalogues/ (Status, Type)
 └─ utils/ (bases, DTOs, servicios)
```

## Endpoints

_Rutas base detectadas en controladores:_

- **StatusController.cs** → `/status`  ·
- **TypeController.cs** → `/type`  ·
- **ReportsController.cs** → `/reports`  · HttpGet unvalidation, HttpPost request/{id}
- **rolController.cs** → `/rol`  ·
- **userController.cs** → `/user`  · HttpGet {userName}, HttpPost register, HttpPost forgotPassword, HttpPost resetPassword, HttpGet confirmEmail, HttpPost login, HttpPost renewToken
- **cataloguesController.cs** → `/(sin [Route])`  ·
- **voteController.cs** → `/votes`  ·

> Ejemplos destacados:

- `POST /user/login` – autentica y devuelve tokens.
- `POST /user/register` – registro de usuario.
- `GET /reports/unvalidation` – reportes pendientes de validación.
- `POST /reports/request/{id}` – solicitar financiamiento a Finanzas (para eventos o mantenimiento).
- `POST /votes` – votar por un reporte.
- `GET /status`, `GET /type` – catálogos.

> Swagger: habilítalo en desarrollo navegando a `http://localhost:5003/swagger` (si está configurado).

## Configuración local

### Requisitos

- .NET SDK 8
- PostgreSQL 14+ (o **Neon**)
- Node 20+ (para probar el Front)

### Variables/secretos

Evita comprometer llaves en Git. Usa **dotnet user-secrets** en desarrollo y **GitHub Secrets** en CI/CD.

Campos relevantes en `appsettings*.json`:

```json
{
  "ConnectionStrings": {"DefaultConnection": "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=True;"},
  "keyJwt": "<JWT_SECRET>",
  "Email": {"Host": "smtp.gmail.com", "Port": "587", "UserName": "", "Password": ""},
  "FrontUrl": "http://localhost:4200",
  "Urls": "http://localhost:5003"
}
```

Sugerencia (desarrollo):

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=..."
dotnet user-secrets set "keyJwt" "<genera-una-clave-larga>"
dotnet user-secrets set "Email:UserName" "tu_correo@gmail.com"
dotnet user-secrets set "Email:Password" "app_password"
```

### Base de datos

```bash
dotnet tool restore              # habilitar dotnet-ef (local)
dotnet ef database update        # aplica migraciones en la conexión configurada
```

### Ejecutar

```bash
dotnet restore
dotnet build
dotnet run                       # API en http://localhost:5003
```

## Pruebas

- Soporte para pruebas con `xUnit`/`NUnit` (si el proyecto de tests existe): `dotnet test`.

## Seguridad aplicada

- Autenticación con **JWT** y ASP.NET Core Identity.
- **CORS** permitido hacia el Front.
- Validaciones en DTOs y reglas de negocio (controladores base).
- Sugerido: habilitar **HTTPS** en producción, usar **app passwords** para SMTP, rotación de llaves JWT.

## Deploy (sugerencias)

- **Docker**: ver workflow en `.github/workflows/docker-image.yml`.
- **Render/Azure/GCP**: publicar imagen o artefacto manual/CI.
- **Variables**: usar `ConnectionStrings__DefaultConnection`, `keyJwt`, etc.

## Contribución

Consulta `CONTRIBUTING.md` y el **flujo de ramas** (GitHub Flow). Usa **Conventional Commits** y Pull Requests con checklist.

## Licencia

MIT. Ver `LICENSE`.
