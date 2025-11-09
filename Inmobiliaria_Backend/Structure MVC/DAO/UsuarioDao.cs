using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class UsuarioDao
    {
        // Crear usuario
        public async Task<(bool exito, string mensaje, int? id)> CrearUsuarioAsync(Usuario usuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar DNI único
                if (await ExisteDniAsync(usuario.Dni))
                {
                    return (false, "El DNI ya está registrado", null);
                }

                // Verificar email único
                if (await ExisteEmailAsync(usuario.Email))
                {
                    return (false, "El email ya está registrado", null);
                }

                string query = @"INSERT INTO usuario 
                    (Nombre, Dni, Email, Password, Telefono, IntentosLogin, IdEstadoUsuario, 
                     UltimoLoginAt, CreadoAt, ActualizadoAt) 
                    VALUES (@Nombre, @Dni, @Email, @Password, @Telefono, @IntentosLogin, @IdEstadoUsuario, 
                            @UltimoLoginAt, @CreadoAt, @ActualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Dni", usuario.Dni);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Password", usuario.Password);
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(usuario.Telefono) ? DBNull.Value : usuario.Telefono);
                cmd.Parameters.AddWithValue("@IntentosLogin", usuario.IntentosLogin);
                cmd.Parameters.AddWithValue("@IdEstadoUsuario", usuario.IdEstadoUsuario.HasValue ? (object)usuario.IdEstadoUsuario.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@UltimoLoginAt", usuario.UltimoLoginAt.HasValue ? (object)usuario.UltimoLoginAt.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@CreadoAt", usuario.CreadoAt);
                cmd.Parameters.AddWithValue("@ActualizadoAt", usuario.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Usuario creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear usuario: {ex.Message}", null);
            }
        }

        // Obtener usuario por ID
        public async Task<(bool exito, string mensaje, Usuario? usuario)> ObtenerUsuarioPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM usuario WHERE IdUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var usuario = MapearUsuario(reader);
                    return (true, "Usuario encontrado", usuario);
                }

                return (false, "Usuario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener usuario: {ex.Message}", null);
            }
        }

        // Obtener usuario por email
        public async Task<(bool exito, string mensaje, Usuario? usuario)> ObtenerUsuarioPorEmailAsync(string email)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM usuario WHERE Email = @email";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email.ToLower());

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var usuario = MapearUsuario(reader);
                    return (true, "Usuario encontrado", usuario);
                }

                return (false, "Usuario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener usuario: {ex.Message}", null);
            }
        }

        // Obtener usuario por DNI
        public async Task<(bool exito, string mensaje, Usuario? usuario)> ObtenerUsuarioPorDniAsync(string dni)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM usuario WHERE Dni = @dni";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@dni", dni);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var usuario = MapearUsuario(reader);
                    return (true, "Usuario encontrado", usuario);
                }

                return (false, "Usuario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener usuario: {ex.Message}", null);
            }
        }

        // Login
        public async Task<(bool exito, string mensaje, Usuario? usuario)> LoginAsync(string email, string password)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM usuario WHERE Email = @email";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email.ToLower());

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var usuario = MapearUsuario(reader);

                    // Verificar si está bloqueado
                    if (usuario.EstaBloqueado)
                    {
                        return (false, "Usuario bloqueado por múltiples intentos fallidos", null);
                    }

                    // Verificar password
                    if (usuario.VerificarPassword(password))
                    {
                        return (true, "Login exitoso", usuario);
                    }
                    else
                    {
                        return (false, "Credenciales inválidas", usuario);
                    }
                }

                return (false, "Usuario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error en login: {ex.Message}", null);
            }
        }

        // Actualizar login exitoso
        public async Task<(bool exito, string mensaje)> ActualizarLoginExitosoAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE usuario SET 
                    UltimoLoginAt = @ultimoLogin,
                    IntentosLogin = 0,
                    ActualizadoAt = @actualizadoAt
                    WHERE IdUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.Parameters.AddWithValue("@ultimoLogin", DateTime.UtcNow.Date);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.UtcNow);

                await cmd.ExecuteNonQueryAsync();
                return (true, "Login registrado exitosamente");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar login: {ex.Message}");
            }
        }

        // Registrar intento fallido
        public async Task<(bool exito, string mensaje)> RegistrarIntentoFallidoAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE usuario SET 
                    IntentosLogin = IntentosLogin + 1,
                    ActualizadoAt = @actualizadoAt
                    WHERE IdUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.UtcNow);

                await cmd.ExecuteNonQueryAsync();
                return (true, "Intento fallido registrado");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al registrar intento: {ex.Message}");
            }
        }

        // Listar usuarios
        public async Task<(bool exito, string mensaje, List<Usuario>? usuarios)> ListarUsuariosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM usuario ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var usuarios = new List<Usuario>();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapearUsuario(reader));
                }

                return (true, $"Se encontraron {usuarios.Count} usuarios", usuarios);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar usuarios: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Usuario>? usuarios, int totalRegistros)>
            ListarUsuariosPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM usuario";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM usuario 
                    ORDER BY CreadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var usuarios = new List<Usuario>();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapearUsuario(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", usuarios, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar usuarios: {ex.Message}", null, 0);
            }
        }

        // Buscar usuarios
        public async Task<(bool exito, string mensaje, List<Usuario>? usuarios)>
            BuscarUsuariosAsync(string? termino = null, bool? bloqueados = null, int? idEstado = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var condiciones = new List<string>();
                var parametros = new List<MySqlParameter>();

                if (!string.IsNullOrWhiteSpace(termino))
                {
                    condiciones.Add("(Nombre LIKE @termino OR Email LIKE @termino OR Dni LIKE @termino)");
                    parametros.Add(new MySqlParameter("@termino", $"%{termino}%"));
                }

                if (bloqueados.HasValue)
                {
                    if (bloqueados.Value)
                    {
                        condiciones.Add("IntentosLogin >= 5");
                    }
                    else
                    {
                        condiciones.Add("IntentosLogin < 5");
                    }
                }

                if (idEstado.HasValue)
                {
                    condiciones.Add("IdEstadoUsuario = @idEstado");
                    parametros.Add(new MySqlParameter("@idEstado", idEstado.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT * FROM usuario 
                    {whereClause}
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var usuarios = new List<Usuario>();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapearUsuario(reader));
                }

                return (true, $"Se encontraron {usuarios.Count} usuarios", usuarios);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar usuarios: {ex.Message}", null);
            }
        }

        // Obtener usuarios bloqueados
        public async Task<(bool exito, string mensaje, List<Usuario>? usuarios)> ObtenerUsuariosBloqueadosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM usuario WHERE IntentosLogin >= 5 ORDER BY ActualizadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var usuarios = new List<Usuario>();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapearUsuario(reader));
                }

                return (true, $"Se encontraron {usuarios.Count} usuarios bloqueados", usuarios);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener usuarios bloqueados: {ex.Message}", null);
            }
        }

        // Obtener usuarios inactivos
        public async Task<(bool exito, string mensaje, List<Usuario>? usuarios)> ObtenerUsuariosInactivosAsync(int diasInactividad = 90)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-diasInactividad);

                string query = @"SELECT * FROM usuario 
                    WHERE UltimoLoginAt IS NOT NULL AND UltimoLoginAt < @fechaLimite
                    ORDER BY UltimoLoginAt ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaLimite", fechaLimite);

                using var reader = await cmd.ExecuteReaderAsync();
                var usuarios = new List<Usuario>();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapearUsuario(reader));
                }

                return (true, $"Se encontraron {usuarios.Count} usuarios inactivos", usuarios);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener usuarios inactivos: {ex.Message}", null);
            }
        }

        // Actualizar usuario
        public async Task<(bool exito, string mensaje)> ActualizarUsuarioAsync(Usuario usuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar email único (excluyendo el usuario actual)
                var (existeEmail, _, usuarioExistente) = await ObtenerUsuarioPorEmailAsync(usuario.Email);
                if (existeEmail && usuarioExistente != null && usuarioExistente.IdUsuario != usuario.IdUsuario)
                {
                    return (false, "El email ya está registrado en otro usuario");
                }

                string query = @"UPDATE usuario SET 
                    Nombre = @Nombre,
                    Email = @Email,
                    Telefono = @Telefono,
                    IdEstadoUsuario = @IdEstadoUsuario,
                    ActualizadoAt = @ActualizadoAt
                    WHERE IdUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Email", usuario.Email);
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(usuario.Telefono) ? DBNull.Value : usuario.Telefono);
                cmd.Parameters.AddWithValue("@IdEstadoUsuario", usuario.IdEstadoUsuario.HasValue ? (object)usuario.IdEstadoUsuario.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@ActualizadoAt", usuario.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Usuario actualizado exitosamente");
                }

                return (false, "No se encontró el usuario a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar usuario: {ex.Message}");
            }
        }

        // Cambiar contraseña
        public async Task<(bool exito, string mensaje)> CambiarPasswordAsync(int idUsuario, string nuevaPassword)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE usuario SET 
                    Password = @password,
                    ActualizadoAt = @actualizadoAt
                    WHERE IdUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.Parameters.AddWithValue("@password", nuevaPassword);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.UtcNow);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Contraseña actualizada exitosamente");
                }

                return (false, "No se encontró el usuario");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al cambiar contraseña: {ex.Message}");
            }
        }

        // Desbloquear usuario
        public async Task<(bool exito, string mensaje)> DesbloquearUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE usuario SET 
                    IntentosLogin = 0,
                    ActualizadoAt = @actualizadoAt
                    WHERE IdUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.UtcNow);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Usuario desbloqueado exitosamente");
                }

                return (false, "No se encontró el usuario");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al desbloquear usuario: {ex.Message}");
            }
        }

        // Eliminar usuario
        public async Task<(bool exito, string mensaje)> EliminarUsuarioAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM usuario WHERE IdUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Usuario eliminado exitosamente");
                }

                return (false, "No se encontró el usuario a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar usuario: {ex.Message}");
            }
        }

        // Verificar si existe DNI
        public async Task<bool> ExisteDniAsync(string dni)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM usuario WHERE Dni = @dni";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@dni", dni);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Verificar si existe email
        public async Task<bool> ExisteEmailAsync(string email)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM usuario WHERE Email = @email";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email.ToLower());

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Obtener estadísticas
        public async Task<(bool exito, string mensaje, Dictionary<string, object>? estadisticas)>
            ObtenerEstadisticasAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT 
                    COUNT(*) as TotalUsuarios,
                    COUNT(CASE WHEN IntentosLogin >= 5 THEN 1 END) as Bloqueados,
                    COUNT(CASE WHEN UltimoLoginAt IS NULL THEN 1 END) as NuncaLogueados,
                    COUNT(CASE WHEN UltimoLoginAt >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 END) as Activos,
                    COUNT(CASE WHEN UltimoLoginAt < DATE_SUB(NOW(), INTERVAL 90 DAY) THEN 1 END) as Inactivos
                    FROM usuario";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalUsuarios"] = reader.GetInt32("TotalUsuarios");
                    estadisticas["bloqueados"] = reader.GetInt32("Bloqueados");
                    estadisticas["nuncaLogueados"] = reader.GetInt32("NuncaLogueados");
                    estadisticas["activos"] = reader.GetInt32("Activos");
                    estadisticas["inactivos"] = reader.GetInt32("Inactivos");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear resultados
        private Usuario MapearUsuario(MySqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32("IdUsuario"),
                Nombre = reader.GetString("Nombre"),
                Dni = reader.GetString("Dni"),
                Email = reader.GetString("Email"),
                Password = reader.GetString("Password"),
                Telefono = !reader.IsDBNull(reader.GetOrdinal("Telefono")) ? reader.GetString("Telefono") : null,
                IntentosLogin = reader.GetInt32("IntentosLogin"),
                IdEstadoUsuario = !reader.IsDBNull(reader.GetOrdinal("IdEstadoUsuario")) ? reader.GetInt32("IdEstadoUsuario") : null,
                UltimoLoginAt = !reader.IsDBNull(reader.GetOrdinal("UltimoLoginAt")) ? reader.GetDateTime("UltimoLoginAt") : null,
                CreadoAt = reader.GetDateTime("CreadoAt"),
                ActualizadoAt = reader.GetDateTime("ActualizadoAt")
            };
        }
    }
}
