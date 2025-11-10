using MySqlConnector;
using backend_csharpcd_inmo.Structure_MVC.Models;
using backend_csharpcd_inmo.Structure_MVC.Utils;
using System.Data;

namespace backend_csharpcd_inmo.Structure_MVC.DAO
{
    public class EmpresaDao
    {
        // Crear empresa
        public async Task<(bool exito, string mensaje, int? id)> CrearEmpresaAsync(Empresa empresa)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                // Verificar si el RUC ya existe
                var existeRuc = await VerificarRucExisteAsync(empresa.Ruc);
                if (existeRuc)
                {
                    return (false, "El RUC ya está registrado en el sistema", null);
                }

                string query = @"INSERT INTO empresa 
                    (idUsuario, nombre, ruc, direccion, email, telefono, tipoEmpresa, fechaRegistro, actualizadoAt) 
                    VALUES (@idUsuario, @nombre, @ruc, @direccion, @email, @telefono, @tipoEmpresa, @fechaRegistro, @actualizadoAt);
                    SELECT LAST_INSERT_ID();";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", empresa.IdUsuario);
                cmd.Parameters.AddWithValue("@nombre", empresa.Nombre);
                cmd.Parameters.AddWithValue("@ruc", empresa.Ruc);
                cmd.Parameters.AddWithValue("@direccion", string.IsNullOrEmpty(empresa.Direccion) ? DBNull.Value : empresa.Direccion);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(empresa.Email) ? DBNull.Value : empresa.Email);
                cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(empresa.Telefono) ? DBNull.Value : empresa.Telefono);
                cmd.Parameters.AddWithValue("@tipoEmpresa", string.IsNullOrEmpty(empresa.TipoEmpresa) ? DBNull.Value : empresa.TipoEmpresa);
                cmd.Parameters.AddWithValue("@fechaRegistro", empresa.FechaRegistro);
                cmd.Parameters.AddWithValue("@actualizadoAt", empresa.ActualizadoAt);

                var result = await cmd.ExecuteScalarAsync();
                int nuevoId = Convert.ToInt32(result);

                return (true, "Empresa creada exitosamente", nuevoId);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al crear empresa: {ex.Message}", null);
            }
        }

        // Obtener empresa por ID
        public async Task<(bool exito, string mensaje, Empresa? empresa)> ObtenerEmpresaPorIdAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT e.*, u.nombre as NombreUsuario 
                    FROM empresa e
                    LEFT JOIN usuario u ON e.idUsuario = u.idUsuario
                    WHERE e.idEmpresa = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var empresa = MapearEmpresa(reader);
                    return (true, "Empresa encontrada", empresa);
                }

                return (false, "Empresa no encontrada", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener empresa: {ex.Message}", null);
            }
        }

        // Obtener empresa por RUC
        public async Task<(bool exito, string mensaje, Empresa? empresa)> ObtenerEmpresaPorRucAsync(string ruc)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT e.*, u.nombre as NombreUsuario 
                    FROM empresa e
                    LEFT JOIN usuario u ON e.idUsuario = u.idUsuario
                    WHERE e.ruc = @ruc";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ruc", ruc);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var empresa = MapearEmpresa(reader);
                    return (true, "Empresa encontrada", empresa);
                }

                return (false, "Empresa no encontrada", null);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener empresa: {ex.Message}", null);
            }
        }

        // Listar todas las empresas
        public async Task<(bool exito, string mensaje, List<Empresa>? empresas)> ListarEmpresasAsync()
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT e.*, u.nombre as NombreUsuario 
                    FROM empresa e
                    LEFT JOIN usuario u ON e.idUsuario = u.idUsuario
                    ORDER BY e.fechaRegistro DESC";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var empresas = new List<Empresa>();
                while (await reader.ReadAsync())
                {
                    empresas.Add(MapearEmpresa(reader));
                }

                return (true, $"Se encontraron {empresas.Count} empresas", empresas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar empresas: {ex.Message}", null);
            }
        }

        // Listar con paginación
        public async Task<(bool exito, string mensaje, List<Empresa>? empresas, int totalRegistros)>
            ListarEmpresasPaginadasAsync(int pagina, int tamanoPagina)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null, 0);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string countQuery = "SELECT COUNT(*) FROM empresa";
                using var countCmd = new MySqlCommand(countQuery, connection);
                int totalRegistros = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                string query = @"SELECT e.*, u.nombre as NombreUsuario 
                    FROM empresa e
                    LEFT JOIN usuario u ON e.idUsuario = u.idUsuario
                    ORDER BY e.fechaRegistro DESC
                    LIMIT @limit OFFSET @offset";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@limit", tamanoPagina);
                cmd.Parameters.AddWithValue("@offset", (pagina - 1) * tamanoPagina);

                using var reader = await cmd.ExecuteReaderAsync();
                var empresas = new List<Empresa>();
                while (await reader.ReadAsync())
                {
                    empresas.Add(MapearEmpresa(reader));
                }

                return (true, $"Página {pagina} obtenida exitosamente", empresas, totalRegistros);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al listar empresas: {ex.Message}", null, 0);
            }
        }

        // Buscar empresas
        public async Task<(bool exito, string mensaje, List<Empresa>? empresas)>
            BuscarEmpresasAsync(string? termino = null, string? tipoEmpresa = null, int? idUsuario = null)
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
                    condiciones.Add("(e.nombre LIKE @termino OR e.ruc LIKE @termino OR e.direccion LIKE @termino OR e.email LIKE @termino)");
                    parametros.Add(new MySqlParameter("@termino", $"%{termino}%"));
                }

                if (!string.IsNullOrWhiteSpace(tipoEmpresa))
                {
                    condiciones.Add("e.tipoEmpresa LIKE @tipoEmpresa");
                    parametros.Add(new MySqlParameter("@tipoEmpresa", $"%{tipoEmpresa}%"));
                }

                if (idUsuario.HasValue)
                {
                    condiciones.Add("e.idUsuario = @idUsuario");
                    parametros.Add(new MySqlParameter("@idUsuario", idUsuario.Value));
                }

                string whereClause = condiciones.Count > 0 ?
                    "WHERE " + string.Join(" AND ", condiciones) : "";

                string query = $@"SELECT e.*, u.nombre as NombreUsuario 
                    FROM empresa e
                    LEFT JOIN usuario u ON e.idUsuario = u.idUsuario
                    {whereClause}
                    ORDER BY e.fechaRegistro DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddRange(parametros.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();
                var empresas = new List<Empresa>();
                while (await reader.ReadAsync())
                {
                    empresas.Add(MapearEmpresa(reader));
                }

                return (true, $"Se encontraron {empresas.Count} empresas", empresas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al buscar empresas: {ex.Message}", null);
            }
        }

        // Obtener empresas por usuario
        public async Task<(bool exito, string mensaje, List<Empresa>? empresas)>
            ObtenerEmpresasPorUsuarioAsync(int idUsuario)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT e.*, u.nombre as NombreUsuario 
                    FROM empresa e
                    LEFT JOIN usuario u ON e.idUsuario = u.idUsuario
                    WHERE e.idUsuario = @idUsuario
                    ORDER BY e.fechaRegistro DESC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                using var reader = await cmd.ExecuteReaderAsync();
                var empresas = new List<Empresa>();
                while (await reader.ReadAsync())
                {
                    empresas.Add(MapearEmpresa(reader));
                }

                return (true, $"Se encontraron {empresas.Count} empresas del usuario", empresas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener empresas del usuario: {ex.Message}", null);
            }
        }

        // Obtener empresas por tipo
        public async Task<(bool exito, string mensaje, List<Empresa>? empresas)>
            ObtenerEmpresasPorTipoAsync(string tipoEmpresa)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje, null);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = @"SELECT e.*, u.nombre as NombreUsuario 
                    FROM empresa e
                    LEFT JOIN usuario u ON e.idUsuario = u.idUsuario
                    WHERE e.tipoEmpresa LIKE @tipoEmpresa
                    ORDER BY e.nombre ASC";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@tipoEmpresa", $"%{tipoEmpresa}%");

                using var reader = await cmd.ExecuteReaderAsync();
                var empresas = new List<Empresa>();
                while (await reader.ReadAsync())
                {
                    empresas.Add(MapearEmpresa(reader));
                }

                return (true, $"Se encontraron {empresas.Count} empresas del tipo especificado", empresas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener empresas por tipo: {ex.Message}", null);
            }
        }

        // Actualizar empresa
        // ✅ CORREGIDO: Incluir IdUsuario en el UPDATE
        public async Task<(bool exito, string mensaje)> ActualizarEmpresaAsync(Empresa empresa)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;

            try
            {
                // Verificar si el RUC existe en otra empresa
                var (existeExito, _, empresaExistente) = await ObtenerEmpresaPorRucAsync(empresa.Ruc);
                if (existeExito && empresaExistente != null && empresaExistente.IdEmpresa != empresa.IdEmpresa)
                {
                    return (false, "El RUC ya está registrado en otra empresa");
                }

                // ✅ AGREGAR idUsuario = @idUsuario
                string query = @"UPDATE empresa SET 
            idUsuario = @idUsuario,
            nombre = @nombre,
            ruc = @ruc,
            direccion = @direccion,
            email = @email,
            telefono = @telefono,
            tipoEmpresa = @tipoEmpresa,
            actualizadoAt = @actualizadoAt
            WHERE idEmpresa = @id";

                using var cmd = new MySqlCommand(query, connection);

                // ✅ AGREGAR parámetro
                cmd.Parameters.AddWithValue("@idUsuario", empresa.IdUsuario);
                cmd.Parameters.AddWithValue("@id", empresa.IdEmpresa);
                cmd.Parameters.AddWithValue("@nombre", empresa.Nombre);
                cmd.Parameters.AddWithValue("@ruc", empresa.Ruc);
                cmd.Parameters.AddWithValue("@direccion", string.IsNullOrEmpty(empresa.Direccion) ? DBNull.Value : empresa.Direccion);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrEmpty(empresa.Email) ? DBNull.Value : empresa.Email);
                cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(empresa.Telefono) ? DBNull.Value : empresa.Telefono);
                cmd.Parameters.AddWithValue("@tipoEmpresa", string.IsNullOrEmpty(empresa.TipoEmpresa) ? DBNull.Value : empresa.TipoEmpresa);
                cmd.Parameters.AddWithValue("@actualizadoAt", empresa.ActualizadoAt);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Empresa actualizada exitosamente");
                }

                return (false, "No se encontró la empresa a actualizar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al actualizar empresa: {ex.Message}");
            }
        }


        // Eliminar empresa
        public async Task<(bool exito, string mensaje)> EliminarEmpresaAsync(int id)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return (false, connectionResult.Mensaje);
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "DELETE FROM empresa WHERE idEmpresa = @id";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                int filasAfectadas = await cmd.ExecuteNonQueryAsync();

                if (filasAfectadas > 0)
                {
                    return (true, "Empresa eliminada exitosamente");
                }

                return (false, "No se encontró la empresa a eliminar");
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al eliminar empresa: {ex.Message}");
            }
        }

        // Verificar si un RUC existe
        public async Task<bool> VerificarRucExisteAsync(string ruc)
        {
            var connectionResult = db_single.GetConnection();
            if (!connectionResult.Exito || connectionResult.Conexion == null)
            {
                return false;
            }

            using var connection = connectionResult.Conexion;
            try
            {
                string query = "SELECT COUNT(*) FROM empresa WHERE ruc = @ruc";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ruc", ruc);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
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
                    COUNT(*) as TotalEmpresas,
                    COUNT(CASE WHEN ruc LIKE '20%' THEN 1 END) as EmpresasJuridicas,
                    COUNT(CASE WHEN ruc LIKE '10%' THEN 1 END) as PersonasNaturales,
                    COUNT(DISTINCT tipoEmpresa) as TiposEmpresa,
                    COUNT(DISTINCT idUsuario) as UsuariosConEmpresas
                    FROM empresa";

                using var cmd = new MySqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var estadisticas = new Dictionary<string, object>();
                if (await reader.ReadAsync())
                {
                    estadisticas["totalEmpresas"] = reader.GetInt32("TotalEmpresas");
                    estadisticas["empresasJuridicas"] = reader.GetInt32("EmpresasJuridicas");
                    estadisticas["personasNaturales"] = reader.GetInt32("PersonasNaturales");
                    estadisticas["tiposEmpresa"] = reader.GetInt32("TiposEmpresa");
                    estadisticas["usuariosConEmpresas"] = reader.GetInt32("UsuariosConEmpresas");
                }

                return (true, "Estadísticas obtenidas exitosamente", estadisticas);
            }
            catch (MySqlException ex)
            {
                return (false, $"Error al obtener estadísticas: {ex.Message}", null);
            }
        }

        // Método auxiliar para mapear resultados
        private Empresa MapearEmpresa(MySqlDataReader reader)
        {
            return new Empresa
            {
                IdEmpresa = reader.GetInt32("idEmpresa"),
                IdUsuario = reader.GetInt32("idUsuario"),
                Nombre = reader.GetString("nombre"),
                Ruc = reader.GetString("ruc"),
                Direccion = !reader.IsDBNull(reader.GetOrdinal("direccion")) ? reader.GetString("direccion") : string.Empty,
                Email = !reader.IsDBNull(reader.GetOrdinal("email")) ? reader.GetString("email") : string.Empty,
                Telefono = !reader.IsDBNull(reader.GetOrdinal("telefono")) ? reader.GetString("telefono") : string.Empty,
                TipoEmpresa = !reader.IsDBNull(reader.GetOrdinal("tipoEmpresa")) ? reader.GetString("tipoEmpresa") : string.Empty,
                FechaRegistro = reader.GetDateTime("fechaRegistro"),
                ActualizadoAt = reader.GetDateTime("actualizadoAt")
            };
        }
    }
}
