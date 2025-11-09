using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class AdministradorDao
    {
        private readonly UsuarioDao _usuarioDao = new UsuarioDao();

        // Crear administrador (transacción: usuario + administrador)
        public async Task<(bool exito, string mensaje, int? idAdministrador, int? idUsuario)> CrearAdministradorAsync(Usuario usuario)
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

                // 2. Crear registro en tabla administrador
                string query = @"INSERT INTO administrador (idUsuario) 
                               VALUES (@idUsuario);
                               SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection, transaction);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario.Value);

                var result = await cmd.ExecuteScalarAsync();
                int idAdministrador = Convert.ToInt32(result);

                await transaction.CommitAsync();
                return (true, "Administrador creado exitosamente", idAdministrador, idUsuario);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                return (false, $"Error al crear administrador: {ex.Message}", null, null);
            }
        }

        // Obtener administrador por ID
        public async Task<(bool exito, string mensaje, Administrador? administrador)> ObtenerAdministradorPorIdAsync(int id)
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
                    FROM administrador a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    WHERE a.idAdministrador = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var administrador = MapearAdministrador(reader);
                    return (true, "Administrador encontrado", administrador);
                }

                return (false, "Administrador no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener administrador: {ex.Message}", null);
            }
        }

        // Obtener administrador por ID de usuario
        public async Task<(bool exito, string mensaje, Administrador? administrador)> ObtenerAdministradorPorIdUsuarioAsync(int idUsuario)
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
                    FROM administrador a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    WHERE a.idUsuario = @idUsuario";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var administrador = MapearAdministrador(reader);
                    return (true, "Administrador encontrado", administrador);
                }

                return (false, "Administrador no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener administrador: {ex.Message}", null);
            }
        }

        // Listar todos los administradores
        public async Task<(bool exito, string mensaje, List<Administrador>? administradores)> ListarAdministradoresAsync()
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
                    FROM administrador a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    ORDER BY u.CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var administradores = new List<Administrador>();
                while (await reader.ReadAsync())
                {
                    administradores.Add(MapearAdministrador(reader));
                }

                return (true, $"Se encontraron {administradores.Count} administradores", administradores);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar administradores: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Administrador>? administradores, int totalRegistros)>
            ListarAdministradoresPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM administrador";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT a.*, u.* 
                    FROM administrador a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    ORDER BY u.CreadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var administradores = new List<Administrador>();
                while (await reader.ReadAsync())
                {
                    administradores.Add(MapearAdministrador(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", administradores, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar administradores: {ex.Message}", null, 0);
            }
        }

        // Verificar si un usuario es administrador
        public async Task<bool> EsAdministradorAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM administrador WHERE idUsuario = @idUsuario";

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

        // Eliminar administrador
        public async Task<(bool exito, string mensaje)> EliminarAdministradorAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM administrador WHERE idAdministrador = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Administrador eliminado exitosamente");
                }

                return (false, "No se encontró el administrador a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar administrador: {ex.Message}");
            }
        }

        // Obtener estadísticas de administradores
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
                    COUNT(*) as TotalAdministradores,
                    COUNT(CASE WHEN u.IntentosLogin >= 5 THEN 1 END) as Bloqueados,
                    COUNT(CASE WHEN u.UltimoLoginAt >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 END) as Activos
                    FROM administrador a
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalAdministradores"] = reader.GetInt32("TotalAdministradores");
                    estadisticas["bloqueados"] = reader.GetInt32("Bloqueados");
                    estadisticas["activos"] = reader.GetInt32("Activos");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear
        private Administrador MapearAdministrador(MySqlDataReader reader)
        {
            return new Administrador
            {
                IdAdministrador = reader.GetInt32("idAdministrador"),
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
