using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class ClienteDao
    {
        private readonly UsuarioDao _usuarioDao = new UsuarioDao();

        // Crear cliente (transacción: usuario + cliente)
        public async Task<(bool exito, string mensaje, int? idCliente, int? idUsuario)> CrearClienteAsync(Usuario usuario)
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

                // 2. Crear registro en tabla cliente
                string query = @"INSERT INTO cliente (idUsuario) 
                               VALUES (@idUsuario);
                               SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection, transaction);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario.Value);

                var result = await cmd.ExecuteScalarAsync();
                int idCliente = Convert.ToInt32(result);

                await transaction.CommitAsync();
                return (true, "Cliente creado exitosamente", idCliente, idUsuario);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                return (false, $"Error al crear cliente: {ex.Message}", null, null);
            }
        }

        // Obtener cliente por ID
        public async Task<(bool exito, string mensaje, Cliente? cliente)> ObtenerClientePorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT c.*, u.* 
                    FROM cliente c
                    INNER JOIN usuario u ON c.idUsuario = u.IdUsuario
                    WHERE c.idCliente = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var cliente = MapearCliente(reader);
                    return (true, "Cliente encontrado", cliente);
                }

                return (false, "Cliente no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener cliente: {ex.Message}", null);
            }
        }

        // Obtener cliente por ID de usuario
        public async Task<(bool exito, string mensaje, Cliente? cliente)> ObtenerClientePorIdUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT c.*, u.* 
                    FROM cliente c
                    INNER JOIN usuario u ON c.idUsuario = u.IdUsuario
                    WHERE c.idUsuario = @idUsuario";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var cliente = MapearCliente(reader);
                    return (true, "Cliente encontrado", cliente);
                }

                return (false, "Cliente no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener cliente: {ex.Message}", null);
            }
        }

        // Listar todos los clientes
        public async Task<(bool exito, string mensaje, List<Cliente>? clientes)> ListarClientesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT c.*, u.* 
                    FROM cliente c
                    INNER JOIN usuario u ON c.idUsuario = u.IdUsuario
                    ORDER BY u.CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var clientes = new List<Cliente>();
                while (await reader.ReadAsync())
                {
                    clientes.Add(MapearCliente(reader));
                }

                return (true, $"Se encontraron {clientes.Count} clientes", clientes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar clientes: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Cliente>? clientes, int totalRegistros)>
            ListarClientesPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM cliente";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT c.*, u.* 
                    FROM cliente c
                    INNER JOIN usuario u ON c.idUsuario = u.IdUsuario
                    ORDER BY u.CreadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var clientes = new List<Cliente>();
                while (await reader.ReadAsync())
                {
                    clientes.Add(MapearCliente(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", clientes, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar clientes: {ex.Message}", null, 0);
            }
        }

        // Buscar clientes
        public async Task<(bool exito, string mensaje, List<Cliente>? clientes)>
            BuscarClientesAsync(string? termino = null)
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

                string query = $@"SELECT c.*, u.* 
                    FROM cliente c
                    INNER JOIN usuario u ON c.idUsuario = u.IdUsuario
                    {whereClause}
                    ORDER BY u.Nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var clientes = new List<Cliente>();
                while (await reader.ReadAsync())
                {
                    clientes.Add(MapearCliente(reader));
                }

                return (true, $"Se encontraron {clientes.Count} clientes", clientes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar clientes: {ex.Message}", null);
            }
        }

        // Verificar si un usuario es cliente
        public async Task<bool> EsClienteAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM cliente WHERE idUsuario = @idUsuario";

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

        // Eliminar cliente
        public async Task<(bool exito, string mensaje)> EliminarClienteAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM cliente WHERE idCliente = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Cliente eliminado exitosamente");
                }

                return (false, "No se encontró el cliente a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar cliente: {ex.Message}");
            }
        }

        // Obtener estadísticas de clientes
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
                    COUNT(*) as TotalClientes,
                    COUNT(CASE WHEN u.IntentosLogin >= 5 THEN 1 END) as Bloqueados,
                    COUNT(CASE WHEN u.UltimoLoginAt >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 END) as Activos,
                    (SELECT COUNT(*) FROM postulacion p 
                     INNER JOIN cliente c ON p.IdCliente = c.idCliente) as TotalPostulaciones
                    FROM cliente c
                    INNER JOIN usuario u ON c.idUsuario = u.IdUsuario";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalClientes"] = reader.GetInt32("TotalClientes");
                    estadisticas["bloqueados"] = reader.GetInt32("Bloqueados");
                    estadisticas["activos"] = reader.GetInt32("Activos");
                    estadisticas["totalPostulaciones"] = reader.GetInt32("TotalPostulaciones");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear
        private Cliente MapearCliente(MySqlDataReader reader)
        {
            return new Cliente
            {
                IdCliente = reader.GetInt32("idCliente"),
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
