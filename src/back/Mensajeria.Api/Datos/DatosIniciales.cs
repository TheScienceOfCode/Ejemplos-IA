using Mensajeria.Api.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Api.Datos;

/// <summary>Usuarios de prueba para poder hacer login sin registrarse (solo en desarrollo).</summary>
public static class DatosIniciales
{
    public static async Task CargarAsync(MensajeriaDbContext db, IPasswordHasher<Usuario> hasher)
    {
        await CrearSiNoExisteAsync(db, hasher, "Alice", "alice@ejemplo.com", "123456");
        await CrearSiNoExisteAsync(db, hasher, "Bob", "bob@ejemplo.com", "123456");
    }

    private static async Task CrearSiNoExisteAsync(
        MensajeriaDbContext db, IPasswordHasher<Usuario> hasher, string nombre, string correo, string contrasena)
    {
        // Se puede ejecutar en cada arranque: si el usuario ya existe, no hace nada
        if (await db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre)) return;

        var usuario = new Usuario { NombreUsuario = nombre, Correo = correo };
        usuario.HashContrasena = hasher.HashPassword(usuario, contrasena);
        db.Buzones.Add(new Buzon { Usuario = usuario });
        await db.SaveChangesAsync();
    }
}
