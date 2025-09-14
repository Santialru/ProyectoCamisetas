using ProyectoCamisetas.Models;

namespace ProyectoCamisetas.Repository
{
    /// <summary>
    /// Acceso a usuario (solo OWNER) para autenticación.
    /// Pensado para inyectarse en el controlador de usuario.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Obtiene el único usuario owner, si existe.
        /// </summary>
        Task<User?> GetOwnerAsync(CancellationToken ct = default);

        /// <summary>
        /// Intenta autenticar al owner por usuario o email.
        /// Devuelve el usuario si las credenciales son válidas; en caso contrario null.
        /// Aplica política de intentos fallidos/bloqueo si corresponde.
        /// </summary>
        Task<User?> LoginOwnerAsync(string usernameOrEmail, string password, CancellationToken ct = default);
    }
}

