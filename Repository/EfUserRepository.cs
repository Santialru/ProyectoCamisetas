using Microsoft.EntityFrameworkCore;
using ProyectoCamisetas.Data;
using ProyectoCamisetas.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoCamisetas.Repository
{
    public class EfUserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        private const int Pbkdf2Iterations = 100_000;
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32;  // 256-bit
        private const int MaxIntentosFallidos = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public EfUserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetOwnerAsync(CancellationToken ct = default)
        {
            return await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Rol == RolUsuario.Owner, ct);
        }

        public async Task<User?> LoginOwnerAsync(string usernameOrEmail, string password, CancellationToken ct = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Rol == RolUsuario.Owner && (
                    EF.Functions.ILike(u.Usuario, usernameOrEmail) ||
                    EF.Functions.ILike(u.Email, usernameOrEmail)
                ), ct);

            if (user is null)
                return null;

            if (!user.PuedeAutenticar)
                return null;

            if (!VerifyPassword(user, password))
            {
                user.IntentosFallidos++;
                if (user.IntentosFallidos >= MaxIntentosFallidos)
                {
                    user.BloqueadoHasta = DateTimeOffset.UtcNow.Add(LockoutDuration);
                    user.IntentosFallidos = 0;
                }
                await _db.SaveChangesAsync(ct);
                return null;
            }

            user.IntentosFallidos = 0;
            user.BloqueadoHasta = null;
            user.UltimoAcceso = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
            return user;
        }

        private static bool VerifyPassword(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return false;

            // Soporte transicional: si no hay salt o el salt no es Base64 válido,
            // asumimos que PasswordHash guarda texto plano (legacy)
            if (string.IsNullOrWhiteSpace(user.PasswordSalt))
            {
                return string.Equals(user.PasswordHash, password, StringComparison.Ordinal);
            }

            try
            {
                var saltBytes = Convert.FromBase64String(user.PasswordSalt!);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Pbkdf2Iterations, HashAlgorithmName.SHA256);
                var key = pbkdf2.GetBytes(KeySize);
                var computedHash = Convert.ToBase64String(key);
                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(computedHash),
                    Encoding.UTF8.GetBytes(user.PasswordHash)
                );
            }
            catch (FormatException)
            {
                // Salt no es Base64 válido: comparar como texto plano para compatibilidad
                return string.Equals(user.PasswordHash, password, StringComparison.Ordinal);
            }
        }

        public static (string Hash, string Salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(KeySize);
            return (Convert.ToBase64String(key), Convert.ToBase64String(salt));
        }

        // Semilla del owner: crear o actualizar
        public async Task SeedOrUpdateOwnerAsync(string usuario, string email, string password, bool activo = true, CancellationToken ct = default)
        {
            var owner = await _db.Users.FirstOrDefaultAsync(u => u.Rol == RolUsuario.Owner, ct);
            var (hash, salt) = HashPassword(password);
            if (owner is null)
            {
                owner = new User
                {
                    Usuario = usuario,
                    Email = email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Rol = RolUsuario.Owner,
                    Activo = activo
                };
                _db.Users.Add(owner);
            }
            else
            {
                owner.Usuario = usuario;
                owner.Email = email;
                owner.PasswordHash = hash;
                owner.PasswordSalt = salt;
                owner.Activo = activo;
                owner.ActualizadoEn = DateTimeOffset.UtcNow;
            }
            await _db.SaveChangesAsync(ct);
        }
    }
}
