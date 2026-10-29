# API de Mensajería — ASP.NET Core + EF Core + Docker

Ejemplo educativo: usuarios que se envían mensajes. Muestra Controladores → Servicios → EF Core (DbContext) → SQLite, inyección de dependencias, migraciones, JWT y Swagger.

**Solo necesitas Docker** (con Docker Compose). No hace falta instalar .NET.

## Estructura

```
bin/          salida (resultados de las pruebas)
run/          scripts para correr todo
src/back/     la API (Mensajeria.Api), sus pruebas (Mensajeria.Pruebas), Dockerfile y docker-compose.yml
src/front/    (pendiente)
```

Dentro de `src/back/Mensajeria.Api`: `Modelos/` (Usuario, Buzon, Mensaje) · `Datos/` (DbContext y datos de prueba) · `Servicios/` (lógica de negocio) · `Controllers/` · `Dtos/` · `Utilidades/` · `Migrations/` · `Program.cs` (registro de dependencias y arranque).

## Comandos

| Acción | Comando |
|---|---|
| Iniciar (construye y levanta) | `./run/iniciar.sh` |
| Detener (conserva los datos) | `./run/detener.sh` |
| Detener y borrar la base de datos | `./run/detener.sh --borrar-datos` |
| Pruebas automáticas (en Docker) | `./run/probar.sh` |
| Verificar con peticiones HTTP reales | `./run/verificar-api.sh` |
| Verificar que los datos persisten | `./run/verificar-persistencia.sh` |
| Ver cómo evolucionan las migraciones | `./run/demo-migraciones.sh` |
| Crear una migración nueva | `./run/nueva-migracion.sh NombreMigracion` |

Los scripts son Bash. Los `verificar-*` usan además `curl`.

## Usar Swagger

1. Abre <http://localhost:8080/swagger>.
2. Usuarios de prueba ya creados (contraseña `123456`): **Alice** y **Bob**. También puedes crear los tuyos con `POST /api/auth/registro`.
3. `POST /api/auth/login` como Alice y copia el valor de `token`.
4. Botón **Authorize** → pega el token → *Authorize*.
5. `POST /api/mensajes` con `destinatarioNombreUsuario: "Bob"`, un asunto y un contenido.
6. Pulsa *Authorize → Logout*, haz login como Bob y autentícate con su token.
7. `GET /api/buzon`: aparece el mensaje de Alice (anota su `id`).
8. `GET /api/mensajes/{id}` para leerlo y `PUT /api/mensajes/{id}/leido` para marcarlo como leído.
9. Autenticado como Alice, `GET /api/buzon`: devuelve el buzón de Alice, sin el mensaje que era para Bob. El buzón siempre sale del token, nunca de un Id que envíe el cliente.

## Endpoints

| Método y ruta | Descripción |
|---|---|
| `POST /api/auth/registro` | Registra un usuario y crea su buzón |
| `POST /api/auth/login` | Devuelve un token JWT |
| `POST /api/mensajes` | Envía un mensaje a otro usuario |
| `GET /api/buzon` | Mi buzón (mensajes recibidos) |
| `GET /api/mensajes/enviados` | Mensajes que envié |
| `GET /api/mensajes/{id}` | Leer un mensaje (solo remitente o destinatario) |
| `PUT /api/mensajes/{id}/leido` | Marcar como leído (solo el destinatario) |

Códigos: `400` datos inválidos · `401` sin token o login incorrecto · `404` no existe o no es tuyo · `409` usuario/correo repetido.

## Migraciones

Hay tres en `src/back/Mensajeria.Api/Migrations`: `InitialUsers` → `AddInboxesAndMessages` → `AddMessageReadStatus`. Se aplican solas al arrancar la API; la base SQLite vive en el volumen Docker `datos`, así que reconstruir o reiniciar no borra información. `./run/demo-migraciones.sh` lo muestra: crea una base con las migraciones 1 y 2, registra un usuario, aplica la 3 y comprueba que el usuario sigue ahí.

## Configuración y seguridad

- La clave JWT de desarrollo está en `appsettings.Development.json` (**solo para desarrollo**). Para usar otra: variable de entorno `Jwt__Clave` (mínimo 32 caracteres). Fuera de `Development` la API no arranca sin ella.
- Los usuarios de prueba solo se crean con `DatosIniciales:Cargar=true` (activo en Development).
- Las contraseñas se guardan con `PasswordHasher` y nunca se devuelven; el usuario se identifica por el claim `sub` del token, nunca por un Id enviado por el cliente.
