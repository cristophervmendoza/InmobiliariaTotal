using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class CitaHistorialDao
    {
        // Crear historial
        public async Task<(bool exito, string mensaje, int? id)> CrearHistorialAsync(CitaHistorial historial)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO citahistorial 
                    (idCita, citaEstadoId, observacion, creadoAt) 
                    VALUES (@idCita, @citaEstadoId, @observacion, @creadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCita", historial.IdCita);
                cmd.Parameters.AddWithValue("@citaEstadoId", historial.CitaEstadoId);
                cmd.Parameters.AddWithValue("@observacion", string.IsNullOrEmpty(historial.Observacion) ? DBNull.Value : historial.Observacion);
                cmd.Parameters.AddWithValue("@creadoAt", historial.CreadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Historial registrado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear historial: {ex.Message}", null);
            }
        }

        // Obtener historial por ID
        public async Task<(bool exito, string mensaje, CitaHistorial? historial)> ObtenerHistorialPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    WHERE ch.idHistorial = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var historial = MapearHistorial(reader);
                    return (true, "Historial encontrado", historial);
                }

                return (false, "Historial no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener historial: {ex.Message}", null);
            }
        }

        // Obtener historial por cita
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales)>
            ObtenerHistorialPorCitaAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    WHERE ch.idCita = @idCita
                    ORDER BY ch.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCita", idCita);

                using var reader = await cmd.ExecuteReaderAsync();
                var historiales = new List<CitaHistorial>();
                while (await reader.ReadAsync())
                {
                    historiales.Add(MapearHistorial(reader));
                }

                return (true, $"Se encontraron {historiales.Count} registros", historiales);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener historial: {ex.Message}", null);
            }
        }

        // Listar todos los historiales
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales)> ListarHistorialesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    ORDER BY ch.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var historiales = new List<CitaHistorial>();
                while (await reader.ReadAsync())
                {
                    historiales.Add(MapearHistorial(reader));
                }

                return (true, $"Se encontraron {historiales.Count} historiales", historiales);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar historiales: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales, int totalRegistros)>
            ListarHistorialesPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM citahistorial";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    ORDER BY ch.creadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var historiales = new List<CitaHistorial>();
                while (await reader.ReadAsync())
                {
                    historiales.Add(MapearHistorial(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", historiales, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar historiales: {ex.Message}", null, 0);
            }
        }

        // Buscar historiales por estado
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales)>
            BuscarHistorialesPorEstadoAsync(int idEstado)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    WHERE ch.citaEstadoId = @idEstado
                    ORDER BY ch.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idEstado", idEstado);

                using var reader = await cmd.ExecuteReaderAsync();
                var historiales = new List<CitaHistorial>();
                while (await reader.ReadAsync())
                {
                    historiales.Add(MapearHistorial(reader));
                }

                return (true, $"Se encontraron {historiales.Count} historiales", historiales);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar historiales: {ex.Message}", null);
            }
        }

        // Buscar historiales por rango de fechas
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales)>
            BuscarHistorialesPorFechaAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null)
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

                if (fechaInicio.HasValue)
                {
                    condiciones.Add("ch.creadoAt >= @fechaInicio");
                    parametros.Add(new MySqlParameter("@fechaInicio", fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    condiciones.Add("ch.creadoAt <= @fechaFin");
                    parametros.Add(new MySqlParameter("@fechaFin", fechaFin.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    {whereClause}
                    ORDER BY ch.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var historiales = new List<CitaHistorial>();
                while (await reader.ReadAsync())
                {
                    historiales.Add(MapearHistorial(reader));
                }

                return (true, $"Se encontraron {historiales.Count} historiales", historiales);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar historiales: {ex.Message}", null);
            }
        }

        // Obtener historiales recientes
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales)>
            ObtenerHistorialesRecientesAsync(int dias = 7)
        {
            var fechaInicio = DateTime.Now.AddDays(-dias);
            return await BuscarHistorialesPorFechaAsync(fechaInicio: fechaInicio);
        }

        // Obtener historiales de hoy
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales)>
            ObtenerHistorialesDeHoyAsync()
        {
            var hoy = DateTime.Now.Date;
            var manana = hoy.AddDays(1);
            return await BuscarHistorialesPorFechaAsync(fechaInicio: hoy, fechaFin: manana);
        }

        // Obtener último historial de una cita
        public async Task<(bool exito, string mensaje, CitaHistorial? historial)>
            ObtenerUltimoHistorialCitaAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    WHERE ch.idCita = @idCita
                    ORDER BY ch.creadoAt DESC
                    LIMIT 1";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCita", idCita);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var historial = MapearHistorial(reader);
                    return (true, "Último historial encontrado", historial);
                }

                return (false, "No hay historial para esta cita", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener último historial: {ex.Message}", null);
            }
        }

        // Contar cambios de estado de una cita
        public async Task<(bool exito, string mensaje, int total)>
            ContarCambiosEstadoCitaAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM citahistorial WHERE idCita = @idCita";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCita", idCita);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"La cita tiene {total} cambios de estado", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar cambios: {ex.Message}", 0);
            }
        }

        // Obtener historiales con observaciones
        public async Task<(bool exito, string mensaje, List<CitaHistorial>? historiales)>
            ObtenerHistorialesConObservacionesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ch.*, 
                    ec.idEstadoCita, ec.nombre as estadoNombre,
                    c.fecha as citaFecha, c.hora as citaHora
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    LEFT JOIN cita c ON ch.idCita = c.idCita
                    WHERE ch.observacion IS NOT NULL AND ch.observacion != ''
                    ORDER BY ch.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var historiales = new List<CitaHistorial>();
                while (await reader.ReadAsync())
                {
                    historiales.Add(MapearHistorial(reader));
                }

                return (true, $"Se encontraron {historiales.Count} historiales con observaciones", historiales);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener historiales: {ex.Message}", null);
            }
        }

        // Actualizar observación de historial
        public async Task<(bool exito, string mensaje)>
            ActualizarObservacionAsync(int idHistorial, string observacion)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE citahistorial SET 
                    observacion = @observacion
                    WHERE idHistorial = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idHistorial);
                cmd.Parameters.AddWithValue("@observacion", observacion);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Observación actualizada exitosamente");
                }

                return (false, "No se encontró el historial");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar observación: {ex.Message}");
            }
        }

        // Eliminar historial
        public async Task<(bool exito, string mensaje)> EliminarHistorialAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM citahistorial WHERE idHistorial = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Historial eliminado exitosamente");
                }

                return (false, "No se encontró el historial a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar historial: {ex.Message}");
            }
        }

        // Eliminar historial de una cita
        public async Task<(bool exito, string mensaje)> EliminarHistorialPorCitaAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM citahistorial WHERE idCita = @idCita";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCita", idCita);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, $"{filasAfectadas} registro(s) de historial eliminado(s)");
                }

                return (false, "No se encontró historial para esta cita");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar historial: {ex.Message}");
            }
        }

        // Obtener estadísticas de cambios de estado
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
                    COUNT(*) as TotalRegistros,
                    COUNT(DISTINCT idCita) as CitasConHistorial,
                    COUNT(CASE WHEN observacion IS NOT NULL AND observacion != '' THEN 1 END) as ConObservacion,
                    COUNT(CASE WHEN DATE(creadoAt) = CURDATE() THEN 1 END) as RegistrosHoy,
                    COUNT(CASE WHEN creadoAt >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 END) as UltimaSemana
                    FROM citahistorial";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalRegistros"] = reader.GetInt32("TotalRegistros");
                    estadisticas["citasConHistorial"] = reader.GetInt32("CitasConHistorial");
                    estadisticas["conObservacion"] = reader.GetInt32("ConObservacion");
                    estadisticas["registrosHoy"] = reader.GetInt32("RegistrosHoy");
                    estadisticas["ultimaSemana"] = reader.GetInt32("UltimaSemana");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Obtener estadísticas por estado
        public async Task<(bool exito, string mensaje, Dictionary<string, int>? estadisticas)>
            ObtenerEstadisticasPorEstadoAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ec.nombre, COUNT(*) as cantidad
                    FROM citahistorial ch
                    LEFT JOIN estadocita ec ON ch.citaEstadoId = ec.idEstadoCita
                    GROUP BY ec.nombre
                    ORDER BY cantidad DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, int>();
                while (await reader.ReadAsync())
                {
                    var nombre = !reader.IsDBNull(reader.GetOrdinal("nombre")) ? reader.GetString("nombre") : "Sin Estado";
                    estadisticas[nombre] = reader.GetInt32("cantidad");
                }

                return (true, "Estadísticas por estado obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Verificar si una cita tiene historial
        public async Task<bool> CitaTieneHistorialAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM citahistorial WHERE idCita = @idCita";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCita", idCita);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Método auxiliar para mapear
        private CitaHistorial MapearHistorial(MySqlDataReader reader)
        {
            return new CitaHistorial
            {
                IdHistorial = reader.GetInt32("idHistorial"),
                IdCita = reader.GetInt32("idCita"),
                CitaEstadoId = reader.GetInt32("citaEstadoId"),
                Observacion = !reader.IsDBNull(reader.GetOrdinal("observacion")) ? reader.GetString("observacion") : string.Empty,
                CreadoAt = reader.GetDateTime("creadoAt"),
                EstadoCita = new EstadoCita
                {
                    IdEstadoCita = reader.GetInt32("idEstadoCita"),
                    Nombre = !reader.IsDBNull(reader.GetOrdinal("estadoNombre")) ? reader.GetString("estadoNombre") : string.Empty,
                    CreadoAt = DateTime.Now,
                    ActualizadoAt = DateTime.Now
                },
                Cita = new Cita
                {
                    IdCita = reader.GetInt32("idCita"),
                    Fecha = !reader.IsDBNull(reader.GetOrdinal("citaFecha")) ? reader.GetDateTime("citaFecha") : DateTime.MinValue,
                    Hora = !reader.IsDBNull(reader.GetOrdinal("citaHora")) ? reader.GetTimeSpan("citaHora") : TimeSpan.Zero,
                    IdCliente = 0,
                    IdAgente = 0,
                    IdEstadoCita = reader.GetInt32("citaEstadoId"),
                    CreadoAt = DateTime.Now,
                    ActualizadoAt = DateTime.Now
                }
            };
        }
    }
}
