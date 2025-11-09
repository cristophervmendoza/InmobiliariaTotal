using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class EstadoPropiedadDao
    {
        // Crear estado de propiedad
        public async Task<(bool exito, string mensaje, int? id)> CrearEstadoPropiedadAsync(EstadoPropiedad estadoPropiedad)
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
                if (await ExisteEstadoPropiedadAsync(estadoPropiedad.Nombre))
                {
                    return (false, "Ya existe un estado de propiedad con ese nombre", null);
                }

                string query = @"INSERT INTO estadopropiedad 
                    (nombre, creadoAt, actualizadoAt) 
                    VALUES (@nombre, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", estadoPropiedad.Nombre);
                cmd.Parameters.AddWithValue("@creadoAt", estadoPropiedad.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", estadoPropiedad.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Estado de propiedad creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear estado de propiedad: {ex.Message}", null);
            }
        }

        // Obtener estado de propiedad por ID
        public async Task<(bool exito, string mensaje, EstadoPropiedad? estadoPropiedad)> ObtenerEstadoPropiedadPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadopropiedad WHERE idEstadoPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoPropiedad = MapearEstadoPropiedad(reader);
                    return (true, "Estado de propiedad encontrado", estadoPropiedad);
                }

                return (false, "Estado de propiedad no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de propiedad: {ex.Message}", null);
            }
        }

        // Obtener estado de propiedad por nombre
        public async Task<(bool exito, string mensaje, EstadoPropiedad? estadoPropiedad)> ObtenerEstadoPropiedadPorNombreAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadopropiedad WHERE LOWER(nombre) = LOWER(@nombre)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nombre);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoPropiedad = MapearEstadoPropiedad(reader);
                    return (true, "Estado de propiedad encontrado", estadoPropiedad);
                }

                return (false, "Estado de propiedad no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de propiedad: {ex.Message}", null);
            }
        }

        // Listar todos los estados de propiedad
        public async Task<(bool exito, string mensaje, List<EstadoPropiedad>? estadosPropiedad)> ListarEstadosPropiedadAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadopropiedad ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadosPropiedad = new List<EstadoPropiedad>();
                while (await reader.ReadAsync())
                {
                    estadosPropiedad.Add(MapearEstadoPropiedad(reader));
                }

                return (true, $"Se encontraron {estadosPropiedad.Count} estados de propiedad", estadosPropiedad);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de propiedad: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<EstadoPropiedad>? estadosPropiedad, int totalRegistros)>
            ListarEstadosPropiedadPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM estadopropiedad";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM estadopropiedad 
                    ORDER BY nombre ASC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosPropiedad = new List<EstadoPropiedad>();
                while (await reader.ReadAsync())
                {
                    estadosPropiedad.Add(MapearEstadoPropiedad(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", estadosPropiedad, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de propiedad: {ex.Message}", null, 0);
            }
        }

        // Buscar estados de propiedad
        public async Task<(bool exito, string mensaje, List<EstadoPropiedad>? estadosPropiedad)>
            BuscarEstadosPropiedadAsync(string? termino = null)
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
                    whereClause = "WHERE nombre LIKE @termino";
                }

                string query = $@"SELECT * FROM estadopropiedad 
                    {whereClause}
                    ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosPropiedad = new List<EstadoPropiedad>();
                while (await reader.ReadAsync())
                {
                    estadosPropiedad.Add(MapearEstadoPropiedad(reader));
                }

                return (true, $"Se encontraron {estadosPropiedad.Count} estados de propiedad", estadosPropiedad);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar estados de propiedad: {ex.Message}", null);
            }
        }

        // Actualizar estado de propiedad
        public async Task<(bool exito, string mensaje)> ActualizarEstadoPropiedadAsync(EstadoPropiedad estadoPropiedad)
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
                string checkQuery = @"SELECT COUNT(*) FROM estadopropiedad 
                    WHERE LOWER(nombre) = LOWER(@nombre) AND idEstadoPropiedad != @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@nombre", estadoPropiedad.Nombre);
                checkCmd.Parameters.AddWithValue("@id", estadoPropiedad.IdEstadoPropiedad);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, "Ya existe otro estado de propiedad con ese nombre");
                }

                string query = @"UPDATE estadopropiedad SET 
                    nombre = @nombre,
                    actualizadoAt = @actualizadoAt
                    WHERE idEstadoPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", estadoPropiedad.IdEstadoPropiedad);
                cmd.Parameters.AddWithValue("@nombre", estadoPropiedad.Nombre);
                cmd.Parameters.AddWithValue("@actualizadoAt", estadoPropiedad.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de propiedad actualizado exitosamente");
                }

                return (false, "No se encontró el estado de propiedad a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar estado de propiedad: {ex.Message}");
            }
        }

        // Eliminar estado de propiedad
        public async Task<(bool exito, string mensaje)> EliminarEstadoPropiedadAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar si hay propiedades usando este estado
                string checkQuery = "SELECT COUNT(*) FROM propiedad WHERE IdEstadoPropiedad = @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, $"No se puede eliminar el estado porque tiene {count} propiedad(es) asociada(s)");
                }

                string query = "DELETE FROM estadopropiedad WHERE idEstadoPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de propiedad eliminado exitosamente");
                }

                return (false, "No se encontró el estado de propiedad a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar estado de propiedad: {ex.Message}");
            }
        }

        // Verificar si existe un estado con el nombre
        public async Task<bool> ExisteEstadoPropiedadAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM estadopropiedad WHERE LOWER(nombre) = LOWER(@nombre)";

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

        // Contar propiedades por estado
        public async Task<(bool exito, string mensaje, int total)> ContarPropiedadesPorEstadoAsync(int idEstadoPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM propiedad WHERE IdEstadoPropiedad = @idEstadoPropiedad";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idEstadoPropiedad", idEstadoPropiedad);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El estado tiene {total} propiedad(es)", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar propiedades: {ex.Message}", 0);
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
                    ep.idEstadoPropiedad,
                    ep.nombre,
                    COUNT(p.IdPropiedad) as totalPropiedades,
                    COALESCE(AVG(p.Precio), 0) as precioPromedio,
                    COALESCE(MIN(p.Precio), 0) as precioMinimo,
                    COALESCE(MAX(p.Precio), 0) as precioMaximo
                    FROM estadopropiedad ep
                    LEFT JOIN propiedad p ON ep.idEstadoPropiedad = p.IdEstadoPropiedad
                    GROUP BY ep.idEstadoPropiedad, ep.nombre
                    ORDER BY totalPropiedades DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var stats = new Dictionary<string, object>
                    {
                        ["idEstadoPropiedad"] = reader.GetInt32("idEstadoPropiedad"),
                        ["nombre"] = reader.GetString("nombre"),
                        ["totalPropiedades"] = reader.GetInt32("totalPropiedades"),
                        ["precioPromedio"] = Math.Round(reader.GetDecimal("precioPromedio"), 2),
                        ["precioMinimo"] = reader.GetDecimal("precioMinimo"),
                        ["precioMaximo"] = reader.GetDecimal("precioMaximo")
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

        // Obtener estados más usados
        public async Task<(bool exito, string mensaje, List<EstadoPropiedad>? estados)>
            ObtenerEstadosMasUsadosAsync(int limite = 5)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ep.*, COUNT(p.IdPropiedad) as uso
                    FROM estadopropiedad ep
                    LEFT JOIN propiedad p ON ep.idEstadoPropiedad = p.IdEstadoPropiedad
                    GROUP BY ep.idEstadoPropiedad
                    ORDER BY uso DESC
                    LIMIT @limite";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limite", limite);

                using var reader = await cmd.ExecuteReaderAsync();
                var estados = new List<EstadoPropiedad>();
                while (await reader.ReadAsync())
                {
                    estados.Add(MapearEstadoPropiedad(reader));
                }

                return (true, $"Se encontraron {estados.Count} estados más usados", estados);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estados más usados: {ex.Message}", null);
            }
        }

        // Obtener estados sin uso
        public async Task<(bool exito, string mensaje, List<EstadoPropiedad>? estados)>
            ObtenerEstadosSinUsoAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT ep.*
                    FROM estadopropiedad ep
                    LEFT JOIN propiedad p ON ep.idEstadoPropiedad = p.IdEstadoPropiedad
                    WHERE p.IdPropiedad IS NULL
                    ORDER BY ep.nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estados = new List<EstadoPropiedad>();
                while (await reader.ReadAsync())
                {
                    estados.Add(MapearEstadoPropiedad(reader));
                }

                return (true, $"Se encontraron {estados.Count} estados sin uso", estados);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estados sin uso: {ex.Message}", null);
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
                var estadosPredeterminados = new[]
                {
                    "Disponible",
                    "Vendida",
                    "Alquilada",
                    "Reservada",
                    "En Proceso",
                    "Retirada",
                    "Mantenimiento",
                    "Pendiente",
                    "Suspendida",
                    "Archivada"
                };

                int creados = 0;
                foreach (var estado in estadosPredeterminados)
                {
                    if (!await ExisteEstadoPropiedadAsync(estado))
                    {
                        var estadoPropiedad = new EstadoPropiedad
                        {
                            Nombre = estado,
                            CreadoAt = new DateTime(2020, 1, 1),
                            ActualizadoAt = new DateTime(2020, 1, 1)
                        };

                        var (exito, _, _) = await CrearEstadoPropiedadAsync(estadoPropiedad);
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
        private EstadoPropiedad MapearEstadoPropiedad(MySqlDataReader reader)
        {
            return new EstadoPropiedad
            {
                IdEstadoPropiedad = reader.GetInt32("idEstadoPropiedad"),
                Nombre = reader.GetString("nombre"),
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
