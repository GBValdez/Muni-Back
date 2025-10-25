# API — Resumen rápido

## Autenticación
- `POST /user/login` → { email/userName, password } → tokens de acceso/refresh
- `POST /user/register`
- `POST /user/forgotPassword` / `POST /user/resetPassword`
- `GET  /user/confirmEmail`

## Reportes (Participación Ciudadana)
- `GET  /reports` (filtros básicos)
- `GET  /reports/unvalidation`
- `POST /reports` (crear)
- `PUT  /reports/{id}` (editar)
- `DELETE /reports/{id}` (eliminar/archivar)
- `POST /reports/request/{id}` (solicitar financiamiento a Finanzas)

## Votos
- `POST /votes` (votar por reporte)

## Catálogos
- `GET /status` / `GET /type`

> Nota: rutas exactas pueden variar en tiempo; revisar Swagger en modo desarrollo.
