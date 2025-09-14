using System.Security.Cryptography;
using System.Text;
using ProyectoCamisetas.Models;

namespace ProyectoCamisetas.Repository
{
    /// <summary>
    /// Implementación simple en memoria para el acceso del usuario OWNER.
    /// Más adelante puede reemplazarse por EF Core sin cambiar el contrato.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        // Configuración básica de seguridad
        private const int Pbkdf2Iterations = 100_000;
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32;  // 256-bit
        private const int MaxIntentosFallidos = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private readonly object _lock = new();
        private User? _owner; // único usuario permitido

        public Task<User?> GetOwnerAsync(CancellationToken ct = default)
        {
            lock (_lock)
            {
                return Task.FromResult(_owner);
            }
        }

        public Task<User?> LoginOwnerAsync(string usernameOrEmail, string password, CancellationToken ct = default)
        {
            lock (_lock)
            {
                if (_owner is null)
                    return Task.FromResult<User?>(null);

                // Comprobar si puede autenticarse (activo/no bloqueado)
                if (!_owner.PuedeAutenticar)
                    return Task.FromResult<User?>(null);

                var matchesIdentifier = string.Equals(_owner.Usuario, usernameOrEmail, StringComparison.OrdinalIgnoreCase)
                                         || string.Equals(_owner.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase);

                if (!matchesIdentifier)
                {
                    // No corresponde al owner
                    return Task.FromResult<User?>(null);
                }

                var ok = VerifyPassword(_owner, password);

                if (ok)
                {
                    _owner.IntentosFallidos = 0;
                    _owner.BloqueadoHasta = null;
                    _owner.UltimoAcceso = DateTimeOffset.UtcNow;
                    return Task.FromResult<User?>(_owner);
                }

                _owner.IntentosFallidos++;
                if (_owner.IntentosFallidos >= MaxIntentosFallidos)
                {
                    _owner.BloqueadoHasta = DateTimeOffset.UtcNow.Add(LockoutDuration);
                    _owner.IntentosFallidos = 0; // opcional: reiniciar contador al bloquear
                }
                return Task.FromResult<User?>(null);
            }
        }

        // Métodos de soporte y utilidad
        private static bool VerifyPassword(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash) || string.IsNullOrWhiteSpace(user.PasswordSalt))
                return false;

            var saltBytes = Convert.FromBase64String(user.PasswordSalt!);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);
            var computedHash = Convert.ToBase64String(key);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHash),
                Encoding.UTF8.GetBytes(user.PasswordHash)
            );
        }

        private static (string Hash, string Salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);
            return (Convert.ToBase64String(key), Convert.ToBase64String(salt));
        }

        // API opcional para configurar el OWNER (semilla). No está en la interfaz a propósito.
        public void SeedOrUpdateOwner(string usuario, string email, string password, bool activo = true)
        {
            var (hash, salt) = HashPassword(password);
            lock (_lock)
            {
                if (_owner is null)
                {
                    _owner = new User
                    {
                        Usuario = usuario,
                        Email = email,
                        PasswordHash = hash,
                        PasswordSalt = salt,
                        Rol = RolUsuario.Owner,
                        Activo = activo
                    };
                }
                else
                {
                    _owner.Usuario = usuario;
                    _owner.Email = email;
                    _owner.PasswordHash = hash;
                    _owner.PasswordSalt = salt;
                    _owner.Activo = activo;
                }
            }
        }
    }
}

