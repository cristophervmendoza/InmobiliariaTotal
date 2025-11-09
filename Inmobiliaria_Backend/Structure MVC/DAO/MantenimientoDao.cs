using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class MantenimientoDao
    {
        // Crear mantenimiento
        public async Task<(bool exito, string mensaje, int? id)> CrearMantenimientoAsync(Mantenimiento mantenimiento)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO mantenimiento 
                    (idPropiedad, tipo, descripcion, costo, fechaProgramada, fechaRealizada, 
                     idEstadoMantenimiento, idUsuario, creadoAt, actualizadoAt) 
                    VALUES (@idPropiedad, @tipo, @descripcion, @costo, @fechaProgramada, @fechaRealizada, 
                            @idEstadoMantenimiento, @idUsuario, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", mantenimiento.IdPropiedad);
                cmd.Parameters.AddWithValue("@tipo", mantenimiento.Tipo);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(mantenimiento.Descripcion) ? DBNull.Value : mantenimiento.Descripcion);
                cmd.Parameters.AddWithValue("@costo", mantenimiento.Costo.HasValue ? (object)mantenimiento.Costo.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaProgramada", mantenimiento.FechaProgramada.HasValue ? (object)mantenimiento.FechaProgramada.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaRealizada", mantenimiento.FechaRealizada.HasValue ? (object)mantenimiento.FechaRealizada.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@idEstadoMantenimiento", mantenimiento.IdEstadoMantenimiento);
                cmd.Parameters.AddWithValue("@idUsuario", mantenimiento.IdUsuario.HasValue ? (object)mantenimiento.IdUsuario.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@creadoAt", mantenimiento.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", mantenimiento.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Mantenimiento creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear mantenimiento: {ex.Message}", null);
            }
        }

        // Obtener mantenimiento por ID
        public async Task<(bool exito, string mensaje, Mantenimiento? mantenimiento)> ObtenerMantenimientoPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT m.*, 
                    em.idEstadoMantenimiento, em.nombre as estadoNombre,
                    u.IdUsuario, u.Nombre as usuarioNombre
                    FROM mantenimiento m
                    LEFT JOIN estadomantenimiento em ON m.idEstadoMantenimiento = em.idEstadoMantenimiento
                    LEFT JOIN usuario u ON m.idUsuario = u.IdUsuario
                    WHERE m.idMantenimiento = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var mantenimiento = MapearMantenimiento(reader);
                    return (true, "Mantenimiento encontrado", mantenimiento);
                }

                return (false, "Mantenimiento no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener mantenimiento: {ex.Message}", null);
            }
        }

        // Listar todos los mantenimientos
        public async Task<(bool exito, string mensaje, List<Mantenimiento>? mantenimientos)> ListarMantenimientosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT m.*, 
                    em.nombre as estadoNombre,
                    u.Nombre as usuarioNombre
                    FROM mantenimiento m
                    LEFT JOIN estadomantenimiento em ON m.idEstadoMantenimiento = em.idEstadoMantenimiento
                    LEFT JOIN usuario u ON m.idUsuario = u.IdUsuario
                    ORDER BY m.fechaProgramada DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var mantenimientos = new List<Mantenimiento>();
                while (await reader.ReadAsync())
                {
                    mantenimientos.Add(MapearMantenimientoSimple(reader));
                }

                return (true, $"Se encontraron {mantenimientos.Count} mantenimientos", mantenimientos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar mantenimientos: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Mantenimiento>? mantenimientos, int totalRegistros)>
            ListarMantenimientosPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM mantenimiento";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT m.*, 
                    em.nombre as estadoNombre,
                    u.Nombre as usuarioNombre
                    FROM mantenimiento m
                    LEFT JOIN estadomantenimiento em ON m.idEstadoMantenimiento = em.idEstadoMantenimiento
                    LEFT JOIN usuario u ON m.idUsuario = u.IdUsuario
                    ORDER BY m.fechaProgramada DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var mantenimientos = new List<Mantenimiento>();
                while (await reader.ReadAsync())
                {
                    mantenimientos.Add(MapearMantenimientoSimple(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", mantenimientos, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar mantenimientos: {ex.Message}", null, 0);
            }
        }

        // Buscar mantenimientos
        public async Task<(bool exito, string mensaje, List<Mantenimiento>? mantenimientos)>
            BuscarMantenimientosAsync(int? idPropiedad = null, int? idEstado = null, string? tipo = null,
                                     DateTime? fechaInicio = null, DateTime? fechaFin = null, int? idUsuario = null)
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

                if (idPropiedad.HasValue)
                {
                    condiciones.Add("m.idPropiedad = @idPropiedad");
                    parametros.Add(new MySqlParameter("@idPropiedad", idPropiedad.Value));
                }

                if (idEstado.HasValue)
                {
                    condiciones.Add("m.idEstadoMantenimiento = @idEstado");
                    parametros.Add(new MySqlParameter("@idEstado", idEstado.Value));
                }

                if (!string.IsNullOrWhiteSpace(tipo))
                {
                    condiciones.Add("m.tipo LIKE @tipo");
                    parametros.Add(new MySqlParameter("@tipo", $"%{tipo}%"));
                }

                if (fechaInicio.HasValue)
                {
                    condiciones.Add("m.fechaProgramada >= @fechaInicio");
                    parametros.Add(new MySqlParameter("@fechaInicio", fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    condiciones.Add("m.fechaProgramada <= @fechaFin");
                    parametros.Add(new MySqlParameter("@fechaFin", fechaFin.Value));
                }

                if (idUsuario.HasValue)
                {
                    condiciones.Add("m.idUsuario = @idUsuario");
                    parametros.Add(new MySqlParameter("@idUsuario", idUsuario.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT m.*, 
                    em.nombre as estadoNombre,
                    u.Nombre as usuarioNombre
                    FROM mantenimiento m
                    LEFT JOIN estadomantenimiento em ON m.idEstadoMantenimiento = em.idEstadoMantenimiento
                    LEFT JOIN usuario u ON m.idUsuario = u.IdUsuario
                    {whereClause}
                    ORDER BY m.fechaProgramada DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var mantenimientos = new List<Mantenimiento>();
                while (await reader.ReadAsync())
                {
                    mantenimientos.Add(MapearMantenimientoSimple(reader));
                }

                return (true, $"Se encontraron {mantenimientos.Count} mantenimientos", mantenimientos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar mantenimientos: {ex.Message}", null);
            }
        }

        // Obtener mantenimientos por propiedad
        public async Task<(bool exito, string mensaje, List<Mantenimiento>? mantenimientos)>
            ObtenerMantenimientosPorPropiedadAsync(int idPropiedad)
        {
            return await BuscarMantenimientosAsync(idPropiedad: idPropiedad);
        }

        // Obtener mantenimientos pendientes
        public async Task<(bool exito, string mensaje, List<Mantenimiento>? mantenimientos)>
            ObtenerMantenimientosPendientesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT m.*, 
                    em.nombre as estadoNombre,
                    u.Nombre as usuarioNombre
                    FROM mantenimiento m
                    INNER JOIN estadomantenimiento em ON m.idEstadoMantenimiento = em.idEstadoMantenimiento
                    LEFT JOIN usuario u ON m.idUsuario = u.IdUsuario
                    WHERE LOWER(em.nombre) IN ('pendiente', 'programado')
                    AND m.fechaProgramada >= CURDATE()
                    ORDER BY m.fechaProgramada ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var mantenimientos = new List<Mantenimiento>();
                while (await reader.ReadAsync())
                {
                    mantenimientos.Add(MapearMantenimientoSimple(reader));
                }

                return (true, $"Se encontraron {mantenimientos.Count} mantenimientos pendientes", mantenimientos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener mantenimientos pendientes: {ex.Message}", null);
            }
        }

        // Obtener mantenimientos vencidos
        public async Task<(bool exito, string mensaje, List<Mantenimiento>? mantenimientos)>
            ObtenerMantenimientosVencidosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT m.*, 
                    em.nombre as estadoNombre,
                    u.Nombre as usuarioNombre
                    FROM mantenimiento m
                    INNER JOIN estadomantenimiento em ON m.idEstadoMantenimiento = em.idEstadoMantenimiento
                    LEFT JOIN usuario u ON m.idUsuario = u.IdUsuario
                    WHERE LOWER(em.nombre) IN ('pendiente', 'programado')
                    AND m.fechaProgramada < CURDATE()
                    ORDER BY m.fechaProgramada ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var mantenimientos = new List<Mantenimiento>();
                while (await reader.ReadAsync())
                {
                    mantenimientos.Add(MapearMantenimientoSimple(reader));
                }

                return (true, $"Se encontraron {mantenimientos.Count} mantenimientos vencidos", mantenimientos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener mantenimientos vencidos: {ex.Message}", null);
            }
        }

        // Obtener mantenimientos próximos
        public async Task<(bool exito, string mensaje, List<Mantenimiento>? mantenimientos)>
            ObtenerMantenimientosProximosAsync(int dias = 7)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var fechaLimite = DateTime.Now.Date.AddDays(dias);

                string query = @"SELECT m.*, 
                    em.nombre as estadoNombre,
                    u.Nombre as usuarioNombre
                    FROM mantenimiento m
                    INNER JOIN estadomantenimiento em ON m.idEstadoMantenimiento = em.idEstadoMantenimiento
                    LEFT JOIN usuario u ON m.idUsuario = u.IdUsuario
                    WHERE LOWER(em.nombre) IN ('pendiente', 'programado')
                    AND m.fechaProgramada BETWEEN CURDATE() AND @fechaLimite
                    ORDER BY m.fechaProgramada ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaLimite", fechaLimite);

                using var reader = await cmd.ExecuteReaderAsync();
                var mantenimientos = new List<Mantenimiento>();
                while (await reader.ReadAsync())
                {
                    mantenimientos.Add(MapearMantenimientoSimple(reader));
                }

                return (true, $"Se encontraron {mantenimientos.Count} mantenimientos próximos", mantenimientos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener mantenimientos próximos: {ex.Message}", null);
            }
        }

        // Calcular costo total por propiedad
        public async Task<(bool exito, string mensaje, decimal total)>
            CalcularCostoTotalPorPropiedadAsync(int idPropiedad, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var condiciones = new List<string> { "idPropiedad = @idPropiedad", "costo IS NOT NULL" };
                var parametros = new List<MySqlParameter> { new MySqlParameter("@idPropiedad", idPropiedad) };

                if (fechaInicio.HasValue)
                {
                    condiciones.Add("fechaRealizada >= @fechaInicio");
                    parametros.Add(new MySqlParameter("@fechaInicio", fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    condiciones.Add("fechaRealizada <= @fechaFin");
                    parametros.Add(new MySqlParameter("@fechaFin", fechaFin.Value));
                }

                string whereClause = "WHERE " + string.Join(" AND ", condiciones);

                string query = $"SELECT COALESCE(SUM(costo), 0) FROM mantenimiento {whereClause}";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                var result = await cmd.ExecuteScalarAsync();
                decimal total = Convert.ToDecimal(result);

                return (true, $"Costo total: {total:C}", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al calcular costo total: {ex.Message}", 0);
            }
        }

        // Actualizar mantenimiento
        public async Task<(bool exito, string mensaje)> ActualizarMantenimientoAsync(Mantenimiento mantenimiento)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE mantenimiento SET 
                    tipo = @tipo,
                    descripcion = @descripcion,
                    costo = @costo,
                    fechaProgramada = @fechaProgramada,
                    fechaRealizada = @fechaRealizada,
                    idEstadoMantenimiento = @idEstadoMantenimiento,
                    idUsuario = @idUsuario,
                    actualizadoAt = @actualizadoAt
                    WHERE idMantenimiento = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", mantenimiento.IdMantenimiento);
                cmd.Parameters.AddWithValue("@tipo", mantenimiento.Tipo);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(mantenimiento.Descripcion) ? DBNull.Value : mantenimiento.Descripcion);
                cmd.Parameters.AddWithValue("@costo", mantenimiento.Costo.HasValue ? (object)mantenimiento.Costo.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaProgramada", mantenimiento.FechaProgramada.HasValue ? (object)mantenimiento.FechaProgramada.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaRealizada", mantenimiento.FechaRealizada.HasValue ? (object)mantenimiento.FechaRealizada.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@idEstadoMantenimiento", mantenimiento.IdEstadoMantenimiento);
                cmd.Parameters.AddWithValue("@idUsuario", mantenimiento.IdUsuario.HasValue ? (object)mantenimiento.IdUsuario.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@actualizadoAt", mantenimiento.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Mantenimiento actualizado exitosamente");
                }

                return (false, "No se encontró el mantenimiento a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar mantenimiento: {ex.Message}");
            }
        }

        // Completar mantenimiento
        public async Task<(bool exito, string mensaje)> CompletarMantenimientoAsync(int idMantenimiento, decimal costo, int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Obtener ID del estado "Completado"
                string queryEstado = "SELECT idEstadoMantenimiento FROM estadomantenimiento WHERE LOWER(nombre) = 'completado' LIMIT 1";
                using var cmdEstado = new MySqlCommand(queryEstado, connection);
                var resultEstado = await cmdEstado.ExecuteScalarAsync();

                if (resultEstado == null)
                {
                    return (false, "No se encontró el estado 'Completado' en el sistema");
                }

                int idEstadoCompletado = Convert.ToInt32(resultEstado);

                string query = @"UPDATE mantenimiento SET 
                    fechaRealizada = @fechaRealizada,
                    costo = @costo,
                    idEstadoMantenimiento = @idEstado,
                    idUsuario = @idUsuario,
                    actualizadoAt = @actualizadoAt
                    WHERE idMantenimiento = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idMantenimiento);
                cmd.Parameters.AddWithValue("@fechaRealizada", DateTime.Now);
                cmd.Parameters.AddWithValue("@costo", costo);
                cmd.Parameters.AddWithValue("@idEstado", idEstadoCompletado);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.Now);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Mantenimiento completado exitosamente");
                }

                return (false, "No se encontró el mantenimiento");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al completar mantenimiento: {ex.Message}");
            }
        }

        // Eliminar mantenimiento
        public async Task<(bool exito, string mensaje)> EliminarMantenimientoAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM mantenimiento WHERE idMantenimiento = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Mantenimiento eliminado exitosamente");
                }

                return (false, "No se encontró el mantenimiento a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar mantenimiento: {ex.Message}");
            }
        }

        // Obtener estadísticas
        public async Task<(bool exito, string mensaje, Dictionary<string, object>? estadisticas)>
            ObtenerEstadisticasAsync(int? idPropiedad = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string whereClause = idPropiedad.HasValue ? "WHERE idPropiedad = @idPropiedad" : "";

                string query = $@"SELECT 
                    COUNT(*) as TotalMantenimientos,
                    COUNT(CASE WHEN fechaRealizada IS NOT NULL THEN 1 END) as Completados,
                    COUNT(CASE WHEN fechaRealizada IS NULL AND fechaProgramada >= CURDATE() THEN 1 END) as Pendientes,
                    COUNT(CASE WHEN fechaRealizada IS NULL AND fechaProgramada < CURDATE() THEN 1 END) as Vencidos,
                    COALESCE(SUM(CASE WHEN fechaRealizada IS NOT NULL THEN costo END), 0) as CostoTotal,
                    COALESCE(AVG(CASE WHEN fechaRealizada IS NOT NULL THEN costo END), 0) as CostoPromedio
                    FROM mantenimiento
                    {whereClause}";

                using var cmd = new MySqlCommand(query, connection);
                if (idPropiedad.HasValue)
                {
                    cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad.Value);
                }

                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalMantenimientos"] = reader.GetInt32("TotalMantenimientos");
                    estadisticas["completados"] = reader.GetInt32("Completados");
                    estadisticas["pendientes"] = reader.GetInt32("Pendientes");
                    estadisticas["vencidos"] = reader.GetInt32("Vencidos");
                    estadisticas["costoTotal"] = reader.GetDecimal("CostoTotal");
                    estadisticas["costoPromedio"] = reader.GetDecimal("CostoPromedio");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Métodos auxiliares para mapear
        private Mantenimiento MapearMantenimiento(MySqlDataReader reader)
        {
            return new Mantenimiento
            {
                IdMantenimiento = reader.GetInt32("idMantenimiento"),
                IdPropiedad = reader.GetInt32("idPropiedad"),
                Tipo = reader.GetString("tipo"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("descripcion")) ? reader.GetString("descripcion") : string.Empty,
                Costo = !reader.IsDBNull(reader.GetOrdinal("costo")) ? reader.GetDecimal("costo") : null,
                FechaProgramada = !reader.IsDBNull(reader.GetOrdinal("fechaProgramada")) ? reader.GetDateTime("fechaProgramada") : null,
                FechaRealizada = !reader.IsDBNull(reader.GetOrdinal("fechaRealizada")) ? reader.GetDateTime("fechaRealizada") : null,
                IdEstadoMantenimiento = reader.GetInt32("idEstadoMantenimiento"),
                IdUsuario = !reader.IsDBNull(reader.GetOrdinal("idUsuario")) ? reader.GetInt32("idUsuario") : null,
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt"),
                EstadoMantenimiento = new EstadoMantenimiento
                {
                    IdEstadoMantenimiento = reader.GetInt32("idEstadoMantenimiento"),
                    Nombre = !reader.IsDBNull(reader.GetOrdinal("estadoNombre")) ? reader.GetString("estadoNombre") : string.Empty,
                    Descripcion = string.Empty,
                    CreadoAt = DateTime.Now,
                    ActualizadoAt = DateTime.Now
                },
                Usuario = !reader.IsDBNull(reader.GetOrdinal("IdUsuario")) ? new Usuario
                {
                    IdUsuario = reader.GetInt32("IdUsuario"),
                    Nombre = reader.GetString("usuarioNombre")
                } : null
            };
        }

        private Mantenimiento MapearMantenimientoSimple(MySqlDataReader reader)
        {
            return new Mantenimiento
            {
                IdMantenimiento = reader.GetInt32("idMantenimiento"),
                IdPropiedad = reader.GetInt32("idPropiedad"),
                Tipo = reader.GetString("tipo"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("descripcion")) ? reader.GetString("descripcion") : string.Empty,
                Costo = !reader.IsDBNull(reader.GetOrdinal("costo")) ? reader.GetDecimal("costo") : null,
                FechaProgramada = !reader.IsDBNull(reader.GetOrdinal("fechaProgramada")) ? reader.GetDateTime("fechaProgramada") : null,
                FechaRealizada = !reader.IsDBNull(reader.GetOrdinal("fechaRealizada")) ? reader.GetDateTime("fechaRealizada") : null,
                IdEstadoMantenimiento = reader.GetInt32("idEstadoMantenimiento"),
                IdUsuario = !reader.IsDBNull(reader.GetOrdinal("idUsuario")) ? reader.GetInt32("idUsuario") : null,
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
