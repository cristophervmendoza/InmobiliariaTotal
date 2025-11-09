using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class SolicitudPropiedadDao
    {
        // Crear solicitud de propiedad
        public async Task<(bool exito, string mensaje, int? id)> CrearSolicitudPropiedadAsync(SolicitudPropiedad solicitud)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO solicitudpropiedad 
                    (idUsuario, titulo, descripcion, fotoPropiedad, solicitudEstado, creadoAt, actualizadoAt) 
                    VALUES (@idUsuario, @titulo, @descripcion, @fotoPropiedad, @solicitudEstado, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", solicitud.IdUsuario);
                cmd.Parameters.AddWithValue("@titulo", solicitud.Titulo);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(solicitud.Descripcion) ? DBNull.Value : solicitud.Descripcion);
                cmd.Parameters.AddWithValue("@fotoPropiedad", string.IsNullOrEmpty(solicitud.FotoPropiedad) ? DBNull.Value : solicitud.FotoPropiedad);
                cmd.Parameters.AddWithValue("@solicitudEstado", solicitud.SolicitudEstado ?? "Pendiente");
                cmd.Parameters.AddWithValue("@creadoAt", solicitud.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", solicitud.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Solicitud de propiedad creada exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear solicitud de propiedad: {ex.Message}", null);
            }
        }

        // Obtener solicitud por ID
        public async Task<(bool exito, string mensaje, SolicitudPropiedad? solicitud)> ObtenerSolicitudPropiedadPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT sp.*, 
                    u.IdUsuario, u.Nombre as usuarioNombre, u.Email as usuarioEmail
                    FROM solicitudpropiedad sp
                    LEFT JOIN usuario u ON sp.idUsuario = u.IdUsuario
                    WHERE sp.idSolicitud = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var solicitud = MapearSolicitud(reader);
                    return (true, "Solicitud de propiedad encontrada", solicitud);
                }

                return (false, "Solicitud de propiedad no encontrada", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener solicitud de propiedad: {ex.Message}", null);
            }
        }

        // Listar todas las solicitudes
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)> ListarSolicitudesPropiedadAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT sp.*, 
                    u.Nombre as usuarioNombre
                    FROM solicitudpropiedad sp
                    LEFT JOIN usuario u ON sp.idUsuario = u.IdUsuario
                    ORDER BY sp.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var solicitudes = new List<SolicitudPropiedad>();
                while (await reader.ReadAsync())
                {
                    solicitudes.Add(MapearSolicitudSimple(reader));
                }

                return (true, $"Se encontraron {solicitudes.Count} solicitudes de propiedad", solicitudes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar solicitudes de propiedad: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes, int totalRegistros)>
            ListarSolicitudesPropiedadPaginadasAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM solicitudpropiedad";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT sp.*, 
                    u.Nombre as usuarioNombre
                    FROM solicitudpropiedad sp
                    LEFT JOIN usuario u ON sp.idUsuario = u.IdUsuario
                    ORDER BY sp.creadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var solicitudes = new List<SolicitudPropiedad>();
                while (await reader.ReadAsync())
                {
                    solicitudes.Add(MapearSolicitudSimple(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", solicitudes, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar solicitudes de propiedad: {ex.Message}", null, 0);
            }
        }

        // Buscar solicitudes con filtros
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)>
            BuscarSolicitudesPropiedadAsync(string? termino = null, string? estado = null, int? idUsuario = null)
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
                    condiciones.Add("(sp.titulo LIKE @termino OR sp.descripcion LIKE @termino)");
                    parametros.Add(new MySqlParameter("@termino", $"%{termino}%"));
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    condiciones.Add("LOWER(sp.solicitudEstado) = LOWER(@estado)");
                    parametros.Add(new MySqlParameter("@estado", estado));
                }

                if (idUsuario.HasValue)
                {
                    condiciones.Add("sp.idUsuario = @idUsuario");
                    parametros.Add(new MySqlParameter("@idUsuario", idUsuario.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT sp.*, 
                    u.Nombre as usuarioNombre
                    FROM solicitudpropiedad sp
                    LEFT JOIN usuario u ON sp.idUsuario = u.IdUsuario
                    {whereClause}
                    ORDER BY sp.creadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var solicitudes = new List<SolicitudPropiedad>();
                while (await reader.ReadAsync())
                {
                    solicitudes.Add(MapearSolicitudSimple(reader));
                }

                return (true, $"Se encontraron {solicitudes.Count} solicitudes de propiedad", solicitudes);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar solicitudes de propiedad: {ex.Message}", null);
            }
        }

        // Obtener solicitudes por usuario
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)>
            ObtenerSolicitudesPorUsuarioAsync(int idUsuario)
        {
            return await BuscarSolicitudesPropiedadAsync(idUsuario: idUsuario);
        }

        // Obtener solicitudes por estado
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)>
            ObtenerSolicitudesPorEstadoAsync(string estado)
        {
            return await BuscarSolicitudesPropiedadAsync(estado: estado);
        }

        // Obtener solicitudes pendientes
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)>
            ObtenerSolicitudesPendientesAsync()
        {
            return await BuscarSolicitudesPropiedadAsync(estado: "Pendiente");
        }

        // Obtener solicitudes aprobadas
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)>
            ObtenerSolicitudesAprobadasAsync()
        {
            return await BuscarSolicitudesPropiedadAsync(estado: "Aprobada");
        }

        // Obtener solicitudes rechazadas
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)>
            ObtenerSolicitudesRechazadasAsync()
        {
            return await BuscarSolicitudesPropiedadAsync(estado: "Rechazada");
        }

        // Obtener solicitudes en revisión
        public async Task<(bool exito, string mensaje, List<SolicitudPropiedad>? solicitudes)>
            ObtenerSolicitudesEnRevisionAsync()
        {
            return await BuscarSolicitudesPropiedadAsync(estado: "En Revision");
        }

        // Actualizar solicitud de propiedad
        public async Task<(bool exito, string mensaje)> ActualizarSolicitudPropiedadAsync(SolicitudPropiedad solicitud)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE solicitudpropiedad SET 
                    titulo = @titulo,
                    descripcion = @descripcion,
                    fotoPropiedad = @fotoPropiedad,
                    solicitudEstado = @solicitudEstado,
                    actualizadoAt = @actualizadoAt
                    WHERE idSolicitud = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", solicitud.IdSolicitud);
                cmd.Parameters.AddWithValue("@titulo", solicitud.Titulo);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(solicitud.Descripcion) ? DBNull.Value : solicitud.Descripcion);
                cmd.Parameters.AddWithValue("@fotoPropiedad", string.IsNullOrEmpty(solicitud.FotoPropiedad) ? DBNull.Value : solicitud.FotoPropiedad);
                cmd.Parameters.AddWithValue("@solicitudEstado", solicitud.SolicitudEstado);
                cmd.Parameters.AddWithValue("@actualizadoAt", solicitud.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Solicitud de propiedad actualizada exitosamente");
                }

                return (false, "No se encontró la solicitud de propiedad a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar solicitud de propiedad: {ex.Message}");
            }
        }

        // Cambiar estado de solicitud
        public async Task<(bool exito, string mensaje)> CambiarEstadoSolicitudAsync(int idSolicitud, string nuevoEstado)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"UPDATE solicitudpropiedad SET 
                    solicitudEstado = @solicitudEstado,
                    actualizadoAt = @actualizadoAt
                    WHERE idSolicitud = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idSolicitud);
                cmd.Parameters.AddWithValue("@solicitudEstado", nuevoEstado);
                cmd.Parameters.AddWithValue("@actualizadoAt", DateTime.Now);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Estado de solicitud actualizado exitosamente");
                }

                return (false, "No se encontró la solicitud");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al cambiar estado de solicitud: {ex.Message}");
            }
        }

        // Aprobar solicitud
        public async Task<(bool exito, string mensaje)> AprobarSolicitudAsync(int idSolicitud)
        {
            return await CambiarEstadoSolicitudAsync(idSolicitud, "Aprobada");
        }

        // Rechazar solicitud
        public async Task<(bool exito, string mensaje)> RechazarSolicitudAsync(int idSolicitud)
        {
            return await CambiarEstadoSolicitudAsync(idSolicitud, "Rechazada");
        }

        // Poner en revisión
        public async Task<(bool exito, string mensaje)> PonerEnRevisionAsync(int idSolicitud)
        {
            return await CambiarEstadoSolicitudAsync(idSolicitud, "En Revision");
        }

        // Cancelar solicitud
        public async Task<(bool exito, string mensaje)> CancelarSolicitudAsync(int idSolicitud)
        {
            return await CambiarEstadoSolicitudAsync(idSolicitud, "Cancelada");
        }

        // Eliminar solicitud de propiedad
        public async Task<(bool exito, string mensaje)> EliminarSolicitudPropiedadAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM solicitudpropiedad WHERE idSolicitud = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Solicitud de propiedad eliminada exitosamente");
                }

                return (false, "No se encontró la solicitud de propiedad a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar solicitud de propiedad: {ex.Message}");
            }
        }

        // Contar solicitudes por usuario
        public async Task<(bool exito, string mensaje, int total)> ContarSolicitudesPorUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM solicitudpropiedad WHERE idUsuario = @idUsuario";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El usuario tiene {total} solicitud(es)", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar solicitudes: {ex.Message}", 0);
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
                    COUNT(*) as TotalSolicitudes,
                    COUNT(CASE WHEN LOWER(solicitudEstado) = 'pendiente' THEN 1 END) as Pendientes,
                    COUNT(CASE WHEN LOWER(solicitudEstado) = 'en revision' THEN 1 END) as EnRevision,
                    COUNT(CASE WHEN LOWER(solicitudEstado) = 'aprobada' THEN 1 END) as Aprobadas,
                    COUNT(CASE WHEN LOWER(solicitudEstado) = 'rechazada' THEN 1 END) as Rechazadas,
                    COUNT(CASE WHEN LOWER(solicitudEstado) = 'cancelada' THEN 1 END) as Canceladas,
                    COUNT(CASE WHEN fotoPropiedad IS NOT NULL THEN 1 END) as ConFoto,
                    COUNT(DISTINCT idUsuario) as UsuariosUnicos
                    FROM solicitudpropiedad";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalSolicitudes"] = reader.GetInt32("TotalSolicitudes");
                    estadisticas["pendientes"] = reader.GetInt32("Pendientes");
                    estadisticas["enRevision"] = reader.GetInt32("EnRevision");
                    estadisticas["aprobadas"] = reader.GetInt32("Aprobadas");
                    estadisticas["rechazadas"] = reader.GetInt32("Rechazadas");
                    estadisticas["canceladas"] = reader.GetInt32("Canceladas");
                    estadisticas["conFoto"] = reader.GetInt32("ConFoto");
                    estadisticas["usuariosUnicos"] = reader.GetInt32("UsuariosUnicos");
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
                string query = @"SELECT solicitudEstado, COUNT(*) as cantidad
                    FROM solicitudpropiedad
                    GROUP BY solicitudEstado
                    ORDER BY cantidad DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, int>();
                while (await reader.ReadAsync())
                {
                    var estado = !reader.IsDBNull(reader.GetOrdinal("solicitudEstado")) ?
                        reader.GetString("solicitudEstado") : "Sin Estado";
                    estadisticas[estado] = reader.GetInt32("cantidad");
                }

                return (true, "Estadísticas por estado obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas por estado: {ex.Message}", null);
            }
        }

        // Métodos auxiliares para mapear
        private SolicitudPropiedad MapearSolicitud(MySqlDataReader reader)
        {
            return new SolicitudPropiedad
            {
                IdSolicitud = reader.GetInt32("idSolicitud"),
                IdUsuario = reader.GetInt32("idUsuario"),
                Titulo = reader.GetString("titulo"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("descripcion")) ? reader.GetString("descripcion") : string.Empty,
                FotoPropiedad = !reader.IsDBNull(reader.GetOrdinal("fotoPropiedad")) ? reader.GetString("fotoPropiedad") : string.Empty,
                SolicitudEstado = !reader.IsDBNull(reader.GetOrdinal("solicitudEstado")) ? reader.GetString("solicitudEstado") : string.Empty,
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt"),
                Usuario = new Usuario
                {
                    IdUsuario = reader.GetInt32("IdUsuario"),
                    Nombre = !reader.IsDBNull(reader.GetOrdinal("usuarioNombre")) ? reader.GetString("usuarioNombre") : string.Empty,
                    Email = !reader.IsDBNull(reader.GetOrdinal("usuarioEmail")) ? reader.GetString("usuarioEmail") : string.Empty
                }
            };
        }

        private SolicitudPropiedad MapearSolicitudSimple(MySqlDataReader reader)
        {
            return new SolicitudPropiedad
            {
                IdSolicitud = reader.GetInt32("idSolicitud"),
                IdUsuario = reader.GetInt32("idUsuario"),
                Titulo = reader.GetString("titulo"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("descripcion")) ? reader.GetString("descripcion") : string.Empty,
                FotoPropiedad = !reader.IsDBNull(reader.GetOrdinal("fotoPropiedad")) ? reader.GetString("fotoPropiedad") : string.Empty,
                SolicitudEstado = !reader.IsDBNull(reader.GetOrdinal("solicitudEstado")) ? reader.GetString("solicitudEstado") : string.Empty,
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
