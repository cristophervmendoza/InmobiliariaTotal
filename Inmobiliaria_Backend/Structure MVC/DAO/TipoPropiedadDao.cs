using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class TipoPropiedadDao
    {
        // Crear tipo de propiedad
        public async Task<(bool exito, string mensaje, int? id)> CrearTipoPropiedadAsync(TipoPropiedad tipoPropiedad)
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
                if (await ExisteTipoPropiedadAsync(tipoPropiedad.Nombre))
                {
                    return (false, "Ya existe un tipo de propiedad con ese nombre", null);
                }

                string query = @"INSERT INTO tipopropiedad 
                    (nombre, descripcion, creadoAt, actualizadoAt) 
                    VALUES (@nombre, @descripcion, @creadoAt, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", tipoPropiedad.Nombre);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(tipoPropiedad.Descripcion) ? DBNull.Value : tipoPropiedad.Descripcion);
                cmd.Parameters.AddWithValue("@creadoAt", tipoPropiedad.CreadoAt);
                cmd.Parameters.AddWithValue("@actualizadoAt", tipoPropiedad.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Tipo de propiedad creado exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear tipo de propiedad: {ex.Message}", null);
            }
        }

        // Obtener tipo de propiedad por ID
        public async Task<(bool exito, string mensaje, TipoPropiedad? tipoPropiedad)> ObtenerTipoPropiedadPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM tipopropiedad WHERE idTipoPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var tipoPropiedad = MapearTipoPropiedad(reader);
                    return (true, "Tipo de propiedad encontrado", tipoPropiedad);
                }

                return (false, "Tipo de propiedad no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipo de propiedad: {ex.Message}", null);
            }
        }

        // Obtener tipo de propiedad por nombre
        public async Task<(bool exito, string mensaje, TipoPropiedad? tipoPropiedad)> ObtenerTipoPropiedadPorNombreAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM tipopropiedad WHERE LOWER(nombre) = LOWER(@nombre)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nombre);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var tipoPropiedad = MapearTipoPropiedad(reader);
                    return (true, "Tipo de propiedad encontrado", tipoPropiedad);
                }

                return (false, "Tipo de propiedad no encontrado", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipo de propiedad: {ex.Message}", null);
            }
        }

        // Listar todos los tipos de propiedad
        public async Task<(bool exito, string mensaje, List<TipoPropiedad>? tiposPropiedad)> ListarTiposPropiedadAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT * FROM tipopropiedad ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var tiposPropiedad = new List<TipoPropiedad>();
                while (await reader.ReadAsync())
                {
                    tiposPropiedad.Add(MapearTipoPropiedad(reader));
                }

                return (true, $"Se encontraron {tiposPropiedad.Count} tipos de propiedad", tiposPropiedad);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar tipos de propiedad: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<TipoPropiedad>? tiposPropiedad, int totalRegistros)>
            ListarTiposPropiedadPaginadosAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM tipopropiedad";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT * FROM tipopropiedad 
                    ORDER BY nombre ASC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var tiposPropiedad = new List<TipoPropiedad>();
                while (await reader.ReadAsync())
                {
                    tiposPropiedad.Add(MapearTipoPropiedad(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", tiposPropiedad, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar tipos de propiedad: {ex.Message}", null, 0);
            }
        }

        // Buscar tipos de propiedad
        public async Task<(bool exito, string mensaje, List<TipoPropiedad>? tiposPropiedad)>
            BuscarTiposPropiedadAsync(string? termino = null)
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
                    whereClause = "WHERE nombre LIKE @termino OR descripcion LIKE @termino";
                }

                string query = $@"SELECT * FROM tipopropiedad 
                    {whereClause}
                    ORDER BY nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                if (!string.IsNullOrWhiteSpace(termino))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                var tiposPropiedad = new List<TipoPropiedad>();
                while (await reader.ReadAsync())
                {
                    tiposPropiedad.Add(MapearTipoPropiedad(reader));
                }

                return (true, $"Se encontraron {tiposPropiedad.Count} tipos de propiedad", tiposPropiedad);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar tipos de propiedad: {ex.Message}", null);
            }
        }

        // Actualizar tipo de propiedad
        public async Task<(bool exito, string mensaje)> ActualizarTipoPropiedadAsync(TipoPropiedad tipoPropiedad)
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
                string checkQuery = @"SELECT COUNT(*) FROM tipopropiedad 
                    WHERE LOWER(nombre) = LOWER(@nombre) AND idTipoPropiedad != @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@nombre", tipoPropiedad.Nombre);
                checkCmd.Parameters.AddWithValue("@id", tipoPropiedad.IdTipoPropiedad);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, "Ya existe otro tipo de propiedad con ese nombre");
                }

                string query = @"UPDATE tipopropiedad SET 
                    nombre = @nombre,
                    descripcion = @descripcion,
                    actualizadoAt = @actualizadoAt
                    WHERE idTipoPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", tipoPropiedad.IdTipoPropiedad);
                cmd.Parameters.AddWithValue("@nombre", tipoPropiedad.Nombre);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(tipoPropiedad.Descripcion) ? DBNull.Value : tipoPropiedad.Descripcion);
                cmd.Parameters.AddWithValue("@actualizadoAt", tipoPropiedad.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Tipo de propiedad actualizado exitosamente");
                }

                return (false, "No se encontró el tipo de propiedad a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar tipo de propiedad: {ex.Message}");
            }
        }

        // Eliminar tipo de propiedad
        public async Task<(bool exito, string mensaje)> EliminarTipoPropiedadAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar si hay propiedades usando este tipo
                string checkQuery = "SELECT COUNT(*) FROM propiedad WHERE IdTipoPropiedad = @id";
                using var checkCmd = new MySqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (count > 0)
                {
                    return (false, $"No se puede eliminar el tipo de propiedad porque tiene {count} propiedad(es) asociada(s)");
                }

                string query = "DELETE FROM tipopropiedad WHERE idTipoPropiedad = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Tipo de propiedad eliminado exitosamente");
                }

                return (false, "No se encontró el tipo de propiedad a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar tipo de propiedad: {ex.Message}");
            }
        }

        // Verificar si existe un tipo de propiedad con el nombre
        public async Task<bool> ExisteTipoPropiedadAsync(string nombre)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM tipopropiedad WHERE LOWER(nombre) = LOWER(@nombre)";

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

        // Contar propiedades por tipo
        public async Task<(bool exito, string mensaje, int total)> ContarPropiedadesPorTipoAsync(int idTipoPropiedad)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM propiedad WHERE IdTipoPropiedad = @idTipoPropiedad";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idTipoPropiedad", idTipoPropiedad);

                var result = await cmd.ExecuteScalarAsync();
                int total = Convert.ToInt32(result);

                return (true, $"El tipo de propiedad tiene {total} propiedad(es)", total);
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
                    tp.idTipoPropiedad,
                    tp.nombre,
                    tp.descripcion,
                    COUNT(p.IdPropiedad) as totalPropiedades,
                    COALESCE(AVG(p.Precio), 0) as precioPromedio,
                    COALESCE(MIN(p.Precio), 0) as precioMinimo,
                    COALESCE(MAX(p.Precio), 0) as precioMaximo
                    FROM tipopropiedad tp
                    LEFT JOIN propiedad p ON tp.idTipoPropiedad = p.IdTipoPropiedad
                    GROUP BY tp.idTipoPropiedad, tp.nombre, tp.descripcion
                    ORDER BY totalPropiedades DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var stats = new Dictionary<string, object>
                    {
                        ["idTipoPropiedad"] = reader.GetInt32("idTipoPropiedad"),
                        ["nombre"] = reader.GetString("nombre"),
                        ["descripcion"] = !reader.IsDBNull(reader.GetOrdinal("descripcion")) ? reader.GetString("descripcion") : string.Empty,
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

        // Obtener tipos más usados
        public async Task<(bool exito, string mensaje, List<TipoPropiedad>? tipos)>
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
                string query = @"SELECT tp.*, COUNT(p.IdPropiedad) as uso
                    FROM tipopropiedad tp
                    LEFT JOIN propiedad p ON tp.idTipoPropiedad = p.IdTipoPropiedad
                    GROUP BY tp.idTipoPropiedad
                    ORDER BY uso DESC
                    LIMIT @limite";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limite", limite);

                using var reader = await cmd.ExecuteReaderAsync();
                var tipos = new List<TipoPropiedad>();
                while (await reader.ReadAsync())
                {
                    tipos.Add(MapearTipoPropiedad(reader));
                }

                return (true, $"Se encontraron {tipos.Count} tipos más usados", tipos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipos más usados: {ex.Message}", null);
            }
        }

        // Obtener tipos sin uso
        public async Task<(bool exito, string mensaje, List<TipoPropiedad>? tipos)>
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
                    FROM tipopropiedad tp
                    LEFT JOIN propiedad p ON tp.idTipoPropiedad = p.IdTipoPropiedad
                    WHERE p.IdPropiedad IS NULL
                    ORDER BY tp.nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var tipos = new List<TipoPropiedad>();
                while (await reader.ReadAsync())
                {
                    tipos.Add(MapearTipoPropiedad(reader));
                }

                return (true, $"Se encontraron {tipos.Count} tipos sin uso", tipos);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener tipos sin uso: {ex.Message}", null);
            }
        }

        // Inicializar tipos de propiedad predeterminados
        public async Task<(bool exito, string mensaje)> InicializarTiposPropiedadPredeterminadosAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                var tiposPredeterminados = new Dictionary<string, string>
                {
                    { "Casa", "Vivienda unifamiliar independiente" },
                    { "Departamento", "Unidad de vivienda en edificio multifamiliar" },
                    { "Terreno", "Lote de tierra sin construcción" },
                    { "Local Comercial", "Espacio destinado a actividades comerciales" },
                    { "Oficina", "Espacio de trabajo profesional" },
                    { "Edificio", "Construcción completa de múltiples niveles" },
                    { "Quinta", "Propiedad con amplio terreno y jardín" },
                    { "Cochera", "Espacio para estacionamiento de vehículos" },
                    { "Deposito", "Espacio para almacenamiento" },
                    { "Almacen", "Espacio industrial para almacenaje" },
                    { "Campo", "Propiedad rural o agrícola" },
                    { "Granja", "Propiedad agrícola con instalaciones" },
                    { "Hacienda", "Propiedad rural de gran extensión" },
                    { "Casa de Playa", "Vivienda en zona costera" },
                    { "Casa de Campo", "Vivienda en zona rural" },
                    { "Penthouse", "Departamento de lujo en último piso" },
                    { "Duplex", "Vivienda de dos niveles" },
                    { "Triplex", "Vivienda de tres niveles" },
                    { "Loft", "Espacio diáfano adaptado como vivienda" },
                    { "Estudio", "Departamento de un solo ambiente" }
                };

                int creados = 0;
                foreach (var tipo in tiposPredeterminados)
                {
                    if (!await ExisteTipoPropiedadAsync(tipo.Key))
                    {
                        var tipoPropiedad = new TipoPropiedad
                        {
                            Nombre = tipo.Key,
                            Descripcion = tipo.Value,
                            CreadoAt = new DateTime(2020, 1, 1),
                            ActualizadoAt = new DateTime(2020, 1, 1)
                        };

                        var (exito, _, _) = await CrearTipoPropiedadAsync(tipoPropiedad);
                        if (exito) creados++;
                    }
                }

                return (true, $"Se crearon {creados} tipos de propiedad predeterminados");
            }
            catch (Exception ex)
            {
                return (false, $"Error al inicializar tipos de propiedad: {ex.Message}");
            }
        }

        // Método auxiliar para mapear
        private TipoPropiedad MapearTipoPropiedad(MySqlDataReader reader)
        {
            return new TipoPropiedad
            {
                IdTipoPropiedad = reader.GetInt32("idTipoPropiedad"),
                Nombre = reader.GetString("nombre"),
                Descripcion = !reader.IsDBNull(reader.GetOrdinal("descripcion")) ? reader.GetString("descripcion") : string.Empty,
                CreadoAt = reader.GetDateTime("creadoAt"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
