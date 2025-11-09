using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class PostulacionDao
    {
        // Crear postulación
        public async Task<(bool exito, string mensaje, int? id)> CrearPostulacionAsync(Postulacion postulacion)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO postulacion 
                    (IdCliente, Descripcion, CvFile, CreadoAt, ActualizadoAt) 
                    VALUES (@IdCliente, @Descripcion, @CvFile, @CreadoAt, @ActualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdCliente", postulacion.IdCliente);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(postulacion.Descripcion) ? DBNull.Value : postulacion.Descripcion);
                cmd.Parameters.AddWithValue("@CvFile", string.IsNullOrEmpty(postulacion.CvFile) ? DBNull.Value : postulacion.CvFile);
                cmd.Parameters.AddWithValue("@CreadoAt", postulacion.CreadoAt);
                cmd.Parameters.AddWithValue("@ActualizadoAt", postulacion.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Postulación creada exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear postulación: {ex.Message}", null);
            }
        }

        // Obtener postulación por ID
        public async Task<(bool exito, string mensaje, Postulacion? postulacion)> ObtenerPostulacionPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Sin JOIN - solo la tabla postulacion
                string query = @"SELECT * FROM postulacion WHERE IdPostulacion = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var postulacion = MapearPostulacion(reader);
                    return (true, "Postulación encontrada", postulacion);
                }

                return (false, "Postulación no encontrada", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener postulación: {ex.Message}", null);
            }
        }

        // Listar todas las postulaciones
        public async Task<(bool exito, string mensaje, List<Postulacion>? postulaciones)> ListarPostulacionesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM postulacion ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var postulaciones = new List<Postulacion>();
                while (await reader.ReadAsync())
                {
                    postulaciones.Add(MapearPostulacion(reader));
                }

                return (true, $"Se encontraron {postulaciones.Count} postulaciones", postulaciones);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar postulaciones: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Postulacion>? postulaciones, int totalRegistros)>
            ListarPostulacionesPaginadasAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM postulacion";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM postulacion 
                    ORDER BY CreadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var postulaciones = new List<Postulacion>();
                while (await reader.ReadAsync())
                {
                    postulaciones.Add(MapearPostulacion(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", postulaciones, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar postulaciones: {ex.Message}", null, 0);
            }
        }

        // Buscar postulaciones
        public async Task<(bool exito, string mensaje, List<Postulacion>? postulaciones)>
            BuscarPostulacionesAsync(string? termino = null, int? idCliente = null, bool? tieneCV = null)
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
                    condiciones.Add("Descripcion LIKE @termino");
                    parametros.Add(new MySqlParameter("@termino", $"%{termino}%"));
                }

                if (idCliente.HasValue)
                {
                    condiciones.Add("IdCliente = @idCliente");
                    parametros.Add(new MySqlParameter("@idCliente", idCliente.Value));
                }

                if (tieneCV.HasValue)
                {
                    if (tieneCV.Value)
                    {
                        condiciones.Add("CvFile IS NOT NULL AND CvFile != ''");
                    }
                    else
                    {
                        condiciones.Add("(CvFile IS NULL OR CvFile = '')");
                    }
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT * FROM postulacion 
                    {whereClause}
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var postulaciones = new List<Postulacion>();
                while (await reader.ReadAsync())
                {
                    postulaciones.Add(MapearPostulacion(reader));
                }

                return (true, $"Se encontraron {postulaciones.Count} postulaciones", postulaciones);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar postulaciones: {ex.Message}", null);
            }
        }

        // Obtener postulaciones por cliente
        public async Task<(bool exito, string mensaje, List<Postulacion>? postulaciones)>
            ObtenerPostulacionesPorClienteAsync(int idCliente)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM postulacion 
                    WHERE IdCliente = @idCliente
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCliente", idCliente);

                using var reader = await cmd.ExecuteReaderAsync();
                var postulaciones = new List<Postulacion>();
                while (await reader.ReadAsync())
                {
                    postulaciones.Add(MapearPostulacion(reader));
                }

                return (true, $"Se encontraron {postulaciones.Count} postulaciones del cliente", postulaciones);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener postulaciones del cliente: {ex.Message}", null);
            }
        }

        // Obtener postulaciones recientes
        public async Task<(bool exito, string mensaje, List<Postulacion>? postulaciones)>
            ObtenerPostulacionesRecientesAsync(int dias = 7)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-dias);

                string query = @"SELECT * FROM postulacion 
                    WHERE CreadoAt >= @fechaLimite
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaLimite", fechaLimite);

                using var reader = await cmd.ExecuteReaderAsync();
                var postulaciones = new List<Postulacion>();
                while (await reader.ReadAsync())
                {
                    postulaciones.Add(MapearPostulacion(reader));
                }

                return (true, $"Se encontraron {postulaciones.Count} postulaciones recientes", postulaciones);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener postulaciones recientes: {ex.Message}", null);
            }
        }

        // Obtener postulaciones completas (con CV y descripción)
        public async Task<(bool exito, string mensaje, List<Postulacion>? postulaciones)>
            ObtenerPostulacionesCompletasAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM postulacion 
                    WHERE Descripcion IS NOT NULL 
                    AND CvFile IS NOT NULL 
                    AND CvFile != ''
                    AND LENGTH(Descripcion) >= 50
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var postulaciones = new List<Postulacion>();
                while (await reader.ReadAsync())
                {
                    postulaciones.Add(MapearPostulacion(reader));
                }

                return (true, $"Se encontraron {postulaciones.Count} postulaciones completas", postulaciones);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener postulaciones completas: {ex.Message}", null);
            }
        }

        // Actualizar postulación
        public async Task<(bool exito, string mensaje)> ActualizarPostulacionAsync(Postulacion postulacion)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE postulacion SET 
                    Descripcion = @Descripcion,
                    CvFile = @CvFile,
                    ActualizadoAt = @ActualizadoAt
                    WHERE IdPostulacion = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", postulacion.IdPostulacion);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(postulacion.Descripcion) ? DBNull.Value : postulacion.Descripcion);
                cmd.Parameters.AddWithValue("@CvFile", string.IsNullOrEmpty(postulacion.CvFile) ? DBNull.Value : postulacion.CvFile);
                cmd.Parameters.AddWithValue("@ActualizadoAt", postulacion.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Postulación actualizada exitosamente");
                }

                return (false, "No se encontró la postulación a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar postulación: {ex.Message}");
            }
        }

        // Eliminar postulación
        public async Task<(bool exito, string mensaje)> EliminarPostulacionAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM postulacion WHERE IdPostulacion = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Postulación eliminada exitosamente");
                }

                return (false, "No se encontró la postulación a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar postulación: {ex.Message}");
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
                    COUNT(*) as TotalPostulaciones,
                    COUNT(CASE WHEN CvFile IS NOT NULL AND CvFile != '' THEN 1 END) as ConCV,
                    COUNT(CASE WHEN CvFile IS NULL OR CvFile = '' THEN 1 END) as SinCV,
                    COUNT(CASE WHEN Descripcion IS NOT NULL AND LENGTH(Descripcion) >= 50 
                              AND CvFile IS NOT NULL AND CvFile != '' THEN 1 END) as Completas,
                    COUNT(CASE WHEN DATEDIFF(NOW(), CreadoAt) <= 7 THEN 1 END) as Recientes,
                    COUNT(DISTINCT IdCliente) as ClientesUnicos
                    FROM postulacion";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalPostulaciones"] = reader.GetInt32("TotalPostulaciones");
                    estadisticas["conCV"] = reader.GetInt32("ConCV");
                    estadisticas["sinCV"] = reader.GetInt32("SinCV");
                    estadisticas["completas"] = reader.GetInt32("Completas");
                    estadisticas["recientes"] = reader.GetInt32("Recientes");
                    estadisticas["clientesUnicos"] = reader.GetInt32("ClientesUnicos");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear resultados
        private Postulacion MapearPostulacion(MySqlDataReader reader)
        {
            return new Postulacion
            {
                IdPostulacion = reader.GetInt32("IdPostulacion"),
                IdCliente = reader.GetInt32("IdCliente"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? reader.GetString("Descripcion") : null,
                CvFile = !reader.IsDBNull(reader.GetOrdinal("CvFile")) ? reader.GetString("CvFile") : null,
                CreadoAt = reader.GetDateTime("CreadoAt"),
                ActualizadoAt = reader.GetDateTime("ActualizadoAt")
            };
        }
    }
}
