using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class EstadoMantenimientoDao
    {
        // Crear estado de mantenimiento
        public async Task<(bool exito, string mensaje, int? id)> CrearEstadoMantenimientoAsync(EstadoMantenimiento estadoMantenimiento)
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
                if (await ExisteEstadoAsync(estadoMantenimiento.Nombre))
                {
                    return (false, "Ya existe un estado con ese nombre", null);
                }

                string query = @"INSERT INTO estadomantenimiento 
                    (nombre, descripcion, creadoAt, actualizadoAt) 
                    VALUES (@nombre, @descripcion, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", estadoMantenimiento.Nombre);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(estadoMantenimiento.Descripcion) ? DBNull.Value : estadoMantenimiento.Descripcion);
                cmd.Parameters.AddWithValue("@creadoAt", estadoMantenimiento.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", estadoMantenimiento.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Estado de mantenimiento creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear estado de mantenimiento: {ex.Message}", null);
            }
        }

        // Obtener estado por ID
        public async Task<(bool exito, string mensaje, EstadoMantenimiento? estadoMantenimiento)> ObtenerEstadoMantenimientoPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadomantenimiento WHERE idEstadoMantenimiento = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoMantenimiento = MapearEstadoMantenimiento(reader);
                    return (true, "Estado de mantenimiento encontrado", estadoMantenimiento);
                }

                return (false, "Estado de mantenimiento no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de mantenimiento: {ex.Message}", null);
            }
        }

        // Obtener estado por nombre
        public async Task<(bool exito, string mensaje, EstadoMantenimiento? estadoMantenimiento)> ObtenerEstadoMantenimientoPorNombreAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadomantenimiento WHERE LOWER(nombre) = LOWER(@nombre)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nombre);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoMantenimiento = MapearEstadoMantenimiento(reader);
                    return (true, "Estado de mantenimiento encontrado", estadoMantenimiento);
                }

                return (false, "Estado de mantenimiento no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de mantenimiento: {ex.Message}", null);
            }
        }

        // Listar todos los estados
        public async Task<(bool exito, string mensaje, List<EstadoMantenimiento>? estadosMantenimiento)> ListarEstadosMantenimientoAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadomantenimiento ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadosMantenimiento = new List<EstadoMantenimiento>();
                while (await reader.ReadAsync())
                {
                    estadosMantenimiento.Add(MapearEstadoMantenimiento(reader));
                }

                return (true, $"Se encontraron {estadosMantenimiento.Count} estados de mantenimiento", estadosMantenimiento);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de mantenimiento: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<EstadoMantenimiento>? estadosMantenimiento, int totalRegistros)>
            ListarEstadosMantenimientoPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM estadomantenimiento";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM estadomantenimiento 
                    ORDER BY nombre ASC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosMantenimiento = new List<EstadoMantenimiento>();
                while (await reader.ReadAsync())
                {
                    estadosMantenimiento.Add(MapearEstadoMantenimiento(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", estadosMantenimiento, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de mantenimiento: {ex.Message}", null, 0);
            }
        }

        // Buscar estados
        public async Task<(bool exito, string mensaje, List<EstadoMantenimiento>? estadosMantenimiento)>
            BuscarEstadosMantenimientoAsync(string? termino = null)
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
                    whereClause = "WHERE nombre LIKE @termino OR descripcion LIKE @termino";
                }

                string query = $@"SELECT * FROM estadomantenimiento 
                    {whereClause}
                    ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosMantenimiento = new List<EstadoMantenimiento>();
                while (await reader.ReadAsync())
                {
                    estadosMantenimiento.Add(MapearEstadoMantenimiento(reader));
                }

                return (true, $"Se encontraron {estadosMantenimiento.Count} estados de mantenimiento", estadosMantenimiento);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar estados de mantenimiento: {ex.Message}", null);
            }
        }

        // Actualizar estado
        public async Task<(bool exito, string mensaje)> ActualizarEstadoMantenimientoAsync(EstadoMantenimiento estadoMantenimiento)
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
                string checkQuery = @"SELECT COUNT(*) FROM estadomantenimiento 
                    WHERE LOWER(nombre) = LOWER(@nombre) AND idEstadoMantenimiento != @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@nombre", estadoMantenimiento.Nombre);
                checkCmd.Parameters.AddWithValue("@id", estadoMantenimiento.IdEstadoMantenimiento);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, "Ya existe otro estado con ese nombre");
                }

                string query = @"UPDATE estadomantenimiento SET 
                    nombre = @nombre,
                    descripcion = @descripcion,
                    actualizadoAt = @actualizadoAt
                    WHERE idEstadoMantenimiento = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", estadoMantenimiento.IdEstadoMantenimiento);
                cmd.Parameters.AddWithValue("@nombre", estadoMantenimiento.Nombre);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(estadoMantenimiento.Descripcion) ? DBNull.Value : estadoMantenimiento.Descripcion);
                cmd.Parameters.AddWithValue("@actualizadoAt", estadoMantenimiento.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de mantenimiento actualizado exitosamente");
                }

                return (false, "No se encontró el estado de mantenimiento a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar estado de mantenimiento: {ex.Message}");
            }
        }

        // Eliminar estado
        public async Task<(bool exito, string mensaje)> EliminarEstadoMantenimientoAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar si hay mantenimientos usando este estado
                string checkQuery = "SELECT COUNT(*) FROM mantenimiento WHERE idEstadoMantenimiento = @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, $"No se puede eliminar el estado porque tiene {count} mantenimiento(s) asociado(s)");
                }

                string query = "DELETE FROM estadomantenimiento WHERE idEstadoMantenimiento = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de mantenimiento eliminado exitosamente");
                }

                return (false, "No se encontró el estado de mantenimiento a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar estado de mantenimiento: {ex.Message}");
            }
        }

        // Verificar si existe un estado con el nombre
        public async Task<bool> ExisteEstadoAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM estadomantenimiento WHERE LOWER(nombre) = LOWER(@nombre)";

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

        // Contar mantenimientos por estado
        public async Task<(bool exito, string mensaje, int total)> ContarMantenimientosPorEstadoAsync(int idEstado)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM mantenimiento WHERE idEstadoMantenimiento = @idEstado";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idEstado", idEstado);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El estado tiene {total} mantenimiento(s)", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar mantenimientos: {ex.Message}", 0);
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
                    em.idEstadoMantenimiento,
                    em.nombre,
                    COUNT(m.idMantenimiento) as totalMantenimientos,
                    COUNT(CASE WHEN m.fechaProgramada >= CURDATE() THEN 1 END) as mantenimientosActivos
                    FROM estadomantenimiento em
                    LEFT JOIN mantenimiento m ON em.idEstadoMantenimiento = m.idEstadoMantenimiento
                    GROUP BY em.idEstadoMantenimiento, em.nombre
                    ORDER BY totalMantenimientos DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var stats = new Dictionary<string, object>
                    {
                        ["idEstadoMantenimiento"] = reader.GetInt32("idEstadoMantenimiento"),
                        ["nombre"] = reader.GetString("nombre"),
                        ["totalMantenimientos"] = reader.GetInt32("totalMantenimientos"),
                        ["mantenimientosActivos"] = reader.GetInt32("mantenimientosActivos")
                    };
                    estadisticas.Add(stats);
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
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
                    { "Pendiente", "Mantenimiento pendiente de programación" },
                    { "Programado", "Mantenimiento programado con fecha establecida" },
                    { "En Proceso", "Mantenimiento en ejecución" },
                    { "Completado", "Mantenimiento completado exitosamente" },
                    { "Cancelado", "Mantenimiento cancelado" },
                    { "Suspendido", "Mantenimiento suspendido temporalmente" },
                    { "Reprogramado", "Mantenimiento reprogramado para otra fecha" },
                    { "En Revision", "Mantenimiento en revisión" },
                    { "Aprobado", "Mantenimiento aprobado para ejecución" },
                    { "Rechazado", "Mantenimiento rechazado" }
                };

                int creados = 0;
                foreach (var estado in estadosPredeterminados)
                {
                    if (!await ExisteEstadoAsync(estado.Key))
                    {
                        var estadoMantenimiento = new EstadoMantenimiento
                        {
                            Nombre = estado.Key,
                            Descripcion = estado.Value,
                            CreadoAt = new DateTime(2020, 1, 1),
                            ActualizadoAt = new DateTime(2020, 1, 1)
                        };

                        var (exito, _, _) = await CrearEstadoMantenimientoAsync(estadoMantenimiento);
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
        private EstadoMantenimiento MapearEstadoMantenimiento(MySqlDataReader reader)
        {
            return new EstadoMantenimiento
            {
                IdEstadoMantenimiento = reader.GetInt32("idEstadoMantenimiento"),
                Nombre = reader.GetString("nombre"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("descripcion")) ? reader.GetString("descripcion") : string.Empty,
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
