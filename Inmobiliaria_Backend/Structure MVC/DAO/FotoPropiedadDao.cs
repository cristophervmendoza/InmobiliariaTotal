using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class FotoPropiedadDao
    {
        // Crear foto de propiedad
        public async Task<(bool exito, string mensaje, int? id)> CrearFotoPropiedadAsync(FotoPropiedad fotoPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Si esta foto será principal, desmarcar las demás de la propiedad
                if (fotoPropiedad.EsPrincipal)
                {
                    await DesmarcarFotoPrincipalAsync(fotoPropiedad.IdPropiedad);
                }

                string query = @"INSERT INTO fotopropiedad 
                    (IdPropiedad, RutaFoto, EsPrincipal, Descripcion, CreadoAt) 
                    VALUES (@IdPropiedad, @RutaFoto, @EsPrincipal, @Descripcion, @CreadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdPropiedad", fotoPropiedad.IdPropiedad);
                cmd.Parameters.AddWithValue("@RutaFoto", fotoPropiedad.RutaFoto);
                cmd.Parameters.AddWithValue("@EsPrincipal", fotoPropiedad.EsPrincipal);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(fotoPropiedad.Descripcion) ? DBNull.Value : fotoPropiedad.Descripcion);
                cmd.Parameters.AddWithValue("@CreadoAt", fotoPropiedad.CreadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Foto de propiedad creada exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear foto de propiedad: {ex.Message}", null);
            }
        }

        // Obtener foto por ID
        public async Task<(bool exito, string mensaje, FotoPropiedad? foto)> ObtenerFotoPropiedadPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM fotopropiedad WHERE IdFoto = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var foto = MapearFotoPropiedad(reader);
                    return (true, "Foto de propiedad encontrada", foto);
                }

                return (false, "Foto de propiedad no encontrada", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener foto de propiedad: {ex.Message}", null);
            }
        }

        // Listar todas las fotos
        public async Task<(bool exito, string mensaje, List<FotoPropiedad>? fotos)> ListarFotosPropiedadAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM fotopropiedad ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var fotos = new List<FotoPropiedad>();
                while (await reader.ReadAsync())
                {
                    fotos.Add(MapearFotoPropiedad(reader));
                }

                return (true, $"Se encontraron {fotos.Count} fotos de propiedad", fotos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar fotos de propiedad: {ex.Message}", null);
            }
        }

        // Obtener fotos por propiedad
        public async Task<(bool exito, string mensaje, List<FotoPropiedad>? fotos)> ObtenerFotosPorPropiedadAsync(int idPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM fotopropiedad 
                    WHERE IdPropiedad = @idPropiedad 
                    ORDER BY EsPrincipal DESC, CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad);

                using var reader = await cmd.ExecuteReaderAsync();
                var fotos = new List<FotoPropiedad>();
                while (await reader.ReadAsync())
                {
                    fotos.Add(MapearFotoPropiedad(reader));
                }

                return (true, $"Se encontraron {fotos.Count} foto(s) para la propiedad", fotos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener fotos de la propiedad: {ex.Message}", null);
            }
        }

        // Obtener foto principal de propiedad
        public async Task<(bool exito, string mensaje, FotoPropiedad? foto)> ObtenerFotoPrincipalAsync(int idPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM fotopropiedad 
                    WHERE IdPropiedad = @idPropiedad AND EsPrincipal = 1 
                    LIMIT 1";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var foto = MapearFotoPropiedad(reader);
                    return (true, "Foto principal encontrada", foto);
                }

                return (false, "No hay foto principal para esta propiedad", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener foto principal: {ex.Message}", null);
            }
        }

        // Obtener fotos secundarias de propiedad
        public async Task<(bool exito, string mensaje, List<FotoPropiedad>? fotos)> ObtenerFotosSecundariasAsync(int idPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM fotopropiedad 
                    WHERE IdPropiedad = @idPropiedad AND EsPrincipal = 0 
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad);

                using var reader = await cmd.ExecuteReaderAsync();
                var fotos = new List<FotoPropiedad>();
                while (await reader.ReadAsync())
                {
                    fotos.Add(MapearFotoPropiedad(reader));
                }

                return (true, $"Se encontraron {fotos.Count} foto(s) secundaria(s)", fotos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener fotos secundarias: {ex.Message}", null);
            }
        }

        // Actualizar foto de propiedad
        public async Task<(bool exito, string mensaje)> ActualizarFotoPropiedadAsync(FotoPropiedad fotoPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Si esta foto será principal, desmarcar las demás de la propiedad
                if (fotoPropiedad.EsPrincipal)
                {
                    await DesmarcarFotoPrincipalAsync(fotoPropiedad.IdPropiedad, fotoPropiedad.IdFoto);
                }

                string query = @"UPDATE fotopropiedad SET 
                    RutaFoto = @RutaFoto,
                    EsPrincipal = @EsPrincipal,
                    Descripcion = @Descripcion
                    WHERE IdFoto = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", fotoPropiedad.IdFoto);
                cmd.Parameters.AddWithValue("@RutaFoto", fotoPropiedad.RutaFoto);
                cmd.Parameters.AddWithValue("@EsPrincipal", fotoPropiedad.EsPrincipal);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(fotoPropiedad.Descripcion) ? DBNull.Value : fotoPropiedad.Descripcion);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Foto de propiedad actualizada exitosamente");
                }

                return (false, "No se encontró la foto de propiedad a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar foto de propiedad: {ex.Message}");
            }
        }

        // Marcar foto como principal
        public async Task<(bool exito, string mensaje)> MarcarComoPrincipalAsync(int idFoto)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Obtener la foto para saber a qué propiedad pertenece
                var (exito, _, foto) = await ObtenerFotoPropiedadPorIdAsync(idFoto);
                if (!exito || foto == null)
                {
                    return (false, "Foto no encontrada");
                }

                // Desmarcar otras fotos principales de la misma propiedad
                await DesmarcarFotoPrincipalAsync(foto.IdPropiedad, idFoto);

                // Marcar esta foto como principal
                string query = "UPDATE fotopropiedad SET EsPrincipal = 1 WHERE IdFoto = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", idFoto);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Foto marcada como principal exitosamente");
                }

                return (false, "No se pudo marcar la foto como principal");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al marcar foto como principal: {ex.Message}");
            }
        }

        // Desmarcar foto principal
        private async Task<bool> DesmarcarFotoPrincipalAsync(int idPropiedad, int? idFotoExcluir = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "UPDATE fotopropiedad SET EsPrincipal = 0 WHERE IdPropiedad = @idPropiedad";

                if (idFotoExcluir.HasValue)
                {
                    query += " AND IdFoto != @idFotoExcluir";
                }

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad);
                if (idFotoExcluir.HasValue)
                {
                    cmd.Parameters.AddWithValue("@idFotoExcluir", idFotoExcluir.Value);
                }

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Eliminar foto de propiedad
        public async Task<(bool exito, string mensaje)> EliminarFotoPropiedadAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM fotopropiedad WHERE IdFoto = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Foto de propiedad eliminada exitosamente");
                }

                return (false, "No se encontró la foto de propiedad a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar foto de propiedad: {ex.Message}");
            }
        }

        // Eliminar todas las fotos de una propiedad
        public async Task<(bool exito, string mensaje, int cantidad)> EliminarFotosPorPropiedadAsync(int idPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM fotopropiedad WHERE IdPropiedad = @idPropiedad";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, $"{filasAfectadas} foto(s) eliminada(s) exitosamente", filasAfectadas);
                }

                return (false, "No se encontraron fotos para eliminar", 0);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar fotos: {ex.Message}", 0);
            }
        }

        // Contar fotos por propiedad
        public async Task<(bool exito, string mensaje, int total)> ContarFotosPorPropiedadAsync(int idPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM fotopropiedad WHERE IdPropiedad = @idPropiedad";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"La propiedad tiene {total} foto(s)", total);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al contar fotos: {ex.Message}", 0);
            }
        }

        // Verificar si propiedad tiene foto principal
        public async Task<bool> TieneFotoPrincipalAsync(int idPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM fotopropiedad WHERE IdPropiedad = @idPropiedad AND EsPrincipal = 1";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idPropiedad", idPropiedad);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Obtener estadísticas
        public async Task<(bool exito, string mensaje, Dictionary<string, object>? estadisticas)> ObtenerEstadisticasAsync()
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
                    COUNT(*) as TotalFotos,
                    COUNT(DISTINCT IdPropiedad) as PropiedadesConFotos,
                    COUNT(CASE WHEN EsPrincipal = 1 THEN 1 END) as FotosPrincipales,
                    COUNT(CASE WHEN EsPrincipal = 0 THEN 1 END) as FotosSecundarias,
                    AVG(fotosPorPropiedad.cantidad) as PromedioFotosPorPropiedad
                    FROM fotopropiedad
                    LEFT JOIN (
                        SELECT IdPropiedad, COUNT(*) as cantidad 
                        FROM fotopropiedad 
                        GROUP BY IdPropiedad
                    ) as fotosPorPropiedad ON fotopropiedad.IdPropiedad = fotosPorPropiedad.IdPropiedad";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalFotos"] = reader.GetInt32("TotalFotos");
                    estadisticas["propiedadesConFotos"] = reader.GetInt32("PropiedadesConFotos");
                    estadisticas["fotosPrincipales"] = reader.GetInt32("FotosPrincipales");
                    estadisticas["fotosSecundarias"] = reader.GetInt32("FotosSecundarias");
                    estadisticas["promedioFotosPorPropiedad"] = !reader.IsDBNull(reader.GetOrdinal("PromedioFotosPorPropiedad")) ?
                        Math.Round(reader.GetDecimal("PromedioFotosPorPropiedad"), 2) : 0;
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear
        private FotoPropiedad MapearFotoPropiedad(MySqlDataReader reader)
        {
            return new FotoPropiedad
            {
                IdFoto = reader.GetInt32("IdFoto"),
                IdPropiedad = reader.GetInt32("IdPropiedad"),
                RutaFoto = reader.GetString("RutaFoto"),
                EsPrincipal = reader.GetBoolean("EsPrincipal"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? reader.GetString("Descripcion") : null,
                CreadoAt = reader.GetDateTime("CreadoAt")
            };
        }
    }
}
