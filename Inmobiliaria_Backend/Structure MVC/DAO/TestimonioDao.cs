using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class TestimonioDao
    {
        // Crear testimonio
        public async Task<(bool exito, string mensaje, int? id)> CrearTestimonioAsync(Testimonio testimonio)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO testimonio 
                    (idUsuario, contenido, fecha, valoracion, creadoAt, actualizadoAt) 
                    VALUES (@idUsuario, @contenido, @fecha, @valoracion, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", testimonio.IdUsuario);
                cmd.Parameters.AddWithValue("@contenido", testimonio.Contenido);
                cmd.Parameters.AddWithValue("@fecha", testimonio.Fecha);
                cmd.Parameters.AddWithValue("@valoracion", testimonio.Valoracion.HasValue ?
                    (object)testimonio.Valoracion.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@creadoAt", testimonio.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", testimonio.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Testimonio creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear testimonio: {ex.Message}", null);
            }
        }

        // Obtener testimonio por ID
        public async Task<(bool exito, string mensaje, Testimonio? testimonio)> ObtenerTestimonioPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT t.*, u.nombre as NombreUsuario 
                    FROM testimonio t
                    LEFT JOIN usuario u ON t.idUsuario = u.idUsuario
                    WHERE t.idTestimonio = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var testimonio = MapearTestimonio(reader);
                    return (true, "Testimonio encontrado", testimonio);
                }

                return (false, "Testimonio no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener testimonio: {ex.Message}", null);
            }
        }

        // Listar todos los testimonios
        public async Task<(bool exito, string mensaje, List<Testimonio>? testimonios)> ListarTestimoniosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT t.*, u.nombre as NombreUsuario 
                    FROM testimonio t
                    LEFT JOIN usuario u ON t.idUsuario = u.idUsuario
                    ORDER BY t.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var testimonios = new List<Testimonio>();
                while (await reader.ReadAsync())
                {
                    testimonios.Add(MapearTestimonio(reader));
                }

                return (true, $"Se encontraron {testimonios.Count} testimonios", testimonios);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar testimonios: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Testimonio>? testimonios, int totalRegistros)>
            ListarTestimoniosPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Contar total de registros
                string countQuery = "SELECT COUNT(*) FROM testimonio";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                // Obtener registros paginados
                string query = @"SELECT t.*, u.nombre as NombreUsuario 
                    FROM testimonio t
                    LEFT JOIN usuario u ON t.idUsuario = u.idUsuario
                    ORDER BY t.creadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var testimonios = new List<Testimonio>();
                while (await reader.ReadAsync())
                {
                    testimonios.Add(MapearTestimonio(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", testimonios, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar testimonios: {ex.Message}", null, 0);
            }
        }

        // Buscar testimonios
        public async Task<(bool exito, string mensaje, List<Testimonio>? testimonios)>
            BuscarTestimoniosAsync(string termino, int? valoracion = null, int? idUsuario = null)
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
                    condiciones.Add("(t.contenido LIKE @termino OR u.nombre LIKE @termino)");
                    parametros.Add(new MySqlParameter("@termino", $"%{termino}%"));
                }

                if (valoracion.HasValue)
                {
                    condiciones.Add("t.valoracion = @valoracion");
                    parametros.Add(new MySqlParameter("@valoracion", valoracion.Value));
                }

                if (idUsuario.HasValue)
                {
                    condiciones.Add("t.idUsuario = @idUsuario");
                    parametros.Add(new MySqlParameter("@idUsuario", idUsuario.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT t.*, u.nombre as NombreUsuario 
                    FROM testimonio t
                    LEFT JOIN usuario u ON t.idUsuario = u.idUsuario
                    {whereClause}
                    ORDER BY t.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var testimonios = new List<Testimonio>();
                while (await reader.ReadAsync())
                {
                    testimonios.Add(MapearTestimonio(reader));
                }

                return (true, $"Se encontraron {testimonios.Count} testimonios", testimonios);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar testimonios: {ex.Message}", null);
            }
        }

        // Actualizar testimonio
        public async Task<(bool exito, string mensaje)> ActualizarTestimonioAsync(Testimonio testimonio)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE testimonio SET 
                    contenido = @contenido,
                    fecha = @fecha,
                    valoracion = @valoracion,
                    actualizadoAt = @actualizadoAt
                    WHERE idTestimonio = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", testimonio.IdTestimonio);
                cmd.Parameters.AddWithValue("@contenido", testimonio.Contenido);
                cmd.Parameters.AddWithValue("@fecha", testimonio.Fecha);
                cmd.Parameters.AddWithValue("@valoracion", testimonio.Valoracion.HasValue ?
                    (object)testimonio.Valoracion.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@actualizadoAt", testimonio.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Testimonio actualizado exitosamente");
                }

                return (false, "No se encontró el testimonio a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar testimonio: {ex.Message}");
            }
        }

        // Eliminar testimonio
        public async Task<(bool exito, string mensaje)> EliminarTestimonioAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM testimonio WHERE idTestimonio = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Testimonio eliminado exitosamente");
                }

                return (false, "No se encontró el testimonio a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar testimonio: {ex.Message}");
            }
        }

        // Obtener testimonios por usuario
        public async Task<(bool exito, string mensaje, List<Testimonio>? testimonios)>
            ObtenerTestimoniosPorUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT t.*, u.nombre as NombreUsuario 
                    FROM testimonio t
                    LEFT JOIN usuario u ON t.idUsuario = u.idUsuario
                    WHERE t.idUsuario = @idUsuario
                    ORDER BY t.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                var testimonios = new List<Testimonio>();
                while (await reader.ReadAsync())
                {
                    testimonios.Add(MapearTestimonio(reader));
                }

                return (true, $"Se encontraron {testimonios.Count} testimonios", testimonios);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener testimonios del usuario: {ex.Message}", null);
            }
        }

        // Obtener estadísticas de testimonios
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
                    COUNT(*) as Total,
                    AVG(valoracion) as PromedioValoracion,
                    COUNT(CASE WHEN valoracion >= 4 THEN 1 END) as TestimoniosPositivos,
                    COUNT(CASE WHEN valoracion <= 2 THEN 1 END) as TestimoniosNegativos
                    FROM testimonio";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["total"] = reader.GetInt32("Total");
                    estadisticas["promedioValoracion"] = !reader.IsDBNull(reader.GetOrdinal("PromedioValoracion")) ?
                        Math.Round(reader.GetDouble("PromedioValoracion"), 2) : 0;
                    estadisticas["testimoniosPositivos"] = reader.GetInt32("TestimoniosPositivos");
                    estadisticas["testimoniosNegativos"] = reader.GetInt32("TestimoniosNegativos");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear resultados
        private Testimonio MapearTestimonio(MySqlDataReader reader)
        {
            return new Testimonio
            {
                IdTestimonio = reader.GetInt32("idTestimonio"),
                IdUsuario = reader.GetInt32("idUsuario"),
                Contenido = reader.GetString("contenido"),
                Fecha = reader.GetDateTime("fecha"),
                Valoracion = !reader.IsDBNull(reader.GetOrdinal("valoracion")) ?
                    reader.GetInt32("valoracion") : null,
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
