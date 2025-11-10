using MySqlConnector;


namespace backend_csharpcd_inmo.Structure_MVC.Utils
{
    public class ConnectionResult
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = "";
        public MySqlConnection? Conexion { get; set; }
    }

    public static class db_single
    {
        private static readonly string connectionString =
            "Server=localhost;" +
            "Port=3307;" +
            "Database=waaa;" +
            "User Id=root;" +
            "Password=;" +
            "Connection Timeout=5;" +
            "Pooling=true;" +
            "Minimum Pool Size=0;" +
            "Maximum Pool Size=100;";

        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public static ConnectionResult GetConnection()
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                var connection = new MySqlConnection(connectionString);

                try
                {
                    connection.Open();

                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        using var cmd = new MySqlCommand("SELECT 1", connection);
                        var result = cmd.ExecuteScalar();

                        if (result != null && Convert.ToInt32(result) == 1)
                        {
                            return new ConnectionResult
                            {
                                Exito = true,
                                Mensaje = "Conexión exitosa y verificada.",
                                Conexion = connection
                            };
                        }
                        else
                        {
                            connection.Close();
                            return new ConnectionResult
                            {
                                Exito = false,
                                Mensaje = "Conexión abierta pero sin respuesta correcta (SELECT 1 falló)."
                            };
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    if (attempt < MaxRetries)
                    {
                        Thread.Sleep(RetryDelayMs);
                    }
                    else
                    {
                        return new ConnectionResult
                        {
                            Exito = false,
                            Mensaje = $"Se agotaron los intentos de conexión: {ex.Message}"
                        };
                    }
                }
            }

            return new ConnectionResult
            {
                Exito = false,
                Mensaje = "No se pudo establecer conexión con la base de datos."
            };
        }
    }
}
