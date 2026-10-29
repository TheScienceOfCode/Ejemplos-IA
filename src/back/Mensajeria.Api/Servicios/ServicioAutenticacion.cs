using Mensajeria.Api.Datos;
using Mensajeria.Api.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Api.Servicios;

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly MensajeriaDbContext _db;
    private readonly IPasswordHasher<Usuario> _hasher;

    // Inyección de dependencias por constructor: ASP.NET nos entrega estas piezas ya construidas
    public ServicioAutenticacion(MensajeriaDbContext db, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<Usuario?> RegistrarAsync(string nombreUsuario, string correo, string contrasena)
    {
        nombreUsuario = nombreUsuario.Trim();
        correo = correo.Trim();

        var yaExiste = await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario || u.Correo == correo);
        if (yaExiste) return null;

        var usuario = new Usuario { NombreUsuario = nombreUsuario, Correo = correo };
        usuario.HashContrasena = _hasher.HashPassword(usuario, contrasena);

        // El buzón se crea junto con el usuario; EF Core guarda ambos en una sola transacción.
        _db.Buzones.Add(new Buzon { Usuario = usuario });
        await _db.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string nombreUsuario, string contrasena)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario.Trim());
        if (usuario is null) return null;

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.HashContrasena, contrasena);
        if (resultado == PasswordVerificationResult.Failed) return null;

        return usuario;
    }
}
