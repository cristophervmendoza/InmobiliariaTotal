using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class PropiedadDao
    {
        // Crear propiedad
        public async Task<(bool exito, string mensaje, int? id)> CrearPropiedadAsync(Propiedad propiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"INSERT INTO propiedad 
                    (IdUsuario, IdTipoPropiedad, IdEstadoPropiedad, Titulo, Direccion, Precio, 
                     Descripcion, AreaTerreno, TipoMoneda, Habitacion, Bano, Estacionamiento, 
                     FotoPropiedad, CreadoAt, ActualizadoAt) 
                    VALUES (@IdUsuario, @IdTipoPropiedad, @IdEstadoPropiedad, @Titulo, @Direccion, @Precio, 
                            @Descripcion, @AreaTerreno, @TipoMoneda, @Habitacion, @Bano, @Estacionamiento, 
                            @FotoPropiedad, @CreadoAt, @ActualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdUsuario", propiedad.IdUsuario);
                cmd.Parameters.AddWithValue("@IdTipoPropiedad", propiedad.IdTipoPropiedad);
                cmd.Parameters.AddWithValue("@IdEstadoPropiedad", propiedad.IdEstadoPropiedad);
                cmd.Parameters.AddWithValue("@Titulo", propiedad.Titulo);
                cmd.Parameters.AddWithValue("@Direccion", propiedad.Direccion);
                cmd.Parameters.AddWithValue("@Precio", propiedad.Precio);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(propiedad.Descripcion) ? DBNull.Value : propiedad.Descripcion);
                cmd.Parameters.AddWithValue("@AreaTerreno", propiedad.AreaTerreno.HasValue ? (object)propiedad.AreaTerreno.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@TipoMoneda", propiedad.TipoMoneda);
                cmd.Parameters.AddWithValue("@Habitacion", propiedad.Habitacion.HasValue ? (object)propiedad.Habitacion.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Bano", propiedad.Bano.HasValue ? (object)propiedad.Bano.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Estacionamiento", propiedad.Estacionamiento.HasValue ? (object)propiedad.Estacionamiento.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@FotoPropiedad", string.IsNullOrEmpty(propiedad.FotoPropiedad) ? DBNull.Value : propiedad.FotoPropiedad);
                cmd.Parameters.AddWithValue("@CreadoAt", propiedad.CreadoAt);
                cmd.Parameters.AddWithValue("@ActualizadoAt", propiedad.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Propiedad creada exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear propiedad: {ex.Message}", null);
            }
        }

        // Obtener propiedad por ID
        public async Task<(bool exito, string mensaje, Propiedad? propiedad)> ObtenerPropiedadPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM propiedad WHERE IdPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var propiedad = MapearPropiedad(reader);
                    return (true, "Propiedad encontrada", propiedad);
                }

                return (false, "Propiedad no encontrada", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener propiedad: {ex.Message}", null);
            }
        }

        // Listar todas las propiedades
        public async Task<(bool exito, string mensaje, List<Propiedad>? propiedades)> ListarPropiedadesAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM propiedad ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var propiedades = new List<Propiedad>();
                while (await reader.ReadAsync())
                {
                    propiedades.Add(MapearPropiedad(reader));
                }

                return (true, $"Se encontraron {propiedades.Count} propiedades", propiedades);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar propiedades: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Propiedad>? propiedades, int totalRegistros)>
            ListarPropiedadesPaginadasAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM propiedad";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM propiedad 
                    ORDER BY CreadoAt DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var propiedades = new List<Propiedad>();
                while (await reader.ReadAsync())
                {
                    propiedades.Add(MapearPropiedad(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", propiedades, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar propiedades: {ex.Message}", null, 0);
            }
        }

        // Buscar propiedades con filtros avanzados
        public async Task<(bool exito, string mensaje, List<Propiedad>? propiedades)>
            BuscarPropiedadesAsync(string? termino = null, int? idTipoPropiedad = null,
                                  int? idEstadoPropiedad = null, decimal? precioMin = null,
                                  decimal? precioMax = null, int? habitacionesMin = null,
                                  string? tipoMoneda = null, int? idUsuario = null)
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
                    condiciones.Add("(Titulo LIKE @termino OR Direccion LIKE @termino OR Descripcion LIKE @termino)");
                    parametros.Add(new MySqlParameter("@termino", $"%{termino}%"));
                }

                if (idTipoPropiedad.HasValue)
                {
                    condiciones.Add("IdTipoPropiedad = @idTipoPropiedad");
                    parametros.Add(new MySqlParameter("@idTipoPropiedad", idTipoPropiedad.Value));
                }

                if (idEstadoPropiedad.HasValue)
                {
                    condiciones.Add("IdEstadoPropiedad = @idEstadoPropiedad");
                    parametros.Add(new MySqlParameter("@idEstadoPropiedad", idEstadoPropiedad.Value));
                }

                if (precioMin.HasValue)
                {
                    condiciones.Add("Precio >= @precioMin");
                    parametros.Add(new MySqlParameter("@precioMin", precioMin.Value));
                }

                if (precioMax.HasValue)
                {
                    condiciones.Add("Precio <= @precioMax");
                    parametros.Add(new MySqlParameter("@precioMax", precioMax.Value));
                }

                if (habitacionesMin.HasValue)
                {
                    condiciones.Add("Habitacion >= @habitacionesMin");
                    parametros.Add(new MySqlParameter("@habitacionesMin", habitacionesMin.Value));
                }

                if (!string.IsNullOrWhiteSpace(tipoMoneda))
                {
                    condiciones.Add("TipoMoneda = @tipoMoneda");
                    parametros.Add(new MySqlParameter("@tipoMoneda", tipoMoneda));
                }

                if (idUsuario.HasValue)
                {
                    condiciones.Add("IdUsuario = @idUsuario");
                    parametros.Add(new MySqlParameter("@idUsuario", idUsuario.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT * FROM propiedad 
                    {whereClause}
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var propiedades = new List<Propiedad>();
                while (await reader.ReadAsync())
                {
                    propiedades.Add(MapearPropiedad(reader));
                }

                return (true, $"Se encontraron {propiedades.Count} propiedades", propiedades);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar propiedades: {ex.Message}", null);
            }
        }

        // Obtener propiedades por usuario
        public async Task<(bool exito, string mensaje, List<Propiedad>? propiedades)>
            ObtenerPropiedadesPorUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT * FROM propiedad 
                    WHERE IdUsuario = @idUsuario
                    ORDER BY CreadoAt DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                var propiedades = new List<Propiedad>();
                while (await reader.ReadAsync())
                {
                    propiedades.Add(MapearPropiedad(reader));
                }

                return (true, $"Se encontraron {propiedades.Count} propiedades del usuario", propiedades);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener propiedades del usuario: {ex.Message}", null);
            }
        }

        // Obtener propiedades por rango de precio
        public async Task<(bool exito, string mensaje, List<Propiedad>? propiedades)>
            ObtenerPropiedadesPorRangoPrecioAsync(decimal precioMin, decimal precioMax, string? tipoMoneda = null)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string whereMoneda = !string.IsNullOrWhiteSpace(tipoMoneda) ?
                    "AND TipoMoneda = @tipoMoneda" : "";

                string query = $@"SELECT * FROM propiedad 
                    WHERE Precio >= @precioMin AND Precio <= @precioMax
                    {whereMoneda}
                    ORDER BY Precio ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@precioMin", precioMin);
                cmd.Parameters.AddWithValue("@precioMax", precioMax);
                if (!string.IsNullOrWhiteSpace(tipoMoneda))
                {
                    cmd.Parameters.AddWithValue("@tipoMoneda", tipoMoneda);
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var propiedades = new List<Propiedad>();
                while (await reader.ReadAsync())
                {
                    propiedades.Add(MapearPropiedad(reader));
                }

                return (true, $"Se encontraron {propiedades.Count} propiedades en el rango de precio", propiedades);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener propiedades por rango de precio: {ex.Message}", null);
            }
        }

        // Actualizar propiedad
        // Actualizar propiedad
        public async Task<(bool exito, string mensaje)> ActualizarPropiedadAsync(Propiedad propiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // ✅ AGREGADO: IdUsuario en el UPDATE
                string query = @"
            UPDATE propiedad 
            SET IdUsuario = @IdUsuario,
                IdTipoPropiedad = @IdTipoPropiedad,
                IdEstadoPropiedad = @IdEstadoPropiedad,
                Titulo = @Titulo,
                Direccion = @Direccion,
                Precio = @Precio,
                Descripcion = @Descripcion,
                AreaTerreno = @AreaTerreno,
                TipoMoneda = @TipoMoneda,
                Habitacion = @Habitacion,
                Bano = @Bano,
                Estacionamiento = @Estacionamiento,
                FotoPropiedad = @FotoPropiedad,
                ActualizadoAt = @ActualizadoAt
            WHERE IdPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);

                // ✅ AGREGADO: Parámetro IdUsuario
                cmd.Parameters.AddWithValue("@IdUsuario", propiedad.IdUsuario);
                cmd.Parameters.AddWithValue("@id", propiedad.IdPropiedad);
                cmd.Parameters.AddWithValue("@IdTipoPropiedad", propiedad.IdTipoPropiedad);
                cmd.Parameters.AddWithValue("@IdEstadoPropiedad", propiedad.IdEstadoPropiedad);
                cmd.Parameters.AddWithValue("@Titulo", propiedad.Titulo);
                cmd.Parameters.AddWithValue("@Direccion", propiedad.Direccion);
                cmd.Parameters.AddWithValue("@Precio", propiedad.Precio);
                cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(propiedad.Descripcion) ? DBNull.Value : propiedad.Descripcion);
                cmd.Parameters.AddWithValue("@AreaTerreno", propiedad.AreaTerreno.HasValue ? (object)propiedad.AreaTerreno.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@TipoMoneda", propiedad.TipoMoneda);
                cmd.Parameters.AddWithValue("@Habitacion", propiedad.Habitacion.HasValue ? (object)propiedad.Habitacion.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Bano", propiedad.Bano.HasValue ? (object)propiedad.Bano.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Estacionamiento", propiedad.Estacionamiento.HasValue ? (object)propiedad.Estacionamiento.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@FotoPropiedad", string.IsNullOrEmpty(propiedad.FotoPropiedad) ? DBNull.Value : propiedad.FotoPropiedad);
                cmd.Parameters.AddWithValue("@ActualizadoAt", propiedad.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Propiedad actualizada exitosamente");
                }

                return (false, "No se encontró la propiedad a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar propiedad: {ex.Message}");
            }
        }


        // Eliminar propiedad
        public async Task<(bool exito, string mensaje)> EliminarPropiedadAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM propiedad WHERE IdPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Propiedad eliminada exitosamente");
                }

                return (false, "No se encontró la propiedad a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar propiedad: {ex.Message}");
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
                    COUNT(*) as TotalPropiedades,
                    AVG(Precio) as PrecioPromedio,
                    MIN(Precio) as PrecioMinimo,
                    MAX(Precio) as PrecioMaximo,
                    COUNT(CASE WHEN FotoPropiedad IS NOT NULL THEN 1 END) as ConFoto,
                    COUNT(DISTINCT IdUsuario) as UsuariosUnicos
                    FROM propiedad";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalPropiedades"] = reader.GetInt32("TotalPropiedades");
                    estadisticas["precioPromedio"] = !reader.IsDBNull(reader.GetOrdinal("PrecioPromedio")) ?
                        Math.Round(reader.GetDecimal("PrecioPromedio"), 2) : 0;
                    estadisticas["precioMinimo"] = !reader.IsDBNull(reader.GetOrdinal("PrecioMinimo")) ?
                        reader.GetDecimal("PrecioMinimo") : 0;
                    estadisticas["precioMaximo"] = !reader.IsDBNull(reader.GetOrdinal("PrecioMaximo")) ?
                        reader.GetDecimal("PrecioMaximo") : 0;
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

        // Método auxiliar para mapear resultados
        private Propiedad MapearPropiedad(MySqlDataReader reader)
        {
            return new Propiedad
            {
                IdPropiedad = reader.GetInt32("IdPropiedad"),
                IdUsuario = reader.GetInt32("IdUsuario"),
                IdTipoPropiedad = reader.GetInt32("IdTipoPropiedad"),
                IdEstadoPropiedad = reader.GetInt32("IdEstadoPropiedad"),
                Titulo = reader.GetString("Titulo"),
                Direccion = reader.GetString("Direccion"),
                Precio = reader.GetDecimal("Precio"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? reader.GetString("Descripcion") : null,
                AreaTerreno = !reader.IsDBNull(reader.GetOrdinal("AreaTerreno")) ? reader.GetDecimal("AreaTerreno") : null,
                TipoMoneda = reader.GetString("TipoMoneda"),
                Habitacion = !reader.IsDBNull(reader.GetOrdinal("Habitacion")) ? reader.GetInt32("Habitacion") : null,
                Bano = !reader.IsDBNull(reader.GetOrdinal("Bano")) ? reader.GetInt32("Bano") : null,
                Estacionamiento = !reader.IsDBNull(reader.GetOrdinal("Estacionamiento")) ? reader.GetInt32("Estacionamiento") : null,
                FotoPropiedad = !reader.IsDBNull(reader.GetOrdinal("FotoPropiedad")) ? reader.GetString("FotoPropiedad") : null,
                CreadoAt = reader.GetDateTime("CreadoAt"),
                ActualizadoAt = reader.GetDateTime("ActualizadoAt")
            };
        }
    }
}
