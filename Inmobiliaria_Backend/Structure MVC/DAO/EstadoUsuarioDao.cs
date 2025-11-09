using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class EstadoUsuarioDao
    {
        // Crear estado de usuario
        public async Task<(bool exito, string mensaje, int? id)> CrearEstadoUsuarioAsync(EstadoUsuario estadoUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar que no exista un estado con el mismo nombre
                if (await ExisteEstadoUsuarioAsync(estadoUsuario.Nombre))
                {
                    return (false, "Ya existe un estado de usuario con ese nombre", null);
                }

                string query = @"INSERT INTO estadousuario 
                    (Nombre, Descripcion, Activo) 
                    VALUES (@Nombre, @Descripcion, @Activo);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Nombre", estadoUsuario.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(estadoUsuario.Descripcion) ? DBNull.Value : estadoUsuario.Descripcion);
                cmd.Parameters.AddWithValue("@Activo", estadoUsuario.Activo);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Estado de usuario creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear estado de usuario: {ex.Message}", null);
            }
        }

        // Obtener estado de usuario por ID
        public async Task<(bool exito, string mensaje, EstadoUsuario? estadoUsuario)> ObtenerEstadoUsuarioPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estado_usuario WHERE idEstadoUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoUsuario = MapearEstadoUsuario(reader);
                    return (true, "Estado de usuario encontrado", estadoUsuario);
                }

                return (false, "Estado de usuario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de usuario: {ex.Message}", null);
            }
        }

        // Obtener estado de usuario por nombre
        public async Task<(bool exito, string mensaje, EstadoUsuario? estadoUsuario)> ObtenerEstadoUsuarioPorNombreAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estado_usuario WHERE LOWER(Nombre) = LOWER(@nombre)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nombre);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoUsuario = MapearEstadoUsuario(reader);
                    return (true, "Estado de usuario encontrado", estadoUsuario);
                }

                return (false, "Estado de usuario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de usuario: {ex.Message}", null);
            }
        }

        // Listar todos los estados de usuario
        public async Task<(bool exito, string mensaje, List<EstadoUsuario>? estadosUsuario)> ListarEstadosUsuarioAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadousuario ORDER BY Nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadosUsuario = new List<EstadoUsuario>();
                while (await reader.ReadAsync())
                {
                    estadosUsuario.Add(MapearEstadoUsuario(reader));
                }

                return (true, $"Se encontraron {estadosUsuario.Count} estados de usuario", estadosUsuario);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de usuario: {ex.Message}", null);
            }
        }

        // Listar estados activos
        public async Task<(bool exito, string mensaje, List<EstadoUsuario>? estadosUsuario)> ListarEstadosActivosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadousuario WHERE Activo = 1 ORDER BY Nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadosUsuario = new List<EstadoUsuario>();
                while (await reader.ReadAsync())
                {
                    estadosUsuario.Add(MapearEstadoUsuario(reader));
                }

                return (true, $"Se encontraron {estadosUsuario.Count} estados activos", estadosUsuario);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados activos: {ex.Message}", null);
            }
        }

        // Listar estados inactivos
        public async Task<(bool exito, string mensaje, List<EstadoUsuario>? estadosUsuario)> ListarEstadosInactivosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadousuario WHERE Activo = 0 ORDER BY Nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadosUsuario = new List<EstadoUsuario>();
                while (await reader.ReadAsync())
                {
                    estadosUsuario.Add(MapearEstadoUsuario(reader));
                }

                return (true, $"Se encontraron {estadosUsuario.Count} estados inactivos", estadosUsuario);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados inactivos: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<EstadoUsuario>? estadosUsuario, int totalRegistros)>
            ListarEstadosUsuarioPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM estadousuario";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM estadousuario 
                    ORDER BY Nombre ASC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosUsuario = new List<EstadoUsuario>();
                while (await reader.ReadAsync())
                {
                    estadosUsuario.Add(MapearEstadoUsuario(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", estadosUsuario, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de usuario: {ex.Message}", null, 0);
            }
        }

        // Buscar estados de usuario
        public async Task<(bool exito, string mensaje, List<EstadoUsuario>? estadosUsuario)>
            BuscarEstadosUsuarioAsync(string? termino = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string whereClause = "";
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    whereClause = "WHERE Nombre LIKE @termino OR Descripcion LIKE @termino";
                }

                string query = $@"SELECT * FROM estadousuario 
                    {whereClause}
                    ORDER BY Nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosUsuario = new List<EstadoUsuario>();
                while (await reader.ReadAsync())
                {
                    estadosUsuario.Add(MapearEstadoUsuario(reader));
                }

                return (true, $"Se encontraron {estadosUsuario.Count} estados de usuario", estadosUsuario);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar estados de usuario: {ex.Message}", null);
            }
        }

        // Actualizar estado de usuario
        public async Task<(bool exito, string mensaje)> ActualizarEstadoUsuarioAsync(EstadoUsuario estadoUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar que no exista otro estado con el mismo nombre
                string checkQuery = @"SELECT COUNT(*) FROM estadousuario 
                    WHERE LOWER(Nombre) = LOWER(@Nombre) AND idEstadoUsuario != @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@Nombre", estadoUsuario.Nombre);
                checkCmd.Parameters.AddWithValue("@id", estadoUsuario.IdEstadoUsuario);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, "Ya existe otro estado de usuario con ese nombre");
                }

                string query = @"UPDATE estado_usuario SET 
                    Nombre = @Nombre,
                    Descripcion = @Descripcion,
                    Activo = @Activo
                    WHERE idEstadoUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", estadoUsuario.IdEstadoUsuario);
                cmd.Parameters.AddWithValue("@Nombre", estadoUsuario.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(estadoUsuario.Descripcion) ? DBNull.Value : estadoUsuario.Descripcion);
                cmd.Parameters.AddWithValue("@Activo", estadoUsuario.Activo);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de usuario actualizado exitosamente");
                }

                return (false, "No se encontró el estado de usuario a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar estado de usuario: {ex.Message}");
            }
        }

        // Activar estado de usuario
        public async Task<(bool exito, string mensaje)> ActivarEstadoUsuarioAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "UPDATE estadousuario SET Activo = 1 WHERE idEstadoUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de usuario activado exitosamente");
                }

                return (false, "No se encontró el estado de usuario");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al activar estado de usuario: {ex.Message}");
            }
        }

        // Desactivar estado de usuario
        public async Task<(bool exito, string mensaje)> DesactivarEstadoUsuarioAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "UPDATE estadousuario SET Activo = 0 WHERE idEstadoUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de usuario desactivado exitosamente");
                }

                return (false, "No se encontró el estado de usuario");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al desactivar estado de usuario: {ex.Message}");
            }
        }

        // Eliminar estado de usuario
        public async Task<(bool exito, string mensaje)> EliminarEstadoUsuarioAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar si hay usuarios usando este estado
                string checkQuery = "SELECT COUNT(*) FROM usuario WHERE idEstadoUsuario = @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, $"No se puede eliminar el estado porque tiene {count} usuario(s) asociado(s)");
                }

                string query = "DELETE FROM estadousuario WHERE idEstadoUsuario = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de usuario eliminado exitosamente");
                }

                return (false, "No se encontró el estado de usuario a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar estado de usuario: {ex.Message}");
            }
        }

        // Verificar si existe un estado con el nombre
        public async Task<bool> ExisteEstadoUsuarioAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM estadousuario WHERE LOWER(Nombre) = LOWER(@nombre)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nombre);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Contar usuarios por estado
        public async Task<(bool exito, string mensaje, int total)> ContarUsuariosPorEstadoAsync(int idEstadoUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM usuario WHERE idEstadoUsuario = @idEstadoUsuario";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idEstadoUsuario", idEstadoUsuario);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El estado tiene {total} usuario(s)", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar usuarios: {ex.Message}", 0);
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
                    COUNT(*) as TotalEstados,
                    COUNT(CASE WHEN Activo = 1 THEN 1 END) as EstadosActivos,
                    COUNT(CASE WHEN Activo = 0 THEN 1 END) as EstadosInactivos
                    FROM estado_usuario";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalEstados"] = reader.GetInt32("TotalEstados");
                    estadisticas["estadosActivos"] = reader.GetInt32("EstadosActivos");
                    estadisticas["estadosInactivos"] = reader.GetInt32("EstadosInactivos");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Obtener estadísticas de uso
        public async Task<(bool exito, string mensaje, List<Dictionary<string, object>>? estadisticas)>
            ObtenerEstadisticasUsoAsync()
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
                    eu.idEstadoUsuario,
                    eu.Nombre,
                    eu.Descripcion,
                    eu.Activo,
                    COUNT(u.IdUsuario) as totalUsuarios
                    FROM estado_usuario eu
                    LEFT JOIN usuario u ON eu.idEstadoUsuario = u.idEstadoUsuario
                    GROUP BY eu.idEstadoUsuario, eu.Nombre, eu.Descripcion, eu.Activo
                    ORDER BY totalUsuarios DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var stats = new Dictionary<string, object>
                    {
                        ["idEstadoUsuario"] = reader.GetInt32("idEstadoUsuario"),
                        ["nombre"] = reader.GetString("Nombre"),
                        ["descripcion"] = !reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? reader.GetString("Descripcion") : string.Empty,
                        ["activo"] = reader.GetBoolean("Activo"),
                        ["totalUsuarios"] = reader.GetInt32("totalUsuarios")
                    };
                    estadisticas.Add(stats);
                }

                return (true, "Estadísticas de uso obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas de uso: {ex.Message}", null);
            }
        }

        // Inicializar estados predeterminados
        public async Task<(bool exito, string mensaje)> InicializarEstadosPredeterminadosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var estadosPredeterminados = new Dictionary<string, string>
                {
                    { "Activo", "Usuario activo en el sistema" },
                    { "Inactivo", "Usuario temporalmente inactivo" },
                    { "Suspendido", "Usuario suspendido por incumplimiento" },
                    { "Bloqueado", "Usuario bloqueado por seguridad" },
                    { "Pendiente", "Usuario pendiente de activación" },
                    { "Verificado", "Usuario verificado y activo" }
                };

                int creados = 0;
                foreach (var estado in estadosPredeterminados)
                {
                    if (!await ExisteEstadoUsuarioAsync(estado.Key))
                    {
                        var estadoUsuario = new EstadoUsuario
                        {
                            Nombre = estado.Key,
                            Descripcion = estado.Value,
                            Activo = true
                        };

                        var (exito, _, _) = await CrearEstadoUsuarioAsync(estadoUsuario);
                        if (exito) creados++;
                    }
                }

                return (true, $"Se crearon {creados} estados predeterminados");
            }
            catch (Exception ex)
            {
                return (false, $"Error al inicializar estados: {ex.Message}");
            }
        }

        // Método auxiliar para mapear
        private EstadoUsuario MapearEstadoUsuario(MySqlDataReader reader)
        {
            return new EstadoUsuario
            {
                IdEstadoUsuario = reader.GetInt32("idEstadoUsuario"),
                Nombre = reader.GetString("Nombre"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? reader.GetString("Descripcion") : null,
                Activo = reader.GetBoolean("Activo")
            };
        }
    }
}
