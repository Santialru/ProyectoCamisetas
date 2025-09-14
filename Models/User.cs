using System.ComponentModel.DataAnnotations;

namespace ProyectoCamisetas.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(30, MinimumLength = 3)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Almacenar SIEMPRE la contraseña como hash + salt
        [Required]
        [Display(Name = "Hash de contraseña")]
        public string PasswordHash { get; set; } = string.Empty;

        [Display(Name = "Salt")]
        public string? PasswordSalt { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public RolUsuario Rol { get; set; } = RolUsuario.Owner; // Único rol con acceso

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Intentos fallidos")]
        [Range(0, 100)]
        public int IntentosFallidos { get; set; }

        [Display(Name = "Bloqueado hasta")]
        public DateTimeOffset? BloqueadoHasta { get; set; }

        [Display(Name = "Último acceso")]
        public DateTimeOffset? UltimoAcceso { get; set; }

        [Display(Name = "Creado")]
        public DateTimeOffset CreadoEn { get; set; } = DateTimeOffset.UtcNow;

        [Display(Name = "Actualizado")]
        public DateTimeOffset? ActualizadoEn { get; set; }

        // Conveniencia: indica si se puede autenticar ahora
        public bool PuedeAutenticar => Activo && (BloqueadoHasta is null || BloqueadoHasta <= DateTimeOffset.UtcNow);
    }

    public enum RolUsuario
    {
        Owner = 0
    }
}
