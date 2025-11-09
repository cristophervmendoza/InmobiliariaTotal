using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class AgendaDao
    {
        // Crear evento de agenda
        public async Task<(bool exito, string mensaje, int? id)> CrearAgendaAsync(Agenda agenda)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO agenda 
                    (IdUsuario, Titulo, Tipo, Telefono, FechaHora, DescripcionEvento, 
                     Estado, Ubicacion, CreadoAt, ActualizadoAt, IdTipoPrioridad) 
                    VALUES (@IdUsuario, @Titulo, @Tipo, @Telefono, @FechaHora, @DescripcionEvento, 
                            @Estado, @Ubicacion, @CreadoAt, @ActualizadoAt, @IdTipoPrioridad);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdUsuario", agenda.IdUsuario);
                cmd.Parameters.AddWithValue("@Titulo", agenda.Titulo);
                cmd.Parameters.AddWithValue("@Tipo", string.IsNullOrEmpty(agenda.Tipo) ? DBNull.Value : agenda.Tipo);
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(agenda.Telefono) ? DBNull.Value : agenda.Telefono);
                cmd.Parameters.AddWithValue("@FechaHora", agenda.FechaHora);
                cmd.Parameters.AddWithValue("@DescripcionEvento", string.IsNullOrEmpty(agenda.DescripcionEvento) ? DBNull.Value : agenda.DescripcionEvento);
                cmd.Parameters.AddWithValue("@Estado", agenda.Estado ?? "Pendiente");
                cmd.Parameters.AddWithValue("@Ubicacion", string.IsNullOrEmpty(agenda.Ubicacion) ? DBNull.Value : agenda.Ubicacion);
                cmd.Parameters.AddWithValue("@CreadoAt", agenda.CreadoAt);
                cmd.Parameters.AddWithValue("@ActualizadoAt", agenda.ActualizadoAt);
                cmd.Parameters.AddWithValue("@IdTipoPrioridad", agenda.IdTipoPrioridad.HasValue ? (object)agenda.IdTipoPrioridad.Value : DBNull.Value);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Evento de agenda creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear evento de agenda: {ex.Message}", null);
            }
        }

        // Obtener agenda por ID
        public async Task<(bool exito, string mensaje, Agenda? agenda)> ObtenerAgendaPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Corregido: Removí el JOIN con tipoprioridad ya que no existe en tu esquema
                string query = @"SELECT a.*, u.nombre as NombreUsuario 
                    FROM agenda a
                    LEFT JOIN usuario u ON a.IdUsuario = u.idUsuario
                    WHERE a.IdAgenda = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var agenda = MapearAgenda(reader);
                    return (true, "Evento de agenda encontrado", agenda);
                }

                return (false, "Evento de agenda no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener evento de agenda: {ex.Message}", null);
            }
        }

        // Listar todas las agendas
        public async Task<(bool exito, string mensaje, List<Agenda>? agendas)> ListarAgendasAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT a.*, u.nombre as NombreUsuario 
                    FROM agenda a
                    LEFT JOIN usuario u ON a.IdUsuario = u.idUsuario
                    ORDER BY a.FechaHora ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var agendas = new List<Agenda>();
                while (await reader.ReadAsync())
                {
                    agendas.Add(MapearAgenda(reader));
                }

                return (true, $"Se encontraron {agendas.Count} eventos", agendas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar eventos de agenda: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Agenda>? agendas, int totalRegistros)>
            ListarAgendasPaginadasAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM agenda";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT a.*, u.nombre as NombreUsuario 
                    FROM agenda a
                    LEFT JOIN usuario u ON a.IdUsuario = u.idUsuario
                    ORDER BY a.FechaHora ASC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var agendas = new List<Agenda>();
                while (await reader.ReadAsync())
                {
                    agendas.Add(MapearAgenda(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", agendas, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar eventos: {ex.Message}", null, 0);
            }
        }

        // Buscar eventos
        public async Task<(bool exito, string mensaje, List<Agenda>? agendas)>
            BuscarAgendasAsync(string? termino = null, string? estado = null, int? idUsuario = null,
                              DateTime? fechaInicio = null, DateTime? fechaFin = null, int? idTipoPrioridad = null)
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
                    condiciones.Add("(a.Titulo LIKE @termino OR a.DescripcionEvento LIKE @termino OR a.Ubicacion LIKE @termino)");
                    parametros.Add(new MySqlParameter("@termino", $"%{termino}%"));
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    condiciones.Add("a.Estado = @estado");
                    parametros.Add(new MySqlParameter("@estado", estado));
                }

                if (idUsuario.HasValue)
                {
                    condiciones.Add("a.IdUsuario = @idUsuario");
                    parametros.Add(new MySqlParameter("@idUsuario", idUsuario.Value));
                }

                if (fechaInicio.HasValue)
                {
                    condiciones.Add("a.FechaHora >= @fechaInicio");
                    parametros.Add(new MySqlParameter("@fechaInicio", fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    condiciones.Add("a.FechaHora <= @fechaFin");
                    parametros.Add(new MySqlParameter("@fechaFin", fechaFin.Value));
                }

                if (idTipoPrioridad.HasValue)
                {
                    condiciones.Add("a.IdTipoPrioridad = @idTipoPrioridad");
                    parametros.Add(new MySqlParameter("@idTipoPrioridad", idTipoPrioridad.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT a.*, u.nombre as NombreUsuario 
                    FROM agenda a
                    LEFT JOIN usuario u ON a.IdUsuario = u.idUsuario
                    {whereClause}
                    ORDER BY a.FechaHora ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var agendas = new List<Agenda>();
                while (await reader.ReadAsync())
                {
                    agendas.Add(MapearAgenda(reader));
                }

                return (true, $"Se encontraron {agendas.Count} eventos", agendas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar eventos: {ex.Message}", null);
            }
        }

        // Obtener eventos próximos (próximas 24 horas)
        public async Task<(bool exito, string mensaje, List<Agenda>? agendas)>
            ObtenerEventosProximosAsync(int? idUsuario = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var ahora = DateTime.UtcNow;
                var limite = ahora.AddHours(24);

                string whereUsuario = idUsuario.HasValue ? "AND a.IdUsuario = @idUsuario" : "";

                string query = $@"SELECT a.*, u.nombre as NombreUsuario 
                    FROM agenda a
                    LEFT JOIN usuario u ON a.IdUsuario = u.idUsuario
                    WHERE a.FechaHora >= @ahora AND a.FechaHora <= @limite 
                    AND a.Estado NOT IN ('Cancelado', 'Completado')
                    {whereUsuario}
                    ORDER BY a.FechaHora ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ahora", ahora);
                cmd.Parameters.AddWithValue("@limite", limite);
                if (idUsuario.HasValue)
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario.Value);
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var agendas = new List<Agenda>();
                while (await reader.ReadAsync())
                {
                    agendas.Add(MapearAgenda(reader));
                }

                return (true, $"Se encontraron {agendas.Count} eventos próximos", agendas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener eventos próximos: {ex.Message}", null);
            }
        }

        // Obtener eventos por rango de fechas
        public async Task<(bool exito, string mensaje, List<Agenda>? agendas)>
            ObtenerEventosPorRangoAsync(DateTime fechaInicio, DateTime fechaFin, int? idUsuario = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string whereUsuario = idUsuario.HasValue ? "AND a.IdUsuario = @idUsuario" : "";

                string query = $@"SELECT a.*, u.nombre as NombreUsuario 
                    FROM agenda a
                    LEFT JOIN usuario u ON a.IdUsuario = u.idUsuario
                    WHERE a.FechaHora >= @fechaInicio AND a.FechaHora <= @fechaFin
                    {whereUsuario}
                    ORDER BY a.FechaHora ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                if (idUsuario.HasValue)
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario.Value);
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var agendas = new List<Agenda>();
                while (await reader.ReadAsync())
                {
                    agendas.Add(MapearAgenda(reader));
                }

                return (true, $"Se encontraron {agendas.Count} eventos en el rango especificado", agendas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener eventos por rango: {ex.Message}", null);
            }
        }

        // Obtener eventos por usuario
        public async Task<(bool exito, string mensaje, List<Agenda>? agendas)>
            ObtenerEventosPorUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT a.*, u.nombre as NombreUsuario 
                    FROM agenda a
                    LEFT JOIN usuario u ON a.IdUsuario = u.idUsuario
                    WHERE a.IdUsuario = @idUsuario
                    ORDER BY a.FechaHora ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                var agendas = new List<Agenda>();
                while (await reader.ReadAsync())
                {
                    agendas.Add(MapearAgenda(reader));
                }

                return (true, $"Se encontraron {agendas.Count} eventos del usuario", agendas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener eventos del usuario: {ex.Message}", null);
            }
        }

        // Actualizar agenda
        public async Task<(bool exito, string mensaje)> ActualizarAgendaAsync(Agenda agenda)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE agenda SET 
                    Titulo = @Titulo,
                    Tipo = @Tipo,
                    Telefono = @Telefono,
                    FechaHora = @FechaHora,
                    DescripcionEvento = @DescripcionEvento,
                    Estado = @Estado,
                    Ubicacion = @Ubicacion,
                    ActualizadoAt = @ActualizadoAt,
                    IdTipoPrioridad = @IdTipoPrioridad
                    WHERE IdAgenda = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", agenda.IdAgenda);
                cmd.Parameters.AddWithValue("@Titulo", agenda.Titulo);
                cmd.Parameters.AddWithValue("@Tipo", string.IsNullOrEmpty(agenda.Tipo) ? DBNull.Value : agenda.Tipo);
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrEmpty(agenda.Telefono) ? DBNull.Value : agenda.Telefono);
                cmd.Parameters.AddWithValue("@FechaHora", agenda.FechaHora);
                cmd.Parameters.AddWithValue("@DescripcionEvento", string.IsNullOrEmpty(agenda.DescripcionEvento) ? DBNull.Value : agenda.DescripcionEvento);
                cmd.Parameters.AddWithValue("@Estado", agenda.Estado ?? "Pendiente");
                cmd.Parameters.AddWithValue("@Ubicacion", string.IsNullOrEmpty(agenda.Ubicacion) ? DBNull.Value : agenda.Ubicacion);
                cmd.Parameters.AddWithValue("@ActualizadoAt", agenda.ActualizadoAt);
                cmd.Parameters.AddWithValue("@IdTipoPrioridad", agenda.IdTipoPrioridad.HasValue ? (object)agenda.IdTipoPrioridad.Value : DBNull.Value);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Evento de agenda actualizado exitosamente");
                }

                return (false, "No se encontró el evento de agenda a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar evento de agenda: {ex.Message}");
            }
        }

        // Cambiar estado del evento
        public async Task<(bool exito, string mensaje)> CambiarEstadoAsync(int id, string nuevoEstado)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE agenda SET 
                    Estado = @nuevoEstado,
                    ActualizadoAt = @actualizadoAt
                    WHERE IdAgenda = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@nuevoEstado", nuevoEstado);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.UtcNow);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, $"Estado cambiado a {nuevoEstado} exitosamente");
                }

                return (false, "No se encontró el evento");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al cambiar estado: {ex.Message}");
            }
        }

        // Eliminar agenda
        public async Task<(bool exito, string mensaje)> EliminarAgendaAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM agenda WHERE IdAgenda = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Evento de agenda eliminado exitosamente");
                }

                return (false, "No se encontró el evento de agenda a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar evento de agenda: {ex.Message}");
            }
        }

        // Obtener estadísticas
        public async Task<(bool exito, string mensaje, Dictionary<string, object>? estadisticas)>
            ObtenerEstadisticasAsync(int? idUsuario = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string whereUsuario = idUsuario.HasValue ? "WHERE IdUsuario = @idUsuario" : "";

                string query = $@"SELECT 
                    COUNT(*) as TotalEventos,
                    COUNT(CASE WHEN Estado = 'Pendiente' THEN 1 END) as EventosPendientes,
                    COUNT(CASE WHEN Estado = 'Confirmado' THEN 1 END) as EventosConfirmados,
                    COUNT(CASE WHEN Estado = 'Cancelado' THEN 1 END) as EventosCancelados,
                    COUNT(CASE WHEN Estado = 'Completado' THEN 1 END) as EventosCompletados,
                    COUNT(CASE WHEN FechaHora < @ahora THEN 1 END) as EventosPasados,
                    COUNT(CASE WHEN FechaHora >= @ahora THEN 1 END) as EventosFuturos
                    FROM agenda
                    {whereUsuario}";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ahora", DateTime.UtcNow);
                if (idUsuario.HasValue)
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario.Value);
                }

                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalEventos"] = reader.GetInt32("TotalEventos");
                    estadisticas["eventosPendientes"] = reader.GetInt32("EventosPendientes");
                    estadisticas["eventosConfirmados"] = reader.GetInt32("EventosConfirmados");
                    estadisticas["eventosCancelados"] = reader.GetInt32("EventosCancelados");
                    estadisticas["eventosCompletados"] = reader.GetInt32("EventosCompletados");
                    estadisticas["eventosPasados"] = reader.GetInt32("EventosPasados");
                    estadisticas["eventosFuturos"] = reader.GetInt32("EventosFuturos");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear resultados
        private Agenda MapearAgenda(MySqlDataReader reader)
        {
            return new Agenda
            {
                IdAgenda = reader.GetInt32("IdAgenda"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                Titulo = reader.GetString("Titulo"),
                Tipo = !reader.IsDBNull(reader.GetOrdinal("Tipo")) ? reader.GetString("Tipo") : null,
                Telefono = !reader.IsDBNull(reader.GetOrdinal("Telefono")) ? reader.GetString("Telefono") : null,
                FechaHora = reader.GetDateTime("FechaHora"),
                DescripcionEvento = !reader.IsDBNull(reader.GetOrdinal("DescripcionEvento")) ? reader.GetString("DescripcionEvento") : null,
                Estado = !reader.IsDBNull(reader.GetOrdinal("Estado")) ? reader.GetString("Estado") : "Pendiente",
                Ubicacion = !reader.IsDBNull(reader.GetOrdinal("Ubicacion")) ? reader.GetString("Ubicacion") : null,
                CreadoAt = reader.GetDateTime("CreadoAt"),
                ActualizadoAt = reader.GetDateTime("ActualizadoAt"),
                IdTipoPrioridad = !reader.IsDBNull(reader.GetOrdinal("IdTipoPrioridad")) ? reader.GetInt32("IdTipoPrioridad") : null
            };
        }
    }
}
