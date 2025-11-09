using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class AgenteInmobiliarioDao
    {
        private readonly UsuarioDao _usuarioDao = new UsuarioDao();

        // Crear agente inmobiliario (transacción: usuario + agente)
        public async Task<(bool exito, string mensaje, int? idAgente, int? idUsuario)> CrearAgenteAsync(Usuario usuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, null);
            }

            using var connection = connectionResult.Conexion;
            MySqlTransaction? transaction = null;

            try
            {
                transaction = await connection.BeginTransactionAsync();

                // 1. Crear usuario base
                var (exitoUsuario, mensajeUsuario, idUsuario) = await _usuarioDao.CrearUsuarioAsync(usuario);
                if (!exitoUsuario || !idUsuario.HasValue)
                {
                    await transaction.RollbackAsync();
                    return (false, mensajeUsuario, null, null);
                }

                // 2. Crear registro en tabla agenteinmobiliario
                string query = @"INSERT INTO agenteinmobiliario (idUsuario) 
                               VALUES (@idUsuario);
                               SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection, transaction);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario.Value);

                var result = await cmd.ExecuteScalarAsync();
                int idAgente = Convert.ToInt32(result);

                await transaction.CommitAsync();
                return (true, "Agente inmobiliario creado exitosamente", idAgente, idUsuario);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                return (false, $"Error al crear agente inmobiliario: {ex.Message}", null, null);
            }
        }

        // Obtener agente por ID
        public async Task<(bool exito, string mensaje, AgenteInmobiliario? agente)> ObtenerAgentePorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT a.*, u.* 
                    FROM agenteinmobiliario a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    WHERE a.idAgente = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var agente = MapearAgente(reader);
                    return (true, "Agente inmobiliario encontrado", agente);
                }

                return (false, "Agente inmobiliario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener agente inmobiliario: {ex.Message}", null);
            }
        }

        // Obtener agente por ID de usuario
        public async Task<(bool exito, string mensaje, AgenteInmobiliario? agente)> ObtenerAgentePorIdUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT a.*, u.* 
                    FROM agenteinmobiliario a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    WHERE a.idUsuario = @idUsuario";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var agente = MapearAgente(reader);
                    return (true, "Agente inmobiliario encontrado", agente);
                }

                return (false, "Agente inmobiliario no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener agente inmobiliario: {ex.Message}", null);
            }
        }

        // Listar todos los agentes
        public async Task<(bool exito, string mensaje, List<AgenteInmobiliario>? agentes)> ListarAgentesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT a.*, u.* 
                    FROM agenteinmobiliario a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    ORDER BY u.CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var agentes = new List<AgenteInmobiliario>();
                while (await reader.ReadAsync())
                {
                    agentes.Add(MapearAgente(reader));
                }

                return (true, $"Se encontraron {agentes.Count} agentes inmobiliarios", agentes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar agentes inmobiliarios: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<AgenteInmobiliario>? agentes, int totalRegistros)>
            ListarAgentesPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM agenteinmobiliario";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT a.*, u.* 
                    FROM agenteinmobiliario a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    ORDER BY u.CreadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var agentes = new List<AgenteInmobiliario>();
                while (await reader.ReadAsync())
                {
                    agentes.Add(MapearAgente(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", agentes, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar agentes inmobiliarios: {ex.Message}", null, 0);
            }
        }

        // Buscar agentes
        public async Task<(bool exito, string mensaje, List<AgenteInmobiliario>? agentes)>
            BuscarAgentesAsync(string? termino = null)
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
                    whereClause = "WHERE u.Nombre LIKE @termino OR u.Email LIKE @termino OR u.Dni LIKE @termino";
                }

                string query = $@"SELECT a.*, u.* 
                    FROM agenteinmobiliario a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    {whereClause}
                    ORDER BY u.Nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var agentes = new List<AgenteInmobiliario>();
                while (await reader.ReadAsync())
                {
                    agentes.Add(MapearAgente(reader));
                }

                return (true, $"Se encontraron {agentes.Count} agentes inmobiliarios", agentes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar agentes inmobiliarios: {ex.Message}", null);
            }
        }

        // Verificar si un usuario es agente inmobiliario
        public async Task<bool> EsAgenteAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM agenteinmobiliario WHERE idUsuario = @idUsuario";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Eliminar agente inmobiliario
        public async Task<(bool exito, string mensaje)> EliminarAgenteAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM agenteinmobiliario WHERE idAgente = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Agente inmobiliario eliminado exitosamente");
                }

                return (false, "No se encontró el agente inmobiliario a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar agente inmobiliario: {ex.Message}");
            }
        }

        // Obtener estadísticas de agentes
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
                    COUNT(*) as TotalAgentes,
                    COUNT(CASE WHEN u.IntentosLogin >= 5 THEN 1 END) as Bloqueados,
                    COUNT(CASE WHEN u.UltimoLoginAt >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 END) as Activos,
                    (SELECT COUNT(*) FROM cita ct 
                     WHERE ct.IdAgente IN (SELECT idAgente FROM agenteinmobiliario)) as TotalCitas
                    FROM agenteinmobiliario a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalAgentes"] = reader.GetInt32("TotalAgentes");
                    estadisticas["bloqueados"] = reader.GetInt32("Bloqueados");
                    estadisticas["activos"] = reader.GetInt32("Activos");
                    estadisticas["totalCitas"] = reader.GetInt32("TotalCitas");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear
        private AgenteInmobiliario MapearAgente(MySqlDataReader reader)
        {
            return new AgenteInmobiliario
            {
                IdAgente = reader.GetInt32("idAgente"),
                IdUsuario = reader.GetInt32("idUsuario"),
                Usuario = new Usuario
                {
                    IdUsuario = reader.GetInt32("IdUsuario"),
                    Nombre = reader.GetString("Nombre"),
                    Dni = reader.GetString("Dni"),
                    Email = reader.GetString("Email"),
                    Telefono = !reader.IsDBNull(reader.GetOrdinal("Telefono")) ? reader.GetString("Telefono") : null,
                    IntentosLogin = reader.GetInt32("IntentosLogin"),
                    IdEstadoUsuario = !reader.IsDBNull(reader.GetOrdinal("IdEstadoUsuario")) ? reader.GetInt32("IdEstadoUsuario") : null,
                    UltimoLoginAt = !reader.IsDBNull(reader.GetOrdinal("UltimoLoginAt")) ? reader.GetDateTime("UltimoLoginAt") : null,
                    CreadoAt = reader.GetDateTime("CreadoAt"),
                    ActualizadoAt = reader.GetDateTime("ActualizadoAt")
                }
            };
        }
    }
}
