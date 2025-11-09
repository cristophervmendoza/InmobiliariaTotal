using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class CitaDao
    {
        // Crear cita con validación completa
        public async Task<(bool exito, string mensaje, int? id)> CrearCitaAsync(Cita cita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar disponibilidad del agente
                if (await ExisteConflictoHorarioAsync(cita.IdAgente, cita.Fecha, cita.Hora, null))
                {
                    return (false, "El agente ya tiene una cita programada en ese horario", null);
                }

                // Verificar que el cliente no tenga otra cita en el mismo horario
                if (await ClienteTieneCitaEnHorarioAsync(cita.IdCliente, cita.Fecha, cita.Hora, null))
                {
                    return (false, "El cliente ya tiene una cita programada en ese horario", null);
                }

                string query = @"INSERT INTO cita 
                    (fecha, hora, idCliente, idAgente, idEstadoCita, creadoAt, actualizadoAt) 
                    VALUES (@fecha, @hora, @idCliente, @idAgente, @idEstadoCita, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fecha", cita.Fecha.Date);
                cmd.Parameters.AddWithValue("@hora", cita.Hora);
                cmd.Parameters.AddWithValue("@idCliente", cita.IdCliente);
                cmd.Parameters.AddWithValue("@idAgente", cita.IdAgente);
                cmd.Parameters.AddWithValue("@idEstadoCita", cita.IdEstadoCita);
                cmd.Parameters.AddWithValue("@creadoAt", cita.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", cita.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Cita creada exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear cita: {ex.Message}", null);
            }
        }

        // Obtener cita por ID con información completa
        public async Task<(bool exito, string mensaje, Cita? cita)> ObtenerCitaPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT c.*, 
                    cl.idCliente, cl.idUsuario as clienteIdUsuario,
                    uc.IdUsuario as clienteUsrId, uc.Nombre as clienteNombre, uc.Email as clienteEmail, 
                    uc.Dni as clienteDni, uc.Telefono as clienteTelefono,
                    a.idAgente, a.idUsuario as agenteIdUsuario,
                    ua.IdUsuario as agenteUsrId, ua.Nombre as agenteNombre, ua.Email as agenteEmail, 
                    ua.Dni as agenteDni, ua.Telefono as agenteTelefono,
                    ec.idEstadoCita as estadoCitaId, ec.nombre as estadoCitaNombre
                    FROM cita c
                    INNER JOIN cliente cl ON c.idCliente = cl.idCliente
                    INNER JOIN usuario uc ON cl.idUsuario = uc.IdUsuario
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario ua ON a.idUsuario = ua.IdUsuario
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    WHERE c.idCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var cita = MapearCitaCompleta(reader);
                    return (true, "Cita encontrada", cita);
                }

                return (false, "Cita no encontrada", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener cita: {ex.Message}", null);
            }
        }

        // Listar todas las citas
        public async Task<(bool exito, string mensaje, List<Cita>? citas)> ListarCitasAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT c.*, 
                    cl.idCliente, uc.Nombre as clienteNombre,
                    a.idAgente, ua.Nombre as agenteNombre,
                    ec.nombre as estadoCitaNombre
                    FROM cita c
                    INNER JOIN cliente cl ON c.idCliente = cl.idCliente
                    INNER JOIN usuario uc ON cl.idUsuario = uc.IdUsuario
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario ua ON a.idUsuario = ua.IdUsuario
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    ORDER BY c.fecha DESC, c.hora DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var citas = new List<Cita>();
                while (await reader.ReadAsync())
                {
                    citas.Add(MapearCitaSimple(reader));
                }

                return (true, $"Se encontraron {citas.Count} citas", citas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar citas: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Cita>? citas, int totalRegistros)>
            ListarCitasPaginadasAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM cita";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT c.*, 
                    cl.idCliente, uc.Nombre as clienteNombre,
                    a.idAgente, ua.Nombre as agenteNombre,
                    ec.nombre as estadoCitaNombre
                    FROM cita c
                    INNER JOIN cliente cl ON c.idCliente = cl.idCliente
                    INNER JOIN usuario uc ON cl.idUsuario = uc.IdUsuario
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario ua ON a.idUsuario = ua.IdUsuario
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    ORDER BY c.fecha DESC, c.hora DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var citas = new List<Cita>();
                while (await reader.ReadAsync())
                {
                    citas.Add(MapearCitaSimple(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", citas, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar citas: {ex.Message}", null, 0);
            }
        }

        // Buscar citas con filtros avanzados
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            BuscarCitasAsync(int? idCliente = null, int? idAgente = null, int? idEstado = null,
                            DateTime? fechaInicio = null, DateTime? fechaFin = null)
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

                if (idCliente.HasValue)
                {
                    condiciones.Add("c.idCliente = @idCliente");
                    parametros.Add(new MySqlParameter("@idCliente", idCliente.Value));
                }

                if (idAgente.HasValue)
                {
                    condiciones.Add("c.idAgente = @idAgente");
                    parametros.Add(new MySqlParameter("@idAgente", idAgente.Value));
                }

                if (idEstado.HasValue)
                {
                    condiciones.Add("c.idEstadoCita = @idEstado");
                    parametros.Add(new MySqlParameter("@idEstado", idEstado.Value));
                }

                if (fechaInicio.HasValue)
                {
                    condiciones.Add("c.fecha >= @fechaInicio");
                    parametros.Add(new MySqlParameter("@fechaInicio", fechaInicio.Value));
                }

                if (fechaFin.HasValue)
                {
                    condiciones.Add("c.fecha <= @fechaFin");
                    parametros.Add(new MySqlParameter("@fechaFin", fechaFin.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT c.*, 
                    cl.idCliente, uc.Nombre as clienteNombre,
                    a.idAgente, ua.Nombre as agenteNombre,
                    ec.nombre as estadoCitaNombre
                    FROM cita c
                    INNER JOIN cliente cl ON c.idCliente = cl.idCliente
                    INNER JOIN usuario uc ON cl.idUsuario = uc.IdUsuario
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario ua ON a.idUsuario = ua.IdUsuario
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    {whereClause}
                    ORDER BY c.fecha DESC, c.hora DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var citas = new List<Cita>();
                while (await reader.ReadAsync())
                {
                    citas.Add(MapearCitaSimple(reader));
                }

                return (true, $"Se encontraron {citas.Count} citas", citas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar citas: {ex.Message}", null);
            }
        }

        // Obtener citas por cliente
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasPorClienteAsync(int idCliente)
        {
            return await BuscarCitasAsync(idCliente: idCliente);
        }

        // Obtener citas por agente
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasPorAgenteAsync(int idAgente)
        {
            return await BuscarCitasAsync(idAgente: idAgente);
        }

        // Obtener citas por estado
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasPorEstadoAsync(int idEstado)
        {
            return await BuscarCitasAsync(idEstado: idEstado);
        }

        // Obtener citas de hoy
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasDeHoyAsync()
        {
            var hoy = DateTime.Now.Date;
            return await BuscarCitasAsync(fechaInicio: hoy, fechaFin: hoy);
        }

        // Obtener citas de la semana
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasDeLaSemanaAsync()
        {
            var inicioSemana = DateTime.Now.Date;
            var finSemana = inicioSemana.AddDays(7);
            return await BuscarCitasAsync(fechaInicio: inicioSemana, fechaFin: finSemana);
        }

        // Obtener citas del mes
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasDelMesAsync()
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);
            return await BuscarCitasAsync(fechaInicio: inicioMes, fechaFin: finMes);
        }

        // Obtener citas próximas
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasProximasAsync(int dias = 7)
        {
            var fechaInicio = DateTime.Now.Date;
            var fechaFin = DateTime.Now.Date.AddDays(dias);
            return await BuscarCitasAsync(fechaInicio: fechaInicio, fechaFin: fechaFin);
        }

        // Obtener citas pasadas
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasPasadasAsync(int dias = 30)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var fechaInicio = DateTime.Now.Date.AddDays(-dias);
                var fechaFin = DateTime.Now.Date.AddDays(-1);

                string query = @"SELECT c.*, 
                    cl.idCliente, uc.Nombre as clienteNombre,
                    a.idAgente, ua.Nombre as agenteNombre,
                    ec.nombre as estadoCitaNombre
                    FROM cita c
                    INNER JOIN cliente cl ON c.idCliente = cl.idCliente
                    INNER JOIN usuario uc ON cl.idUsuario = uc.IdUsuario
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario ua ON a.idUsuario = ua.IdUsuario
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    WHERE c.fecha >= @fechaInicio AND c.fecha <= @fechaFin
                    ORDER BY c.fecha DESC, c.hora DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);

                using var reader = await cmd.ExecuteReaderAsync();
                var citas = new List<Cita>();
                while (await reader.ReadAsync())
                {
                    citas.Add(MapearCitaSimple(reader));
                }

                return (true, $"Se encontraron {citas.Count} citas pasadas", citas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener citas pasadas: {ex.Message}", null);
            }
        }

        // Obtener citas pendientes de confirmación
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasPendientesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT c.*, 
                    cl.idCliente, uc.Nombre as clienteNombre,
                    a.idAgente, ua.Nombre as agenteNombre,
                    ec.nombre as estadoCitaNombre
                    FROM cita c
                    INNER JOIN cliente cl ON c.idCliente = cl.idCliente
                    INNER JOIN usuario uc ON cl.idUsuario = uc.IdUsuario
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario ua ON a.idUsuario = ua.IdUsuario
                    INNER JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    WHERE LOWER(ec.nombre) = 'pendiente' AND c.fecha >= CURDATE()
                    ORDER BY c.fecha ASC, c.hora ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var citas = new List<Cita>();
                while (await reader.ReadAsync())
                {
                    citas.Add(MapearCitaSimple(reader));
                }

                return (true, $"Se encontraron {citas.Count} citas pendientes", citas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener citas pendientes: {ex.Message}", null);
            }
        }

        // Obtener citas confirmadas
        public async Task<(bool exito, string mensaje, List<Cita>? citas)>
            ObtenerCitasConfirmadasAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT c.*, 
                    cl.idCliente, uc.Nombre as clienteNombre,
                    a.idAgente, ua.Nombre as agenteNombre,
                    ec.nombre as estadoCitaNombre
                    FROM cita c
                    INNER JOIN cliente cl ON c.idCliente = cl.idCliente
                    INNER JOIN usuario uc ON cl.idUsuario = uc.IdUsuario
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario ua ON a.idUsuario = ua.IdUsuario
                    INNER JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    WHERE LOWER(ec.nombre) = 'confirmada' AND c.fecha >= CURDATE()
                    ORDER BY c.fecha ASC, c.hora ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var citas = new List<Cita>();
                while (await reader.ReadAsync())
                {
                    citas.Add(MapearCitaSimple(reader));
                }

                return (true, $"Se encontraron {citas.Count} citas confirmadas", citas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener citas confirmadas: {ex.Message}", null);
            }
        }

        // Obtener disponibilidad de agente por día
        public async Task<(bool exito, string mensaje, List<TimeSpan>? horasDisponibles)>
            ObtenerDisponibilidadAgenteAsync(int idAgente, DateTime fecha)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT hora FROM cita 
                    WHERE idAgente = @idAgente 
                    AND fecha = @fecha
                    AND idEstadoCita NOT IN (
                        SELECT idEstadoCita FROM estadocita 
                        WHERE LOWER(nombre) IN ('cancelada', 'rechazada')
                    )
                    ORDER BY hora ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idAgente", idAgente);
                cmd.Parameters.AddWithValue("@fecha", fecha.Date);

                using var reader = await cmd.ExecuteReaderAsync();
                var horasOcupadas = new List<TimeSpan>();
                while (await reader.ReadAsync())
                {
                    horasOcupadas.Add(reader.GetTimeSpan("hora"));
                }

                // Generar horas disponibles (8:00 AM - 7:00 PM, intervalos de 1 hora)
                var horasDisponibles = new List<TimeSpan>();
                for (int hora = 8; hora < 20; hora++)
                {
                    var timeSlot = new TimeSpan(hora, 0, 0);
                    if (!horasOcupadas.Any(h => Math.Abs((h - timeSlot).TotalMinutes) < 60))
                    {
                        horasDisponibles.Add(timeSlot);
                    }
                }

                return (true, $"Se encontraron {horasDisponibles.Count} horas disponibles", horasDisponibles);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener disponibilidad: {ex.Message}", null);
            }
        }

        // Verificar conflicto de horario para agente
        public async Task<bool> ExisteConflictoHorarioAsync(int idAgente, DateTime fecha, TimeSpan hora, int? idCitaExcluir = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT COUNT(*) FROM cita c
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    WHERE c.idAgente = @idAgente 
                    AND c.fecha = @fecha 
                    AND ABS(TIMESTAMPDIFF(MINUTE, c.hora, @hora)) < 60
                    AND (ec.nombre NOT IN ('Cancelada', 'Rechazada') OR ec.nombre IS NULL)";

                if (idCitaExcluir.HasValue)
                {
                    query += " AND c.idCita != @idCitaExcluir";
                }

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idAgente", idAgente);
                cmd.Parameters.AddWithValue("@fecha", fecha.Date);
                cmd.Parameters.AddWithValue("@hora", hora);
                if (idCitaExcluir.HasValue)
                {
                    cmd.Parameters.AddWithValue("@idCitaExcluir", idCitaExcluir.Value);
                }

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Verificar si cliente tiene cita en el horario
        public async Task<bool> ClienteTieneCitaEnHorarioAsync(int idCliente, DateTime fecha, TimeSpan hora, int? idCitaExcluir = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT COUNT(*) FROM cita c
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    WHERE c.idCliente = @idCliente 
                    AND c.fecha = @fecha 
                    AND ABS(TIMESTAMPDIFF(MINUTE, c.hora, @hora)) < 60
                    AND (ec.nombre NOT IN ('Cancelada', 'Rechazada') OR ec.nombre IS NULL)";

                if (idCitaExcluir.HasValue)
                {
                    query += " AND c.idCita != @idCitaExcluir";
                }

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@fecha", fecha.Date);
                cmd.Parameters.AddWithValue("@hora", hora);
                if (idCitaExcluir.HasValue)
                {
                    cmd.Parameters.AddWithValue("@idCitaExcluir", idCitaExcluir.Value);
                }

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Actualizar cita
        public async Task<(bool exito, string mensaje)> ActualizarCitaAsync(Cita cita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar disponibilidad si cambió fecha/hora/agente
                if (await ExisteConflictoHorarioAsync(cita.IdAgente, cita.Fecha, cita.Hora, cita.IdCita))
                {
                    return (false, "El agente ya tiene una cita programada en ese horario");
                }

                if (await ClienteTieneCitaEnHorarioAsync(cita.IdCliente, cita.Fecha, cita.Hora, cita.IdCita))
                {
                    return (false, "El cliente ya tiene una cita programada en ese horario");
                }

                string query = @"UPDATE cita SET 
                    fecha = @fecha,
                    hora = @hora,
                    idAgente = @idAgente,
                    idEstadoCita = @idEstadoCita,
                    actualizadoAt = @actualizadoAt
                    WHERE idCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", cita.IdCita);
                cmd.Parameters.AddWithValue("@fecha", cita.Fecha.Date);
                cmd.Parameters.AddWithValue("@hora", cita.Hora);
                cmd.Parameters.AddWithValue("@idAgente", cita.IdAgente);
                cmd.Parameters.AddWithValue("@idEstadoCita", cita.IdEstadoCita);
                cmd.Parameters.AddWithValue("@actualizadoAt", cita.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Cita actualizada exitosamente");
                }

                return (false, "No se encontró la cita a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar cita: {ex.Message}");
            }
        }

        // Cambiar estado de cita
        public async Task<(bool exito, string mensaje)> CambiarEstadoCitaAsync(int idCita, int idEstado)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE cita SET 
                    idEstadoCita = @idEstado,
                    actualizadoAt = @actualizadoAt
                    WHERE idCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idCita);
                cmd.Parameters.AddWithValue("@idEstado", idEstado);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.Now);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de cita actualizado exitosamente");
                }

                return (false, "No se encontró la cita");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al cambiar estado: {ex.Message}");
            }
        }

        // Cancelar cita
        public async Task<(bool exito, string mensaje)> CancelarCitaAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Obtener el ID del estado "Cancelada"
                string queryEstado = "SELECT idEstadoCita FROM estadocita WHERE LOWER(nombre) = 'cancelada' LIMIT 1";
                using var cmdEstado = new MySqlCommand(queryEstado, connection);
                var resultEstado = await cmdEstado.ExecuteScalarAsync();

                if (resultEstado == null)
                {
                    return (false, "No se encontró el estado 'Cancelada' en el sistema");
                }

                int idEstadoCancelada = Convert.ToInt32(resultEstado);

                return await CambiarEstadoCitaAsync(idCita, idEstadoCancelada);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al cancelar cita: {ex.Message}");
            }
        }

        // Confirmar cita
        public async Task<(bool exito, string mensaje)> ConfirmarCitaAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string queryEstado = "SELECT idEstadoCita FROM estadocita WHERE LOWER(nombre) = 'confirmada' LIMIT 1";
                using var cmdEstado = new MySqlCommand(queryEstado, connection);
                var resultEstado = await cmdEstado.ExecuteScalarAsync();

                if (resultEstado == null)
                {
                    return (false, "No se encontró el estado 'Confirmada' en el sistema");
                }

                int idEstadoConfirmada = Convert.ToInt32(resultEstado);

                return await CambiarEstadoCitaAsync(idCita, idEstadoConfirmada);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al confirmar cita: {ex.Message}");
            }
        }

        // Completar cita
        public async Task<(bool exito, string mensaje)> CompletarCitaAsync(int idCita)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string queryEstado = "SELECT idEstadoCita FROM estadocita WHERE LOWER(nombre) = 'completada' LIMIT 1";
                using var cmdEstado = new MySqlCommand(queryEstado, connection);
                var resultEstado = await cmdEstado.ExecuteScalarAsync();

                if (resultEstado == null)
                {
                    return (false, "No se encontró el estado 'Completada' en el sistema");
                }

                int idEstadoCompletada = Convert.ToInt32(resultEstado);

                return await CambiarEstadoCitaAsync(idCita, idEstadoCompletada);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al completar cita: {ex.Message}");
            }
        }

        // Reprogramar cita
        public async Task<(bool exito, string mensaje)> ReprogramarCitaAsync(int idCita, DateTime nuevaFecha, TimeSpan nuevaHora)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Obtener la cita actual
                var (exito, _, citaActual) = await ObtenerCitaPorIdAsync(idCita);
                if (!exito || citaActual == null)
                {
                    return (false, "Cita no encontrada");
                }

                // Verificar disponibilidad
                if (await ExisteConflictoHorarioAsync(citaActual.IdAgente, nuevaFecha, nuevaHora, idCita))
                {
                    return (false, "El agente ya tiene una cita programada en ese horario");
                }

                if (await ClienteTieneCitaEnHorarioAsync(citaActual.IdCliente, nuevaFecha, nuevaHora, idCita))
                {
                    return (false, "El cliente ya tiene una cita programada en ese horario");
                }

                // Obtener estado "Reprogramada"
                string queryEstado = "SELECT idEstadoCita FROM estadocita WHERE LOWER(nombre) = 'reprogramada' LIMIT 1";
                using var cmdEstado = new MySqlCommand(queryEstado, connection);
                var resultEstado = await cmdEstado.ExecuteScalarAsync();

                int? idEstadoReprogramada = resultEstado != null ? Convert.ToInt32(resultEstado) : null;

                string query = @"UPDATE cita SET 
                    fecha = @fecha,
                    hora = @hora,
                    idEstadoCita = @idEstado,
                    actualizadoAt = @actualizadoAt
                    WHERE idCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idCita);
                cmd.Parameters.AddWithValue("@fecha", nuevaFecha.Date);
                cmd.Parameters.AddWithValue("@hora", nuevaHora);
                cmd.Parameters.AddWithValue("@idEstado", idEstadoReprogramada ?? citaActual.IdEstadoCita);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.Now);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Cita reprogramada exitosamente");
                }

                return (false, "No se pudo reprogramar la cita");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al reprogramar cita: {ex.Message}");
            }
        }

        // Eliminar cita
        public async Task<(bool exito, string mensaje)> EliminarCitaAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM cita WHERE idCita = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Cita eliminada exitosamente");
                }

                return (false, "No se encontró la cita a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar cita: {ex.Message}");
            }
        }

        // Obtener estadísticas generales
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
                    COUNT(*) as TotalCitas,
                    COUNT(CASE WHEN fecha >= CURDATE() THEN 1 END) as CitasProximas,
                    COUNT(CASE WHEN fecha < CURDATE() THEN 1 END) as CitasPasadas,
                    COUNT(CASE WHEN fecha = CURDATE() THEN 1 END) as CitasHoy,
                    COUNT(DISTINCT idCliente) as ClientesUnicos,
                    COUNT(DISTINCT idAgente) as AgentesActivos
                    FROM cita";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalCitas"] = reader.GetInt32("TotalCitas");
                    estadisticas["citasProximas"] = reader.GetInt32("CitasProximas");
                    estadisticas["citasPasadas"] = reader.GetInt32("CitasPasadas");
                    estadisticas["citasHoy"] = reader.GetInt32("CitasHoy");
                    estadisticas["clientesUnicos"] = reader.GetInt32("ClientesUnicos");
                    estadisticas["agentesActivos"] = reader.GetInt32("AgentesActivos");
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
                    FROM cita c
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
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
                return (false, $"Error al obtener estadísticas por estado: {ex.Message}", null);
            }
        }

        // Obtener estadísticas por agente
        public async Task<(bool exito, string mensaje, List<Dictionary<string, object>>? estadisticas)>
            ObtenerEstadisticasPorAgenteAsync()
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
                    a.idAgente,
                    u.Nombre as nombreAgente,
                    COUNT(*) as totalCitas,
                    COUNT(CASE WHEN c.fecha >= CURDATE() THEN 1 END) as citasProximas,
                    COUNT(CASE WHEN ec.nombre = 'Completada' THEN 1 END) as citasCompletadas
                    FROM cita c
                    INNER JOIN agenteinmobiliario a ON c.idAgente = a.idAgente
                    INNER JOIN usuario u ON a.idUsuario = u.IdUsuario
                    LEFT JOIN estadocita ec ON c.idEstadoCita = ec.idEstadoCita
                    GROUP BY a.idAgente, u.Nombre
                    ORDER BY totalCitas DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var stats = new Dictionary<string, object>
                    {
                        ["idAgente"] = reader.GetInt32("idAgente"),
                        ["nombreAgente"] = reader.GetString("nombreAgente"),
                        ["totalCitas"] = reader.GetInt32("totalCitas"),
                        ["citasProximas"] = reader.GetInt32("citasProximas"),
                        ["citasCompletadas"] = reader.GetInt32("citasCompletadas")
                    };
                    estadisticas.Add(stats);
                }

                return (true, $"Estadísticas de {estadisticas.Count} agentes obtenidas", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas por agente: {ex.Message}", null);
            }
        }

        // Contar citas por cliente
        public async Task<(bool exito, string mensaje, int total)> ContarCitasPorClienteAsync(int idCliente)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM cita WHERE idCliente = @idCliente";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idCliente", idCliente);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El cliente tiene {total} citas", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar citas: {ex.Message}", 0);
            }
        }

        // Contar citas por agente
        public async Task<(bool exito, string mensaje, int total)> ContarCitasPorAgenteAsync(int idAgente)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM cita WHERE idAgente = @idAgente";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idAgente", idAgente);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El agente tiene {total} citas", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar citas: {ex.Message}", 0);
            }
        }

        // Métodos auxiliares para mapear
        private Cita MapearCitaCompleta(MySqlDataReader reader)
        {
            return new Cita
            {
                IdCita = reader.GetInt32("idCita"),
                Fecha = reader.GetDateTime("fecha"),
                Hora = reader.GetTimeSpan("hora"),
                IdCliente = reader.GetInt32("idCliente"),
                IdAgente = reader.GetInt32("idAgente"),
                IdEstadoCita = reader.GetInt32("idEstadoCita"),
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt"),
                Cliente = new Cliente
                {
                    IdCliente = reader.GetInt32("idCliente"),
                    IdUsuario = reader.GetInt32("clienteIdUsuario"),
                    Usuario = new Usuario
                    {
                        IdUsuario = reader.GetInt32("clienteUsrId"),
                        Nombre = reader.GetString("clienteNombre"),
                        Email = reader.GetString("clienteEmail"),
                        Dni = reader.GetString("clienteDni"),
                        Telefono = !reader.IsDBNull(reader.GetOrdinal("clienteTelefono")) ? reader.GetString("clienteTelefono") : null
                    }
                },
                Agente = new AgenteInmobiliario
                {
                    IdAgente = reader.GetInt32("idAgente"),
                    IdUsuario = reader.GetInt32("agenteIdUsuario"),
                    Usuario = new Usuario
                    {
                        IdUsuario = reader.GetInt32("agenteUsrId"),
                        Nombre = reader.GetString("agenteNombre"),
                        Email = reader.GetString("agenteEmail"),
                        Dni = reader.GetString("agenteDni"),
                        Telefono = !reader.IsDBNull(reader.GetOrdinal("agenteTelefono")) ? reader.GetString("agenteTelefono") : null
                    }
                },
                EstadoCita = new EstadoCita
                {
                    IdEstadoCita = reader.GetInt32("estadoCitaId"),
                    Nombre = reader.GetString("estadoCitaNombre"),
                    CreadoAt = DateTime.Now,
                    ActualizadoAt = DateTime.Now
                }
            };
        }

        private Cita MapearCitaSimple(MySqlDataReader reader)
        {
            return new Cita
            {
                IdCita = reader.GetInt32("idCita"),
                Fecha = reader.GetDateTime("fecha"),
                Hora = reader.GetTimeSpan("hora"),
                IdCliente = reader.GetInt32("idCliente"),
                IdAgente = reader.GetInt32("idAgente"),
                IdEstadoCita = reader.GetInt32("idEstadoCita"),
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
