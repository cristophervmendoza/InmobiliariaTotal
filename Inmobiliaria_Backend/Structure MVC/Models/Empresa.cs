using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace backend_csharpcd_inmo.Structure_MVC.Models
{
    [Table("empresa")]
    public class Empresa : IValidatableObject
    {
        [Key]
        [Column("idEmpresa")]
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        [Column("idUsuario")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de la empresa es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s\.\,\-&]+$", ErrorMessage = "El nombre contiene caracteres no permitidos")]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El RUC es obligatorio")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener exactamente 11 caracteres")]
        [RegularExpression(@"^(10|15|17|20)\d{9}$", ErrorMessage = "El RUC debe iniciar con 10, 15, 17 o 20 y contener 11 dígitos")]
        [Column("ruc")]
        public string Ruc { get; set; }

        [StringLength(500, ErrorMessage = "La dirección no puede exceder los 500 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.\,\-\#°\/]+$", ErrorMessage = "La dirección contiene caracteres no permitidos")]
        [Column("direccion")]
        public string Direccion { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El email debe tener entre 5 y 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Formato de email inválido")]
        [Column("email")]
        public string Email { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "El teléfono debe tener exactamente 9 caracteres")]
        [RegularExpression(@"^9\d{8}$", ErrorMessage = "El teléfono debe iniciar con 9 y contener solo dígitos")]
        [Column("telefono")]
        public string Telefono { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "El tipo de empresa debe tener entre 3 y 200 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\.\,\-]+$", ErrorMessage = "El tipo de empresa contiene caracteres no permitidos")]
        [Column("tipoEmpresa")]
        public string TipoEmpresa { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatoria")]
        [DataType(DataType.Date)]
        [Column("fechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Required(ErrorMessage = "La fecha de actualización es obligatoria")]
        [DataType(DataType.Date)]
        [Column("actualizadoAt")]
        public DateTime ActualizadoAt { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [NotMapped]
        public string NombreUsuario => Usuario?.Nombre;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Ruc))
            {
                if (!ValidarDigitoVerificadorRuc(Ruc))
                {
                    yield return new ValidationResult("El dígito verificador del RUC no es válido", new[] { nameof(Ruc) });
                }

                if (Ruc.All(c => c == Ruc[0]))
                {
                    yield return new ValidationResult("El RUC no puede contener todos los dígitos iguales", new[] { nameof(Ruc) });
                }

                if (Ruc.Contains("00000") || Ruc.Contains("11111") || Ruc.Contains("99999"))
                {
                    yield return new ValidationResult("El RUC contiene patrones sospechosos", new[] { nameof(Ruc) });
                }

                var tipoRuc = Ruc.Substring(0, 2);
                if (tipoRuc == "20" && string.IsNullOrEmpty(TipoEmpresa))
                {
                    yield return new ValidationResult("Las empresas con RUC tipo 20 deben especificar el tipo de empresa", new[] { nameof(TipoEmpresa) });
                }
            }

            if (!string.IsNullOrEmpty(Nombre))
            {
                if (Nombre.Trim().Length < 2)
                {
                    yield return new ValidationResult("El nombre no puede contener solo espacios", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"(.)\1{4,}"))
                {
                    yield return new ValidationResult("El nombre contiene caracteres repetidos sospechosos", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"^\s|\s$"))
                {
                    yield return new ValidationResult("El nombre no puede iniciar o terminar con espacios", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"\s{2,}"))
                {
                    yield return new ValidationResult("El nombre no puede contener múltiples espacios consecutivos", new[] { nameof(Nombre) });
                }

                if (Nombre.Length < 10 && !Regex.IsMatch(Nombre, @"[a-zA-Z]{3,}"))
                {
                    yield return new ValidationResult("El nombre parece no ser válido", new[] { nameof(Nombre) });
                }

                var palabrasProhibidas = new[] { "test", "prueba", "ejemplo", "xxx", "aaa", "bbb" };
                if (palabrasProhibidas.Any(p => Nombre.ToLower().Contains(p)))
                {
                    yield return new ValidationResult("El nombre contiene palabras no permitidas", new[] { nameof(Nombre) });
                }

                if (Nombre.All(c => char.IsUpper(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El nombre no puede estar completamente en mayúsculas", new[] { nameof(Nombre) });
                }

                if (Nombre.All(c => char.IsLower(c) || !char.IsLetter(c)))
                {
                    yield return new ValidationResult("El nombre no puede estar completamente en minúsculas", new[] { nameof(Nombre) });
                }

                if (Regex.IsMatch(Nombre, @"[<>{}[\]\\\/\|`~]"))
                {
                    yield return new ValidationResult("El nombre contiene caracteres de código no permitidos", new[] { nameof(Nombre) });
                }
            }

            if (!string.IsNullOrEmpty(Email))
            {
                var emailParts = Email.Split('@');
                if (emailParts.Length == 2)
                {
                    if (emailParts[0].Length < 2)
                    {
                        yield return new ValidationResult("La parte local del email es demasiado corta", new[] { nameof(Email) });
                    }

                    if (emailParts[1].Split('.').Any(part => part.Length < 2))
                    {
                        yield return new ValidationResult("El dominio del email no es válido", new[] { nameof(Email) });
                    }

                    if (emailParts[0].StartsWith(".") || emailParts[0].EndsWith("."))
                    {
                        yield return new ValidationResult("El email no puede iniciar o terminar con punto antes del @", new[] { nameof(Email) });
                    }

                    if (emailParts[0].Contains(".."))
                    {
                        yield return new ValidationResult("El email no puede contener puntos consecutivos", new[] { nameof(Email) });
                    }

                    var dominiosTemporales = new[] { "tempmail.com", "guerrillamail.com", "10minutemail.com", "throwaway.email", "mailinator.com" };
                    if (dominiosTemporales.Any(d => emailParts[1].ToLower().Contains(d)))
                    {
                        yield return new ValidationResult("No se permiten correos temporales", new[] { nameof(Email) });
                    }

                    if (Regex.IsMatch(emailParts[1], @"^\d+\.\d+\.\d+\.\d+$"))
                    {
                        yield return new ValidationResult("El dominio no puede ser una dirección IP", new[] { nameof(Email) });
                    }
                }

                if (Email.Count(c => c == '@') != 1)
                {
                    yield return new ValidationResult("El email debe contener exactamente un símbolo @", new[] { nameof(Email) });
                }
            }

            if (!string.IsNullOrEmpty(Telefono))
            {
                if (Telefono.All(c => c == Telefono[0]))
                {
                    yield return new ValidationResult("El teléfono no puede contener todos los dígitos iguales", new[] { nameof(Telefono) });
                }

                if (Regex.IsMatch(Telefono, @"(012|123|234|345|456|567|678|789|987|876|765|654|543|432|321|210)"))
                {
                    yield return new ValidationResult("El teléfono contiene secuencias sospechosas", new[] { nameof(Telefono) });
                }

                var operadoresValidos = new[] { "91", "92", "93", "94", "95", "96", "97", "98", "99" };
                var prefijo = Telefono.Substring(0, 2);
                if (!operadoresValidos.Contains(prefijo))
                {
                    yield return new ValidationResult("El prefijo del teléfono no corresponde a un operador válido en Perú", new[] { nameof(Telefono) });
                }
            }

            if (FechaRegistro > DateTime.Now)
            {
                yield return new ValidationResult("La fecha de registro no puede ser futura", new[] { nameof(FechaRegistro) });
            }

            if (FechaRegistro < new DateTime(1900, 1, 1))
            {
                yield return new ValidationResult("La fecha de registro no es válida", new[] { nameof(FechaRegistro) });
            }

            if (FechaRegistro.Year < 1950)
            {
                yield return new ValidationResult("La fecha de registro parece ser demasiado antigua", new[] { nameof(FechaRegistro) });
            }

            if (ActualizadoAt > DateTime.Now.AddDays(1))
            {
                yield return new ValidationResult("La fecha de actualización no puede ser futura", new[] { nameof(ActualizadoAt) });
            }

            if (ActualizadoAt < FechaRegistro)
            {
                yield return new ValidationResult("La fecha de actualización no puede ser anterior a la fecha de registro", new[] { nameof(ActualizadoAt) });
            }

            var diasDiferencia = (ActualizadoAt - FechaRegistro).TotalDays;
            if (diasDiferencia > 36500)
            {
                yield return new ValidationResult("El rango de fechas parece inconsistente", new[] { nameof(ActualizadoAt), nameof(FechaRegistro) });
            }

            if (!string.IsNullOrEmpty(Direccion))
            {
                if (Direccion.Trim().Length < 5)
                {
                    yield return new ValidationResult("La dirección debe tener al menos 5 caracteres significativos", new[] { nameof(Direccion) });
                }

                if (Regex.IsMatch(Direccion, @"^\s|\s$"))
                {
                    yield return new ValidationResult("La dirección no puede iniciar o terminar con espacios", new[] { nameof(Direccion) });
                }

                if (Regex.IsMatch(Direccion, @"\s{3,}"))
                {
                    yield return new ValidationResult("La dirección contiene demasiados espacios consecutivos", new[] { nameof(Direccion) });
                }

                if (!Regex.IsMatch(Direccion, @"[a-zA-Z]{3,}"))
                {
                    yield return new ValidationResult("La dirección debe contener al menos una palabra significativa", new[] { nameof(Direccion) });
                }

                if (Regex.IsMatch(Direccion, @"[<>{}[\]\\|`~]"))
                {
                    yield return new ValidationResult("La dirección contiene caracteres no permitidos", new[] { nameof(Direccion) });
                }
            }

            if (!string.IsNullOrEmpty(TipoEmpresa))
            {
                if (TipoEmpresa.Trim().Length < 3)
                {
                    yield return new ValidationResult("El tipo de empresa debe tener al menos 3 caracteres significativos", new[] { nameof(TipoEmpresa) });
                }

                if (Regex.IsMatch(TipoEmpresa, @"^\s|\s$"))
                {
                    yield return new ValidationResult("El tipo de empresa no puede iniciar o terminar con espacios", new[] { nameof(TipoEmpresa) });
                }

                if (Regex.IsMatch(TipoEmpresa, @"\s{2,}"))
                {
                    yield return new ValidationResult("El tipo de empresa no puede contener múltiples espacios consecutivos", new[] { nameof(TipoEmpresa) });
                }

                var tiposValidos = new[] { "SAC", "S.A.C", "S.A.C.", "SRL", "S.R.L", "S.R.L.", "SA", "S.A", "S.A.", "EIRL", "E.I.R.L", "E.I.R.L." };
                var contieneTipoValido = tiposValidos.Any(t => TipoEmpresa.ToUpper().Contains(t));

                if (!contieneTipoValido && TipoEmpresa.Length < 10)
                {
                    yield return new ValidationResult("El tipo de empresa no parece ser válido", new[] { nameof(TipoEmpresa) });
                }
            }

            if (!string.IsNullOrEmpty(Nombre) && !string.IsNullOrEmpty(Ruc))
            {
                if (Nombre.Contains(Ruc))
                {
                    yield return new ValidationResult("El nombre no debe contener el RUC", new[] { nameof(Nombre) });
                }
            }

            if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Nombre))
            {
                var nombreSinEspacios = Nombre.Replace(" ", "").ToLower();
                var emailLocal = Email.Split('@')[0].ToLower();

                if (emailLocal.Length > 5 && nombreSinEspacios.Length > 5)
                {
                    var similitud = CalcularSimilitudLevenshtein(nombreSinEspacios.Substring(0, Math.Min(15, nombreSinEspacios.Length)),
                                                                   emailLocal.Substring(0, Math.Min(15, emailLocal.Length)));

                    if (similitud < 3 && emailLocal.Length > 10)
                    {
                        yield return new ValidationResult("El email parece no corresponder al nombre de la empresa", new[] { nameof(Email) });
                    }
                }
            }
        }

        private bool ValidarDigitoVerificadorRuc(string ruc)
        {
            if (string.IsNullOrEmpty(ruc) || ruc.Length != 11)
                return false;

            int[] factores = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < 10; i++)
            {
                if (!char.IsDigit(ruc[i]))
                    return false;
                suma += (ruc[i] - '0') * factores[i];
            }

            int resto = suma % 11;
            int digitoVerificador = resto == 0 ? 0 : 11 - resto;

            return digitoVerificador == (ruc[10] - '0');
        }

        private int CalcularSimilitudLevenshtein(string s1, string s2)
        {
            int[,] distancia = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                distancia[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                distancia[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int costo = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                    distancia[i, j] = Math.Min(
                        Math.Min(distancia[i - 1, j] + 1, distancia[i, j - 1] + 1),
                        distancia[i - 1, j - 1] + costo);
                }
            }

            return distancia[s1.Length, s2.Length];
        }
    }
}
