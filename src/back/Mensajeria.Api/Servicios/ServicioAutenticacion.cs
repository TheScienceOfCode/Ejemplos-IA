using Mensajeria.Api.Datos;
using Mensajeria.Api.Dtos;
using Mensajeria.Api.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mensajeria.Api.Servicios;

public interface IServicioAutenticacion
{
    /// <summary>Devuelve null si el nombre o el correo ya existen.</summary>
    Task<UsuarioRespuesta?> RegistrarAsync(RegistroSolicitud solicitud);
    /// <summary>Devuelve null si el usuario o la contraseña son incorrectos.</summary>
    Task<TokenRespuesta?> LoginAsync(LoginSolicitud solicitud);
}

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly MensajeriaDbContext _db;
    private readonly IServicioJwt _jwt;
    private readonly IPasswordHasher<Usuario> _hasher;

    // Inyección de dependencias por constructor: ASP.NET nos entrega estas piezas ya construidas
    public ServicioAutenticacion(MensajeriaDbContext db, IServicioJwt jwt, IPasswordHasher<Usuario> hasher)
    {
        _db = db;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async /// <summary>Devuelve null si el nombre o el correo ya existen.</summary>
    Task<UsuarioRespuesta?> RegistrarAsync(RegistroSolicitud solicitud)
    {
        var nombre = solicitud.NombreUsuario.Trim();
        var correo = solicitud.Correo.Trim();

        var yaExiste = await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre || u.Correo == correo);
        if (yaExiste) return null;

        var usuario = new Usuario { NombreUsuario = nombre, Correo = correo };
        usuario.HashContrasena = _hasher.HashPassword(usuario, solicitud.Contrasena);

        // El buzón se crea junto con el usuario; EF Core guarda ambos en una sola transacción.
        _db.Buzones.Add(new Buzon { Usuario = usuario });

        await _db.SaveChangesAsync();

        return new UsuarioRespuesta
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Correo = usuario.Correo,
            CreadoEn = usuario.CreadoEn
        };
    }

    public async /// <summary>Devuelve null si el usuario o la contraseña son incorrectos.</summary>
    Task<TokenRespuesta?> LoginAsync(LoginSolicitud solicitud)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == solicitud.NombreUsuario.Trim());

        // Mismo mensaje si el usuario no existe o la contraseña es incorrecta (no revelamos cuál falló)
        var resultado = usuario is null
            ? PasswordVerificationResult.Failed
            : _hasher.VerifyHashedPassword(usuario, usuario.HashContrasena, solicitud.Contrasena);

        if (usuario is null || resultado == PasswordVerificationResult.Failed) return null;

        var (token, expira) = _jwt.CrearToken(usuario);
        return new TokenRespuesta { Token = token, ExpiraEn = expira, NombreUsuario = usuario.NombreUsuario };
    }
}
