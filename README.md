# API de Tareas — ASP.NET Core + EF Core + Docker

API REST mínima para gestionar tareas (crear, listar, consultar, actualizar y borrar). Flujo: **HTTP → Controller → EF Core → SQLite**.

Solo necesitas Docker con Compose (si no hay Docker, los scripts usan Podman). No hace falta instalar .NET.

## Estructura

```
bin/          salida generada (no se versiona)
run/          scripts Bash: build, start, stop, restart, test
src/back/     backend ASP.NET Core (TareaApi), con las migraciones en Migrations/
src/front/    reservado para futuras lecciones
Dockerfile
docker-compose.yml
```

## Uso

```bash
./run/build.sh     # construye la imagen
./run/start.sh     # inicia la API
./run/test.sh      # prueba la API con peticiones HTTP reales (requiere start)
./run/stop.sh      # detiene la API; los datos se conservan
./run/restart.sh   # stop + start
```

Los datos viven en un volumen Docker (`tarea-data`), así que sobreviven a reinicios y reconstrucciones.

## Swagger

Con la API iniciada abre <http://localhost:5080/swagger>. Desde ahí puedes ejecutar los cinco endpoints:

| Métarea | Ruta              | Acción            |
|--------|-------------------|-------------------|
| GET    | `/api/tareas`      | Listar tareas     |
| GET    | `/api/tareas/{id}` | Obtener una tarea |
| POST   | `/api/tareas`      | Crear             |
| PUT    | `/api/tareas/{id}` | Actualizar        |
| DELETE | `/api/tareas/{id}` | Borrar            |

`status` acepta `Pending`, `InProgress` o `Done`.

## Migraciones de EF Core

Una migración es un archivo C# que describe un cambio en el esquema de la base de datos. EF Core las aplica en orden y anota cuáles ya se ejecutaron en la tabla `__EFMigrationsHistory`. La API aplica las pendientes al arrancar.

1. `InitialCreate`: crea la tabla `Tareas` con `Id`, `Title`, `Description` y `Status`.
2. `AddDueDate`: añade la columna opcional `DueDate`. La base de datos existente evoluciona sin perder las tareas guardadas.
