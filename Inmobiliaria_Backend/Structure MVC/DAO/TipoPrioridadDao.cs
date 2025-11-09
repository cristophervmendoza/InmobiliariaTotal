using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class TipoPrioridadDao
    {
        // Crear tipo de prioridad
        public async Task<(bool exito, string mensaje, int? id)> CrearTipoPrioridadAsync(TipoPrioridad tipoPrioridad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar que no exista un tipo con el mismo nombre
                if (await ExisteTipoPrioridadAsync(tipoPrioridad.Nombre))
                {
                    return (false, "Ya existe un tipo de prioridad con ese nombre", null);
                }

                string query = @"INSERT INTO tipoprioridad 
                    (nombre, creadoAt, actualizadoAt) 
                    VALUES (@nombre, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", tipoPrioridad.Nombre);
                cmd.Parameters.AddWithValue("@creadoAt", tipoPrioridad.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", tipoPrioridad.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Tipo de prioridad creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear tipo de prioridad: {ex.Message}", null);
            }
        }

        // Obtener tipo de prioridad por ID
        public async Task<(bool exito, string mensaje, TipoPrioridad? tipoPrioridad)> ObtenerTipoPrioridadPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM tipoprioridad WHERE idTipoPrioridad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var tipoPrioridad = MapearTipoPrioridad(reader);
                    return (true, "Tipo de prioridad encontrado", tipoPrioridad);
                }

                return (false, "Tipo de prioridad no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipo de prioridad: {ex.Message}", null);
            }
        }

        // Obtener tipo de prioridad por nombre
        public async Task<(bool exito, string mensaje, TipoPrioridad? tipoPrioridad)> ObtenerTipoPrioridadPorNombreAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM tipoprioridad WHERE LOWER(nombre) = LOWER(@nombre)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nombre);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var tipoPrioridad = MapearTipoPrioridad(reader);
                    return (true, "Tipo de prioridad encontrado", tipoPrioridad);
                }

                return (false, "Tipo de prioridad no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipo de prioridad: {ex.Message}", null);
            }
        }

        // Listar todos los tipos de prioridad
        public async Task<(bool exito, string mensaje, List<TipoPrioridad>? tiposPrioridad)> ListarTiposPrioridadAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM tipoprioridad ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var tiposPrioridad = new List<TipoPrioridad>();
                while (await reader.ReadAsync())
                {
                    tiposPrioridad.Add(MapearTipoPrioridad(reader));
                }

                return (true, $"Se encontraron {tiposPrioridad.Count} tipos de prioridad", tiposPrioridad);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar tipos de prioridad: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<TipoPrioridad>? tiposPrioridad, int totalRegistros)>
            ListarTiposPrioridadPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM tipoprioridad";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM tipoprioridad 
                    ORDER BY nombre ASC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var tiposPrioridad = new List<TipoPrioridad>();
                while (await reader.ReadAsync())
                {
                    tiposPrioridad.Add(MapearTipoPrioridad(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", tiposPrioridad, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar tipos de prioridad: {ex.Message}", null, 0);
            }
        }

        // Buscar tipos de prioridad
        public async Task<(bool exito, string mensaje, List<TipoPrioridad>? tiposPrioridad)>
            BuscarTiposPrioridadAsync(string? termino = null)
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

                string query = $@"SELECT * FROM tipoprioridad 
                    {whereClause}
                    ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var tiposPrioridad = new List<TipoPrioridad>();
                while (await reader.ReadAsync())
                {
                    tiposPrioridad.Add(MapearTipoPrioridad(reader));
                }

                return (true, $"Se encontraron {tiposPrioridad.Count} tipos de prioridad", tiposPrioridad);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar tipos de prioridad: {ex.Message}", null);
            }
        }

        // Actualizar tipo de prioridad
        public async Task<(bool exito, string mensaje)> ActualizarTipoPrioridadAsync(TipoPrioridad tipoPrioridad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar que no exista otro tipo con el mismo nombre
                string checkQuery = @"SELECT COUNT(*) FROM tipoprioridad 
                    WHERE LOWER(nombre) = LOWER(@nombre) AND idTipoPrioridad != @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@nombre", tipoPrioridad.Nombre);
                checkCmd.Parameters.AddWithValue("@id", tipoPrioridad.IdTipoPrioridad);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, "Ya existe otro tipo de prioridad con ese nombre");
                }

                string query = @"UPDATE tipoprioridad SET 
                    nombre = @nombre,
                    actualizadoAt = @actualizadoAt
                    WHERE idTipoPrioridad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", tipoPrioridad.IdTipoPrioridad);
                cmd.Parameters.AddWithValue("@nombre", tipoPrioridad.Nombre);
                cmd.Parameters.AddWithValue("@actualizadoAt", tipoPrioridad.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Tipo de prioridad actualizado exitosamente");
                }

                return (false, "No se encontró el tipo de prioridad a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar tipo de prioridad: {ex.Message}");
            }
        }

        // Eliminar tipo de prioridad
        public async Task<(bool exito, string mensaje)> EliminarTipoPrioridadAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar si hay agendas usando este tipo de prioridad
                string checkQuery = "SELECT COUNT(*) FROM agenda WHERE IdTipoPrioridad = @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, $"No se puede eliminar el tipo de prioridad porque tiene {count} agenda(s) asociada(s)");
                }

                string query = "DELETE FROM tipoprioridad WHERE idTipoPrioridad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Tipo de prioridad eliminado exitosamente");
                }

                return (false, "No se encontró el tipo de prioridad a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar tipo de prioridad: {ex.Message}");
            }
        }

        // Verificar si existe un tipo de prioridad con el nombre
        public async Task<bool> ExisteTipoPrioridadAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM tipoprioridad WHERE LOWER(nombre) = LOWER(@nombre)";

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

        // Contar agendas por tipo de prioridad
        public async Task<(bool exito, string mensaje, int total)> ContarAgendasPorTipoPrioridadAsync(int idTipoPrioridad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM agenda WHERE IdTipoPrioridad = @idTipoPrioridad";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idTipoPrioridad", idTipoPrioridad);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El tipo de prioridad tiene {total} agenda(s)", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar agendas: {ex.Message}", 0);
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
                    tp.idTipoPrioridad,
                    tp.nombre,
                    COUNT(a.IdAgenda) as totalAgendas,
                    COUNT(CASE WHEN a.FechaHora >= NOW() THEN 1 END) as agendasActivas
                    FROM tipoprioridad tp
                    LEFT JOIN agenda a ON tp.idTipoPrioridad = a.IdTipoPrioridad
                    GROUP BY tp.idTipoPrioridad, tp.nombre
                    ORDER BY totalAgendas DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var stats = new Dictionary<string, object>
                    {
                        ["idTipoPrioridad"] = reader.GetInt32("idTipoPrioridad"),
                        ["nombre"] = reader.GetString("nombre"),
                        ["totalAgendas"] = reader.GetInt32("totalAgendas"),
                        ["agendasActivas"] = reader.GetInt32("agendasActivas")
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

        // Obtener tipos de prioridad más usados
        public async Task<(bool exito, string mensaje, List<TipoPrioridad>? tipos)>
            ObtenerTiposMasUsadosAsync(int limite = 5)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT tp.*, COUNT(a.IdAgenda) as uso
                    FROM tipoprioridad tp
                    LEFT JOIN agenda a ON tp.idTipoPrioridad = a.IdTipoPrioridad
                    GROUP BY tp.idTipoPrioridad
                    ORDER BY uso DESC
                    LIMIT @limite";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limite", limite);

                using var reader = await cmd.ExecuteReaderAsync();
                var tipos = new List<TipoPrioridad>();
                while (await reader.ReadAsync())
                {
                    tipos.Add(MapearTipoPrioridad(reader));
                }

                return (true, $"Se encontraron {tipos.Count} tipos más usados", tipos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipos más usados: {ex.Message}", null);
            }
        }

        // Obtener tipos sin uso
        public async Task<(bool exito, string mensaje, List<TipoPrioridad>? tipos)>
            ObtenerTiposSinUsoAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT tp.*
                    FROM tipoprioridad tp
                    LEFT JOIN agenda a ON tp.idTipoPrioridad = a.IdTipoPrioridad
                    WHERE a.IdAgenda IS NULL
                    ORDER BY tp.nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var tipos = new List<TipoPrioridad>();
                while (await reader.ReadAsync())
                {
                    tipos.Add(MapearTipoPrioridad(reader));
                }

                return (true, $"Se encontraron {tipos.Count} tipos sin uso", tipos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipos sin uso: {ex.Message}", null);
            }
        }

        // Inicializar tipos de prioridad predeterminados
        public async Task<(bool exito, string mensaje)> InicializarTiposPrioridadPredeterminadosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var tiposPredeterminados = new[]
                {
                    "Baja",
                    "Media",
                    "Normal",
                    "Alta",
                    "Urgente",
                    "Critica",
                    "Muy Baja",
                    "Muy Alta"
                };

                int creados = 0;
                foreach (var tipo in tiposPredeterminados)
                {
                    if (!await ExisteTipoPrioridadAsync(tipo))
                    {
                        var tipoPrioridad = new TipoPrioridad
                        {
                            Nombre = tipo,
                            CreadoAt = new DateTime(2020, 1, 1),
                            ActualizadoAt = new DateTime(2020, 1, 1)
                        };

                        var (exito, _, _) = await CrearTipoPrioridadAsync(tipoPrioridad);
                        if (exito) creados++;
                    }
                }

                return (true, $"Se crearon {creados} tipos de prioridad predeterminados");
            }
            catch (Exception ex)
            {
                return (false, $"Error al inicializar tipos de prioridad: {ex.Message}");
            }
        }

        // Método auxiliar para mapear
        private TipoPrioridad MapearTipoPrioridad(MySqlDataReader reader)
        {
            return new TipoPrioridad
            {
                IdTipoPrioridad = reader.GetInt32("idTipoPrioridad"),
                Nombre = reader.GetString("nombre"),
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
