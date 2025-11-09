using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("usuario")]
    [Index(nameof(Email), IsUnique = true, Name = "idx_usuario_email")]
    [Index(nameof(Dni), IsUnique = true, Name = "idx_usuario_dni")]
    public class Usuario : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        [RegularExpression(@"^[a-zA-ZÁÉÍÓÚáéíóúñÑüÜ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener exactamente 8 caracteres.")]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe contener exactamente 8 dígitos numéricos.")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El email debe tener entre 5 y 100 caracteres.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El email debe tener un formato válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 255 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono debe tener exactamente 9 caracteres.")]
        [RegularExpression(@"^9[0-9]{8}$", ErrorMessage = "El teléfono debe iniciar con 9 y contener exactamente 9 dígitos.")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        public string? Telefono { get; set; }

        [Range(0, 5, ErrorMessage = "Los intentos de login deben estar entre 0 y 5.")]
        public int IntentosLogin { get; set; } = 0;

        [ForeignKey("EstadoUsuario")]
        [Range(1, int.MaxValue, ErrorMessage = "El IdEstadoUsuario debe ser un número válido mayor que cero.")]
        public int? IdEstadoUsuario { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? UltimoLoginAt { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreadoAt { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ActualizadoAt { get; set; } = DateTime.UtcNow;

        [InverseProperty("Usuario")]
        public virtual EstadoUsuario? EstadoUsuario { get; set; }

        [InverseProperty("Usuario")]
        public virtual ICollection<Propiedad>? Propiedades { get; set; }

        [InverseProperty("Usuario")]
        public virtual ICollection<Agenda>? Agendas { get; set; }

        [NotMapped]
        public bool EstaBloqueado => IntentosLogin >= 5;

        [NotMapped]
        public bool RequiereCambioPassword
        {
            get
            {
                if (UltimoLoginAt == null) return false;
                return (DateTime.UtcNow - UltimoLoginAt.Value).Days > 90;
            }
        }

        [NotMapped]
        public bool EsNuevo => UltimoLoginAt == null;

        [NotMapped]
        public int DiasDesdeUltimoLogin
        {
            get
            {
                if (UltimoLoginAt == null) return -1;
                return (DateTime.UtcNow - UltimoLoginAt.Value).Days;
            }
        }

        [NotMapped]
        public string NombreCorto
        {
            get
            {
                var palabras = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length == 0) return string.Empty;
                if (palabras.Length == 1) return palabras[0];
                return $"{palabras[0]} {palabras[palabras.Length - 1]}";
            }
        }

        [NotMapped]
        public string Iniciales
        {
            get
            {
                var palabras = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length == 0) return string.Empty;
                if (palabras.Length == 1) return palabras[0].Substring(0, Math.Min(2, palabras[0].Length)).ToUpper();
                return $"{palabras[0][0]}{palabras[palabras.Length - 1][0]}".ToUpper();
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (!string.IsNullOrWhiteSpace(Nombre))
            {
                if (Nombre != Nombre.Trim())
                {
                    results.Add(new ValidationResult(
                        "El nombre no debe tener espacios al inicio o final.",
                        new[] { nameof(Nombre) }
                    ));
                }

                if (Regex.IsMatch(Nombre, @"\s{2,}"))
                {
                    results.Add(new ValidationResult(
                        "El nombre no debe tener espacios múltiples.",
                        new[] { nameof(Nombre) }
                    ));
                }

                var palabras = Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Any(p => p.Length < 2))
                {
                    results.Add(new ValidationResult(
                        "Cada palabra del nombre debe tener al menos 2 caracteres.",
                        new[] { nameof(Nombre) }
                    ));
                }

                if (palabras.Length > 10)
                {
                    results.Add(new ValidationResult(
                        "El nombre no puede tener más de 10 palabras.",
                        new[] { nameof(Nombre) }
                    ));
                }

                if (Nombre.Any(char.IsDigit))
                {
                    results.Add(new ValidationResult(
                        "El nombre no debe contener números.",
                        new[] { nameof(Nombre) }
                    ));
                }
            }

            if (!string.IsNullOrWhiteSpace(Dni))
            {
                if (!Dni.All(char.IsDigit))
                {
                    results.Add(new ValidationResult(
                        "El DNI debe contener solo dígitos numéricos.",
                        new[] { nameof(Dni) }
                    ));
                }

                if (Dni.Distinct().Count() == 1)
                {
                    results.Add(new ValidationResult(
                        "El DNI no puede tener todos los dígitos iguales.",
                        new[] { nameof(Dni) }
                    ));
                }

                if (Dni.StartsWith("00000"))
                {
                    results.Add(new ValidationResult(
                        "El DNI no puede iniciar con 00000.",
                        new[] { nameof(Dni) }
                    ));
                }
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (Email != Email.ToLower())
                {
                    results.Add(new ValidationResult(
                        "El email debe estar en minúsculas.",
                        new[] { nameof(Email) }
                    ));
                }

                if (Email != Email.Trim())
                {
                    results.Add(new ValidationResult(
                        "El email no debe tener espacios al inicio o final.",
                        new[] { nameof(Email) }
                    ));
                }

                var dominiosProhibidos = new[] { "tempmail.com", "throwaway.email", "guerrillamail.com", "mailinator.com", "10minutemail.com" };
                var dominio = Email.Split('@').LastOrDefault();
                if (dominio != null && dominiosProhibidos.Contains(dominio.ToLower()))
                {
                    results.Add(new ValidationResult(
                        "No se permiten emails temporales o desechables.",
                        new[] { nameof(Email) }
                    ));
                }

                if (Regex.IsMatch(Email, @"\.{2,}"))
                {
                    results.Add(new ValidationResult(
                        "El email no debe contener puntos consecutivos.",
                        new[] { nameof(Email) }
                    ));
                }

                if (Email.StartsWith(".") || Email.EndsWith("."))
                {
                    results.Add(new ValidationResult(
                        "El email no puede iniciar o terminar con punto.",
                        new[] { nameof(Email) }
                    ));
                }
            }

            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (Password.Length < 8)
                {
                    results.Add(new ValidationResult(
                        "La contraseña debe tener al menos 8 caracteres.",
                        new[] { nameof(Password) }
                    ));
                }

                if (!Regex.IsMatch(Password, @"[A-Z]"))
                {
                    results.Add(new ValidationResult(
                        "La contraseña debe contener al menos una letra mayúscula.",
                        new[] { nameof(Password) }
                    ));
                }

                if (!Regex.IsMatch(Password, @"[a-z]"))
                {
                    results.Add(new ValidationResult(
                        "La contraseña debe contener al menos una letra minúscula.",
                        new[] { nameof(Password) }
                    ));
                }

                if (!Regex.IsMatch(Password, @"[0-9]"))
                {
                    results.Add(new ValidationResult(
                        "La contraseña debe contener al menos un número.",
                        new[] { nameof(Password) }
                    ));
                }

                if (!Regex.IsMatch(Password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                {
                    results.Add(new ValidationResult(
                        "La contraseña debe contener al menos un carácter especial.",
                        new[] { nameof(Password) }
                    ));
                }

                if (Regex.IsMatch(Password, @"(.)\1{2,}"))
                {
                    results.Add(new ValidationResult(
                        "La contraseña no debe tener caracteres repetidos consecutivamente más de 2 veces.",
                        new[] { nameof(Password) }
                    ));
                }

                var patronesComunes = new[] { "123456", "password", "qwerty", "abc123", "111111", "letmein" };
                if (patronesComunes.Any(p => Password.ToLower().Contains(p)))
                {
                    results.Add(new ValidationResult(
                        "La contraseña es demasiado común o predecible.",
                        new[] { nameof(Password) }
                    ));
                }

                if (!string.IsNullOrWhiteSpace(Nombre))
                {
                    var nombreLower = Nombre.ToLower().Replace(" ", "");
                    if (Password.ToLower().Contains(nombreLower))
                    {
                        results.Add(new ValidationResult(
                            "La contraseña no debe contener el nombre del usuario.",
                            new[] { nameof(Password) }
                        ));
                    }
                }
            }

            if (!string.IsNullOrEmpty(Telefono))
            {
                if (!Telefono.StartsWith("9"))
                {
                    results.Add(new ValidationResult(
                        "El teléfono debe iniciar con 9 (formato peruano).",
                        new[] { nameof(Telefono) }
                    ));
                }

                if (Telefono.Distinct().Count() == 1)
                {
                    results.Add(new ValidationResult(
                        "El teléfono no puede tener todos los dígitos iguales.",
                        new[] { nameof(Telefono) }
                    ));
                }
            }

            if (IntentosLogin < 0 || IntentosLogin > 5)
            {
                results.Add(new ValidationResult(
                    "Los intentos de login deben estar entre 0 y 5.",
                    new[] { nameof(IntentosLogin) }
                ));
            }

            if (UltimoLoginAt.HasValue)
            {
                if (UltimoLoginAt.Value > DateTime.UtcNow.Date)
                {
                    results.Add(new ValidationResult(
                        "La fecha del último login no puede ser futura.",
                        new[] { nameof(UltimoLoginAt) }
                    ));
                }

                if (UltimoLoginAt.Value < CreadoAt.Date)
                {
                    results.Add(new ValidationResult(
                        "La fecha del último login no puede ser anterior a la fecha de creación.",
                        new[] { nameof(UltimoLoginAt) }
                    ));
                }
            }

            if (CreadoAt > DateTime.UtcNow.AddMinutes(1))
            {
                results.Add(new ValidationResult(
                    "La fecha de creación no puede ser futura.",
                    new[] { nameof(CreadoAt) }
                ));
            }

            if (ActualizadoAt < CreadoAt)
            {
                results.Add(new ValidationResult(
                    "La fecha de actualización no puede ser anterior a la fecha de creación.",
                    new[] { nameof(ActualizadoAt) }
                ));
            }

            if (ActualizadoAt > DateTime.UtcNow.AddMinutes(1))
            {
                results.Add(new ValidationResult(
                    "La fecha de actualización no puede ser futura.",
                    new[] { nameof(ActualizadoAt) }
                ));
            }

            return results;
        }

        public void ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                throw new ArgumentException("El nombre no puede estar vacío o contener solo espacios.");

            if (Nombre.Length < 2 || Nombre.Length > 100)
                throw new ArgumentException("El nombre debe tener entre 2 y 100 caracteres.");

            if (!Regex.IsMatch(Nombre, @"^[a-zA-ZÁÉÍÓÚáéíóúñÑüÜ\s]+$"))
                throw new ArgumentException("El nombre solo puede contener letras y espacios.");

            if (Nombre != Nombre.Trim())
                throw new ArgumentException("El nombre no debe tener espacios al inicio o final.");

            if (Regex.IsMatch(Nombre, @"\s{2,}"))
                throw new ArgumentException("El nombre no debe tener espacios múltiples.");

            if (string.IsNullOrWhiteSpace(Dni))
                throw new ArgumentException("El DNI no puede estar vacío.");

            if (Dni.Length != 8)
                throw new ArgumentException("El DNI debe tener exactamente 8 caracteres.");

            if (!Regex.IsMatch(Dni, @"^[0-9]{8}$"))
                throw new ArgumentException("El DNI debe contener exactamente 8 dígitos numéricos.");

            if (Dni.Distinct().Count() == 1)
                throw new ArgumentException("El DNI no puede tener todos los dígitos iguales.");

            if (Dni.StartsWith("00000"))
                throw new ArgumentException("El DNI no puede iniciar con 00000.");

            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("El email no puede estar vacío.");

            if (Email.Length < 5 || Email.Length > 100)
                throw new ArgumentException("El email debe tener entre 5 y 100 caracteres.");

            if (!Regex.IsMatch(Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                throw new ArgumentException("El email debe tener un formato válido.");

            if (Email != Email.ToLower())
                throw new ArgumentException("El email debe estar en minúsculas.");

            if (Regex.IsMatch(Email, @"\.{2,}"))
                throw new ArgumentException("El email no debe contener puntos consecutivos.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            if (Password.Length < 8 || Password.Length > 255)
                throw new ArgumentException("La contraseña debe tener entre 8 y 255 caracteres.");

            if (!Regex.IsMatch(Password, @"[A-Z]"))
                throw new ArgumentException("La contraseña debe contener al menos una letra mayúscula.");

            if (!Regex.IsMatch(Password, @"[a-z]"))
                throw new ArgumentException("La contraseña debe contener al menos una letra minúscula.");

            if (!Regex.IsMatch(Password, @"[0-9]"))
                throw new ArgumentException("La contraseña debe contener al menos un número.");

            if (!Regex.IsMatch(Password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                throw new ArgumentException("La contraseña debe contener al menos un carácter especial.");

            if (!string.IsNullOrEmpty(Telefono))
            {
                if (Telefono.Length != 9)
                    throw new ArgumentException("El teléfono debe tener exactamente 9 caracteres.");

                if (!Regex.IsMatch(Telefono, @"^9[0-9]{8}$"))
                    throw new ArgumentException("El teléfono debe iniciar con 9 y contener exactamente 9 dígitos.");

                if (Telefono.Distinct().Count() == 1)
                    throw new ArgumentException("El teléfono no puede tener todos los dígitos iguales.");
            }

            if (IntentosLogin < 0 || IntentosLogin > 5)
                throw new ArgumentException("Los intentos de login deben estar entre 0 y 5.");

            if (IdEstadoUsuario.HasValue && IdEstadoUsuario.Value <= 0)
                throw new ArgumentException("El IdEstadoUsuario debe ser mayor que cero.");

            if (UltimoLoginAt.HasValue && UltimoLoginAt.Value > DateTime.UtcNow.Date)
                throw new ArgumentException("La fecha del último login no puede ser futura.");

            if (UltimoLoginAt.HasValue && UltimoLoginAt.Value < CreadoAt.Date)
                throw new ArgumentException("La fecha del último login no puede ser anterior a la fecha de creación.");

            if (CreadoAt > DateTime.UtcNow.AddMinutes(1))
                throw new ArgumentException("La fecha de creación no puede ser futura.");

            if (ActualizadoAt < CreadoAt)
                throw new ArgumentException("La fecha de actualización no puede ser anterior a la fecha de creación.");

            if (ActualizadoAt > DateTime.UtcNow.AddMinutes(1))
                throw new ArgumentException("La fecha de actualización no puede ser futura.");
        }

        public void ActualizarTiempos()
        {
            ActualizadoAt = DateTime.UtcNow;
        }

        public void RegistrarLogin()
        {
            UltimoLoginAt = DateTime.UtcNow.Date;
            IntentosLogin = 0;
            ActualizarTiempos();
        }

        public void RegistrarIntentoFallido()
        {
            IntentosLogin++;
            ActualizarTiempos();
        }

        public void RestablecerIntentos()
        {
            IntentosLogin = 0;
            ActualizarTiempos();
        }

        public void BloquearUsuario()
        {
            IntentosLogin = 5;
            ActualizarTiempos();
        }

        public void DesbloquearUsuario()
        {
            IntentosLogin = 0;
            ActualizarTiempos();
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerificarPassword(string password)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == Password;
        }

        public void CambiarPassword(string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword))
                throw new ArgumentException("La nueva contraseña no puede estar vacía.");

            if (nuevaPassword.Length < 8)
                throw new ArgumentException("La nueva contraseña debe tener al menos 8 caracteres.");

            Password = HashPassword(nuevaPassword);
            ActualizarTiempos();
        }

        public bool EsPasswordSegura(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            var tieneMayuscula = Regex.IsMatch(password, @"[A-Z]");
            var tieneMinuscula = Regex.IsMatch(password, @"[a-z]");
            var tieneNumero = Regex.IsMatch(password, @"[0-9]");
            var tieneEspecial = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

            return tieneMayuscula && tieneMinuscula && tieneNumero && tieneEspecial;
        }

        public int CalcularFuerzaPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return 0;

            int fuerza = 0;

            if (password.Length >= 8) fuerza += 20;
            if (password.Length >= 12) fuerza += 20;
            if (Regex.IsMatch(password, @"[A-Z]")) fuerza += 15;
            if (Regex.IsMatch(password, @"[a-z]")) fuerza += 15;
            if (Regex.IsMatch(password, @"[0-9]")) fuerza += 15;
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]")) fuerza += 15;

            return Math.Min(fuerza, 100);
        }

        public void LimpiarNombre()
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                Nombre = Regex.Replace(Nombre, @"\s+", " ");
                Nombre = Nombre.Trim();
            }
        }

        public void NormalizarEmail()
        {
            if (!string.IsNullOrEmpty(Email))
            {
                Email = Email.Trim().ToLower();
            }
        }

        public bool EsUsuarioActivo()
        {
            if (!UltimoLoginAt.HasValue)
                return false;

            return DiasDesdeUltimoLogin <= 30;
        }

        public bool EsUsuarioInactivo()
        {
            if (!UltimoLoginAt.HasValue)
                return false;

            return DiasDesdeUltimoLogin > 90;
        }
    }
}
