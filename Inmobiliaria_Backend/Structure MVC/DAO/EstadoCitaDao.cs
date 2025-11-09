using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class EstadoCitaDao
    {
        // Crear estado de cita
        public async Task<(bool exito, string mensaje, int? id)> CrearEstadoCitaAsync(EstadoCita estadoCita)
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
                if (await ExisteEstadoAsync(estadoCita.Nombre))
                {
                    return (false, "Ya existe un estado con ese nombre", null);
                }

                string query = @"INSERT INTO estadocita 
                    (nombre, creadoAt, actualizadoAt) 
                    VALUES (@nombre, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", estadoCita.Nombre);
                cmd.Parameters.AddWithValue("@creadoAt", estadoCita.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", estadoCita.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Estado de cita creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear estado de cita: {ex.Message}", null);
            }
        }

        // Obtener estado de cita por ID
        public async Task<(bool exito, string mensaje, EstadoCita? estadoCita)> ObtenerEstadoCitaPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadocita WHERE idEstadoCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoCita = MapearEstadoCita(reader);
                    return (true, "Estado de cita encontrado", estadoCita);
                }

                return (false, "Estado de cita no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de cita: {ex.Message}", null);
            }
        }

        // Obtener estado de cita por nombre
        public async Task<(bool exito, string mensaje, EstadoCita? estadoCita)> ObtenerEstadoCitaPorNombreAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadocita WHERE LOWER(nombre) = LOWER(@nombre)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nombre);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var estadoCita = MapearEstadoCita(reader);
                    return (true, "Estado de cita encontrado", estadoCita);
                }

                return (false, "Estado de cita no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estado de cita: {ex.Message}", null);
            }
        }

        // Listar todos los estados de cita
        public async Task<(bool exito, string mensaje, List<EstadoCita>? estadosCita)> ListarEstadosCitaAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM estadocita ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadosCita = new List<EstadoCita>();
                while (await reader.ReadAsync())
                {
                    estadosCita.Add(MapearEstadoCita(reader));
                }

                return (true, $"Se encontraron {estadosCita.Count} estados de cita", estadosCita);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de cita: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<EstadoCita>? estadosCita, int totalRegistros)>
            ListarEstadosCitaPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM estadocita";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM estadocita 
                    ORDER BY nombre ASC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosCita = new List<EstadoCita>();
                while (await reader.ReadAsync())
                {
                    estadosCita.Add(MapearEstadoCita(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", estadosCita, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar estados de cita: {ex.Message}", null, 0);
            }
        }

        // Buscar estados de cita
        public async Task<(bool exito, string mensaje, List<EstadoCita>? estadosCita)>
            BuscarEstadosCitaAsync(string? termino = null)
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

                string query = $@"SELECT * FROM estadocita 
                    {whereClause}
                    ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var estadosCita = new List<EstadoCita>();
                while (await reader.ReadAsync())
                {
                    estadosCita.Add(MapearEstadoCita(reader));
                }

                return (true, $"Se encontraron {estadosCita.Count} estados de cita", estadosCita);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar estados de cita: {ex.Message}", null);
            }
        }

        // Actualizar estado de cita
        public async Task<(bool exito, string mensaje)> ActualizarEstadoCitaAsync(EstadoCita estadoCita)
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
                string checkQuery = @"SELECT COUNT(*) FROM estadocita 
                    WHERE LOWER(nombre) = LOWER(@nombre) AND idEstadoCita != @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@nombre", estadoCita.Nombre);
                checkCmd.Parameters.AddWithValue("@id", estadoCita.IdEstadoCita);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, "Ya existe otro estado con ese nombre");
                }

                string query = @"UPDATE estadocita SET 
                    nombre = @nombre,
                    actualizadoAt = @actualizadoAt
                    WHERE idEstadoCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", estadoCita.IdEstadoCita);
                cmd.Parameters.AddWithValue("@nombre", estadoCita.Nombre);
                cmd.Parameters.AddWithValue("@actualizadoAt", estadoCita.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de cita actualizado exitosamente");
                }

                return (false, "No se encontró el estado de cita a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar estado de cita: {ex.Message}");
            }
        }

        // Eliminar estado de cita
        public async Task<(bool exito, string mensaje)> EliminarEstadoCitaAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar si hay citas usando este estado
                string checkQuery = "SELECT COUNT(*) FROM cita WHERE idEstadoCita = @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, $"No se puede eliminar el estado porque tiene {count} cita(s) asociada(s)");
                }

                string query = "DELETE FROM estadocita WHERE idEstadoCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de cita eliminado exitosamente");
                }

                return (false, "No se encontró el estado de cita a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar estado de cita: {ex.Message}");
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
                string query = "SELECT COUNT(*) FROM estadocita WHERE LOWER(nombre) = LOWER(@nombre)";

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

        // Contar citas por estado
        public async Task<(bool exito, string mensaje, int total)> ContarCitasPorEstadoAsync(int idEstado)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM cita WHERE idEstadoCita = @idEstado";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idEstado", idEstado);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El estado tiene {total} cita(s)", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar citas: {ex.Message}", 0);
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
                    ec.idEstadoCita,
                    ec.nombre,
                    COUNT(c.idCita) as totalCitas,
                    COUNT(CASE WHEN c.fecha >= CURDATE() THEN 1 END) as citasActivas
                    FROM estadocita ec
                    LEFT JOIN cita c ON ec.idEstadoCita = c.idEstadoCita
                    GROUP BY ec.idEstadoCita, ec.nombre
                    ORDER BY totalCitas DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var stats = new Dictionary<string, object>
                    {
                        ["idEstadoCita"] = reader.GetInt32("idEstadoCita"),
                        ["nombre"] = reader.GetString("nombre"),
                        ["totalCitas"] = reader.GetInt32("totalCitas"),
                        ["citasActivas"] = reader.GetInt32("citasActivas")
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
        public async Task<(bool exito, string mensaje, List<EstadoCita>? estados)>
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
                string query = @"SELECT ec.*, COUNT(c.idCita) as uso
                    FROM estadocita ec
                    LEFT JOIN cita c ON ec.idEstadoCita = c.idEstadoCita
                    GROUP BY ec.idEstadoCita
                    ORDER BY uso DESC
                    LIMIT @limite";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limite", limite);

                using var reader = await cmd.ExecuteReaderAsync();
                var estados = new List<EstadoCita>();
                while (await reader.ReadAsync())
                {
                    estados.Add(MapearEstadoCita(reader));
                }

                return (true, $"Se encontraron {estados.Count} estados más usados", estados);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estados más usados: {ex.Message}", null);
            }
        }

        // Obtener estados sin uso
        public async Task<(bool exito, string mensaje, List<EstadoCita>? estados)>
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
                string query = @"SELECT ec.*
                    FROM estadocita ec
                    LEFT JOIN cita c ON ec.idEstadoCita = c.idEstadoCita
                    WHERE c.idCita IS NULL
                    ORDER BY ec.nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estados = new List<EstadoCita>();
                while (await reader.ReadAsync())
                {
                    estados.Add(MapearEstadoCita(reader));
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
                    "Pendiente",
                    "Confirmada",
                    "Cancelada",
                    "Completada",
                    "Reprogramada",
                    "En Proceso",
                    "No Asistio",
                    "Rechazada"
                };

                int creados = 0;
                foreach (var estado in estadosPredeterminados)
                {
                    if (!await ExisteEstadoAsync(estado))
                    {
                        var estadoCita = new EstadoCita
                        {
                            Nombre = estado,
                            CreadoAt = new DateTime(2020, 1, 1),
                            ActualizadoAt = new DateTime(2020, 1, 1)
                        };

                        var (exito, _, _) = await CrearEstadoCitaAsync(estadoCita);
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
        private EstadoCita MapearEstadoCita(MySqlDataReader reader)
        {
            return new EstadoCita
            {
                IdEstadoCita = reader.GetInt32("idEstadoCita"),
                Nombre = reader.GetString("nombre"),
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
