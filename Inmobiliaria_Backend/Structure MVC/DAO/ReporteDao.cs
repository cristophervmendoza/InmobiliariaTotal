using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class ReporteDao
    {
        // Crear reporte
        public async Task<(bool exito, string mensaje, int? id)> CrearReporteAsync(Reporte reporte)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO reporte 
                    (TipoReporte, FechaGeneracion, IdAdministrador, Archivo) 
                    VALUES (@TipoReporte, @FechaGeneracion, @IdAdministrador, @Archivo);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@TipoReporte", reporte.TipoReporte);
                cmd.Parameters.AddWithValue("@FechaGeneracion", reporte.FechaGeneracion);
                cmd.Parameters.AddWithValue("@IdAdministrador", reporte.IdAdministrador);
                cmd.Parameters.AddWithValue("@Archivo", string.IsNullOrEmpty(reporte.Archivo) ? DBNull.Value : reporte.Archivo);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Reporte creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear reporte: {ex.Message}", null);
            }
        }

        // Obtener reporte por ID
        public async Task<(bool exito, string mensaje, Reporte? reporte)> ObtenerReportePorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM reporte WHERE IdReporte = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var reporte = MapearReporte(reader);
                    return (true, "Reporte encontrado", reporte);
                }

                return (false, "Reporte no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener reporte: {ex.Message}", null);
            }
        }

        // Listar todos los reportes
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes)> ListarReportesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM reporte ORDER BY FechaGeneracion DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Se encontraron {reportes.Count} reportes", reportes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar reportes: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes, int totalRegistros)>
            ListarReportesPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM reporte";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM reporte 
                    ORDER BY FechaGeneracion DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", reportes, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar reportes: {ex.Message}", null, 0);
            }
        }

        // Buscar reportes
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes)>
            BuscarReportesAsync(string? tipoReporte = null, int? idAdministrador = null,
                               DateTime? fechaInicio = null, DateTime? fechaFin = null, bool? tieneArchivo = null)
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

                if (!string.IsNullOrWhiteSpace(tipoReporte))
                {
                    condiciones.Add("TipoReporte = @tipoReporte");
                    parametros.Add(new MySqlParameter("@tipoReporte", tipoReporte));
                }

                if (idAdministrador.HasValue)
                {
                    condiciones.Add("IdAdministrador = @idAdministrador");
                    parametros.Add(new MySqlParameter("@idAdministrador", idAdministrador.Value));
                }

                if (fechaInicio.HasValue)
                {
                    condiciones.Add("FechaGeneracion >= @fechaInicio");
                    parametros.Add(new MySqlParameter("@fechaInicio", fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    condiciones.Add("FechaGeneracion <= @fechaFin");
                    parametros.Add(new MySqlParameter("@fechaFin", fechaFin.Value));
                }

                if (tieneArchivo.HasValue)
                {
                    if (tieneArchivo.Value)
                    {
                        condiciones.Add("Archivo IS NOT NULL AND Archivo != ''");
                    }
                    else
                    {
                        condiciones.Add("(Archivo IS NULL OR Archivo = '')");
                    }
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT * FROM reporte 
                    {whereClause}
                    ORDER BY FechaGeneracion DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Se encontraron {reportes.Count} reportes", reportes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar reportes: {ex.Message}", null);
            }
        }

        // Obtener reportes por administrador
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes)>
            ObtenerReportesPorAdministradorAsync(int idAdministrador)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM reporte 
                    WHERE IdAdministrador = @idAdministrador
                    ORDER BY FechaGeneracion DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idAdministrador", idAdministrador);

                using var reader = await cmd.ExecuteReaderAsync();
                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Se encontraron {reportes.Count} reportes del administrador", reportes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener reportes del administrador: {ex.Message}", null);
            }
        }

        // Obtener reportes por tipo
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes)>
            ObtenerReportesPorTipoAsync(string tipoReporte)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM reporte 
                    WHERE TipoReporte = @tipoReporte
                    ORDER BY FechaGeneracion DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@tipoReporte", tipoReporte);

                using var reader = await cmd.ExecuteReaderAsync();
                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Se encontraron {reportes.Count} reportes del tipo especificado", reportes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener reportes por tipo: {ex.Message}", null);
            }
        }

        // Obtener reportes recientes
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes)>
            ObtenerReportesRecientesAsync(int horas = 24)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var fechaLimite = DateTime.UtcNow.AddHours(-horas);

                string query = @"SELECT * FROM reporte 
                    WHERE FechaGeneracion >= @fechaLimite
                    ORDER BY FechaGeneracion DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaLimite", fechaLimite);

                using var reader = await cmd.ExecuteReaderAsync();
                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Se encontraron {reportes.Count} reportes recientes", reportes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener reportes recientes: {ex.Message}", null);
            }
        }

        // Obtener reportes por rango de fechas
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes)>
            ObtenerReportesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM reporte 
                    WHERE FechaGeneracion >= @fechaInicio AND FechaGeneracion <= @fechaFin
                    ORDER BY FechaGeneracion DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                using var reader = await cmd.ExecuteReaderAsync();
                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Se encontraron {reportes.Count} reportes en el rango especificado", reportes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener reportes por rango: {ex.Message}", null);
            }
        }

        // Obtener reportes para archivar
        public async Task<(bool exito, string mensaje, List<Reporte>? reportes)>
            ObtenerReportesParaArchivarAsync(int diasAntiguedad = 30)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-diasAntiguedad);

                string query = @"SELECT * FROM reporte 
                    WHERE FechaGeneracion < @fechaLimite
                    ORDER BY FechaGeneracion ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaLimite", fechaLimite);

                using var reader = await cmd.ExecuteReaderAsync();
                var reportes = new List<Reporte>();
                while (await reader.ReadAsync())
                {
                    reportes.Add(MapearReporte(reader));
                }

                return (true, $"Se encontraron {reportes.Count} reportes para archivar", reportes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener reportes para archivar: {ex.Message}", null);
            }
        }

        // Actualizar reporte
        public async Task<(bool exito, string mensaje)> ActualizarReporteAsync(Reporte reporte)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE reporte SET 
                    TipoReporte = @TipoReporte,
                    Archivo = @Archivo
                    WHERE IdReporte = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", reporte.IdReporte);
                cmd.Parameters.AddWithValue("@TipoReporte", reporte.TipoReporte);
                cmd.Parameters.AddWithValue("@Archivo", string.IsNullOrEmpty(reporte.Archivo) ? DBNull.Value : reporte.Archivo);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Reporte actualizado exitosamente");
                }

                return (false, "No se encontró el reporte a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar reporte: {ex.Message}");
            }
        }

        // Eliminar reporte
        public async Task<(bool exito, string mensaje)> EliminarReporteAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM reporte WHERE IdReporte = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Reporte eliminado exitosamente");
                }

                return (false, "No se encontró el reporte a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar reporte: {ex.Message}");
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
                    COUNT(*) as TotalReportes,
                    COUNT(CASE WHEN Archivo IS NOT NULL AND Archivo != '' THEN 1 END) as ConArchivo,
                    COUNT(CASE WHEN Archivo IS NULL OR Archivo = '' THEN 1 END) as SinArchivo,
                    COUNT(CASE WHEN DATEDIFF(NOW(), FechaGeneracion) <= 1 THEN 1 END) as Recientes,
                    COUNT(CASE WHEN TipoReporte = 'Financiero' THEN 1 END) as Financieros,
                    COUNT(DISTINCT IdAdministrador) as AdministradoresUnicos,
                    COUNT(DISTINCT TipoReporte) as TiposUnicos
                    FROM reporte";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalReportes"] = reader.GetInt32("TotalReportes");
                    estadisticas["conArchivo"] = reader.GetInt32("ConArchivo");
                    estadisticas["sinArchivo"] = reader.GetInt32("SinArchivo");
                    estadisticas["recientes"] = reader.GetInt32("Recientes");
                    estadisticas["financieros"] = reader.GetInt32("Financieros");
                    estadisticas["administradoresUnicos"] = reader.GetInt32("AdministradoresUnicos");
                    estadisticas["tiposUnicos"] = reader.GetInt32("TiposUnicos");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear resultados
        private Reporte MapearReporte(MySqlDataReader reader)
        {
            return new Reporte
            {
                IdReporte = reader.GetInt32("IdReporte"),
                TipoReporte = reader.GetString("TipoReporte"),
                FechaGeneracion = reader.GetDateTime("FechaGeneracion"),
                IdAdministrador = reader.GetInt32("IdAdministrador"),
                Archivo = !reader.IsDBNull(reader.GetOrdinal("Archivo")) ? reader.GetString("Archivo") : null
            };
        }
    }
}
